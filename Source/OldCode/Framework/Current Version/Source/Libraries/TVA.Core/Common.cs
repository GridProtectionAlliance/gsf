//*******************************************************************************************************
//  Common.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  04/03/2006 - James R. Carroll
//       Generated original version of source code.
//  12/13/2007 - Darrell Zuercher
//       Edited code comments.
//  09/08/2008 - James R. Carroll
//      Converted to C#.
//  02/13/2009 - Josh Patterson
//      Edited Code Comments
//
//*******************************************************************************************************

using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using TVA.Collections;
using TVA.Reflection;

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

    // This is the location for handy miscellaneous functions that are difficult to categorize elsewhere. For the most
    // part these functions may have the most value in a Visual Basic application which supports importing functions
    // down to a class level, e.g.: Imports TVA.Common

    /// <summary>
    /// Defines common global functions.
    /// </summary>
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
        /// <para>It is expected that this function will only be used in Visual Basic.NET.</para>
        /// <para>
        /// The Array.CreateInstance provides better performance and more direct CLR access for array creation (not to
        /// mention less confusion on the matter of array lengths) in VB.NET, however the returned System.Array is not
        /// typed properly. This function properly casts the return array based on the the type specification helping
        /// when Option Strict is enabled.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code language="VB">
        ///     Dim buffer As Byte() = CreateArray(Of Byte)(12)
        ///     Dim matrix As Integer()() = CreateArray(Of Integer())(10)
        /// </code>
        /// </example>
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
        /// It is expected that this function will only be used in Visual Basic.NET.
        /// </remarks>
        /// <example>
        /// <code language="VB">
        ///     Dim elements As Integer() = CreateArray(12, -1)
        ///     Dim names As String() = CreateArray(100, "undefined")
        /// </code>
        /// </example>
        public static T[] CreateArray<T>(int length, T initialValue)
        {
            T[] typedArray = CreateArray<T>(length);

            // Initializes all elements with the default value.
            for (int x = 0; x < typedArray.Length; x++)
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

        // The following "ToNonNullString" methods extend all class based objects. Note that these extension methods can be
        // called even if the base object is null, hence the value of these methods. Our philosophy for this project has been
        // "not" to apply extensions to all objects (this to avoid general namespace pollution) and make sure extensions are
        // grouped together in their own source file (e.g., StringExtensions.cs); however these items do apply to all items
        // and are essentially type-less hence their location in the "Common" class. These extension methods are at least
        // limited to classes and won't show up on native types and custom structs.

        /// <summary>
        /// Converts value to string, null objects will return an empty string (""). 
        /// </summary>
        /// <typeparam name="T"><see cref="Type"/> of <see cref="Object"/> to convert to string.</typeparam>
        /// <param name="value">Value to convert to string.</param>
        /// <returns><paramref name="value"/> as a string; if <paramref name="value"/> is null, empty string ("") will be returned. </returns>
        public static string ToNonNullString<T>(this T value) where T : class
        {
            return (value == null ? "" : value.ToString());
        }

        /// <summary>
        /// Converts value to string, null objects will return specified <paramref name="nonNullValue"/>.
        /// </summary>
        /// <typeparam name="T"><see cref="Type"/> of <see cref="Object"/> to convert to string.</typeparam>
        /// <param name="value">Value to convert to string.</param>
        /// <param name="nonNullValue"><see cref="String"/> to return if <paramref name="value"/> is null.</param>
        /// <returns><paramref name="value"/> as a string; if <paramref name="value"/> is null, <paramref name="nonNullValue"/> will be returned.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="nonNullValue"/> cannot be null.</exception>
        public static string ToNonNullString<T>(this T value, string nonNullValue) where T : class
        {
            if (nonNullValue == null)
                throw new ArgumentNullException("nonNullValue");

            return (value == null ? nonNullValue : value.ToString());
        }

        // We handle strings as a special version of the ToNullNullString extension to handle documentation a little differently

        /// <summary>
        /// Makes sure returned string value is not null; if this string is null, empty string ("") will be returned. 
        /// </summary>
        /// <param name="value"><see cref="String"/> to verify is not null.</param>
        /// <returns><see cref="String"/> value; if <paramref name="value"/> is null, empty string ("") will be returned. </returns>
        public static string ToNonNullString(this string value)
        {
            return (value == null ? "" : value);
        }

        /// <summary>
        /// Converts <paramref name="value"/> to a <see cref="String"/> using an appropriate <see cref="TypeConverter"/>.
        /// </summary>
        /// <param name="value">Value to convert to a <see cref="String"/>.</param>
        /// <returns><paramref name="value"/> converted to a <see cref="String"/>.</returns>
        /// <remarks>
        /// <para>
        /// If <see cref="TypeConverter"/> fails, the value's <c>ToString()</c> value will be returned.
        /// Returned value will never be null, if no value exists an empty string ("") will be returned.
        /// </para>
        /// <para>
        /// You can use the <see cref="StringExtensions.ConvertToType{T}(string)"/> string extension method to
        /// convert the string back to its original <see cref="Type"/>.
        /// </para>
        /// </remarks>
        public static string TypeConvertToString(object value)
        {
            try
            {
                // Attempt to use type converter to set field value
                TypeConverter converter = TypeDescriptor.GetConverter(value);
                return converter.ConvertToString(value).ToNonNullString();
            }
            catch
            {
                // Otherwise just call object's ToString method
                return value.ToNonNullString();
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

        /// <summary>Determines if given item is equal to its default value (e.g., null or 0.0).</summary>
        /// <param name="item">Object to evaluate.</param>
        /// <returns>Result of evaluation as a <see cref="bool"/>.</returns>
        /// <remarks>
        /// Native types default to zero, not null, therefore this can be used to evaulate if an item is its default (i.e., uninitialized) value.
        /// </remarks>
        public static bool IsDefaultValue(object item)
        {
            // Only reference types can be null, therefore null is its default value
            if (item == null)
                return true;

            Type itemType = item.GetType();

            if (itemType.IsValueType)
            {
                // Handle common types
                IConvertible convertible = item as IConvertible;

                if (convertible != null)
                {
                    switch (convertible.GetTypeCode())
                    {
                        case TypeCode.Boolean:
                            return ((bool)item == default(bool));
                        case TypeCode.SByte:
                            return ((sbyte)item == default(sbyte));
                        case TypeCode.Byte:
                            return ((byte)item == default(byte));
                        case TypeCode.Int16:
                            return ((short)item == default(short));
                        case TypeCode.UInt16:
                            return ((ushort)item == default(ushort));
                        case TypeCode.Int32:
                            return ((int)item == default(int));
                        case TypeCode.UInt32:
                            return ((uint)item == default(uint));
                        case TypeCode.Int64:
                            return ((long)item == default(long));
                        case TypeCode.UInt64:
                            return ((ulong)item == default(ulong));
                        case TypeCode.Single:
                            return ((float)item == default(float));
                        case TypeCode.Double:
                            return ((double)item == default(double));
                        case TypeCode.Decimal:
                            return ((decimal)item == default(decimal));
                        case TypeCode.Char:
                            return ((char)item == default(char));
                        case TypeCode.DateTime:
                            return ((DateTime)item == default(DateTime));
                    }
                }

                // Handle custom value types
                return ((ValueType)item).Equals(Activator.CreateInstance(itemType));
            }

            return (item == null);
        }

        /// <summary>Determines if given item is a reference type.</summary>
        /// <param name="item">Object to evaluate.</param>
        /// <returns>Result of evaluation as a <see cref="bool"/>.</returns>
        public static bool IsReference(object item)
        {
            return !(item is ValueType);
        }
        
        /// <summary>Determines if given item is a reference type but not a string.</summary>
        /// <param name="item">Object to evaluate.</param>
        /// <returns>Result of evaluation as a <see cref="bool"/>.</returns>
        public static bool IsNonStringReference(object item)
        {
            return (IsReference(item) && !(item is string));
        }

        /// <summary>Determines if given item is numeric.</summary>
        /// <param name="item">Object to evaluate.</param>
        /// <returns>Result of evaluation as a <see cref="bool"/>.</returns>
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
        /// <typeparam name="T">Return type <see cref="Type"/> that is the minimum value in the <paramref name="itemList"/>.</typeparam>
        /// <param name="itemList">A variable number of parameters of the specified type.</param>
        /// <returns>Result is the minimum value of type <see cref="Type"/> in the <paramref name="itemList"/>.</returns>
        public static T Min<T>(params T[] itemList)
        {
            return itemList.Min<T>();
        }


        /// <summary>Returns the largest item from a list of parameters.</summary>
        /// <typeparam name="T">Return type <see cref="Type"/> that is the maximum value in the <paramref name="itemList"/>.</typeparam>
        /// <param name="itemList">A variable number of parameters of the specified type .</param>
        /// <returns>Result is the maximum value of type <see cref="Type"/> in the <paramref name="itemList"/>.</returns>
        public static T Max<T>(params T[] itemList)
        {
            return itemList.Max<T>();
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
