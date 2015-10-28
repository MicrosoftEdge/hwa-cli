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
        var chromeOSManifest = <webConverter.IChromeOSManifest>JSON.parse(fs.readFileSync(p.join(dest, "manifest.json"), "utf8").toString());
        var w3cManifest = webConverter.chromeToW3CManifest(chromeOSManifest);
        var rl = readline.createInterface({ input: process.stdin, output: process.stdout });
        rl.question("Identity Name: ", (identityName: string) => {
            rl.question("Publisher Identity: ", (publisherIdentity: string) => {
                rl.question("Publisher Display Name: ", (publisherDisplayName: string) => {
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