// ------------------------------------------------------------------------------------------------
// <copyright file="MjsAccessWhitelistUrl.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace HwaCli.Manifest
{
    using Newtonsoft.Json;

    public class MjsAccessWhitelistUrl
    {
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "apiAccess")]
        public string ApiAccess { get; set; }
    }
}
