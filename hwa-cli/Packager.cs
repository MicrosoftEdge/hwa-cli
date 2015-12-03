// ------------------------------------------------------------------------------------------------
// <copyright file="Packager.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace HwaCli
{
    using System.Diagnostics;
    using System.IO;

    using HwaCli.Logging;

    public class Packager
    {
        public static void PackageAsAppx(Logger logger, string makeAppxPath, string rootPath, string packageName = "App.appx")
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = makeAppxPath,
                Arguments = string.Format("pack /o /d {0} /p {1}", rootPath, Path.Combine(rootPath, packageName)),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            try
            {
                using (var appxProc = Process.Start(startInfo))
                {
                    while (!appxProc.StandardOutput.EndOfStream)
                    {
                        logger.LogVerbose(appxProc.StandardOutput.ReadLine());
                    }

                    appxProc.WaitForExit();
                }
            }
            catch
            {
                logger.LogMessage("Errors encountered, failed to create Appx package.");
                logger.LogError(Errors.AppxCreationFailed);
            }
        }
    }
}
