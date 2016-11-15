//******************************************************************************************************
//  Common.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  04/03/2006 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/13/2007 - Darrell Zuercher
//       Edited code comments.
//  09/08/2008 - J. Ritchie Carroll
//       Converted to C#.
//  02/13/2009 - Josh L. Patterson
//       Edited Code Comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  09/17/2009 - Pinal C. Patel
//       Modified GetApplicationType() to remove dependency on HttpContext.Current.
//  09/28/2010 - Pinal C. Patel
//       Cached the current ApplicationType returned by GetApplicationType() for better performance.
//  12/05/2010 - Pinal C. Patel
//       Added an overload for TypeConvertToString() that takes CultureInfo as a parameter.
//  12/07/2010 - Pinal C. Patel
//       Updated TypeConvertToString() to return an empty string if the passed in value is null.
//  03/09/2011 - Pinal C. Patel
//       Moved UpdateType enumeration from GSF.Services.ServiceProcess namespace for broader usage.
//  04/07/2011 - J. Ritchie Carroll
//       Added ToNonNullNorEmptyString() and ToNonNullNorWhiteSpace() extensions.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using GSF.Collections;
using GSF.Console;
using GSF.IO;
using GSF.Reflection;
using Microsoft.Win32;

namespace GSF
{
    #region [ Enumerations ]

    /// <summary>
    /// Specifies the type of the application.
    /// </summary>
    public enum ApplicationType
    {
        /// <summary>
        /// Application is of unknown type.
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Application doesn't require a subsystem.
        /// </summary>
        Native = 1,
        /// <summary>
        /// Application runs in the Windows GUI subsystem.
        /// </summary>
        WindowsGui = 2,
        /// <summary>
        /// Application runs in the Windows character subsystem.
        /// </summary>
        WindowsCui = 3,
        /// <summary>
        /// Application runs in the OS/2 character subsystem.
        /// </summary>
        OS2Cui = 5,
        /// <summary>
        /// Application runs in the POSIX character subsystem.
        /// </summary>
        PosixCui = 7,
        /// <summary>
        /// Application is a native Win9x driver.
        /// </summary>
        NativeWindows = 8,
        /// <summary>
        /// Application runs in the Windows CE subsystem.
        /// </summary>
        WindowsCEGui = 9,
        /// <summary>
        /// The application is a web site or web application.
        /// </summary>
        Web = 100
    }

    /// <summary>
    /// Indicates the type of update.
    /// </summary>
    public enum UpdateType
    {
        /// <summary>
        /// Update is informational.
        /// </summary>
        Information,
        /// <summary>
        /// Update is a warning.
        /// </summary>
        Warning,
        /// <summary>
        /// Update is an alarm.
        /// </summary>
        Alarm
    }

    #endregion

    // This is the location for handy miscellaneous functions that are difficult to categorize elsewhere. For the most
    // part these functions may have the most value in a Visual Basic application which supports importing functions
    // down to a class level, e.g.: Imports GSF.Common

    /// <summary>
    /// Defines common global functions.
    /// </summary>
    public static partial class Common
    {
        private static ApplicationType? s_applicationType;
        private static string s_osPlatformName;
        private static PlatformID s_osPlatformID = PlatformID.Win32S;

        /// <summary>
        /// Gets the type of the currently executing application.
        /// </summary>
        /// <returns>One of the <see cref="ApplicationType"/> values.</returns>
        public static ApplicationType GetApplicationType()
        {
            if (s_applicationType.HasValue)
                return s_applicationType.Value;

            if (HostingEnvironment.IsHosted)
            {
                s_applicationType = ApplicationType.Web;
            }
            else
            {
                try
                {
                    // References:
                    // - http://support.microsoft.com/kb/65122
                    // - http://support.microsoft.com/kb/90493/en-us
                    // - http://www.codeguru.com/cpp/w-p/system/misc/article.php/c2897/
                    // We will always have an entry assembly for windows application.
                    FileStream exe = new FileStream(AssemblyInfo.EntryAssembly.Location, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    byte[] dosHeader = new byte[64];
                    byte[] exeHeader = new byte[248];
                    byte[] subSystem = new byte[2];
                    exe.Read(dosHeader, 0, dosHeader.Length);
                    exe.Seek(BitConverter.ToInt16(dosHeader, 60), SeekOrigin.Begin);
                    exe.Read(exeHeader, 0, exeHeader.Length);
                    exe.Close();

                    Buffer.BlockCopy(exeHeader, 92, subSystem, 0, 2);

                    s_applicationType = ((ApplicationType)(LittleEndian.ToInt16(subSystem, 0)));
                }
                catch
                {
                    // We are unable to determine the application type.
                    s_applicationType = ApplicationType.Unknown;
                }
            }

            return s_applicationType.Value;
        }

        /// <summary>
        /// Gets the operating system <see cref="PlatformID"/>
        /// </summary>
        /// <returns>The operating system <see cref="PlatformID"/>.</returns>
        /// <remarks>
        /// This function will properly detect the platform ID, even if running on Mac.
        /// </remarks>
        public static PlatformID GetOSPlatformID()
        {
            if (s_osPlatformID != PlatformID.Win32S)
                return s_osPlatformID;

            s_osPlatformID = Environment.OSVersion.Platform;

            if (s_osPlatformID == PlatformID.Unix)
            {
                // Environment.OSVersion.Platform can report Unix when running on Mac OS X
                try
                {
                    s_osPlatformID = Command.Execute("uname").StandardOutput.StartsWith("Darwin", StringComparison.OrdinalIgnoreCase) ? PlatformID.MacOSX : PlatformID.Unix;
                }
                catch
                {
                    // Fall back on looking for Mac specific root folders:
                    if (Directory.Exists("/Applications") && Directory.Exists("/System") && Directory.Exists("/Users") && Directory.Exists("/Volumes"))
                        s_osPlatformID = PlatformID.MacOSX;
                }
            }

            return s_osPlatformID;
        }

        /// <summary>
        /// Gets the operating system product name.
        /// </summary>
        /// <returns>Operating system product name.</returns>
        public static string GetOSProductName()
        {
            if ((object)s_osPlatformName != null)
                return s_osPlatformName;

            switch (GetOSPlatformID())
            {
                case PlatformID.Unix:
                case PlatformID.MacOSX:
                    // Call sw_vers on Mac to get product name and version information, Linux could have this
                    try
                    {
                        string output = Command.Execute("sw_vers").StandardOutput;
                        Dictionary<string, string> kvps = output.ParseKeyValuePairs('\n', ':');
                        if (kvps.Count > 0)
                            s_osPlatformName = kvps.Values.Select(val => val.Trim()).ToDelimitedString(" ");
                    }
                    catch
                    {
                        s_osPlatformName = null;
                    }

                    if (string.IsNullOrEmpty(s_osPlatformName))
                    {
                        // Try some common ways to get product name on Linux, some might work on Mac
                        try
                        {
                            foreach (string fileName in FilePath.GetFileList("/etc/*release*"))
                            {
                                using (StreamReader reader = new StreamReader(fileName))
                                {
                                    string line = reader.ReadLine();

                                    while ((object)line != null)
                                    {
                                        if (line.StartsWith("PRETTY_NAME", StringComparison.OrdinalIgnoreCase) && !line.Contains('#'))
                                        {
                                            string[] parts = line.Split('=');

                                            if (parts.Length == 2)
                                            {
                                                s_osPlatformName = parts[1].Replace("\"", "");
                                                break;
                                            }
                                        }

                                        line = reader.ReadLine();
                                    }
                                }

                                if (!string.IsNullOrEmpty(s_osPlatformName))
                                    break;
                            }
                        }
                        catch
                        {
                            try
                            {
                                string output = Command.Execute("lsb_release", "-a").StandardOutput;
                                Dictionary<string, string> kvps = output.ParseKeyValuePairs('\n', ':');
                                if (kvps.TryGetValue("Description", out s_osPlatformName) && !string.IsNullOrEmpty(s_osPlatformName))
                                    s_osPlatformName = s_osPlatformName.Trim();

                            }
                            catch
                            {
                                s_osPlatformName = null;
                            }
                        }
                    }
                    break;
                default:
                    // Get Windows product name
                    try
                    {
                        s_osPlatformName = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "ProductName", null).ToString();
                    }
                    catch
                    {
                        s_osPlatformName = null;
                    }
                    break;
            }

            if (string.IsNullOrWhiteSpace(s_osPlatformName))
                s_osPlatformName = GetOSPlatformID().ToString();

            if (IsMono)
                s_osPlatformName += " using Mono";

            return s_osPlatformName;
        }

        /// <summary>
        /// Gets the memory usage by the current process.
        /// </summary>
        /// <returns>Memory usage by the current process.</returns>
        public static long GetProcessMemory()
        {
            long processMemory = Environment.WorkingSet;

            if (processMemory == 0 && IsPosixEnvironment)
            {
                try
                {
                    double percentOfTotal;
                    long totalMemory;

                    // Get total system memory
                    using (PerformanceCounter counter = new PerformanceCounter("Mono Memory", "Total Physical Memory"))
                        totalMemory = counter.RawValue;

                    // Get percent of total memory used by current process
                    string output = Command.Execute("ps", string.Format("-p {0} -o %mem", Process.GetCurrentProcess().Id)).StandardOutput;
                    string[] lines = output.Split('\n');

                    if (lines.Length > 1 && double.TryParse(lines[1].Trim(), out percentOfTotal))
                        processMemory = (long)Math.Round(percentOfTotal / 100.0D * totalMemory);
                }
                catch
                {
                    processMemory = -1;
                }
            }

            return processMemory;
        }

        #region [ Old Code ]

        ///// <summary>Returns the smallest item from a list of parameters.</summary>
        ///// <param name="itemList">A variable number of parameters of type <see cref="Object"/></param>
        ///// <returns>Result is the minimum value of type <see cref="Object"/> in the <paramref name="itemList"/>.</returns>
        //public static object Min(params object[] itemList)
        //{
        //    return itemList.Min<object>(CompareObjects);
        //}

        ///// <summary>Returns the largest item from a list of parameters.</summary>
        ///// <param name="itemList">A variable number of parameters of type <see cref="Object"/></param>
        ///// <returns>Result is the maximum value of type <see cref="Object"/> in the <paramref name="itemList"/>.</returns>
        //public static object Max(params object[] itemList)
        //{
        //    return itemList.Max<object>(CompareObjects);
        //}

        ///// <summary>Compares two elements of any type.</summary>
        ///// <param name="x">Object which is compared to object <paramref name="y"/>.</param>
        ///// <param name="y">Object which is compared against.</param>
        ///// <returns>Result of comparison as an <see cref="int"/>.</returns>
        //public static int CompareObjects(object x, object y)
        //{
        //    // Just using Visual Basic runtime to compare two objects of unknown types - this can be a very
        //    // complex process and the VB runtime library is distributed with .NET anyway, so why not use it:

        //    // Note that comparison is based on VB object comparison rules:
        //    // ms-help://MS.VSCC.v80/MS.MSDN.v80/MS.VisualStudio.v80.en/dv_vbalr/html/d6cb12a8-e52e-46a7-8aaf-f804d634a825.htm
        //    return (Microsoft.VisualBasic.CompilerServices.Operators.ConditionalCompareObjectLess(x, y, false) ? -1 :
        //        (Microsoft.VisualBasic.CompilerServices.Operators.ConditionalCompareObjectGreater(x, y, false) ? 1 : 0));
        //}
        // This function is probably not that useful

        ///// <summary>Time zone names enumeration used to look up desired time zone in GetWin32TimeZone function.</summary>
        //public enum TimeZoneName
        //{
        //    DaylightName,
        //    DisplayName,
        //    StandardName
        //}

        ///// <summary>Returns the specified Win32 time zone, using specified name.</summary>
        ///// <param name="name">Value of name used for time zone lookup.</param>
        ///// <param name="lookupBy">Type of name used for time zone lookup.</param>
        //public static TimeZoneInfo GetTimeZone(string name, TimeZoneName lookupBy)
        //{
        //    foreach (TimeZoneInfo timeZone in TimeZoneInfo.GetSystemTimeZones())
        //    {
        //        if (lookupBy == TimeZoneName.DaylightName)
        //        {
        //            if (string.Compare(timeZone.DaylightName, name, true) == 0)
        //                return timeZone;
        //        }
        //        else if (lookupBy == TimeZoneName.DisplayName)
        //        {
        //            if (string.Compare(timeZone.DisplayName, name, true) == 0)
        //                return timeZone;
        //        }
        //        else if (lookupBy == TimeZoneName.StandardName)
        //        {
        //            if (string.Compare(timeZone.StandardName, name, true) == 0)
        //                return timeZone;
        //        }
        //    }

        //    throw new ArgumentException("Windows time zone with " + lookupBy + " of \"" + name + "\" was not found!");
        //}

        #endregion
    }
}
