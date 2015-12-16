// ------------------------------------------------------------------------------------------------
// <copyright file="ChromeManifest.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace HwaCli.Manifest
{
    using Newtonsoft.Json;

    public class ChromeManifest
    {
        [JsonProperty(PropertyName = "app")]
        public ChromeApp App { get; set; }

        [JsonProperty(PropertyName = "manifest_version")]
        public string ManifestVersion { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "default_locale")]
        public string DefaultLocale { get; set; }

        [JsonProperty(PropertyName = "icons")]
        public dynamic Icons { get; set; }

        [JsonProperty(PropertyName = "lang")]
        public string Language { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "short_name")]
        public string ShortName { get; set; }

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

        [JsonProperty(PropertyName = "store_version")]
        public string StoreVersion { get; set; }
    }
}
