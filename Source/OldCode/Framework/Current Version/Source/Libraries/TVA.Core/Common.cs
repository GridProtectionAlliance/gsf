//*******************************************************************************************************
//  Common.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
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
//
//*******************************************************************************************************

#region [ TVA Open Source Agreement ]
/*

 THIS OPEN SOURCE AGREEMENT ("AGREEMENT") DEFINES THE RIGHTS OF USE,REPRODUCTION, DISTRIBUTION,
 MODIFICATION AND REDISTRIBUTION OF CERTAIN COMPUTER SOFTWARE ORIGINALLY RELEASED BY THE
 TENNESSEE VALLEY AUTHORITY, A CORPORATE AGENCY AND INSTRUMENTALITY OF THE UNITED STATES GOVERNMENT
 ("GOVERNMENT AGENCY"). GOVERNMENT AGENCY IS AN INTENDED THIRD-PARTY BENEFICIARY OF ALL SUBSEQUENT
 DISTRIBUTIONS OR REDISTRIBUTIONS OF THE SUBJECT SOFTWARE. ANYONE WHO USES, REPRODUCES, DISTRIBUTES,
 MODIFIES OR REDISTRIBUTES THE SUBJECT SOFTWARE, AS DEFINED HEREIN, OR ANY PART THEREOF, IS, BY THAT
 ACTION, ACCEPTING IN FULL THE RESPONSIBILITIES AND OBLIGATIONS CONTAINED IN THIS AGREEMENT.

 Original Software Designation: openPDC
 Original Software Title: The TVA Open Source Phasor Data Concentrator
 User Registration Requested. Please Visit https://naspi.tva.com/Registration/
 Point of Contact for Original Software: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>

 1. DEFINITIONS

 A. "Contributor" means Government Agency, as the developer of the Original Software, and any entity
 that makes a Modification.

 B. "Covered Patents" mean patent claims licensable by a Contributor that are necessarily infringed by
 the use or sale of its Modification alone or when combined with the Subject Software.

 C. "Display" means the showing of a copy of the Subject Software, either directly or by means of an
 image, or any other device.

 D. "Distribution" means conveyance or transfer of the Subject Software, regardless of means, to
 another.

 E. "Larger Work" means computer software that combines Subject Software, or portions thereof, with
 software separate from the Subject Software that is not governed by the terms of this Agreement.

 F. "Modification" means any alteration of, including addition to or deletion from, the substance or
 structure of either the Original Software or Subject Software, and includes derivative works, as that
 term is defined in the Copyright Statute, 17 USC § 101. However, the act of including Subject Software
 as part of a Larger Work does not in and of itself constitute a Modification.

 G. "Original Software" means the computer software first released under this Agreement by Government
 Agency entitled openPDC, including source code, object code and accompanying documentation, if any.

 H. "Recipient" means anyone who acquires the Subject Software under this Agreement, including all
 Contributors.

 I. "Redistribution" means Distribution of the Subject Software after a Modification has been made.

 J. "Reproduction" means the making of a counterpart, image or copy of the Subject Software.

 K. "Sale" means the exchange of the Subject Software for money or equivalent value.

 L. "Subject Software" means the Original Software, Modifications, or any respective parts thereof.

 M. "Use" means the application or employment of the Subject Software for any purpose.

 2. GRANT OF RIGHTS

 A. Under Non-Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor,
 with respect to its own contribution to the Subject Software, hereby grants to each Recipient a
 non-exclusive, world-wide, royalty-free license to engage in the following activities pertaining to
 the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Modification

 5. Redistribution

 6. Display

 B. Under Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor, with
 respect to its own contribution to the Subject Software, hereby grants to each Recipient under Covered
 Patents a non-exclusive, world-wide, royalty-free license to engage in the following activities
 pertaining to the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Sale

 5. Offer for Sale

 C. The rights granted under Paragraph B. also apply to the combination of a Contributor's Modification
 and the Subject Software if, at the time the Modification is added by the Contributor, the addition of
 such Modification causes the combination to be covered by the Covered Patents. It does not apply to
 any other combinations that include a Modification. 

 D. The rights granted in Paragraphs A. and B. allow the Recipient to sublicense those same rights.
 Such sublicense must be under the same terms and conditions of this Agreement.

 3. OBLIGATIONS OF RECIPIENT

 A. Distribution or Redistribution of the Subject Software must be made under this Agreement except for
 additions covered under paragraph 3H. 

 1. Whenever a Recipient distributes or redistributes the Subject Software, a copy of this Agreement
 must be included with each copy of the Subject Software; and

 2. If Recipient distributes or redistributes the Subject Software in any form other than source code,
 Recipient must also make the source code freely available, and must provide with each copy of the
 Subject Software information on how to obtain the source code in a reasonable manner on or through a
 medium customarily used for software exchange.

 B. Each Recipient must ensure that the following copyright notice appears prominently in the Subject
 Software:

          No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.

 C. Each Contributor must characterize its alteration of the Subject Software as a Modification and
 must identify itself as the originator of its Modification in a manner that reasonably allows
 subsequent Recipients to identify the originator of the Modification. In fulfillment of these
 requirements, Contributor must include a file (e.g., a change log file) that describes the alterations
 made and the date of the alterations, identifies Contributor as originator of the alterations, and
 consents to characterization of the alterations as a Modification, for example, by including a
 statement that the Modification is derived, directly or indirectly, from Original Software provided by
 Government Agency. Once consent is granted, it may not thereafter be revoked.

 D. A Contributor may add its own copyright notice to the Subject Software. Once a copyright notice has
 been added to the Subject Software, a Recipient may not remove it without the express permission of
 the Contributor who added the notice.

 E. A Recipient may not make any representation in the Subject Software or in any promotional,
 advertising or other material that may be construed as an endorsement by Government Agency or by any
 prior Recipient of any product or service provided by Recipient, or that may seek to obtain commercial
 advantage by the fact of Government Agency's or a prior Recipient's participation in this Agreement.

 F. In an effort to track usage and maintain accurate records of the Subject Software, each Recipient,
 upon receipt of the Subject Software, is requested to register with Government Agency by visiting the
 following website: https://naspi.tva.com/Registration/. Recipient's name and personal information
 shall be used for statistical purposes only. Once a Recipient makes a Modification available, it is
 requested that the Recipient inform Government Agency at the web site provided above how to access the
 Modification.

 G. Each Contributor represents that that its Modification does not violate any existing agreements,
 regulations, statutes or rules, and further that Contributor has sufficient rights to grant the rights
 conveyed by this Agreement.

 H. A Recipient may choose to offer, and to charge a fee for, warranty, support, indemnity and/or
 liability obligations to one or more other Recipients of the Subject Software. A Recipient may do so,
 however, only on its own behalf and not on behalf of Government Agency or any other Recipient. Such a
 Recipient must make it absolutely clear that any such warranty, support, indemnity and/or liability
 obligation is offered by that Recipient alone. Further, such Recipient agrees to indemnify Government
 Agency and every other Recipient for any liability incurred by them as a result of warranty, support,
 indemnity and/or liability offered by such Recipient.

 I. A Recipient may create a Larger Work by combining Subject Software with separate software not
 governed by the terms of this agreement and distribute the Larger Work as a single product. In such
 case, the Recipient must make sure Subject Software, or portions thereof, included in the Larger Work
 is subject to this Agreement.

 J. Notwithstanding any provisions contained herein, Recipient is hereby put on notice that export of
 any goods or technical data from the United States may require some form of export license from the
 U.S. Government. Failure to obtain necessary export licenses may result in criminal liability under
 U.S. laws. Government Agency neither represents that a license shall not be required nor that, if
 required, it shall be issued. Nothing granted herein provides any such export license.

 4. DISCLAIMER OF WARRANTIES AND LIABILITIES; WAIVER AND INDEMNIFICATION

 A. No Warranty: THE SUBJECT SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTY OF ANY KIND, EITHER
 EXPRESSED, IMPLIED, OR STATUTORY, INCLUDING, BUT NOT LIMITED TO, ANY WARRANTY THAT THE SUBJECT
 SOFTWARE WILL CONFORM TO SPECIFICATIONS, ANY IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 PARTICULAR PURPOSE, OR FREEDOM FROM INFRINGEMENT, ANY WARRANTY THAT THE SUBJECT SOFTWARE WILL BE ERROR
 FREE, OR ANY WARRANTY THAT DOCUMENTATION, IF PROVIDED, WILL CONFORM TO THE SUBJECT SOFTWARE. THIS
 AGREEMENT DOES NOT, IN ANY MANNER, CONSTITUTE AN ENDORSEMENT BY GOVERNMENT AGENCY OR ANY PRIOR
 RECIPIENT OF ANY RESULTS, RESULTING DESIGNS, HARDWARE, SOFTWARE PRODUCTS OR ANY OTHER APPLICATIONS
 RESULTING FROM USE OF THE SUBJECT SOFTWARE. FURTHER, GOVERNMENT AGENCY DISCLAIMS ALL WARRANTIES AND
 LIABILITIES REGARDING THIRD-PARTY SOFTWARE, IF PRESENT IN THE ORIGINAL SOFTWARE, AND DISTRIBUTES IT
 "AS IS."

 B. Waiver and Indemnity: RECIPIENT AGREES TO WAIVE ANY AND ALL CLAIMS AGAINST GOVERNMENT AGENCY, ITS
 AGENTS, EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT. IF RECIPIENT'S USE
 OF THE SUBJECT SOFTWARE RESULTS IN ANY LIABILITIES, DEMANDS, DAMAGES, EXPENSES OR LOSSES ARISING FROM
 SUCH USE, INCLUDING ANY DAMAGES FROM PRODUCTS BASED ON, OR RESULTING FROM, RECIPIENT'S USE OF THE
 SUBJECT SOFTWARE, RECIPIENT SHALL INDEMNIFY AND HOLD HARMLESS  GOVERNMENT AGENCY, ITS AGENTS,
 EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT, TO THE EXTENT PERMITTED BY
 LAW.  THE FOREGOING RELEASE AND INDEMNIFICATION SHALL APPLY EVEN IF THE LIABILITIES, DEMANDS, DAMAGES,
 EXPENSES OR LOSSES ARE CAUSED, OCCASIONED, OR CONTRIBUTED TO BY THE NEGLIGENCE, SOLE OR CONCURRENT, OF
 GOVERNMENT AGENCY OR ANY PRIOR RECIPIENT.  RECIPIENT'S SOLE REMEDY FOR ANY SUCH MATTER SHALL BE THE
 IMMEDIATE, UNILATERAL TERMINATION OF THIS AGREEMENT.

 5. GENERAL TERMS

 A. Termination: This Agreement and the rights granted hereunder will terminate automatically if a
 Recipient fails to comply with these terms and conditions, and fails to cure such noncompliance within
 thirty (30) days of becoming aware of such noncompliance. Upon termination, a Recipient agrees to
 immediately cease use and distribution of the Subject Software. All sublicenses to the Subject
 Software properly granted by the breaching Recipient shall survive any such termination of this
 Agreement.

 B. Severability: If any provision of this Agreement is invalid or unenforceable under applicable law,
 it shall not affect the validity or enforceability of the remainder of the terms of this Agreement.

 C. Applicable Law: This Agreement shall be subject to United States federal law only for all purposes,
 including, but not limited to, determining the validity of this Agreement, the meaning of its
 provisions and the rights, obligations and remedies of the parties.

 D. Entire Understanding: This Agreement constitutes the entire understanding and agreement of the
 parties relating to release of the Subject Software and may not be superseded, modified or amended
 except by further written agreement duly executed by the parties.

 E. Binding Authority: By accepting and using the Subject Software under this Agreement, a Recipient
 affirms its authority to bind the Recipient to all terms and conditions of this Agreement and that
 Recipient hereby agrees to all terms and conditions herein.

 F. Point of Contact: Any Recipient contact with Government Agency is to be directed to the designated
 representative as follows: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>.

*/
#endregion

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
