// ------------------------------------------------------------------------------------------------
// <copyright file="ConversionException.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace HwaCli.Logging
{
    using System;

    public class ConversionException : Exception
    {
        public ConversionException()
        {
        }

        public ConversionException(string message) : base(message)
        {
        }

        public ConversionException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
