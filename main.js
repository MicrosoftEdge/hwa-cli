#! /usr/bin/env node

if (module.parent) {
    module.exports = {};
} else {
    var hwaCli = require("./bin/cli.js");
    hwaCli.main(process.argv, process.argc);
}