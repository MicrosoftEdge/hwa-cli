declare var phantom: any;

var system = require("system");
var inputPath = system.args[1];
var targetWidth = +system.args[2];
var targetHeight = +system.args[3];
var outputPath = system.args[4];

var page = require('webpage').create();
page.open('about:blank', function () {
    page.onCallback = function () {
        page.clipRect = {
            top: 0,
            left: 0,
            width: targetWidth,
            height: targetHeight
        };
        page.render(outputPath);
        phantom.exit();
    };
    page.evaluate(function (src: string, w: number, h: number) {
        src = 'file:///' + src;
        
        var img = new Image();
        img.onload = () => {
            var div = document.createElement("div");
            div.style.width = w + "px";
            div.style.height = h + "px";
            div.style.backgroundImage = `url("${src.replace(/\\/g, "\\\\")}")`;
            div.style.backgroundRepeat = "no-repeat";

            if (img.width > w || img.height > h) {
                // Scale down
                div.style.backgroundSize = "contain";
            } else {
                // Center
                div.style.backgroundPosition = "50%";
            }
            document.body.style.margin = "0";
            document.body.appendChild(div);
            setTimeout((<any>window).callPhantom, 1);
        };
        img.src = src;
    }, inputPath, targetWidth, targetHeight);
});