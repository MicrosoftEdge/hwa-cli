import cp = require("child_process");
import fs = require("fs");
import net = require("net");
import os = require("os");
import p = require("path");

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

export function main(argv: string[], argc: number) {
    var cmd = (argv[2] || "").toLowerCase();
    var args = argv.slice(3);
    var handled = true;

    switch (cmd.toLowerCase()) {
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
        printDocs("usage");
    }
}

export function printDocs(filename: string) {
    console.log(fs.readFileSync(p.join(p.dirname(process.argv[1]), "docs", filename), "utf8"));
}
