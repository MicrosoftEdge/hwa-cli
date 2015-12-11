// ------------------------------------------------------------------------------------------------
// <copyright file="AssemblyInfoReader.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace HwaCli
{
    using System;
    using System.Reflection;

    public class AssemblyInfoReader
    {
        private Assembly assembly;

        public AssemblyInfoReader()
        {
            this.assembly = Assembly.GetExecutingAssembly();
        }

        public string Product
        {
            get { return this.ReadAttributeValue<AssemblyProductAttribute>("Product"); }
        }

        public Version Version
        {
            get { return this.assembly.GetName().Version; }
        }

        private string ReadAttributeValue<T>(string property)
        {
            object[] attributes = this.assembly.GetCustomAttributes(typeof(T), false);

            if ((attributes != null) && (attributes.Length > 0))
            {
                return typeof(T).GetProperty(property).GetValue(attributes[0], null).ToString();
            }

            return string.Empty;
        }
    }
}
