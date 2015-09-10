import cp = require("child_process");
import fs = require("fs");
import net = require("net");
import os = require("os");
import p = require("path");

var admzip = require("adm-zip");

var debugHost = "localhost";
//var debugHost = "10.137.229.56";

var PORT = 6767;
var MAX_RETRIES = 5;
var PROTOCOL_VERSION = 1;

var sessionFilePath = p.join(os.tmpdir(), "hwa.session");

module ProxyAdapter {
    export function clearSession() {
        if (fs.existsSync(sessionFilePath)) {
            fs.unlink(sessionFilePath);
        }
    }

    export function exit() {
        ProxyAdapter.clearSession();
        rpc("exit", null, true);
    }

    export function launchAppx() {
        rpc("launchAppx");
    }

    export function registerAndLaunchAppxManifest(path: string) {
        // Compress the entire folder where the AppxManifest file is and send it to the VM.
        var zip = new admzip();
        zip.addLocalFolder(p.dirname(p.resolve(path)));
        rpc("deployAppxManifest", zip.toBuffer());
    }
}
var typeCheck: HWAProxyAdapter = ProxyAdapter;
export = ProxyAdapter;


function establishConnection(sessionOnly: boolean, callback: (socket: net.Socket) => any) {
    function doConnect(address: string, success: (socket: net.Socket) => any, error: (e: string) => any) {
        var socket = new net.Socket();
        socket.once("error", (e: string) => {
            socket.destroy();
            error(e);
        });
        socket.connect(6767, address, () => {
            socket.removeAllListeners("error");
            success(socket);
        });
    }

    if (fs.existsSync(sessionFilePath)) {
        // Found session file, try reconnecting
        var sessionFile = fs.readFileSync(sessionFilePath, "utf-8");
        var address = sessionFile;

        doConnect(address,
            socket => {
                // Reconnect successful
                callback(socket);
            },
            error => {
                // Reconnect failed, retry from scratch
                ProxyAdapter.clearSession();
                if (!sessionOnly) {
                    establishConnection(false, callback);
                }
            });
    } else if (!sessionOnly) {
        // Todo: Provision VM with RD
        var address = debugHost;

        // Establish Remote Desktop
        var rdConfig = fs.readFileSync("./templates/rdConfig.rdp", "utf-8");
        rdConfig = rdConfig.replace("{address}", address);
        fs.writeFileSync("./bin/rdConfig.rdp", rdConfig, "utf-8");
        cp.execSync("start " + "./bin/rdConfig.rdp");

        // Connect to remote socket
        var retriesLeft = MAX_RETRIES;
        var successHandler = function successHandler(socket: net.Socket) {
            // Connection successful, save to session file
            fs.writeFileSync(sessionFilePath, address, "utf-8");
            callback(socket);
        };
        var errorHandler = function errorHandler(e: string) {
            // Failed to connect, retry
            retriesLeft--;
            if (retriesLeft > 0) {
                setTimeout(() => {
                    doConnect(address, successHandler, errorHandler);
                }, 10000);
            }
        };
        doConnect(address, successHandler, errorHandler);
    } else {
        callback(null);
    }
}

function rpc(command: string, data?: Buffer, sessionOnly = false) {
    data = data || new Buffer(0);

    var payloadBuffer = Buffer.concat(
        [
            new Buffer(PROTOCOL_VERSION + ";"),
            new Buffer(command + ";"),
            new Buffer(data.length + ";"),
            data
        ]);

    establishConnection(sessionOnly, socket => {
        if (!socket) {
            module.parent.exports.printDocs("connectionError");
            return;
        }
        socket.once("data", () => {
            // Block the CLI tool until the server finishes processing the request
            socket.destroy();
        });
        socket.once("error", (e: any) => {
            console.log(e);
            socket.destroy();
        });
        socket.write(payloadBuffer);
    });
}