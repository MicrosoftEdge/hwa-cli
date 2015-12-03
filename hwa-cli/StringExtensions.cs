// ------------------------------------------------------------------------------------------------
// <copyright file="StringExtensions.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace HwaCli
{
    using System;

    public static class StringExtensions
    {
        public static string NullIfEmpty(this string str)
        {
            return string.IsNullOrEmpty(str) ? null : str;
        }
    }
}
