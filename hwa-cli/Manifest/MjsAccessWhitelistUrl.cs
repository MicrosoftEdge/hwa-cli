// ------------------------------------------------------------------------------------------------
// <copyright file="MjsAccessWhitelistUrl.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace HwaCli.Manifest
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class MjsAccessWhitelistUrl : IComparable<MjsAccessWhitelistUrl>
    {
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "apiAccess")]
        public string ApiAccess { get; set; }

        public int CompareTo(MjsAccessWhitelistUrl other)
        {
            if (other == null)
            {
                return 1;
            }

            return this.Url.ToLowerInvariant().CompareTo(other.Url.ToLowerInvariant());
        }
    }

    public class MjsAccessWhitelistUrlComparer : IEqualityComparer<MjsAccessWhitelistUrl>
    {
        public bool Equals(MjsAccessWhitelistUrl x, MjsAccessWhitelistUrl y)
        {
            return string.Equals(x.Url, y.Url, StringComparison.InvariantCultureIgnoreCase);
        }

        public int GetHashCode(MjsAccessWhitelistUrl obj)
        {
            return obj.Url.GetHashCode();
        }
    }
}
