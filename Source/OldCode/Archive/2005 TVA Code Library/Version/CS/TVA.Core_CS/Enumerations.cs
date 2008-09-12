//*******************************************************************************************************
//  Enumerations.vb - Global enumerations for this namespace
//  Copyright © 2005 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  04/09/2007 - Pinal C. Patel
//      Generated original version of source code.
//  09/08/2008 - J. Ritchie Carroll
//      Converted to C#.
//
//*******************************************************************************************************

/// <summary>
/// Specifies the type of the application.
/// </summary>
namespace TVA
{
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

    /// <summary>Time intervals enumeration used by BaselinedTimestamp function.</summary>
    public enum BaselineTimeInterval
    {
        /// <summary>Baseline timestamp to the second (i.e., starting at zero milliseconds).</summary>
        Second,
        /// <summary>Baseline timestamp to the minute (i.e., starting at zero seconds and milliseconds).</summary>
        Minute,
        /// <summary>Baseline timestamp to the hour (i.e., starting at zero minutes, seconds and milliseconds).</summary>
        Hour,
        /// <summary>Baseline timestamp to the day (i.e., starting at zero hours, minutes, seconds and
        /// milliseconds).</summary>
        Day,
        /// <summary>Baseline timestamp to the month (i.e., starting at day one, zero hours, minutes, seconds
        /// and milliseconds).</summary>
        Month,
        /// <summary>Baseline timestamp to the year (i.e., starting at month one, day one, zero hours, minutes,
        /// seconds and milliseconds).</summary>
        Year
    }

    /// <summary>
    /// The various parts of System.DateTime type.
    /// </summary>
    public enum DateTimePart
    {
        /// <summary>
        /// Minute part.
        /// </summary>
        Minute,
        /// <summary>
        /// Hour part.
        /// </summary>
        Hour,
        /// <summary>
        /// Day part.
        /// </summary>
        Day,
        /// <summary>
        /// Month part.
        /// </summary>
        Month,
        /// <summary>
        /// Day of week part.
        /// </summary>
        DayOfWeek
    }
}