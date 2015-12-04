// ------------------------------------------------------------------------------------------------
// <copyright file="DomainNameParser.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace HwaCli.DomainParser
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;

    public class DomainNameParser
    {
        public static Domain Parse(string uri)
        {
            object createUriObj = null;

            DomainNameParser.CreateUri(uri, CreateUriFlags.Uri_CREATE_ALLOW_IMPLICIT_WILDCARD_SCHEME, new IntPtr(0), out createUriObj);

            IUri iuri = (IUri)createUriObj;
            string domain, host, path, scheme;

            iuri.GetDomain(out domain);
            iuri.GetHost(out host);
            iuri.GetPath(out path);
            iuri.GetSchemeName(out scheme);

            return new Domain
            {
                Scheme = scheme,
                DomainName = domain,
                HostName = host,
                PathName = path
            };
        }

        [DllImport("urlmon.dll")]
        public static extern uint CreateUri(
        [MarshalAs(UnmanagedType.LPWStr)] string sURI,
        [MarshalAs(UnmanagedType.U4)] CreateUriFlags dwFlags,
        IntPtr dwReserved,
        [MarshalAs(UnmanagedType.IUnknown)]
        out object pURI);
    }

    public enum CreateUriFlags
    {
        // The following public Uri_CREATE flags may be passed in 
        // through the dwFlags parameter of the CreateUri functions.
        // Note that ALLOW_RELATIVE and ALLOW_IMPLICIT_WILDCARD_SCHEME are mutually exclusive and may not be passed together.
        Uri_CREATE_ALLOW_RELATIVE = 0x00000001,    // When the scheme is unspecified and not implicit file, assume relative.
        Uri_CREATE_ALLOW_IMPLICIT_WILDCARD_SCHEME = 0x00000002,    // When the scheme is unspecified and not implicit file, assume wildcard.
        Uri_CREATE_ALLOW_IMPLICIT_FILE_SCHEME = 0x00000004,    // When the scheme is unspecified and it starts with X: or \\ assume its a file scheme.
        Uri_CREATE_NOFRAG = 0x00000008,    // If there's a query string don't look for a fragment
        Uri_CREATE_NO_CANONICALIZE = 0x00000010,    // Do not canonicalize the scheme, host, authority, or path
        Uri_CREATE_CANONICALIZE = 0x00000100,    // DEFAULT: Canonicalize the scheme, host, authority, and path
        Uri_CREATE_FILE_USE_DOS_PATH = 0x00000020,    // Use DOS path compat mode for file URI creation
        Uri_CREATE_DECODE_EXTRA_INFO = 0x00000040,    // Beta2 DEFAULT: Decode the contents of query and fragment, then re-encode reserved characters
        Uri_CREATE_NO_DECODE_EXTRA_INFO = 0x00000080,    // Beta1 DEFAULT: Neither decode nor re-encode any part of the query or fragment
        Uri_CREATE_CRACK_UNKNOWN_SCHEMES = 0x00000200,    // Beta2 DEFAULT: Heirarchical URIs with present and unknown schemes will be treated like heirarchical URIs
        Uri_CREATE_NO_CRACK_UNKNOWN_SCHEMES = 0x00000400,    // Beta1 DEFAULT: Heirarchical URIs with present and unknown schemes will be treated like opaque URIs
        Uri_CREATE_PRE_PROCESS_HTML_URI = 0x00000800,    // DEFAULT:  Perform pre-processing on the URI to remove control characters and whitespace as if the URI comes from the raw href value of an HTML page.
        Uri_CREATE_NO_PRE_PROCESS_HTML_URI = 0x00001000,    // Don't perform pre-processing to remove control characters and whitespace as appropriate.
        Uri_CREATE_IE_SETTINGS = 0x00002000,    // Use IE registry settings for such things as whether or not to use IDN.
        Uri_CREATE_NO_IE_SETTINGS = 0x00004000,    // DEFAULT: Don't use IE registry settings.
        Uri_CREATE_NO_ENCODE_FORBIDDEN_CHARACTERS = 0x00008000,    // Don't percent-encode characters that are forbidden by the RFC.
    }
}
