declare function afterEach(teardownFunc: Function): void;
declare function beforeEach(setupFunc: Function): void;
declare function describe(what: string, body: Function): void;
declare function it(should: string, testFunction: (done?: Function) => void): void;
declare var mocha: any;

import assert = require("assert");
import cp = require("child_process");
import fs = require("fs");
import net = require("net");

import adapter = require("../src/adapter");

class MockObject {
    onceHandlers: { eventName: string; callback: Function; }[] = [];
    onHandlers: { eventName: string; callback: Function; }[] = [];

    on(eventName: string, callback: Function) {
        this.onHandlers.push({ eventName: eventName, callback: callback });
    }

    once(eventName: string, callback: Function) {
        this.onceHandlers.push({ eventName: eventName, callback: callback });
    }

    removeAllListeners(eventName: string) {
        for (var i = this.onceHandlers.length - 1; i >= 0; i--) {
            var handler = this.onceHandlers[i];
            if (handler.eventName === eventName) {
                this.onceHandlers.splice(i, 1);
            }
        }
        for (var i = this.onHandlers.length - 1; i >= 0; i--) {
            var handler = this.onHandlers[i];
            if (handler.eventName === eventName) {
                this.onHandlers.splice(i, 1);
            }
        }
    }

    _dispatch(eventName: string, eventObj: any) {
        this.onHandlers.forEach(handler => {
            if (handler.eventName !== eventName) {
                return;
            }
            handler.callback(eventObj);
        });

        for (var i = this.onceHandlers.length - 1; i >= 0; i--) {
            var handler = this.onceHandlers[i];
            if (handler.eventName !== eventName) {
                continue;
            }
            handler.callback(eventObj);
            this.onceHandlers.splice(i, 1);
        }
    }
}

class MockSocket extends MockObject {
    private static _kidnapper: (socket: MockSocket) => void = null;

    destroyed = false;

    constructor() {
        super();
        if (MockSocket._kidnapper) {
            var kidnapper = MockSocket._kidnapper;
            MockSocket._kidnapper = null;
            kidnapper(this);
        }
    }

    connect(port: number, host: string, callback: Function) {
    }

    destroy() {
        this.destroyed = true;
    }

    write(buffer: Buffer) {
        return true;
    }

    static kidnapNextInstance(kidnapper: (socket: MockSocket) => void) {
        MockSocket._kidnapper = kidnapper;
    }
}

class SuccessMockSocket extends MockSocket {
    connect(port: number, host: string, callback: Function) {
        callback();
    }
}

class FailMockSocket extends MockSocket {
    connect(port: number, host: string, callback: Function) {
        this._dispatch("error", {});
    }
}

function makeCache(hostname = "1.2.3.4") {
    fs.writeFileSync(adapter._sessionFilePath, hostname, "utf-8");
}

function monkeyPatch(obj: any, funcName: string, callback: Function, dontRestoreAfterFirstCall: boolean) {
    var orig = obj[funcName];

    var patch = function () {
        var result = callback.apply(obj, arguments);
        if (!dontRestoreAfterFirstCall) {
            (<any>patch).restore();
        }
        return result;
    };
    (<any>patch).restore = function () {
        obj[funcName] = orig;
    };
    (<any>patch).orig = orig;
    obj[funcName] = patch;
}

var _socket = net.Socket;

function setup() {
    // Zipping sums up to a significant amount of time over several
    // tests, we will mock it by default and have tests opt-out by 
    // calling .restore() on _compressPath if necessary
    monkeyPatch(adapter, "_compressPath", function () {
        return new Buffer("testdata");
    }, true);

    // execSync is used to launch remote desktop which we don't want
    // to happen during tests. We will no-op it by default and have
    // tests opt-out if necessary
    monkeyPatch(cp, "execSync", function () { }, true);

    // Disable retries, tests that are interested in testing the retry
    // logic will opt-out
    adapter._numRetries = 0;

    // Delete cache file
    if (fs.existsSync(adapter._sessionFilePath)) {
        fs.unlinkSync(adapter._sessionFilePath);
    }
}

function teardown() {
    // Restore socket class in case it was mocked
    net.Socket = _socket;

    // Restore execSync
    (<any>cp).execSync.restore();

    // Restore any direct APIs on the Adapter
    Object.keys(adapter).forEach(key => {
        var member = (<any>adapter)[key];
        member.restore && member.restore();
    });

    // Delete cache file
    if (fs.existsSync(adapter._sessionFilePath)) {
        fs.unlinkSync(adapter._sessionFilePath);
    }
}

describe("Adapter", function () {
    beforeEach(setup);
    afterEach(teardown);

    describe("registerAndLaunchAppxManifest", function () {

        describe("RDP scenarios", function () {
            it("should launch rdp when no cache file is found", function (done) {
                net.Socket = <any>SuccessMockSocket;
                monkeyPatch(cp, "execSync", function (cmd: string) {
                    if (cmd.indexOf(".rdp") === -1) {
                        assert.fail();
                    }
                    done();
                }, false);
                adapter.registerAndLaunchAppxManifest("test");
            });

            it("should launch rdp when connecting to cached hostname fails", function (done) {
                makeCache();
                net.Socket = <any>FailMockSocket;
                monkeyPatch(cp, "execSync", function (cmd: string) {
                    if (cmd.indexOf(".rdp") === -1) {
                        assert.fail();
                    }
                    done();
                }, false);
                adapter.registerAndLaunchAppxManifest("test");
            });

            it("should not launch rdp when connecting to cached hostname succeeds", function (done) {
                makeCache();
                net.Socket = <any>SuccessMockSocket;
                monkeyPatch(cp, "execSync", function (cmd: string) {
                    assert.fail();
                }, false);

                MockSocket.kidnapNextInstance(socket => {
                    socket.write = function (buffer: Buffer) {
                        done();
                        return true;
                    };
                });
                adapter.registerAndLaunchAppxManifest("test");

                (<any>cp.execSync).restore();
            });
        });

        describe("Caching scenarios", function () {
            it("should connect to cached hostname if cache file exists", function (done) {
                var testHostname = "testHostname";
                makeCache(testHostname);
                net.Socket = <any>SuccessMockSocket;

                MockSocket.kidnapNextInstance(socket => {
                    socket.connect = function (port: number, host: string, callback: Function) {
                        assert.equal(testHostname, host);
                        done();
                    };
                });
                adapter.registerAndLaunchAppxManifest("test");
            });

            it("should create cache file if connection was successful", function (done) {
                net.Socket = <any>SuccessMockSocket;

                MockSocket.kidnapNextInstance(socket => {
                    socket.write = function (buffer: Buffer) {
                        assert.ok(fs.existsSync(adapter._sessionFilePath));
                        done();
                        return true;
                    };
                });
                adapter.registerAndLaunchAppxManifest("test");
            });

            it("should delete cache file if connection failed", function (done) {
                net.Socket = <any>FailMockSocket;
                makeCache();

                MockSocket.kidnapNextInstance(socket => {
                    // This is the fail socket, after which we expect the adapter
                    // to start over and create another socket
                    assert.ok(fs.existsSync(adapter._sessionFilePath));
                    MockSocket.kidnapNextInstance(socket => {
                        // This is the retry attempt, at this point the cache file
                        // should've been deleted
                        assert.ok(!fs.existsSync(adapter._sessionFilePath));
                        done();
                    });
                });
                adapter.registerAndLaunchAppxManifest("test");
            });
        });
    });
});