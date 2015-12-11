// ------------------------------------------------------------------------------------------------
// <copyright file="LoggerException.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace HwaCli.Logging
{
    using System;
    
    public class LoggerException : Exception
    {
        public LoggerException()
        {
        }

        public LoggerException(string message) : base(message)
        {
        }

        public LoggerException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
