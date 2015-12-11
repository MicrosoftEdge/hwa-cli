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
        private static string outputPath;

        private static bool outputToFile;

        private static StreamWriter fileStreamWriter;

        private static LoggerOutputFile outputFile;

        private static Logger instance = null;

        private Logger(string filePath)
        {
            Logger.OutputToConsole = true;
            Logger.Verbose = false;

            Logger.outputToFile = true;
            Logger.outputPath = filePath;
            Logger.outputFile = new LoggerOutputFile();
            Logger.fileStreamWriter = File.Exists(filePath) ? File.AppendText(filePath) : File.CreateText(filePath);
        }

        private Logger()
        {
            Logger.outputToFile = false;
            Logger.OutputToConsole = true;
            Logger.Verbose = false;
        }

        public static bool OutputToConsole { get; set; }

        public static bool Verbose { get; set; }

        public static void CreateLogger(string filePath)
        {
            Logger.instance = Logger.instance ?? new Logger(filePath);
        }

        public static void CreateLogger()
        {
            Logger.instance = Logger.instance ?? new Logger();
        }

        public static void LogError(Error error, params string[] parameters)
        {
            Logger.CheckLoggerInstance();

            error.Params = parameters;
            error.Message = string.Format(error.Message, parameters);

            string errorString = JsonConvert.SerializeObject(error, Formatting.Indented);

            if (Logger.OutputToConsole)
            {
                Console.WriteLine(errorString);
            }

            if (Logger.outputToFile)
            {
                Logger.outputFile.Errors.Add(error);
                Logger.WriteOutputFile();
            }
        }

        public static void LogMessage(string message)
        {
            Logger.CheckLoggerInstance();

            if (Logger.OutputToConsole)
            {
                Console.WriteLine(message);
            }

            if (Logger.outputToFile)
            {
                Logger.outputFile.Messages.Add(message);
                Logger.WriteOutputFile();
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

        private static void WriteOutputFile()
        {
            File.WriteAllText(Logger.outputPath, JsonConvert.SerializeObject(Logger.outputFile, Formatting.Indented));
        }

        private static void CheckLoggerInstance()
        {
            if (Logger.instance == null)
            {
                throw new Exception("Logger not instantiated.");
            }
        }
    }
}
