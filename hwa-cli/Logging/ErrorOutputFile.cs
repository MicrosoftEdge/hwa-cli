using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hwa_cli.Logging
{
    public class ErrorOutputFile
    {
        [JsonProperty(PropertyName = "errors")]
        public IList<Error> Errors { get; set; }

        [JsonProperty(PropertyName = "messages")]
        public IList<string> Messages { get; set; }
    }
}
