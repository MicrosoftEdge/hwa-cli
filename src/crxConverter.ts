import fs = require("fs");
import readline = require("readline");
import os = require("os");
import p = require("path");

import webConverter = require("./webConverter");

var admzip = require("adm-zip");
var guid = require("guid");
var promise = require("promise");
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

export function convert(src: string, dest: string, tmpDir?: string): PromiseLike<void> {
    return new promise((c: Function) => {
        // Setup tmp
        tmpDir = tmpDir || p.join(os.tmpdir(), "crxConverter");
        rimraf.sync(tmpDir);
        fs.mkdirSync(tmpDir);

        // Extract crx
        if (!fs.existsSync(dest)) {
            fs.mkdirSync(dest);
        }
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
                    var xmlManifest = webConverter.w3CToAppxManifest(w3cManifest, fs.readFileSync("./templates/w3c-AppxManifest-template.xml", "utf8"),
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

