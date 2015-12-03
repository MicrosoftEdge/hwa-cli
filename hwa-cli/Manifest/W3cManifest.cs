using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hwa_cli.Manifest
{
    public class W3cManifest
    {
        [JsonProperty(PropertyName = "lang")]
        public string Language { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "short_name")]
        public string ShortName { get; set; }

        [JsonProperty(PropertyName = "icons")]
        public IList<W3cImage> Icons { get; set; }

        [JsonProperty(PropertyName = "splash_screens")]
        public W3cImage[] SplashScreens { get; set; }

        [JsonProperty(PropertyName = "scope")]
        public string Scope { get; set; }

        [JsonProperty(PropertyName = "start_url")]
        public string StartUrl { get; set; }

        [JsonProperty(PropertyName = "display")]
        public string Display { get; set; }

        [JsonProperty(PropertyName = "orientation")]
        public string Orientation { get; set; }

        [JsonProperty(PropertyName = "theme_color")]
        public string ThemeColor { get; set; }

        [JsonProperty(PropertyName = "background_color")]
        public string BackgroundColor { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "mjs_access_whitelist")]
        public IList<MjsAccessWhitelistUrl> MjsAccessWhitelist { get; set; }
    }
}
