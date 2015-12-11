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
        private Error error; 

        private ConversionException()
        {
        }

        private ConversionException(string message) : base(message)
        {
        }

        private ConversionException(string message, Exception inner) : base(message, inner)
        {
        }

        public ConversionException(Error error, params string[] parameters)
        {
            error.Message = string.Format(error.Message, parameters);
            this.error = error;
        }

        public Error Error
        {
            get { return this.error; }
        }
    }
}
