declare var Promise: PromiseConstructorLike;

import fs = require("fs");
import readline = require("readline");
import os = require("os");
import p = require("path");

import webConverter = require("./webConverter");

var admzip = require("adm-zip");
var guid = require("guid");
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

var offsetPublicKeyLength = 8;
var offsetSignatureLength = 12;

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
                        ]);
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

function sanitizeJSONString(str: string) {
    return str.trim().replace(/\r/g, "").replace(/\n/g, "");
}