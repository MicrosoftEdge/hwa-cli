// ------------------------------------------------------------------------------------------------
// <copyright file="Domain.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace HwaCli.DomainParser
{
    using System;

    public class Domain
    {
        public string Scheme { get; set; }

        public string HostName { get; set; }

        public string DomainName { get; set; }

        public string PathName { get; set; }
    }
}
