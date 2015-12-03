// ------------------------------------------------------------------------------------------------
// <copyright file="W3cImage.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace HwaCli.Manifest
{
    using Newtonsoft.Json;

    public class W3cImage
    {
        [JsonProperty(PropertyName = "src")]
        public string Src { get; set; }

        [JsonProperty(PropertyName = "sizes")]
        public string Sizes { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "density")]
        public string Density { get; set; }
    }
}
