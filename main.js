#! /usr/bin/env node

var argv = require('yargs')
	.usage('Usage: $0 <command> [options]')
	.command('convert', 'Converts a supported package to a Windows Appx package.', function(yargs) {
		argv = yargs
			.usage('Usage: $0 convert <package> [options]')
			.demand(2, 'You must specify the path of a package to convert.')
			.option('m', {
				alias: 'more',
				demand: false,
				describe: 'Optional flag to customize converted'
			})
			.help('help')
			.argv;
	})
	.command('deploy', '[EXPERIMENTAL] Registers and launches an AppxManifest or URL.', function(yargs) {
		argv = yargs
			.usage('Usage: $0 deploy <url|AppxManifest>')
			.demand(2, 'You must supply a path to an AppxManifest or url to deploy.')
			.help('help')
			.argv;
	})
	.command('restart', '[EXPERIMENTAL] Starts or restarts the previous Appx deployed using this tool.', function(yargs) {
		argv = yargs
			.usage('Usage: $0 restart')
			.help('help')
			.argv;
	})
	.command('clearsession', '[EXPERIMENTAL] Clears the session file, causing the next deploy to re-provision a server.', function(yargs) {
		argv = yargs
			.usage('Usage: $0 clearsession')
			.help('help')
			.argv;
	})
	.command('exit', '[EXPERIMENTAL] Disconnects the provisioned HWA-Server.', function(yargs) {
		argv = yargs
			.usage('Usage: $0 exit')
			.help('help')
			.argv;
	})
	.demand(1)
	.help('help', "Use 'hwa <command> help' to get help on a specific command.")
	.argv;

if (module.parent) {
    module.exports = {};
} else {
    var hwaCli = require("./bin/src/cli.js");
    hwaCli.main(argv);
;}
