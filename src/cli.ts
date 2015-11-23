/// <reference path="../projects/typings/node.d.ts" />
/// <reference path="../projects/typings/adapter.d.ts" />

import cp = require("child_process");
import fs = require("fs");
import net = require("net");
import os = require("os");
import p = require("path");
import readline = require("readline");

import cloudAppx = require("./cloudAppx");
import crxConverter = require("./crxConverter");
import webConverter = require("./webConverter");

var ncp = require("ncp");
var rimraf = require("rimraf");
var validator = require("validator");

var templateAppxManifestPath = "templates/AppxManifest";
var rootTempPath = p.join(os.tmpdir(), "hwa-client");
var appxManifestTempPath = p.join(rootTempPath, "AppxManifest");
var appxManifestFilePath = p.join(appxManifestTempPath, "AppxManifest.xml");

var hwaAdapter: HWAAdapter;
try {
    hwaAdapter = require("appx-tools");
} catch (e) {
    hwaAdapter = require("./adapter.js");
}

function cleanTemp() {
    rimraf.sync(rootTempPath);
    fs.mkdirSync(rootTempPath);
}
cleanTemp();

export function main(argv: any) {
    var cmd = (argv._[0] || "").toLowerCase();
    var args = argv._.slice(1);
    var handled = true;

    switch (cmd) {
        case "convert":
            if (fs.existsSync(args[0])) {
                var filenameWithoutExt = p.parse(args[0]).name;
                var outputPath = p.join(rootTempPath, "convert");
                crxConverter.convert(argv, args[0], outputPath).then(() => {
                    cloudAppx.invoke(filenameWithoutExt, outputPath, p.join(p.dirname(args[0]), filenameWithoutExt + ".appx"), (e: Error) => {
                        e && console.log(e);
                    });
                }, (e: any) => {
                    e && console.log(e);
                });
            } else {
                handled = false;
            }
            break;

        case "deploy":
            if (fs.existsSync(args[0])) {
                if (p.basename(args[0]).toLocaleLowerCase() === "appxmanifest.xml") {
                    // Argument is the path to an AppxManifest.xml
                    hwaAdapter.registerAndLaunchAppxManifest(args[0]);
                } else {
                    printDocs("fileNotSupported");
                }
            } else if (validator.isURL(args[0], { require_protocol: true })) {
                // Argument is an URL
                // Copy AppxManifest template to temp
                cleanTemp();
                ncp.ncp(p.resolve(p.join(__dirname, "..", templateAppxManifestPath)), appxManifestTempPath, {}, () => {
                    // Edit the template AppxManifest file
                    var file = fs.readFileSync(appxManifestFilePath, "utf8");
                    file = file.replace("{startpage}", args[0]);
                    fs.writeFileSync(appxManifestFilePath, file, "utf8");
                    hwaAdapter.registerAndLaunchAppxManifest(appxManifestFilePath);
                });
            } else {
                handled = false;
            }
            break;

        case "restart":
            hwaAdapter.launchAppx();
            break;

        // Proxy Adapter commands
        case "clearsession":
            var proxyAdapter = <HWAProxyAdapter>hwaAdapter;
            proxyAdapter.clearSession && proxyAdapter.clearSession();
            break;

        case "exit":
            var proxyAdapter = <HWAProxyAdapter>hwaAdapter;
            proxyAdapter.exit && proxyAdapter.exit();
            break;

        default:
            handled = false;
            break;
    }

    if (!handled) {
        argv.help();
    }
}

export function printDocs(filename: string) {    
    var filePath = p.join(p.dirname(fs.realpathSync(__filename)), "../../docs", filename);
    console.log(fs.readFileSync(filePath, "utf8"));
}
