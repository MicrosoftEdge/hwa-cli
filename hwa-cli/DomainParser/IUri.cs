// ------------------------------------------------------------------------------------------------
// <copyright file="IUri.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace HwaCli.DomainParser
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport]
    [Guid("A39EE748-6A27-4817-A6F2-13914BEF5890"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IUri
    {
        UInt32 GetPropertyBSTR([In]UriProperty uriProp, [Out]out string strProperty, [In]UInt32 dwFlags);
        UInt32 GetPropertyLength([In]UriProperty uriProp, [Out] out UInt32 pcPropLen, [In]UInt32 dwFlags);
        UInt32 GetPropertyDWORD([In]UriProperty uriProp, [Out] out UInt32 pcPropValue, [In]UInt32 dwFlags);
        UInt32 HasProperty([In]UriProperty uriProp, [Out] out bool fHasProperty);
        UInt32 GetAbsoluteUri([MarshalAs(UnmanagedType.BStr)][Out] out string sAbsoluteUri);
        UInt32 GetAuthority([MarshalAs(UnmanagedType.BStr)][Out] out string sAuthority);
        UInt32 GetDisplayUri([MarshalAs(UnmanagedType.BStr)][Out] out string sDisplayString);
        UInt32 GetDomain([MarshalAs(UnmanagedType.BStr)][Out] out string sDomain);
        UInt32 GetExtension([MarshalAs(UnmanagedType.BStr)][Out] out string sExtension);
        UInt32 GetFragment([MarshalAs(UnmanagedType.BStr)][Out] out string sFragment);
        UInt32 GetHost([MarshalAs(UnmanagedType.BStr)][Out] out string sHost);
        UInt32 GetPassword([MarshalAs(UnmanagedType.BStr)][Out] out string sPassword);
        UInt32 GetPath([MarshalAs(UnmanagedType.BStr)][Out] out string sPath);
        UInt32 GetPathAndQuery([MarshalAs(UnmanagedType.BStr)][Out] out string sPathAndQuery);
        UInt32 GetQuery([MarshalAs(UnmanagedType.BStr)][Out] out string sQuery);
        UInt32 GetRawUri([MarshalAs(UnmanagedType.BStr)][Out] out string sRawUri);
        UInt32 GetSchemeName([MarshalAs(UnmanagedType.BStr)][Out] out string sSchemeName);
        UInt32 GetUserInfo([MarshalAs(UnmanagedType.BStr)][Out] out string sUserInfo);
        UInt32 GetUserName([MarshalAs(UnmanagedType.BStr)][Out] out string sUserName);
        UInt32 GetHostType([Out] uint dwHostType);
        UInt32 GetPort([Out] uint dwPort);
        UInt32 GetScheme([Out] uint dwScheme);
        UInt32 GetZone([Out] uint dwZone);
        UInt32 GetProperties([Out] uint dwFlags);
        UInt32 IsEqual([In]IUri pUri, [Out] bool fEqual);
    }

    public enum UriProperty
    {
        ABSOLUTE_URI = 0,
        STRING_START = ABSOLUTE_URI,
        AUTHORITY = 1,
        DISPLAY_URI = 2,
        DOMAIN = 3,
        EXTENSION = 4,
        FRAGMENT = 5,
        HOST = 6,
        PASSWORD = 7,
        PATH = 8,
        PATH_AND_QUERY = 9,
        QUERY = 10,
        RAW_URI = 11,
        SCHEME_NAME = 12,
        USER_INFO = 13,
        USER_NAME = 14,
        STRING_LAST = USER_NAME,
        HOST_TYPE = 15,
        DWORD_START = HOST_TYPE,
        PORT = 16,
        SCHEME = 17,
        ZONE = 18,
        DWORD_LAST = ZONE
    }
}
