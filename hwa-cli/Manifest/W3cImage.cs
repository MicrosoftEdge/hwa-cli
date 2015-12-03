using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hwa_cli.Manifest
{
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
