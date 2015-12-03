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
        private string outputPath;

        private bool outputToFile;

        private StreamWriter logger;

        private IList<Error> errorBuffer;

        private IList<string> messageBuffer;

        public Logger(string filePath)
        {
            this.OutputToConsole = true;
            this.Verbose = false;

            this.outputToFile = true;
            this.outputPath = filePath;

            if (!File.Exists(filePath))
            {
                this.logger = new StreamWriter(filePath);
            }
            else
            {
                this.logger = File.AppendText(filePath);
            }

            this.errorBuffer = new List<Error>();
            this.messageBuffer = new List<string>();
        }

        public Logger()
        {
            this.OutputToConsole = true;
            this.Verbose = false;
            this.outputToFile = false;
        }

        public bool OutputToConsole { get; set; }

        public bool Verbose { get; set; }

        public void LogError(Error error, params string[] parameters)
        {
            error.Params = parameters;
            error.Message = string.Format(error.Message, parameters);

            string errorString = JsonConvert.SerializeObject(error, Formatting.Indented);

            if (this.OutputToConsole)
            {
                Console.WriteLine(errorString);
            }

            if (this.outputToFile)
            {
                this.errorBuffer.Add(error);
            }
        }

        public void LogMessage(string message)
        {
            if (this.OutputToConsole)
            {
                Console.WriteLine(message);
            }

            if (this.outputToFile)
            {
                this.messageBuffer.Add(message);
            }
        }

        public void LogMessage(string format, params string[] values)
        {
            var message = string.Format(format, values);
            this.LogMessage(message);
        }

        public void LogVerbose(string message)
        {
            if (this.Verbose)
            {
                this.LogMessage(message);
            }
        }

        public void LogVerbose(string format, params string[] values)
        {
            if (this.Verbose)
            {
                this.LogMessage(format, values);
            }
        }

        public void Close()
        { 
            if (this.outputToFile)
            {
                using (JsonWriter jw = new JsonTextWriter(this.logger))
                {
                    jw.Formatting = Formatting.Indented;

                    var outputFile = new ErrorOutputFile() { Errors = this.errorBuffer, Messages = this.messageBuffer };

                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(jw, outputFile);
                }

                this.logger.Close();

                if (this.OutputToConsole)
                {
                    Console.WriteLine("Wrote output to file at: ", this.outputPath);
                } 
            }
        }
    }
}
