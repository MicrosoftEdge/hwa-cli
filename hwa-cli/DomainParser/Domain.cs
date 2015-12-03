using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hwa_cli.DomainParser
{
    public class Domain
    {
        public string Protocol { get; set; }

        public string FullHostName { get; set; }

        public string HostName { get; set; }

        public string PathName { get; set; }
    }
}
