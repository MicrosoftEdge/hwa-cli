using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hwa_cli.Manifest
{
    public class MjsAccessWhitelistUrl
    {
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "apiAccess")]
        public string ApiAccess { get; set; }
    }
}
