// ------------------------------------------------------------------------------------------------
// <copyright file="ChromeApp.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace HwaCli.Manifest
{
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public class ChromeApp
    {
        [JsonProperty(PropertyName = "urls")]
        public IList<string> Urls { get; set; }

        [JsonProperty(PropertyName = "launch")]
        public ChromeAppLaunch Launch { get; set; }
    }

    public class ChromeAppLaunch
    {
        [JsonProperty(PropertyName = "web_url")]
        public string WebUrl { get; set; }

        [JsonProperty(PropertyName = "local_path")]
        public string LocalPath { get; set; }
    }
}
