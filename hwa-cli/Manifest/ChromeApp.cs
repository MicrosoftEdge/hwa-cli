using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hwa_cli.Manifest
{
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
