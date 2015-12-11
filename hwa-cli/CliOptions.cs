// ------------------------------------------------------------------------------------------------
// <copyright file="CliOptions.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace HwaCli
{
    using System;

    using CommandLine;
    using CommandLine.Text;

    public class CliOptions
    {
        [Option('m', "manifest", 
            Required = true, 
            HelpText = "Input file with JSON manifest to be processed")]
        public string InputFile { get; set; }

        [Option('i', "identityName",
            Required = true,
            HelpText = "Identity GUID")]
        public string IdentityName { get; set; }

        [Option('p', "publisherIdentity",
            Required = true,
            HelpText = "Publisher Identity. e.g. \"CN=author\"")]
        public string PublisherIdentity { get; set; }

        [Option('n', "publisherDisplayName",
            Required = true,
            HelpText = "Displayed name of the publisher.")]
        public string PublisherDisplayName { get; set; }

        [Option('a', "makeAppxPath",
            Required = false,
            HelpText = "Path to the MakeAppx command.")]
        public string MakeAppxPath { get; set; }

        [Option('o', "out",
            Required = false,
            HelpText = "Path to output file for errors and messages.")]
        public string OutputFilePath { get; set; }

        [Option('v', "verbose",
            Required = false,
            DefaultValue = false,
            HelpText = "Indicates if the program should log verbose messages.")]
        public bool Verbose { get; set; }

        [Option('w', "wait", 
            Required = false,
            DefaultValue = false,
            HelpText = "Indicates if the program should wait for user input before closing.")]
        public bool Wait { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText
            {
                Heading = new HeadingInfo("HWA-CLI", "Version 0.10"),
                Copyright = new CopyrightInfo("Microsoft", 2015),
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };

            help.AddPreOptionsLine("Usage: hwa-cli --manifest <manifest-path> --identityName <identity-guid> --publisherIdenty <publisher-identity> --publisherDisplayName <publisher-display-name> --makeAppxPath <path-to-makeappx-utility>");
            help.AddOptions(this);

            return help;
        }
    }
}
