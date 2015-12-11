// ------------------------------------------------------------------------------------------------
// <copyright file="Logger.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace HwaCli.Logging
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Newtonsoft.Json;

    public class Logger
    {
        private static FileLogger fileLogger;

        private static ConsoleLogger consoleLogger;

        public static bool OutputToConsole { get; set; }

        public static bool Verbose { get; set; }

        public static void Initialize(string outputFilePath)
        {
            if (!string.IsNullOrEmpty(outputFilePath))
            {
                Logger.fileLogger = new FileLogger(outputFilePath);
            }

            Logger.consoleLogger = new ConsoleLogger();
        }
        private static void CheckLoggerInitialization()
        {
            if (Logger.consoleLogger == null && Logger.fileLogger == null)
            {
                throw new Exception("Logger not ready; call CreateLogger before using.");
            }
        }


        public static void LogError(Error error, params string[] parameters)
        {
            Logger.CheckLoggerInitialization();

            error.Params = parameters;
            error.Message = string.Format(error.Message, parameters);

            if (Logger.fileLogger != null)
            {
                Logger.fileLogger.LogError(error);
            }

            if (Logger.consoleLogger != null)
            {
                Logger.consoleLogger.LogError(error);
            }
        }

        public static void LogMessage(string message)
        {
            Logger.CheckLoggerInitialization();

            if (Logger.fileLogger != null)
            {
                Logger.fileLogger.LogMessage(message);
            }

            if (Logger.consoleLogger != null)
            {
                Logger.consoleLogger.LogMessage(message);
            }
        }

        public static void LogMessage(string format, params string[] values)
        {
            var message = string.Format(format, values);
            Logger.LogMessage(message);
        }

        public static void LogVerbose(string message)
        {
            if (Logger.Verbose)
            {
                Logger.LogMessage(message);
            }
        }

        public static void LogVerbose(string format, params string[] values)
        {
            if (Logger.Verbose)
            {
                Logger.LogMessage(format, values);
            }
        }

        class FileLogger
        {
            private LoggerOutputFile outputFile;
            private string outputPath;

            public FileLogger(string outputPath)
            {
                this.outputFile = new LoggerOutputFile();
                this.outputPath = outputPath;
            }

            public void LogError(Error error)
            {
                this.outputFile.Errors.Add(error);
                this.WriteOutputFile();
            }

            public void LogMessage(string message)
            {
                this.outputFile.Messages.Add(message);
                this.WriteOutputFile();
            }

            private void WriteOutputFile()
            {
                try
                {
                    File.WriteAllText(this.outputPath, JsonConvert.SerializeObject(this.outputFile, Formatting.Indented));
                }
                catch (Exception)
                {
                    Console.WriteLine(string.Format("Unable to write to output file for logging at: {0}", this.outputPath));
                    throw;
                }
            }
        }

        class ConsoleLogger
        {
            public void LogError(Error error)
            {
                var message = JsonConvert.SerializeObject(error, Formatting.Indented);
                Console.WriteLine(message);
            }

            public void LogMessage(string message)
            {
                Console.WriteLine(message);
            }
        }
    }
}
