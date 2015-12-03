using System;

namespace hwa_cli
{
    public static class StringExtensions
    {
        public static string NullIfEmpty(this String str)
        {
            return String.IsNullOrEmpty(str) ? null : str;
        }
    }
}
