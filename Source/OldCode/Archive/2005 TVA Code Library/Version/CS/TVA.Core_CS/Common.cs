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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic.CompilerServices;
using TVA.Collections;
using TVA.Data;
using TVA.Reflection;

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
    Web = 15
}

/// <summary>Time zone names enumeration used to look up desired time zone in GetWin32TimeZone function.</summary>
public enum TimeZoneName
{
    DaylightName,
    DisplayName,
    StandardName
}

/// <summary>Time names enumeration used by SecondsToText function.</summary>
public struct TimeName
{
    // Note that this is a structure so elements may be used as an index in
    // a string array with having to cast as (int)
    static public int Year = 0;
    static public int Years = 1;
    static public int Day = 2;
    static public int Days = 3;
    static public int Hour = 4;
    static public int Hours = 5;
    static public int Minute = 6;
    static public int Minutes = 7;
    static public int Second = 8;
    static public int Seconds = 9;
    static public int LessThan60Seconds = 10;
    static public int NoSeconds = 11;
}

#endregion

//  This is the location for handy miscellaneous functions that are difficult to categorize elsewhere

/// <summary>Defines common global functions.</summary>
public static class Common
{
    #region [ Common Functions ]

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
    /// Gets the root type in the inheritace hierarchy from which the specified type inherits.
    /// </summary>
    /// <param name="type">The System.Type whose root type is to be found.</param>
    /// <returns>The root type in the inheritance hierarchy from which the specified type inherits.</returns>
    /// <remarks>Unless input type is System.Object, the returned type will never be System.Object, even though all types ultimately inherit from it.</remarks>
    public static Type GetRootType(Type type)
    {
        // Recurse through types until you reach a base type of "System.Object"
        if (type.BaseType != typeof(object)) return GetRootType(type.BaseType);
        return type;
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
                FileStream exe = new FileStream(AssemblyInformation.EntryAssembly.Location, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
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

    /// <summary>Determines if given item is a reference type.</summary>
    public static bool IsReference(this object item)
    {
        return !(item is ValueType);
    }

    /// <summary>Determines if given item is a reference type but not a string.</summary>
    public static bool IsNonStringReference(this object item)
    {
        return (item.IsReference() && !(item is string));
    }

    #endregion

    #region [ Collection Functions ]

    /*-----------------------------------------------------------------------------------------------------*\
     *
     *                    These functions were migrated here from TVA.Collections.Common
     *                  
    \*-----------------------------------------------------------------------------------------------------*/


    /// <summary>Returns the smallest item from a list of parameters.</summary>
    public static object Min(params object[] itemList)
    {
        return itemList.Min<object>(Compare);
    }

    /// <summary>Returns the smallest item from a list of parameters.</summary>
    public static T Min<T>(params T[] itemList)
    {
        return itemList.Min<T>();
    }

    /// <summary>Returns the largest item from a list of parameters.</summary>
    public static object Max(params object[] itemList)
    {
        return itemList.Max<object>(Compare);
    }

    /// <summary>Returns the largest item from a list of parameters.</summary>
    public static T Max<T>(params T[] itemList)
    {
        return itemList.Max<T>();
    }

    /// <summary>Compares two elements of the specified type.</summary>
    public static int Compare<T>(T x, T y)
    {
        return Comparer<T>.Default.Compare(x, y);
    }

    /// <summary>Compares two elements of any type.</summary>
    public static int Compare(object x, object y)
    {
        if (IsReference(x) && IsReference(y))
        {
            // If both items are reference objects, then test object equality by reference.
            // If not equal by overridable Object.Equals function, use default Comparer.
            if (x == y)
            {
                return 0;
            }
            else if (x.GetType().Equals(y.GetType()))
            {
                // Compares two items that are the same type. Sees if the type supports IComparable interface.
                if (x is IComparable)
                {
                    return ((IComparable)x).CompareTo(y);
                }
                else if (x.Equals(y))
                {
                    return 0;
                }
                else
                {
                    return Comparer.Default.Compare(x, y);
                }
            }
            else
            {
                return Comparer.Default.Compare(x, y);
            }
        }
        else
        {
            // Compares non-reference (i.e., value) types, using VB rules.
            // ms-help://MS.VSCC.v80/MS.MSDN.v80/MS.VisualStudio.v80.en/dv_vbalr/html/d6cb12a8-e52e-46a7-8aaf-f804d634a825.htm
            return (Operators.ConditionalCompareObjectLess(x, y, false) ? -1 : (Operators.ConditionalCompareObjectGreater(x, y, false) ? 1 : 0));
        }
    }

    #endregion

    #region [ DateTime Functions ]

    /*-----------------------------------------------------------------------------------------------------*\
     *
     *                    These functions were migrated here from TVA.DateTime.Common
     *                  
    \*-----------------------------------------------------------------------------------------------------*/

    /// <summary>Number of 100-nanosecond ticks in one second.</summary>
    public const long TicksPerSecond = 10000000;

    /// <summary>Number of 100-nanosecond ticks in one millisecond.</summary>
    public const long TicksPerMillisecond = TicksPerSecond / 1000;

    /// <summary>Standard time names used by SecondsToText function.</summary>
    private static string[] m_standardTimeNames = new string[] { "Year", "Years", "Day", "Days", "Hour", "Hours", "Minute", "Minutes", "Second", "Seconds", "Less Than 60 Seconds", "0 Seconds" };

    /// <summary>Standard time names, without seconds, used by SecondsToText function.</summary>
    private static string[] m_standardTimeNamesWithoutSeconds = new string[] { "Year", "Years", "Day", "Days", "Hour", "Hours", "Minute", "Minutes", "Second", "Seconds", "Less Than 1 Minute", "0 Minutes" };

    /// <summary>Converts 100-nanosecond tick intervals to seconds.</summary>
    public static double TicksToSeconds(long ticks)
    {
        return ticks / (double)TicksPerSecond;
    }

    /// <summary>Converts 100-nanosecond tick intervals to milliseconds.</summary>
    public static double TicksToMilliseconds(long ticks)
    {
        return ticks / (double)TicksPerMillisecond;
    }

    /// <summary>Converts seconds to 100-nanosecond tick intervals.</summary>
    public static long SecondsToTicks(double seconds)
    {
        return (int)(seconds * TicksPerSecond);
    }

    /// <summary>Converts milliseconds to 100-nanosecond tick intervals.</summary>
    public static long MillisecondsToTicks(double milliseconds)
    {
        return (int)(milliseconds * TicksPerMillisecond);
    }

    /// <summary>Gets  the number of seconds in the local timezone, including fractional seconds, that have
    /// elapsed since 12:00:00 midnight, January 1, 0001.</summary>
    public static double SystemTimer
    {
        get
        {
            return TicksToSeconds(DateTime.Now.Ticks);
        }
    }

    /// <summary>Gets the number of seconds in the universally coordinated timezone, including fractional
    /// seconds, that have elapsed since 12:00:00 midnight, January 1, 0001.</summary>
    public static double UtcSystemTimer
    {
        get
        {
            return TicksToSeconds(DateTime.UtcNow.Ticks);
        }
    }

    /// <summary>Turns the given number of seconds into textual representation of years, days, hours, minutes
    /// and whole integer seconds.</summary>
    /// <param name="seconds">Seconds to be converted.</param>
    public static string SecondsToText(double seconds)
    {
        return SecondsToText(seconds, 0);
    }

    /// <summary>Turns the given number of seconds into textual representation of years, days, hours, minutes
    /// and seconds.</summary>
    /// <param name="seconds">Seconds to be converted.</param>
    /// <param name="secondPrecision">Number of fractional digits to display for seconds.</param>
    /// <remarks>Set second precision to -1 to suppress seconds display.</remarks>
    public static string SecondsToText(double seconds, int secondPrecision)
    {
        if (secondPrecision < 0)
            return SecondsToText(seconds, secondPrecision, m_standardTimeNamesWithoutSeconds);
        else
            return SecondsToText(seconds, secondPrecision, m_standardTimeNames);
    }

    /// <summary>Turns the given number of seconds into textual representation of years, days, hours, minutes
    /// and seconds given string array of time names. Need one for each TimeName enum item.</summary>
    /// <param name="seconds">Seconds to be converted.</param>
    /// <param name="secondPrecision">Number of fractional digits to display for seconds.</param>
    /// <param name="timeNames">Time names array to use during textal conversion.</param>
    /// <remarks>
    /// <para>Set second precision to -1 to suppress seconds display.</para>
    /// <para>Time names array needs one string entry per element in <see cref="TimeName">TimeName</see>
    /// enumeration.</para>
    /// <para>Example timeNames array: "Year", "Years", "Day", "Days", "Hour", "Hours", "Minute", "Minutes",
    /// "Second", "Seconds", "Less Than 60 Seconds", "0 Seconds".</para>
    /// </remarks>
    public static string SecondsToText(double seconds, int secondPrecision, string[] timeNames)
    {
        StringBuilder timeImage = new StringBuilder();

        int years; // 1 year   = 365.2425 days or 31556952 seconds
        int days; // 1 day    = 86400 seconds
        int hours; // 1 hour   = 3600 seconds
        int minutes; // 1 minute = 60 seconds

        // checks if number of seconds ranges in years.
        years = (int)(seconds / 31556952);

        if (years >= 1)
        {
            // Removes whole years from remaining seconds.
            seconds = seconds - years * 31556952;

            // Appends textual representation of years.
            timeImage.Append(years);
            timeImage.Append(' ');

            if (years == 1)
                timeImage.Append(timeNames[TimeName.Year]);
            else
                timeImage.Append(timeNames[TimeName.Years]);
        }

        // Checks if remaining number of seconds ranges in days.
        days = (int)(seconds / 86400);
        if (days >= 1)
        {
            // Removes whole days from remaining seconds.
            seconds = seconds - days * 86400;

            // Appends textual representation of days.
            timeImage.Append(' ');
            timeImage.Append(days);
            timeImage.Append(' ');

            if (days == 1)
                timeImage.Append(timeNames[TimeName.Day]);
            else
                timeImage.Append(timeNames[TimeName.Days]);
        }

        // Checks if remaining number of seconds ranges in hours.
        hours = (int)(seconds / 3600);
        if (hours >= 1)
        {
            // Removes whole hours from remaining seconds.
            seconds = seconds - hours * 3600;

            // Appends textual representation of hours.
            timeImage.Append(' ');
            timeImage.Append(hours);
            timeImage.Append(' ');

            if (hours == 1)
                timeImage.Append(timeNames[TimeName.Hour]);
            else
                timeImage.Append(timeNames[TimeName.Hours]);
        }

        // Checks if remaining number of seconds ranges in minutes.
        minutes = (int)(seconds / 60);
        if (minutes >= 1)
        {
            // Removes whole minutes from remaining seconds.
            seconds = seconds - minutes * 60;

            // Appends textual representation of minutes.
            timeImage.Append(' ');
            timeImage.Append(minutes);
            timeImage.Append(' ');

            if (minutes == 1)
                timeImage.Append(timeNames[TimeName.Minute]);
            else
                timeImage.Append(timeNames[TimeName.Minutes]);
        }

        // Handles remaining seconds.
        if (secondPrecision == 0)
        {
            // No fractional seconds requested. Rounds seconds to nearest integer.
            int wholeSeconds = (int)System.Math.Round(seconds);

            if (wholeSeconds > 0)
            {
                // Appends textual representation of whole seconds.
                timeImage.Append(' ');
                timeImage.Append(wholeSeconds);
                timeImage.Append(' ');

                if (wholeSeconds == 1)
                    timeImage.Append(timeNames[TimeName.Second]);
                else
                    timeImage.Append(timeNames[TimeName.Seconds]);
            }
        }
        else
        {
            // Handles fractional seconds request.
            if (seconds > 0)
            {
                if (secondPrecision < 0)
                {
                    // If second display has been disabled and less than 60 seconds remain, we still need
                    // to show something.
                    if (timeImage.Length == 0)
                        timeImage.Append(timeNames[TimeName.LessThan60Seconds]);
                }
                else
                {
                    // Appends textual representation of fractional seconds.
                    timeImage.Append(' ');
                    timeImage.Append(seconds.ToString("0." + (new string('0', secondPrecision))));
                    timeImage.Append(' ');

                    if (seconds == 1)
                        timeImage.Append(timeNames[TimeName.Second]);
                    else
                        timeImage.Append(timeNames[TimeName.Seconds]);
                }
            }
        }

        // Handles zero seconds display.
        if (timeImage.Length == 0)
            timeImage.Append(timeNames[TimeName.NoSeconds]);

        return timeImage.ToString().Trim();
    }

    /// <summary>Returns the specified Win32 time zone, using specified name.</summary>
    /// <param name="name">Value of name used for time zone lookup.</param>
    /// <param name="lookupBy">Type of name used for time zone lookup.</param>
    public static TimeZoneInfo GetTimeZone(string name, TimeZoneName lookupBy)
    {
        foreach (TimeZoneInfo timeZone in TimeZoneInfo.GetSystemTimeZones())
        {
            if (lookupBy == TimeZoneName.DaylightName)
            {
                if (string.Compare(timeZone.DaylightName, name, true) == 0)
                    return timeZone;
            }
            else if (lookupBy == TimeZoneName.DisplayName)
            {
                if (string.Compare(timeZone.DisplayName, name, true) == 0)
                    return timeZone;
            }
            else if (lookupBy == TimeZoneName.StandardName)
            {
                if (string.Compare(timeZone.StandardName, name, true) == 0)
                    return timeZone;
            }
        }

        throw new ArgumentException("Windows time zone with " + lookupBy + " of \"" + name + "\" was not found!");
    }

    #endregion

    #region [Data Functions ]

    /*-----------------------------------------------------------------------------------------------------*\
     *
     *                    These functions were migrated here from TVA.Data.Common
     *                  
    \*-----------------------------------------------------------------------------------------------------*/


    /// <summary>
    /// Performs SQL encoding on given T-SQL string.
    /// </summary>
    /// <param name="sql">The string on which SQL encoding is to be performed.</param>
    /// <returns>The SQL encoded string.</returns>
    public static string SqlEncode(string sql)
    {
        return sql.Replace("\'", "\'\'").Replace("/*", "").Replace("--", "");
    }

    /// <summary>
    /// Executes the SQL statement, and returns the number of rows affected.
    /// </summary>
    /// <param name="sql">The SQL statement to be executed.</param>
    /// <param name="connectString">The connection string used for connecting to the data source.</param>
    /// <param name="connectionType">The type of data provider to use for connecting to the data source and executing the SQL statement.</param>
    /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
    /// <returns>The number of rows affected.</returns>
    public static int ExecuteNonQuery(string sql, string connectString, ConnectionType connectionType, int timeout)
    {
        int executionResult = -1;
        IDbConnection connection = null;
        IDbCommand command = null;

        switch (connectionType)
        {
            case ConnectionType.SqlClient:
                connection = new SqlConnection(connectString);
                command = new SqlCommand(sql, (SqlConnection)connection);
                break;
            case ConnectionType.OracleClient:
                connection = new OracleConnection(connectString);
                command = new OracleCommand(sql, (OracleConnection)connection);
                break;
            case ConnectionType.OleDb:
                connection = new OleDbConnection(connectString);
                command = new OleDbCommand(sql, (OleDbConnection)connection);
                break;
        }

        connection.Open();
        command.CommandTimeout = timeout;
        executionResult = command.ExecuteNonQuery();
        connection.Close();
        return executionResult;
    }

    /// <summary>
    /// Converts delimited text to DataTable.
    /// </summary>
    /// <param name="delimitedData">The delimited text to be converted to DataTable.</param>
    /// <param name="delimiter">The character(s) used for delimiting the text.</param>
    /// <param name="header">True, if the delimited text contains header information; otherwise, false.</param>
    /// <returns>A DataTable object.</returns>
    public static DataTable DelimitedDataToDataTable(string delimitedData, string delimiter, bool header)
    {
        DataTable table = new DataTable();
        string pattern = Regex.Escape(delimiter) + "(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))"; //Regex pattern that will be used to split the delimited data.

        delimitedData = delimitedData.Trim().Trim(new char[] { '\r', '\n' }).Replace("\n", ""); //Remove any leading and trailing whitespaces, carriage returns or line feeds.

        string[] lines = delimitedData.Split('\r'); //Splits delimited data into lines.

        int cursor = 0;

        //Assumes that the first line has header information.
        string[] headers = Regex.Split(lines[cursor], pattern);

        //Creates columns.
        if (header)
        {
            //Uses the first row as header row.
            for (int i = 0; i <= headers.Length - 1; i++)
            {
                table.Columns.Add(new DataColumn(headers[i].Trim(new char[] { '\"' }))); //Remove any leading and trailing quotes from the column name.
            }
            cursor++;
        }
        else
        {
            for (int i = 0; i <= headers.Length - 1; i++)
            {
                table.Columns.Add(new DataColumn());
            }
        }

        //Populates the data table with csv data.
        for (; cursor <= lines.Length - 1; cursor++)
        {
            DataRow row = table.NewRow(); //Creates new row.

            //Populates the new row.
            string[] fields = Regex.Split(lines[cursor], pattern);
            for (int i = 0; i <= fields.Length - 1; i++)
            {
                row[i] = fields[i].Trim(new char[] { '\"' }); //Removes any leading and trailing quotes from the data.
            }

            table.Rows.Add(row); //Adds the new row.
        }

        //Returns the data table.
        return table;
    }

    /// <summary>
    /// Converts the DataTable to delimited text.
    /// </summary>
    /// <param name="table">The DataTable whose data is to be converted to delimited text.</param>
    /// <param name="delimiter">The character(s) to be used for delimiting the text.</param>
    /// <param name="quoted">True, if text is to be surrounded by quotes; otherwise, false.</param>
    /// <param name="header">True, if the delimited text should have header information.</param>
    /// <returns>A string of delimited text.</returns>
    public static string DataTableToDelimitedData(DataTable table, string delimiter, bool quoted, bool header)
    {
        StringBuilder data = new StringBuilder();

        //Uses the column names as the headers if headers are requested.
        if (header)
        {
            for (int i = 0; i <= table.Columns.Count - 1; i++)
            {
                data.Append((quoted ? "\"" : "") + table.Columns[i].ColumnName + (quoted ? "\"" : ""));

                if (i < table.Columns.Count - 1)
                {
                    data.Append(delimiter);
                }
            }
            data.Append("\r\n");
        }

        for (int i = 0; i <= table.Rows.Count - 1; i++)
        {
            //Converts data table's data to delimited data.
            for (int j = 0; j <= table.Columns.Count - 1; j++)
            {
                data.Append((quoted ? "\"" : "") + table.Rows[i][j].ToString() + (quoted ? "\"" : ""));

                if (j < table.Columns.Count - 1)
                {
                    data.Append(delimiter);
                }
            }
            data.Append("\r\n");
        }

        //Returns the delimited data.
        return data.ToString();
    }

    #endregion

    #region [ Math Functions ]

    /*-----------------------------------------------------------------------------------------------------*\
     *
     *                    These functions were migrated here from TVA.Math.Common
     *                  
    \*-----------------------------------------------------------------------------------------------------*/

    /// <summary>Ensures parameter passed to function is not zero. Returns -1
    /// if <paramref name="testValue">testValue</paramref> is zero.</summary>
    /// <param name="testValue">Value to test for zero.</param>
    /// <returns>A non-zero value.</returns>
    public static double NotZero(double testValue)
    {
        return NotZero(testValue, -1.0);
    }

    /// <summary>Ensures parameter passed to function is not zero.</summary>
    /// <param name="testValue">Value to test for zero.</param>
    /// <param name="nonZeroReturnValue">Value to return if <paramref name="testValue">testValue</paramref> is
    /// zero.</param>
    /// <returns>A non-zero value.</returns>
    /// <remarks>To optimize performance, this function does not validate that the notZeroReturnValue is not
    /// zero.</remarks>
    public static double NotZero(double testValue, double nonZeroReturnValue)
    {
        return (testValue == 0.0 ? nonZeroReturnValue : testValue);
    }

    /// <summary>Ensures test parameter passed to function is not equal to the specified value.</summary>
    /// <param name="testValue">Value to test.</param>
    /// <param name="notEqualToValue">Value that represents the undesired value (e.g., zero).</param>
    /// <param name="alternateValue">Value to return if <paramref name="testValue">testValue</paramref> is equal
    /// to the undesired value.</param>
    /// <typeparam name="T">Structure or class that implements IEquatable(Of T) (e.g., Double, Single,
    /// Integer, etc.).</typeparam>
    /// <returns>A value not equal to notEqualToValue.</returns>
    /// <remarks>To optimize performance, this function does not validate that the notEqualToValue is not equal
    /// to the alternateValue.</remarks>
    public static T NotEqualTo<T>(T testValue, T notEqualToValue, T alternateValue) where T : IEquatable<T>
    {
        return (((IEquatable<T>)testValue).Equals(notEqualToValue) ? alternateValue : testValue);
    }

    /// <summary>Ensures test parameter passed to function is not less than the specified value.</summary>
    /// <param name="testValue">Value to test.</param>
    /// <param name="notLessThanValue">Value that represents the lower limit for the testValue. This value
    /// is returned if testValue is less than notLessThanValue.</param>
    /// <typeparam name="T">Structure or class that implements IComparable(Of T) (e.g., Double, Single,
    /// Integer, etc.).</typeparam>
    /// <returns>A value not less than notLessThanValue.</returns>
    /// <remarks>If testValue is less than notLessThanValue, then notLessThanValue is returned.</remarks>
    public static T NotLessThan<T>(T testValue, T notLessThanValue) where T : IComparable<T>
    {
        return (((IComparable<T>)testValue).CompareTo(notLessThanValue) < 0 ? notLessThanValue : testValue);
    }

    /// <summary>Ensures test parameter passed to function is not less than the specified value.</summary>
    /// <param name="testValue">Value to test.</param>
    /// <param name="notLessThanValue">Value that represents the lower limit for the testValue.</param>
    /// <param name="alternateValue">Value to return if <paramref name="testValue">testValue</paramref> is
    /// less than <paramref name="notLessThanValue">notLessThanValue</paramref>.</param>
    /// <typeparam name="T">Structure or class that implements IComparable(Of T) (e.g., Double, Single,
    /// Integer, etc.).</typeparam>
    /// <returns>A value not less than notLessThanValue.</returns>
    /// <remarks>To optimize performance, this function does not validate that the notLessThanValue is not
    /// less than the alternateValue.</remarks>
    public static T NotLessThan<T>(T testValue, T notLessThanValue, T alternateValue) where T : IComparable<T>
    {
        return (((IComparable<T>)testValue).CompareTo(notLessThanValue) < 0 ? alternateValue : testValue);
    }

    /// <summary>Ensures test parameter passed to function is not less than or equal to the specified value.</summary>
    /// <param name="testValue">Value to test.</param>
    /// <param name="notLessThanOrEqualToValue">Value that represents the lower limit for the testValue.</param>
    /// <param name="alternateValue">Value to return if <paramref name="testValue">testValue</paramref> is
    /// less than or equal to <paramref name="notLessThanOrEqualToValue">notLessThanOrEqualToValue</paramref>.</param>
    /// <typeparam name="T">Structure or class that implements IComparable(Of T) (e.g., Double, Single,
    /// Integer, etc.).</typeparam>
    /// <returns>A value not less than or equal to notLessThanOrEqualToValue.</returns>
    /// <remarks>To optimize performance, this function does not validate that the notLessThanOrEqualToValue is
    /// not less than or equal to the alternateValue.</remarks>
    public static T NotLessThanOrEqualTo<T>(T testValue, T notLessThanOrEqualToValue, T alternateValue) where T : IComparable<T>
    {
        return (((IComparable<T>)testValue).CompareTo(notLessThanOrEqualToValue) <= 0 ? alternateValue : testValue);
    }

    /// <summary>Ensures test parameter passed to function is not greater than the specified value.</summary>
    /// <param name="testValue">Value to test.</param>
    /// <param name="notGreaterThanValue">Value that represents the upper limit for the testValue. This
    /// value is returned if testValue is greater than notGreaterThanValue.</param>
    /// <typeparam name="T">Structure or class that implements IComparable(Of T) (e.g., Double, Single,
    /// Integer, etc.).</typeparam>
    /// <returns>A value not greater than notGreaterThanValue.</returns>
    /// <remarks>If testValue is greater than notGreaterThanValue, then notGreaterThanValue is returned.</remarks>
    public static T NotGreaterThan<T>(T testValue, T notGreaterThanValue) where T : IComparable<T>
    {
        return (((IComparable<T>)testValue).CompareTo(notGreaterThanValue) > 0 ? notGreaterThanValue : testValue);
    }

    /// <summary>Ensures test parameter passed to function is not greater than the specified value.</summary>
    /// <param name="testValue">Value to test.</param>
    /// <param name="notGreaterThanValue">Value that represents the upper limit for the testValue.</param>
    /// <param name="alternateValue">Value to return if <paramref name="testValue">testValue</paramref> is
    /// greater than <paramref name="notGreaterThanValue">notGreaterThanValue</paramref>.</param>
    /// <typeparam name="T">Structure or class that implements IComparable(Of T) (e.g., Double, Single,
    /// Integer, etc.).</typeparam>
    /// <returns>A value not greater than notGreaterThanValue.</returns>
    /// <remarks>To optimize performance, this function does not validate that the notGreaterThanValue is
    /// not greater than the alternateValue</remarks>
    public static T NotGreaterThan<T>(T testValue, T notGreaterThanValue, T alternateValue) where T : IComparable<T>
    {
        return (((IComparable<T>)testValue).CompareTo(notGreaterThanValue) > 0 ? alternateValue : testValue);
    }

    /// <summary>Ensures test parameter passed to function is not greater than or equal to the specified value.</summary>
    /// <param name="testValue">Value to test.</param>
    /// <param name="notGreaterThanOrEqualToValue">Value that represents the upper limit for the testValue.</param>
    /// <param name="alternateValue">Value to return if <paramref name="testValue">testValue</paramref> is
    /// greater than or equal to <paramref name="notGreaterThanOrEqualToValue">notGreaterThanOrEqualToValue</paramref>.</param>
    /// <typeparam name="T">Structure or class that implements IComparable(Of T) (e.g., Double, Single,
    /// Integer, etc.).</typeparam>
    /// <returns>A value not greater than or equal to notGreaterThanOrEqualToValue.</returns>
    /// <remarks>To optimize performance, this function does not validate that the notGreaterThanOrEqualToValue
    /// is not greater than or equal to the alternateValue.</remarks>
    public static T NotGreaterThanOrEqualTo<T>(T testValue, T notGreaterThanOrEqualToValue, T alternateValue) where T : IComparable<T>
    {
        return (((IComparable<T>)testValue).CompareTo(notGreaterThanOrEqualToValue) >= 0 ? alternateValue : testValue);
    }

    /// <summary>Computes the standard deviation over a sequence of double values.</summary>
    /// <param name="source">Source data sample.</param>
    /// <returns>The standard deviation of the sequence.</returns>
    /// <exception cref="ArgumentNullException">source is null</exception>
    public static double StandardDeviation(this IEnumerable<double> source)
    {
        if (source == null) throw new ArgumentNullException("source", "source is null");

        double sampleAverage = source.Average();
        double totalVariance = 0.0D;
        double dataPointDeviation;
        int sampleCount = 0;

        foreach (double item in source)
        {
            dataPointDeviation = item - sampleAverage;
            totalVariance += dataPointDeviation * dataPointDeviation;
            sampleCount++;
        }

        if (sampleCount > 0)
            return System.Math.Sqrt(totalVariance / sampleCount);
        else
            return 0.0D;
    }

    /// <summary>Computes the standard deviation over a sequence of decimal values.</summary>
    /// <param name="source">Source data sample.</param>
    /// <returns>The standard deviation of the sequence.</returns>
    /// <exception cref="ArgumentNullException">source is null</exception>
    public static decimal StandardDeviation(this IEnumerable<decimal> source)
    {
        if (source == null) throw new ArgumentNullException("source", "source is null");

        decimal sampleAverage = source.Average();
        decimal totalVariance = 0;
        decimal dataPointDeviation;
        int sampleCount = 0;

        foreach (decimal item in source)
        {
            dataPointDeviation = item - sampleAverage;
            totalVariance += dataPointDeviation * dataPointDeviation;
            sampleCount++;
        }

        if (sampleCount > 0)
            return (decimal)System.Math.Sqrt((double)(totalVariance / sampleCount));
        else
            return 0;
    }

    /// <summary>Computes the standard deviation over a sequence of float values.</summary>
    /// <param name="source">Source data sample.</param>
    /// <returns>The standard deviation of the sequence.</returns>
    /// <exception cref="ArgumentNullException">source is null</exception>
    public static float StandardDeviation(this IEnumerable<float> source)
    {
        if (source == null) throw new ArgumentNullException("source", "source is null");

        float sampleAverage = source.Average();
        float totalVariance = 0.0F;
        float dataPointDeviation;
        int sampleCount = 0;

        foreach (float item in source)
        {
            dataPointDeviation = item - sampleAverage;
            totalVariance += dataPointDeviation * dataPointDeviation;
            sampleCount++;
        }

        if (sampleCount > 0)
            return (float)System.Math.Sqrt((double)(totalVariance / sampleCount));
        else
            return 0.0F;
    }

    #endregion
}