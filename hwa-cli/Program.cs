// ------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace HwaCli
{
    using System;
    using System.IO;
    using System.Xml.Linq;

    using HwaCli.Logging;

    public class Program
    {
        public static void Main(string[] args)
        {
            var options = new CliOptions();

            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                Console.WriteLine("Manifest: {0}", options.InputFile);
                Console.WriteLine("Identity Name: {0}", options.IdentityName);
                Console.WriteLine("Publisher Identity: {0}", options.PublisherIdentity);
                Console.WriteLine("Publisher Display Name: {0}", options.PublisherDisplayName);

                Logger logger;
                if (!string.IsNullOrEmpty(options.OutputFilePath))
                {
                    logger = new Logger(options.OutputFilePath);
                }
                else
                {
                    logger = new Logger();
                }

                logger.OutputToConsole = options.OutputToConsole;
                logger.Verbose = options.Verbose;

                if (!File.Exists(options.InputFile))
                {
                    logger.LogError(Errors.ManifestNotFound, new string[] { options.InputFile });
                }

                DirectoryInfo rootPath = Directory.GetParent(options.InputFile);
                var identity = 
                    new IdentityAttributes()
                    {
                        IdentityName = options.IdentityNameAsGuid,
                        PublisherIdentity = options.PublisherIdentity,
                        PublisherDisplayName = options.PublisherDisplayName
                    };
                var converter = new Converter(logger, rootPath);

                XElement appxManifest = null;

                try
                {
                    appxManifest = converter.Convert(File.ReadAllText(options.InputFile), identity);
                    appxManifest.Save(rootPath + "\\" + "AppxManifest.xml");
                }
                catch
                {
                    logger.LogMessage("Errors encountered, failed to create Appx package.");
                }

                if (appxManifest != null && !string.IsNullOrEmpty(options.MakeAppxPath))
                {
                    Packager.PackageAsAppx(logger, options.MakeAppxPath, rootPath.ToString());
                }

                if (options.Wait)
                {
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                }

                logger.Close();
            }
        }
    }
}
