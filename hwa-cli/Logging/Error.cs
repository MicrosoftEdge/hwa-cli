// ------------------------------------------------------------------------------------------------
// <copyright file="Error.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace HwaCli.Logging
{
    using System;

    public class Error
    {
        public int Code { get; set; }

        public string Type { get; set; }

        public string Severity { get; set; }

        public string[] Params { get; set; }

        public string Message { get; set; }
    }

    public class Errors
    {
        private const string ERROR = "ERROR";
        private const string WARNING = "WARNING";

        public static readonly Error ManifestNotFound
            = new Error()
            {
                Code = 1,
                Type = "ManifestNotFound",
                Severity = ERROR,
                Message = "Manifest could not be found at {0}."
            };

        public static readonly Error StartUrlNotSpecified
            = new Error()
            {
                Code = 2,
                Type = "StartUrlNotSpecified",
                Severity = ERROR,
                Message = "The W3C manifest must specify a start_url."
            };

        public static readonly Error AppxCreationFailed
            = new Error()
            {
                Code = 3,
                Type = "AppxCreationFailed",
                Severity = ERROR,
                Message = "Error while running MakeAppx.exe to create Appx package. Reason: {0}"
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

        public static readonly Error NoIconsFound
            = new Error()
            {
                Code = 6,
                Type = "NoIconsFound",
                Severity = ERROR,
                Message = "Manifest must contain at least one icon."
            };

        public static readonly Error RelativePathReferencesParentDirectory
            = new Error()
            {
                Code = 7,
                Type = "RelativePathReferencesParentDirectory",
                Severity = ERROR,
                Message = "Relative paths in manifest cannot reference parent directory using \"..\". Violating path: {0}"
            };

        public static readonly Error RelativePathExpected
            = new Error()
            {
                Code = 8,
                Type = "RelativePathExpected",
                Severity = ERROR,
                Message = "A relative path was expected, but instead found an abosolute path: {0}"
            };

        public static readonly Error UnsupportedProtocolInAcur
            = new Error()
            {
                Code = 9,
                Type = "UnsupportedProtocolInAcur",
                Severity = ERROR,
                Message = "Expected protocol to be in ['http', 'https', '*']. Instead protocol was '{0}'."
            };

        public static readonly Error StoreVersionInvalid
            = new Error()
            {
                Code = 10,
                Type = "StoreVersionInvalid",
                Severity = ERROR,
                Message = "The 'store_version' property must be a valid version of the form '<MAJOR>.<MINOR>.<PATCH>.0' where MAJOR, MINOR, & PATCH are ints >= 0; received value: '{0}'."
            };
    }
}
