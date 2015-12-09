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
        /// <param name="manifest"><seealso cref="ChromeManifest"/></param>
        /// <param name="identityAttrs"><seealso cref="IdentityAttributes"/></param>
        /// <returns><seealso cref="XElement"/></returns>
        private XElement Convert(ChromeManifest manifest, IdentityAttributes identityAttrs)
        {
            string startUrl = manifest.App.Launch.WebUrl;

            if (string.IsNullOrEmpty(startUrl) && !string.IsNullOrEmpty(manifest.App.Launch.LocalPath))
            {
                startUrl = "ms-appx-web:///" + manifest.App.Launch.LocalPath;
            }
            
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

            urls = urls.Concat(manifest.App.Urls ?? new string[] { }).ToList();

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
        /// <param name="manifest"><seealso cref="W3cManifest"/></param>
        /// <param name="identityAttrs"><seealso cref="IdentityAttributes"/></param>
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
                     splashScreen = new W3cImage() { Sizes = "620x300" };

            foreach (var icon in manifest.Icons)
            {
                this.ValidatePath(icon.Src);

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
            this.logger.LogMessage("Store Logo: {0}", logoStore.Src);
            this.logger.LogMessage("Small Logo: {0}", logoSmall.Src);
            this.logger.LogMessage("Large Logo: {0}", logoLarge.Src);
            this.logger.LogMessage("Splash Screen: {0}", splashScreen.Src);

            // Update XML Template
            var appxManifest = XElement.Parse(string.Format(
                @"<?xml version=""1.0"" encoding=""utf-8"" standalone=""yes"" ?>
                <Package xmlns=""http://schemas.microsoft.com/appx/manifest/foundation/windows10"" xmlns:mp=""http://schemas.microsoft.com/appx/2014/phone/manifest"" xmlns:uap=""http://schemas.microsoft.com/appx/manifest/uap/windows10"" xmlns:build=""http://schemas.microsoft.com/developer/appx/2015/build"" IgnorableNamespaces=""uap mp build"">
                  <Identity Name=""{0}"" Version=""1.0.0.0"" Publisher=""{1}""/>
                  <mp:PhoneIdentity PhoneProductId=""{2}"" PhonePublisherId=""00000000-0000-0000-0000-000000000000""/>
                  <build:Metadata>
                    <build:Item Name=""GeneratedFrom"" Value=""{3}"" />
                    <build:Item Name=""GenerationDate"" Value=""{4}"" />
                    <build:Item Name=""ToolVersion"" Value=""{5}"" />
                  </build:Metadata>
                  <Properties>
                    <DisplayName>{6}</DisplayName>
                    <PublisherDisplayName>{7}</PublisherDisplayName>
                    <Logo>{8}</Logo>
                  </Properties>
                  <Dependencies>
                    <TargetDeviceFamily Name=""Windows.Universal"" MinVersion=""10.0.10069.0"" MaxVersionTested=""10.0.10069.0""/>
                  </Dependencies>
                  <Resources>
                    <Resource Language=""{9}""/>
                  </Resources>
                  <Applications>
                    <Application Id=""{10}"" StartPage=""{11}"">
                      <uap:ApplicationContentUriRules>
                        <!-- Default ACURs to allow for common auth methods -->
                        <uap:Rule Type=""include"" WindowsRuntimeAccess=""none"" Match=""https://*.facebook.com/"" />
                        <uap:Rule Type=""include"" WindowsRuntimeAccess=""none"" Match=""https://*.google.com/"" />
                        <uap:Rule Type=""include"" WindowsRuntimeAccess=""none"" Match=""https://*.live.com/"" />
                        <uap:Rule Type=""include"" WindowsRuntimeAccess=""none"" Match=""https://*.youtube.com/"" />
                        <!-- End default ACURs -->
                      </uap:ApplicationContentUriRules>
                      <uap:VisualElements DisplayName=""{12}"" Description=""{13}"" BackgroundColor=""{14}"" Square150x150Logo=""{15}"" Square44x44Logo=""{16}"">
                        <uap:SplashScreen Image=""{17}""/>
                        <uap:InitialRotationPreference>
                          <uap:Rotation Preference=""{18}""/>
                        </uap:InitialRotationPreference>
                      </uap:VisualElements>
                    </Application>
                  </Applications>
                  <Capabilities>
                    <Capability Name=""internetClient""/>
                    <Capability Name=""privateNetworkClientServer""/>
                    <DeviceCapability Name=""microphone"" />
                    <DeviceCapability Name=""location"" />
                    <DeviceCapability Name=""webcam"" />
                  </Capabilities>
                </Package>",
                identityAttrs.IdentityName.ToString(),               // 0,  Package.Identity[Name]
                identityAttrs.PublisherIdentity,                     // 1,  Package.Identity[Publisher]
                Guid.NewGuid().ToString(),                           // 2,  Package.PhoneIdentity[PhoneProductId]
                TOOL_NAME,                                           // 3,  Package.Metadata.Item[Name="GeneratedFrom"][Value]
                DateTime.UtcNow.ToString(),                          // 4,  Package.Metadata.Item[Name="GenerationDate"][Value]
                TOOL_VERSION,                                        // 5,  Package.Metadata.Item[Name="ToolVersion"][Value]
                manifest.ShortName,                                  // 6,  Package.Properties.DisplayName
                identityAttrs.PublisherDisplayName,                  // 7,  Package.Properties.PublisherDisplayName
                logoStore.Src,                                       // 8,  Package.Properties.Logo
                manifest.Language.NullIfEmpty() ?? "en-us",          // 9,  Package.Resources.Resource[Language]
                SanitizeIdentityName(manifest.ShortName),            // 10, Package.Applications.Application[Id]
                manifest.StartUrl,                                   // 11, Package.Applications.Application[StartPage]
                manifest.ShortName,                                  // 12, Package.VisualElements[DisplayName]
                manifest.Description.NullIfEmpty() ?? manifest.Name, // 13, Package.VisualElements[Description]
                manifest.ThemeColor,                                 // 14, Package.VisualElements[BackgroundColor]
                logoLarge.Src,                                       // 15, Package.VisualElements[Square150x150Logo]
                logoSmall.Src,                                       // 16, Package.VisualElements[Square44x44Logo]
                splashScreen.Src,                                    // 17, Package.VisualElements.SplashScreen[Image]
                manifest.Orientation.NullIfEmpty() ?? "portrait"     // 18, Package.VisualElements.InitialRotationPreferences.Rotation[Preference]
                ));

            // Add ACURs
            XNamespace xmlns = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";
            XNamespace xmlnsUap = "http://schemas.microsoft.com/appx/manifest/uap/windows10";
            XElement acurs = appxManifest.Descendants(xmlns + "Application").Descendants(xmlnsUap + "ApplicationContentUriRules").FirstOrDefault();

            Domain domain = DomainNameParser.Parse(manifest.StartUrl);
            string baseUrlPattern = domain.Scheme + "://" + domain.HostName + "/";
            string baseApiAccess = "none";

            if (!string.IsNullOrEmpty(manifest.Scope))
            {
                // If scope is defined, the base access rule is defined by the scope
                var parsedScopeUrl = DomainNameParser.Parse(manifest.Scope);
                if (!string.IsNullOrEmpty(parsedScopeUrl.Scheme) && !string.IsNullOrEmpty(parsedScopeUrl.HostName))
                {
                    baseUrlPattern = manifest.Scope;
                }
                else
                {
                    var pathname = parsedScopeUrl.PathName;
                    pathname = pathname.StartsWith("/") ? pathname : "/" + pathname;
                    baseUrlPattern = domain.Scheme + "://" + domain.HostName + pathname;
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
                    prevDeltaH = deltaH;
                    prevDeltaW = deltaW;
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

        private void ValidatePath(string path)
        {
            if (path.Contains(".."))
            {
                this.logger.LogError(Errors.RelativePathReferencesParentDirectory, path);
                throw new ConversionException("Error: " + Errors.RelativePathReferencesParentDirectory.Type);
            }
        }
    }
}
