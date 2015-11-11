declare var Promise: PromiseConstructorLike;

import cp = require("child_process");
import fs = require("fs");
import readline = require("readline");
import os = require("os");
import p = require("path");

import webConverter = require("./webConverter");

var admzip = require("adm-zip");
var guid = require("guid");
var phantomjs = require("phantomjs");
var rimraf = require("rimraf");

/*
    .crx file format (Little Endian)
    43 72 32 34 - "Cr24"
    xx xx xx xx - crx format version number
    xx xx xx xx - length of the public key in bytes
    xx xx xx xx - length of the signature in bytes
    ........... - the contents of the public key
    ........... - the contents of the signature
    ........... - the contents of the zip file
*/

var assetSizes = {
    logoStore: { w: 50, h: 50 },
    logoSmall: { w: 44, h: 44 },
    logoLarge: { w: 150, h: 150 },
    splashScreen: { w: 620, h: 300 }
};
var offsetPublicKeyLength = 8;
var offsetSignatureLength = 12;

interface IAssetInfo {
    requiredSize: { w: number; h: number; };
    nativeSize: { w: number; h: number; };
    src: string;
}

export function convert(src: string, dest: string) {
    return new Promise<void>(c => {
        // Setup tmp
        rimraf.sync(dest);
        fs.mkdirSync(dest);


        // Extract crx
        extractCrx(src, dest);


        // Convert manifest
        var chromeOSManifest = <webConverter.IChromeOSManifest>JSON.parse(sanitizeJSONString(fs.readFileSync(p.join(dest, "manifest.json"), "utf8")));

        var w3cManifest = webConverter.chromeToW3CManifest(chromeOSManifest, (locale, varName) => {
            // Note: variable name is not case sensitive
            varName = varName.toLowerCase();
            var locResPath = p.join(dest, "_locales", locale, "messages.json");
            if (fs.existsSync(locResPath)) {
                // We've encountered a bunch of malformed JSON so we need to trim them, then replace all newline characters with a space
                var msgs = JSON.parse(sanitizeJSONString(fs.readFileSync(locResPath, "utf8")));
                for (var key in msgs) {
                    if (key.toLowerCase() === varName) {
                        return msgs[key].message;
                    }
                }
            }
            return null;
        });


        // Establish assets
        function matchBest(matchRecord: IAssetInfo, asset: { sizes: string; src: string; }) {
            if (matchRecord.nativeSize === matchRecord.requiredSize) {
                return;
            }
            var prevDeltaW = matchRecord.nativeSize.w - matchRecord.requiredSize.w;
            var prevDeltaH = matchRecord.nativeSize.h - matchRecord.requiredSize.h;
            var prevDelta = Math.abs(prevDeltaW) < Math.abs(prevDeltaW) ? prevDeltaW : prevDeltaH;
            var w = +asset.sizes.split("x")[0]
            var h = +asset.sizes.split("x")[1];
            var deltaW = w - matchRecord.requiredSize.w;
            var deltaH = h - matchRecord.requiredSize.h;
            var delta = Math.abs(deltaW) < Math.abs(deltaH) ? deltaW : deltaH;
            if ((prevDelta < 0 && delta > prevDelta) || (prevDelta > 0 && delta < prevDelta)) {
                // Update rules
                // If the recorded delta is negative, then no image has been set yet, or a smaller-than-ideal image is currently recorded
                //   Use the new image if the delta is greater (closer to 0) than before
                // If the recorded delta is positive, then a larger-than-ideal is currently recorded
                //   Use the new image if the delta is smaller (closer to 0) than before
                matchRecord.nativeSize = { w: w, h: h };
                matchRecord.src = asset.src;
            }
        }
        function resizeAndAddToManifest(assetInfo: IAssetInfo) {
            var newSize = `${assetInfo.requiredSize.w}x${assetInfo.requiredSize.h}`;
            var newSrc = p.parse(assetInfo.src).name + `_scaled_${newSize}.png`;
            resizeImage(p.join(dest, assetInfo.src), assetInfo.requiredSize.w, assetInfo.requiredSize.h, newSrc);
            w3cManifest.icons = w3cManifest.icons.filter(icon => icon.sizes !== newSize);
            w3cManifest.icons.push({
                sizes: newSize,
                src: newSrc
            });
            console.log(`Resized ${assetInfo.src} (${assetInfo.nativeSize.w}x${assetInfo.nativeSize.h}) to ${newSrc}`);
        }
        var logoStore = { nativeSize: { w: Number.NEGATIVE_INFINITY, h: Number.NEGATIVE_INFINITY }, src: "", requiredSize: assetSizes.logoStore };
        var logoSmall = { nativeSize: { w: Number.NEGATIVE_INFINITY, h: Number.NEGATIVE_INFINITY }, src: "", requiredSize: assetSizes.logoSmall };
        var logoLarge = { nativeSize: { w: Number.NEGATIVE_INFINITY, h: Number.NEGATIVE_INFINITY }, src: "", requiredSize: assetSizes.logoLarge };
        var splashScreen = { nativeSize: { w: Number.NEGATIVE_INFINITY, h: Number.NEGATIVE_INFINITY }, src: "", requiredSize: assetSizes.splashScreen };
        var iconAsSplashScreen = !w3cManifest.splash_screens.length;
        for (var i = 0; i < w3cManifest.icons.length; i++) {
            var iconData = w3cManifest.icons[i];
            matchBest(logoStore, iconData);
            matchBest(logoSmall, iconData);
            matchBest(logoLarge, iconData);
            iconAsSplashScreen && matchBest(splashScreen, iconData);
        }
        if (!iconAsSplashScreen) {
            for (var i = 0; i < w3cManifest.splash_screens.length; i++) {
                matchBest(splashScreen, w3cManifest.splash_screens[i]);
            }
        }

        (logoStore.nativeSize.w !== logoStore.requiredSize.w || logoStore.nativeSize.h !== logoStore.requiredSize.h) && resizeAndAddToManifest(logoStore);
        (logoSmall.nativeSize.w !== logoSmall.requiredSize.w || logoSmall.nativeSize.h !== logoSmall.requiredSize.h) && resizeAndAddToManifest(logoSmall);
        (logoLarge.nativeSize.w !== logoLarge.requiredSize.w || logoLarge.nativeSize.h !== logoLarge.requiredSize.h) && resizeAndAddToManifest(logoLarge);
        (splashScreen.nativeSize.w !== splashScreen.requiredSize.w || splashScreen.nativeSize.h !== splashScreen.requiredSize.h) && resizeAndAddToManifest(splashScreen);


        // Convert W3C manifest to Appx manifest
        var rl = readline.createInterface({ input: process.stdin, output: process.stdout });
        rl.question("Identity Name: ", (identityName: string) => {
            //identityName = identityName || "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";
            rl.question("Publisher Identity: ", (publisherIdentity: string) => {
                //publisherIdentity = publisherIdentity || "CN=AUTHOR_NAME";
                rl.question("Publisher Display Name: ", (publisherDisplayName: string) => {
                    //publisherDisplayName = publisherDisplayName || publisherIdentity.substr(3);
                    rl.close();
                    console.log();
                    console.log("Converting manifest to AppxManifest");
                    var xmlManifest = webConverter.w3CToAppxManifest(w3cManifest, fs.readFileSync(p.join(__dirname, "../templates/w3c-AppxManifest-template.xml"), "utf8"),
                        {
                            identityName: identityName,
                            publisherDisplayName: publisherDisplayName,
                            publisherIdentity: publisherIdentity
                        },
                        [
                            { name: "GeneratedFrom", value: "HWA-CLI" },
                            { name: "GenerationDate", value: new Date().toUTCString() },
                            { name: "ToolVersion", value: "0.1.0" }
                        ],
                        assetInfos => {
                            assetInfos.forEach(info => {
                                var newPath = p.join(p.dirname(info.src), p.parse(info.src).name + `_scaled_${info.requiredSize.w}x${info.requiredSize.h}.png`);
                                var outputPath = p.join(dest, p.dirname(info.src), newPath)
                                cp.execFileSync(phantomjs.path, [p.join(__dirname, "/phantom-image.js"), p.join(dest, info.src), "" + info.requiredSize.w, "" + info.requiredSize.h, outputPath]/*, { stdio: [process.stdin, process.stdout, process.stderr] }*/);
                                info.src = newPath;
                                info.nativeSize = info.requiredSize;
                            });
                        });
                    console.log();
        
                    // Write the AppxManifest
                    var appxManifestOutputPath = p.join(dest, "AppxManifest.xml");
                    console.log("Writing AppxManifest: " + appxManifestOutputPath);
                    if (fs.existsSync(appxManifestOutputPath)) {
                        fs.unlinkSync(appxManifestOutputPath);
                    }
                    fs.writeFileSync(appxManifestOutputPath, xmlManifest);
                    console.log();
                    c();
                });
            });
        });
    });
}

export function extractCrx(src: string, dest: string) {
    if (!fs.existsSync(dest)) {
        fs.mkdirSync(dest);
    }
    var zipPath = p.join(dest, guid.raw() + ".zip");

    // Read crx file
    var crxFile = fs.readFileSync(src);

    // Make sure that it is a crx archive by searching for the signature "67 114 50 52"
    if (crxFile[0] !== 67 || crxFile[1] !== 114 || crxFile[2] !== 50 || crxFile[3] !== 52) {
        // src is not a crx archive, just unzip it directly
        new admzip(src).extractAllTo(dest);
        return;
    }

    // Write zip contents
    var lengthPK = crxFile.readUIntLE(offsetPublicKeyLength, 4);
    var lengthSig = crxFile.readUIntLE(offsetSignatureLength, 4);
    var zipContentsOffset = offsetSignatureLength + 4 + lengthPK + lengthSig;
    var zipContents = crxFile.slice(zipContentsOffset)
    fs.writeFileSync(zipPath, zipContents);

    // Extract zip and cleanup
    new admzip(zipPath).extractAllTo(dest);
    fs.unlinkSync(zipPath);
}

function resizeImage(absSrcPath: string, targetWidth: number, targetHeight: number, newFilename: string) {
    var outputPath = p.join(p.dirname(absSrcPath), newFilename);
    cp.execFileSync(phantomjs.path, [p.join(__dirname, "/phantom-image.js"), absSrcPath, "" + targetWidth, "" + targetHeight, outputPath]/*, { stdio: [process.stdin, process.stdout, process.stderr] }*/);
}

function sanitizeJSONString(str: string) {
    return str.trim().replace(/\r/g, "").replace(/\n/g, "");
}