import fs = require("fs");
import os = require("os");
import p = require("path");
import url = require("url");

var archiver = require("archiver");
var request = require("request");

var serviceEndpoint = 'http://cloudappx.azurewebsites.net';

export function invoke(appName: string, appFolder: string, outputPath: string, callback: Function) {
    var archive = archiver('zip');
    var zipFile = p.join(os.tmpdir(), appName + '.zip');
    var output = fs.createWriteStream(zipFile);
    archive.on('error', function (err: any) {
        return callback && callback(err);
    });

    archive.pipe(output);

    archive.directory(appFolder, appName);
    archive.finalize();
    output.on('close', function () {
        var options = {
            method: 'POST',
            url: url.resolve(serviceEndpoint, '/v2/build'),
            encoding: 'binary'
        };
        console.log('Invoking the CloudAppX service...');

        var req = request.post(options, function (err: any, resp: any, body: string) {
            if (err) {
                return callback && callback(err);
            }

            if (resp.statusCode !== 200) {
                return callback && callback(new Error('Failed to create the package. The CloudAppX service returned an error - ' + resp.statusMessage + ' (' + resp.statusCode + '): ' + body));
            }

            fs.writeFile(outputPath, body, { 'encoding': 'binary' }, function (err) {
                if (err) {
                    return callback && callback(err);
                }

                fs.unlink(zipFile, function (err) {
                    return callback && callback(err);
                });
            });
        });

        req.form().append('xml', fs.createReadStream(zipFile));
    });
}