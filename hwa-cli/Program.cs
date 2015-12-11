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
            try
            {
                var options = new CliOptions();

                if (CommandLine.Parser.Default.ParseArguments(args, options))
                {
                    // Create Logger
                    Logger.Initialize(options.OutputFilePath);
                    Logger.OutputToConsole = options.OutputToConsole;
                    Logger.Verbose = options.Verbose;

                    Logger.LogMessage("Manifest: {0}", options.InputFile);
                    Logger.LogMessage("Identity Name: {0}", options.IdentityName);
                    Logger.LogMessage("Publisher Identity: {0}", options.PublisherIdentity);
                    Logger.LogMessage("Publisher Display Name: {0}", options.PublisherDisplayName);

                    if (!File.Exists(options.InputFile))
                    {
                        Logger.LogError(Errors.ManifestNotFound, new string[] { options.InputFile });
                    }

                    DirectoryInfo rootPath = Directory.GetParent(options.InputFile);
                    var identity =
                        new IdentityAttributes()
                        {
                            IdentityName = Guid.Parse(options.IdentityName),
                            PublisherIdentity = options.PublisherIdentity,
                            PublisherDisplayName = options.PublisherDisplayName
                        };
                    var converter = new Converter(rootPath);

                    XElement appxManifest = null;

                    try
                    {
                        appxManifest = converter.Convert(File.ReadAllText(options.InputFile), identity);
                        appxManifest.Save(rootPath + "\\" + "AppxManifest.xml");
                    }
                    catch
                    {
                        Logger.LogMessage("Errors encountered, failed to create Appx package.");
                    }

                    if (appxManifest != null && !string.IsNullOrEmpty(options.MakeAppxPath))
                    {
                        Packager.PackageAsAppx(options.MakeAppxPath, rootPath.ToString());
                    }

                    if (options.Wait)
                    {
                        Console.WriteLine("Press any key to exit...");
                        Console.ReadKey();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                throw;
            }
        }
    }
}
