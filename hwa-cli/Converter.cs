// ------------------------------------------------------------------------------------------------
// <copyright file="Converter.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace HwaCli
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;

    using HwaCli.DomainParser;
    using HwaCli.Logging;
    using HwaCli.Manifest;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class Converter
    {
        private const string TOOL_NAME = "Store .web Tool";

        private const string TOOL_VERSION = "1.0";

        private Logger logger;

        private DirectoryInfo rootPath;

        public Converter(Logger logger, DirectoryInfo rootPath)
        {
            this.logger = logger;
            this.rootPath = rootPath;
        }

        /// <summary>
        ///  This method takes an anonymous object, determines type of manifest, and converts it to Appx
        /// </summary>
        /// <param name="manifestJson">string containing the JSON metadata for the web app manifest</param>
        /// <param name="identity"><seealso cref="IdentityAttributes"/> object defining the identity of the Appx manifest publisher</param>
        /// <returns><seealso cref="XElement"/></returns>
        public XElement Convert(string manifestJson, IdentityAttributes identity)
        {
            XElement xmlManifest = null;
            var manifestJObject = JObject.Parse(manifestJson);

            // The 'app' property is a Chrome specific property that doesn't follow the W3C pattern,
            // and its presence is indicative of a Chrome hosted app.
            if (manifestJObject["app"] != null)
            {
                var chromeManifest = JsonConvert.DeserializeObject<ChromeManifest>(manifestJson);
                xmlManifest = this.Convert(chromeManifest, identity);
            }
            else
            {
                var w3cManifest = JsonConvert.DeserializeObject<W3cManifest>(manifestJson);
                xmlManifest = this.Convert(w3cManifest, identity);
            }

            return xmlManifest;
        }

        /// <summary>
        /// Converts a <seealso cref="ChromeManifest"/> into <seealso cref="XElement"/> object representing AppxManifest.
        /// </summary>
        /// <param name="manifest"></param>
        /// <param name="identityAttrs"></param>
        /// <returns><seealso cref="XElement"/></returns>
        private XElement Convert(ChromeManifest manifest, IdentityAttributes identityAttrs)
        {
            var startUrl = manifest.App.Launch.WebUrl.NullIfEmpty() ?? (!string.IsNullOrEmpty(manifest.App.Launch.LocalPath) ? "ms-appx-web:///" + manifest.App.Launch.LocalPath : string.Empty);
            
            if (string.IsNullOrEmpty(startUrl))
            {
                this.logger.LogError(Errors.LaunchUrlNotSpecified);
                throw new ConversionException("Start url is not specifed in ChromeManifest.");
            }

            var w3cManifest = new W3cManifest()
            {
                Language = manifest.Language.NullIfEmpty() ?? "en-us",
                Name = manifest.Name,
                ShortName = manifest.ShortName.NullIfEmpty() ?? manifest.Name,
                SplashScreens = manifest.SplashScreens,
                Scope = manifest.Scope,
                StartUrl = startUrl,
                Display = manifest.Display,
                Orientation = manifest.Orientation.NullIfEmpty() ?? "portrait",
                ThemeColor = manifest.ThemeColor.NullIfEmpty() ?? "aliceBlue",
                BackgroundColor = manifest.BackgroundColor.NullIfEmpty() ?? "gray",
                Icons = new List<W3cImage>()
            };

            if (!string.IsNullOrEmpty(manifest.DefaultLocale))
            {
                var localResPath = this.rootPath + string.Format("\\_locales\\{0}\\messages.json", manifest.DefaultLocale);
                if (File.Exists(localResPath))
                {
                    var msgs = JObject.Parse(File.ReadAllText(localResPath));

                    PropertyInfo[] properties = typeof(W3cManifest).GetProperties();
                    foreach (var property in properties)
                    {
                        var value = property.GetValue(w3cManifest);
                        if (value != null && value.GetType() == typeof(string) && (((string)value).ToLowerInvariant().IndexOf("__msg_") == 0))
                        {
                            foreach (var obj in msgs)
                            {
                                var varName = ((string)value).Substring("__msg_".Length, ((string)value).Length - "__msg_".Length - "__".Length);
                                if (obj.Key == varName)
                                {
                                    property.SetValue(w3cManifest, obj.Value["message"].Value<string>().Trim());
                                }
                            }
                        }
                    }
                }
            }

            foreach (var size in manifest.Icons.Properties())
            {
                w3cManifest.Icons.Add(new W3cImage()
                    {
                        Sizes = size.Name + "x" + size.Name,
                        Src = size.Value
                    });
            }

            var extractedUrls = new List<MjsAccessWhitelistUrl>();
            var urls = new List<string>() { startUrl };

            urls = urls.Concat(manifest.App.Urls).ToList();

            foreach (var url in urls)
            {
                Domain domain = DomainNameParser.Parse(url);
                var domainName = domain.DomainName;
                var protocol = domain.Scheme;

                if (string.IsNullOrEmpty(domainName))
                {
                    this.logger.LogError(Errors.DomainParsingFailed, url);
                    throw new ConversionException(string.Format("Domain parsing failed for url: {0}", url));
                }

                if (protocol == "http" || protocol == "*" || string.IsNullOrEmpty(protocol))
                {
                    (new List<string> { "http://", "http://*.", "https://", "https://*." })
                        .ForEach(proto => 
                        {
                            extractedUrls.Add(new MjsAccessWhitelistUrl()
                            {
                                Url = proto + domainName + "/",
                                ApiAccess = "none"
                            });
                        });
                }
                else if (protocol == "https")
                {
                    (new List<string> { "https://", "https://*." })
                         .ForEach(proto => 
                         {
                             extractedUrls.Add(new MjsAccessWhitelistUrl()
                             {
                                 Url = proto + domainName + "/",
                                 ApiAccess = "none"
                             });
                         });
                }

                extractedUrls.Add(new MjsAccessWhitelistUrl
                {
                    Url = url,
                    ApiAccess = "none"
                });
            }

            // Remove duplicates
            extractedUrls = extractedUrls.GroupBy(u => u.Url.ToLower()).Select(u => u.First()).ToList();

            w3cManifest.MjsAccessWhitelist = extractedUrls;

            return this.Convert(w3cManifest, identityAttrs);
        }

        /// <summary>
        /// Converts a <seealso cref="W3cManifest"/> into <seealso cref="XElement"/> object representing AppxManifest.
        /// </summary>
        /// <param name="manifest"></param>
        /// <param name="identityAttrs"></param>
        /// <returns><seealso cref="XElement"/></returns>
        private XElement Convert(W3cManifest manifest, IdentityAttributes identityAttrs)
        {
            if (string.IsNullOrEmpty(manifest.StartUrl))
            {
                this.logger.LogError(Errors.StartUrlNotSpecified);
                throw new ConversionException("Start url is not specifed in W3cManifest.");
            }

            if (manifest.Icons.Count < 1)
            {
                this.logger.LogError(Errors.NoIconsFound);
                throw new ConversionException("Manifest must include at least one icon.");
            }

            // Establish assets
            W3cImage logoStore = new W3cImage() { Sizes = "50x50" }, 
                     logoSmall = new W3cImage() { Sizes = "44x44" }, 
                     logoLarge = new W3cImage() { Sizes = "150x150" }, 
                     splashScreen = new W3cImage() { Sizes = "300x620" };

            foreach (var icon in manifest.Icons)
            {
                if (icon.Sizes == logoStore.Sizes)
                {
                    logoStore.Src = SanitizeImgPath(icon.Src);
                }
                else if (icon.Sizes == logoSmall.Sizes)
                {
                    logoSmall.Src = SanitizeImgPath(icon.Src);
                }
                else if (icon.Sizes == logoLarge.Sizes)
                {
                    logoLarge.Src = SanitizeImgPath(icon.Src);
                }
                else if (icon.Sizes == splashScreen.Sizes)
                {
                    splashScreen.Src = SanitizeImgPath(icon.Src);
                }
            }

            logoStore.Src = logoStore.Src.NullIfEmpty() ?? this.FindNearestMatchAndResizeImage(GetHeightFromW3cImage(logoStore), GetWidthFromW3cImage(logoStore), manifest.Icons).Src;
            logoSmall.Src = logoSmall.Src.NullIfEmpty() ?? this.FindNearestMatchAndResizeImage(GetHeightFromW3cImage(logoSmall), GetWidthFromW3cImage(logoSmall), manifest.Icons).Src;
            logoLarge.Src = logoLarge.Src.NullIfEmpty() ?? this.FindNearestMatchAndResizeImage(GetHeightFromW3cImage(logoLarge), GetWidthFromW3cImage(logoLarge), manifest.Icons).Src;
            splashScreen.Src = splashScreen.Src.NullIfEmpty() ?? this.FindNearestMatchAndResizeImage(GetHeightFromW3cImage(splashScreen), GetWidthFromW3cImage(splashScreen), manifest.Icons).Src;

            this.logger.LogMessage("Established assets:");
            this.logger.LogMessage("\tStore Logo: {0}", logoStore.Src);
            this.logger.LogMessage("\tSmall Logo: {0}", logoSmall.Src);
            this.logger.LogMessage("\tLarge Logo: {0}", logoLarge.Src);
            this.logger.LogMessage("\tSplash Screen: {0}", splashScreen.Src);

            // Update XML Template
            var appxManifest = XElement.Load("AppxManifestTemplate.xml");
            XNamespace xmlns = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";
            XNamespace xmlnsMp = "http://schemas.microsoft.com/appx/2014/phone/manifest";
            XNamespace xmlnsUap = "http://schemas.microsoft.com/appx/manifest/uap/windows10";
            XNamespace xmlnsBuild = "http://schemas.microsoft.com/developer/appx/2015/build";

            var identityElem = appxManifest.Descendants(xmlns + "Identity").FirstOrDefault();
            identityElem.Attribute("Version").Value = "1.0.0.0";
            identityElem.Attribute("Name").Value = identityAttrs.IdentityName.ToString();
            identityElem.Attribute("Publisher").Value = identityAttrs.PublisherIdentity;

            appxManifest.Descendants(xmlnsMp + "PhoneIdentity").Attributes("PhoneProductId").FirstOrDefault().Value = Guid.NewGuid().ToString();

            appxManifest.Descendants(xmlns + "DisplayName").FirstOrDefault().Value = manifest.ShortName;

            appxManifest.Descendants(xmlns + "PublisherDisplayName").FirstOrDefault().Value = identityAttrs.PublisherDisplayName;

            appxManifest.Descendants(xmlns + "Logo").FirstOrDefault().Value = logoStore.Src;

            appxManifest.Descendants(xmlns + "Resource").Attributes("Language").FirstOrDefault().Value = !string.IsNullOrEmpty(manifest.Language) ? manifest.Language : "en-us";

            var applicationElem = appxManifest.Descendants(xmlns + "Application").FirstOrDefault();
            applicationElem.Attribute("Id").Value = SanitizeIdentityName(manifest.ShortName);
            applicationElem.Attribute("StartPage").Value = manifest.StartUrl;

            var visualElementsElem = applicationElem.Descendants(xmlnsUap + "VisualElements").FirstOrDefault();
            visualElementsElem.Attribute("DisplayName").Value = manifest.ShortName;
            visualElementsElem.Attribute("Description").Value = string.IsNullOrEmpty(manifest.Description) ? manifest.Name : manifest.Description;
            visualElementsElem.Attribute("BackgroundColor").Value = manifest.ThemeColor;

            visualElementsElem.Attribute("Square150x150Logo").Value = logoLarge.Src;

            visualElementsElem.Attribute("Square44x44Logo").Value = logoSmall.Src;

            applicationElem.Descendants(xmlnsUap + "SplashScreen").Attributes("Image").FirstOrDefault().Value = splashScreen.Src;

            applicationElem.Descendants(xmlnsUap + "Rotation").Attributes("Preference").FirstOrDefault().Value = !string.IsNullOrEmpty(manifest.Orientation) ? manifest.Orientation : "portrait";

            var metadataRootElem = appxManifest.Descendants(xmlnsBuild + "Metadata").FirstOrDefault();
            metadataRootElem.Add(new XElement(xmlnsBuild + "Item", new XAttribute("Name", "GeneratedFrom"), new XAttribute("Value", TOOL_NAME)));
            metadataRootElem.Add(new XElement(xmlnsBuild + "Item", new XAttribute("Name", "GenerationDate"), new XAttribute("Value", DateTime.UtcNow.ToString())));
            metadataRootElem.Add(new XElement(xmlnsBuild + "Item", new XAttribute("Name", "ToolVersion"), new XAttribute("Value", TOOL_VERSION)));

            // Add ACURs
            var acurs = applicationElem.Descendants(xmlnsUap + "ApplicationContentUriRules").FirstOrDefault();
            var urlRegexString = @"(?<protocol>(http|https|\*)://)?(?<hostname>(([\w\*]+\.)+([\w]+)))?(?<pathname>.*)";

            var startUrlMatch = Regex.Match(manifest.StartUrl, urlRegexString);

            string baseUrlPattern = startUrlMatch.Groups["protocol"].Value + startUrlMatch.Groups["hostname"].Value;
            var baseApiAccess = "none";

            if (!string.IsNullOrEmpty(manifest.Scope))
            {
                // If scope is defined, the base access rule is defined by the scope
                var parsedScopeUrl = Regex.Match(manifest.Scope, urlRegexString);
                if (!string.IsNullOrEmpty(parsedScopeUrl.Groups["protocol"].Value) && !string.IsNullOrEmpty(parsedScopeUrl.Groups["hostname"].Value))
                {
                    baseUrlPattern = manifest.Scope;
                }
                else
                {
                    var pathname = parsedScopeUrl.Groups["pathname"].Value;
                    pathname = pathname.StartsWith("/") ? pathname : "/" + pathname;
                    baseUrlPattern = startUrlMatch.Groups["protocol"].Value + startUrlMatch.Groups["hostname"].Value + pathname;
                }
            }

            if (baseUrlPattern.EndsWith("/*"))
            {
                baseUrlPattern = baseUrlPattern.Remove(baseUrlPattern.Length - 2, 2);
            }

            foreach (var urlObj in manifest.MjsAccessWhitelist)
            {
                var accessUrl = urlObj.Url;

                // ignore the * rule
                if (accessUrl != "*")
                {
                    if (accessUrl.EndsWith("/*"))
                    {
                        accessUrl = accessUrl.Remove(baseUrlPattern.Length - 2, 2);
                    }

                    var apiAccess = urlObj.ApiAccess != null ? urlObj.ApiAccess : "none";
                    if (string.Equals(accessUrl, baseUrlPattern, StringComparison.InvariantCultureIgnoreCase))
                    {
                        baseApiAccess = apiAccess;
                    }
                    else
                    {
                        acurs.Add(
                            new XElement(
                                xmlnsUap + "Rule",
                                new XAttribute("Type", "include"),
                                new XAttribute("WindowsRuntimeAccess", apiAccess),
                                new XAttribute("Match", accessUrl)));
                            
                        this.logger.LogMessage("Access Rule added: [{0}] - {1}", apiAccess, accessUrl);
                    }
                }
            }

            // Add base rule
            acurs.Add(
                new XElement(
                    xmlnsUap + "Rule",
                    new XAttribute("Type", "include"),
                    new XAttribute("WindowsRuntimeAccess", baseApiAccess),
                    new XAttribute("Match", baseUrlPattern)));

            this.logger.LogMessage("Access Rule added: [{0}] - {1}", baseApiAccess, baseUrlPattern);

            return appxManifest;
        }

        private static string SanitizeIdentityName(string name)
        {
            string sanitizedName = name;

            // Remove any banned characters
            sanitizedName = Regex.Replace(sanitizedName, @"[^A-Za-z0-9\.]", string.Empty);

            int currentLength;
            do
            {
                currentLength = sanitizedName.Length;

                // If the name starts with a number, remove the number
                sanitizedName = Regex.Replace(sanitizedName, @"^[0-9]", string.Empty);

                // If the name starts with a dot, remove the dot
                sanitizedName = Regex.Replace(sanitizedName, @"^\.", string.Empty);

                // If their is a number right after a dot, remove the number
                sanitizedName = Regex.Replace(sanitizedName, @"\.[0-9]", ".");

                // If there are two consecutive dots, remove one dot
                sanitizedName = Regex.Replace(sanitizedName, @"\.\.", ".");

                // If a name ends with a dot, remove the dot
                sanitizedName = Regex.Replace(sanitizedName, @"\.$", string.Empty);
            } while (currentLength > sanitizedName.Length);

            return sanitizedName;
        }

        private static string SanitizeImgPath(string path)
        {
            // remove leading slash
            string sanitizedPath = Regex.Replace(path, @"^[\/\\]", string.Empty);
            return sanitizedPath;
        }

        private static int GetHeightFromW3cImage(W3cImage img)
        {
            return int.Parse(img.Sizes.Split('x')[1]);
        }

        private static int GetWidthFromW3cImage(W3cImage img)
        {
            return int.Parse(img.Sizes.Split('x')[0]);
        }

        private W3cImage FindNearestMatchAndResizeImage(int h, int w, IList<W3cImage> assets)
        {
            W3cImage result = null;

            var prevDeltaH = int.MaxValue;
            var prevDeltaW = int.MaxValue;

            foreach (var img in assets)
            {
                var imgW = GetWidthFromW3cImage(img);
                var imgH = GetHeightFromW3cImage(img);

                var deltaW = Math.Abs(imgW - w);
                var deltaH = Math.Abs(imgH - h);

                var prevDelta = Math.Min(prevDeltaH, prevDeltaW);
                var delta = Math.Min(deltaH, deltaW);

                if (delta < prevDelta)
                {
                    result = img;
                }
            }

            return this.ResizeAndAddImage(result, h, w);
        }

        private W3cImage ResizeAndAddImage(W3cImage img, int h, int w)
        {
            string outName = string.Format("{0}_scaled_{1}x{2}.png", Path.GetFileNameWithoutExtension(img.Src), w, h);
            string src = Path.Combine(this.rootPath.ToString(), img.Src);
            string outputSrc = Path.Combine(Directory.GetParent(src).ToString(), outName);

            using (var srcImg = Image.FromFile(src))
            {
                using (var destImg = new Bitmap(w, h))
                {
                    using (var gfx = Graphics.FromImage(destImg))
                    {
                        gfx.SmoothingMode = SmoothingMode.AntiAlias;
                        gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        gfx.PixelOffsetMode = PixelOffsetMode.HighQuality;

                        if (srcImg.Width > w || srcImg.Height > h)
                        {
                            // Scale down
                            gfx.DrawImage(srcImg, 0, 0, w, h);
                        }
                        else
                        {
                            // Center
                            var offsetX = (w - srcImg.Width) / 2;
                            var offsetY = (h - srcImg.Height) / 2;
                            gfx.DrawImage(srcImg, offsetX, offsetY, srcImg.Width, srcImg.Height);
                        }

                        destImg.Save(outputSrc);
                    }
                }
            }

            var resImg = new W3cImage()
            {
                Sizes = string.Format("{0}x{1}", w, h),
                Src = Path.Combine(Path.GetDirectoryName(img.Src), outName)
            };

            return resImg;
        }
    }
}
