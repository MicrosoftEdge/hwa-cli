using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hwa_cli.Logging
{
    public class ConversionException : Exception
    {
        public ConversionException() { }

        public ConversionException(string message) : base(message)
        {

        }

        public ConversionException(string message, Exception inner) : base(message, inner)
        {

        }

    }
}
