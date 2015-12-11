// ------------------------------------------------------------------------------------------------
// <copyright file="LoggerOutputFile.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace HwaCli.Logging
{
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public class LoggerOutputFile
    {
        public LoggerOutputFile()
        {
            this.Errors = new List<Error>();
            this.Messages = new List<string>();
        }

        [JsonProperty(PropertyName = "errors")]
        public IList<Error> Errors { get; set; }

        [JsonProperty(PropertyName = "messages")]
        public IList<string> Messages { get; set; }
    }
}
