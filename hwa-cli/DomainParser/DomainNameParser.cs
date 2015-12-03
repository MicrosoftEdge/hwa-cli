// ------------------------------------------------------------------------------------------------
// <copyright file="DomainNameParser.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace HwaCli.DomainParser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using TLD = DomainName.Library;

    public class DomainNameParser
    {
        public const string REGXURLSTRING = @"(?<protocol>((http|https|\*)://)|ms-appx:///)?(?<hostname>(([\w\*]+\.)+([\w]+)))?(?<pathname>.*)";

        public static Domain Parse(string url)
        {
            var urlMatch = Regex.Match(url, REGXURLSTRING);
            string protocol = urlMatch.Groups["protocol"].Value;
            string hostName = urlMatch.Groups["hostname"].Value;
            string pathname = urlMatch.Groups["pathname"].Value;

            TLD.DomainName domainName;
            TLD.DomainName.TryParse(urlMatch.Groups["hostname"].Value, out domainName);

            return new Domain
            {
                Protocol = protocol,
                HostName = domainName.Domain + "." + domainName.TLD,
                FullHostName = hostName,
                PathName = pathname
            };
        }
    }
}
