using hwa_cli.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hwa_cli
{
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
