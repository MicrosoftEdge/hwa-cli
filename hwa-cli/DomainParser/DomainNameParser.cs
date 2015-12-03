using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TLD = DomainName.Library;

namespace hwa_cli.DomainParser
{
    public class DomainNameParser
    {
        public const string REG_MATCH_STRING = @"(?<protocol>((http|https|\*)://)|ms-appx:///)?(?<hostname>(([\w\*]+\.)+([\w]+)))?(?<pathname>.*)";

        public static Domain Parse(string url)
        {
            var urlMatch = Regex.Match(url, REG_MATCH_STRING);
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
