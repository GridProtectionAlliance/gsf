//*******************************************************************************************************
//  Common.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  04/03/2006 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/13/2007 - Darrell Zuercher
//       Edited code comments.
//  09/08/2008 - J. Ritchie Carroll
//      Converted to C#.
//
//*******************************************************************************************************

using System;
using System.IO;
using System.Linq;
using TVA.Collections;
using TVA.Reflection;
using Microsoft.VisualBasic.CompilerServices;

namespace TVA
{
    #region [ Enumerations ]

    /// <summary>Specifies the type of the application.</summary>
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
        /// Application runs in the Posix character subsystem.
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

    #endregion

    //  This is the location for handy miscellaneous functions that are difficult to categorize elsewhere

    /// <summary>Defines common global functions.</summary>
    public static class Common
    {
        /// <summary>Returns one of two strongly-typed objects.</summary>
        /// <returns>One of two objects, depending on the evaluation of given expression.</returns>
        /// <param name="expression">The expression you want to evaluate.</param>
        /// <param name="truePart">Returned if expression evaluates to True.</param>
        /// <param name="falsePart">Returned if expression evaluates to False.</param>
        /// <typeparam name="T">Return type used for immediate expression</typeparam>
        /// <remarks>
        /// <para>This function acts as a strongly-typed immediate if (a.k.a. inline if).</para>
        /// <para>It is expected that this function will only be used in Visual Basic.NET as a strongly-typed IIf replacement.</para>
        /// </remarks>
        public static T IIf<T>(bool expression, T truePart, T falsePart)
        {
            return (expression ? truePart : falsePart);
        }

        /// <summary>Creates a strongly-typed Array.</summary>
        /// <returns>New array of specified type.</returns>
        /// <param name="length">Desired length of new array.</param>
        /// <typeparam name="T">Return type for new array.</typeparam>
        /// <remarks>
        /// <para>
        /// The Array.CreateInstance provides better performance and more direct CLR access for array creation (not to
        /// mention less confusion on the matter of array lengths), however the returned System.Array is not typed properly.
        /// This function properly casts the return array based on the the type specification helping when Option Strict is
        /// enabled.
        /// </para>
        /// <para>
        /// Examples:
        /// <code>
        ///     Dim buffer As Byte() = CreateArray(Of Byte)(12)
        ///     Dim matrix As Integer()() = CreateArray(Of Integer())(10)
        /// </code>
        /// </para>
        /// <para>It is expected that this function will only be used in Visual Basic.NET.</para>
        /// </remarks>
        public static T[] CreateArray<T>(int length)
        {
            // The following provides better performance than "Return New T(length) {}".
            return (T[])Array.CreateInstance(typeof(T), length);
        }

        /// <summary>Creates a strongly-typed Array with an initial value parameter.</summary>
        /// <returns>New array of specified type.</returns>
        /// <param name="length">Desired length of new array.</param>
        /// <param name="initialValue">Value used to initialize all array elements.</param>
        /// <typeparam name="T">Return type for new array.</typeparam>
        /// <remarks>
        /// <para>
        /// Examples:
        /// <code>
        ///     Dim elements As Integer() = CreateArray(12, -1)
        ///     Dim names As String() = CreateArray(100, "undefined")
        /// </code>
        /// </para>
        /// <para>It is expected that this function will only be used in Visual Basic.NET.</para>
        /// </remarks>
        public static T[] CreateArray<T>(int length, T initialValue)
        {
            T[] typedArray = CreateArray<T>(length);

            // Initializes all elements with the default value.
            for (int x = 0; x <= typedArray.Length - 1; x++)
            {
                typedArray[x] = initialValue;
            }

            return typedArray;
        }


        /// <summary>
        /// Gets the type of the currently executing application.
        /// </summary>
        /// <returns>One of the TVA.ApplicationType values.</returns>
        public static ApplicationType GetApplicationType()
        {
            if (System.Web.HttpContext.Current == null)
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

                    Array.Copy(exeHeader, 92, subSystem, 0, 2);

                    return ((ApplicationType)(BitConverter.ToInt16(subSystem, 0)));
                }
                catch
                {
                    // We are unable to determine the application type. This is possible in case of a web app/web site
                    // when this method is being called from a thread other than the main thread, in which case the
                    // System.Web.HttpContext.Current property, used to determine if it is a web app, will not be set.
                    return ApplicationType.Unknown;
                }
            }
            else
            {
                return ApplicationType.Web;
            }
        }

        /// <summary>Gets a high-resolution number of seconds, including fractional seconds, that have
        /// elapsed since 12:00:00 midnight, January 1, 0001.</summary>
        public static double SystemTimer
        {
            get
            {
                return Ticks.ToSeconds(PrecisionTimer.Now.Ticks);
            }
        }

        /// <summary>Determines if given item is a reference type.</summary>
        public static bool IsReference(object item)
        {
            return !(item is ValueType);
        }

        /// <summary>Determines if given item is a reference type but not a string.</summary>
        public static bool IsNonStringReference(object item)
        {
            return (IsReference(item) && !(item is string));
        }

        /// <summary>Determines if given item is numeric.</summary>
        public static bool IsNumeric(object item)
        {
            IConvertible convertible = item as IConvertible;

            if (convertible != null)
            {
                switch (convertible.GetTypeCode())
                {
                    case TypeCode.Boolean:
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                    case TypeCode.Single:
                    case TypeCode.Double:
                    case TypeCode.Decimal:
                        return true;
                    case TypeCode.Char:
                    case TypeCode.String:
                        double result;
                        return double.TryParse(convertible.ToString(null), out result);
                }
            }

            return false;
        }

        /// <summary>Returns the smallest item from a list of parameters.</summary>
        public static object Min(params object[] itemList)
        {
            return itemList.Min<object>(CompareObjects);
        }

        /// <summary>Returns the smallest item from a list of parameters.</summary>
        public static T Min<T>(params T[] itemList)
        {
            return itemList.Min<T>();
        }

        /// <summary>Returns the largest item from a list of parameters.</summary>
        public static object Max(params object[] itemList)
        {
            return itemList.Max<object>(CompareObjects);
        }

        /// <summary>Returns the largest item from a list of parameters.</summary>
        public static T Max<T>(params T[] itemList)
        {
            return itemList.Max<T>();
        }

        /// <summary>Compares two elements of any type.</summary>
        public static int CompareObjects(object x, object y)
        {
            // Just using Visual Basic runtime to compare two objects of unknown types - this can be a very
            // complex process and the VB runtime library is distributed with .NET anyway, so why not use it:

            // Note that comparison is based on VB object comparison rules:
            // ms-help://MS.VSCC.v80/MS.MSDN.v80/MS.VisualStudio.v80.en/dv_vbalr/html/d6cb12a8-e52e-46a7-8aaf-f804d634a825.htm
            return (Operators.ConditionalCompareObjectLess(x, y, false) ? -1 : (Operators.ConditionalCompareObjectGreater(x, y, false) ? 1 : 0));
        }
        /// <summary>Gets the 3-letter month abbreviation for given month number (1 - 12).</summary>
        /// <param name="month">Index of month to display (1 - 12).</param>
        /// <remarks>Month abbreviations are English only.</remarks>
        public static string ShortMonthName(int month)
        {
            switch (month)
            {
                case 1:
                    return "Jan";
                case 2:
                    return "Feb";
                case 3:
                    return "Mar";
                case 4:
                    return "Apr";
                case 5:
                    return "May";
                case 6:
                    return "Jun";
                case 7:
                    return "Jul";
                case 8:
                    return "Aug";
                case 9:
                    return "Sep";
                case 10:
                    return "Oct";
                case 11:
                    return "Nov";
                case 12:
                    return "Dec";
                default:
                    throw new ArgumentOutOfRangeException("month", "Invalid month number \"" + month + "\" specified - expected a value between 1 and 12");
            }
        }

        /// <summary>Gets the full month name for given month number (1 - 12).</summary>
        /// <param name="month">Index of month to display (1 - 12).</param>
        /// <remarks>Month names are English only.</remarks>
        public static string LongMonthName(int month)
        {
            switch (month)
            {
                case 1:
                    return "January";
                case 2:
                    return "February";
                case 3:
                    return "March";
                case 4:
                    return "April";
                case 5:
                    return "May";
                case 6:
                    return "June";
                case 7:
                    return "July";
                case 8:
                    return "August";
                case 9:
                    return "September";
                case 10:
                    return "October";
                case 11:
                    return "November";
                case 12:
                    return "December";
                default:
                    throw new ArgumentOutOfRangeException("month", "Invalid month number \"" + month + "\" specified - expected a value between 1 and 12");
            }
        }

        #region [ Old Code ]

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
