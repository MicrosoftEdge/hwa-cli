using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hwa_cli.Logging
{
    public class Errors
    {
        private const string ERROR = "ERROR";
        private const string WARNING = "WARNING";

        public static readonly Error ManifestNotFound
            = new Error()
            {
                Code = 1,
                Type = "ManifestNotFound",
                Severity = "ERROR",
                Message = "Manifest could not be found at {0}."
            };

        public static readonly Error StartUrlNotSpecified
            = new Error()
            {
                Code = 2,
                Type = "StartUrlNotSpecified",
                Severity = "ERROR",
                Message = "The W3C manifest must specify a start_url."
            };

        public static readonly Error AppxCreationFailed
            = new Error()
            {
                Code = 3,
                Type = "AppxCreationFailed",
                Severity = ERROR,
                Message = "Error while running MakeAppx.exe to create Appx package."
            };

        public static readonly Error LaunchUrlNotSpecified
            = new Error()
            {
                Code = 4,
                Type = "LaunchUrlNotSpecified",
                Severity = ERROR,
                Message = "A value was specified neither at app.launch.web_url nor app.launch.local_path in the JSON manifest."
            };

        public static readonly Error DomainParsingFailed
            = new Error()
            {
                Code = 5,
                Type = "DomainParsingFailed",
                Severity = ERROR,
                Message = "Domain parsing failed for the following url: {0}"
            };
    }

    public class Error
    {
        public int Code { get; set; }

        public string Type { get; set; }

        public string Severity { get; set; }

        public string[] Params { get; set; }

        public string Message { get; set; }
    }
}
