'*******************************************************************************************************
'  Tva.DateTime.Common.vb - Common Date/Time Functions
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  02/23/2003 - James R Carroll
'       Original version of source code generated
'  06/10/2004 - James R Carroll
'       Added SecondsToText overload to allow custom time names, e.g., 1 Min 2 Secs
'  01/05/2005 - James R Carroll
'       Added BaselinedTimestamp function
'  12/21/2005 - James R Carroll
'       2.0 version of source code migrated from 1.1 source (TVA.Shared.DateTime)
'
'*******************************************************************************************************

Imports System.Text

Namespace DateTime

    ''' <summary>
    ''' Defines common global functions related to Date/Time manipulation
    ''' </summary>
    Public NotInheritable Class Common

        ''' <summary>
        ''' <para>Time names enumeration used by SecondsToText function</para>
        ''' </summary>
        Public Enum TimeName
            Year
            Years
            Day
            Days
            Hour
            Hours
            Minute
            Minutes
            Second
            Seconds
            LessThan60Seconds
            NoSeconds
        End Enum

        ''' <summary>
        ''' <para>Standard time names used by SecondsToText function</para>
        ''' </summary>
        Private Shared m_standardTimeNames As String() = New String() {"Year", "Years", "Day", "Days", "Hour", "Hours", "Minute", "Minutes", "Second", "Seconds", "Less Than 60 Seconds", "0 Seconds"}

        ''' <summary>
        ''' <para>Standard time names without seconds used by SecondsToText function</para>
        ''' </summary>
        Private Shared m_standardTimeNamesWithoutSeconds As String() = New String() {"Year", "Years", "Day", "Days", "Hour", "Hours", "Minute", "Minutes", "Second", "Seconds", "Less Than 1 Minute", "0 Minutes"}

        ''' <summary>
        ''' <para>Time zone names enumeration used to look up desired time zone in GetWin32TimeZone function</para>
        ''' </summary>
        Public Enum TimeZoneName
            DaylightName
            DaylightAbbreviation
            DisplayName
            StandardName
            StandardAbbreviation
        End Enum

        ' We define a few common timezones for convenience
        Private Shared m_universalTimeZone As Win32TimeZone
        Private Shared m_easternTimeZone As Win32TimeZone
        Private Shared m_centralTimeZone As Win32TimeZone
        Private Shared m_mountainTimeZone As Win32TimeZone
        Private Shared m_pacificTimeZone As Win32TimeZone

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        ''' <summary>
        ''' Converts ticks to seconds
        ''' </summary>
        Public Shared ReadOnly Property TicksToSeconds(ByVal ticks As Long) As Double
            Get
                Return ticks / 10000000L
            End Get
        End Property

        ''' <summary>
        ''' Converts seconds to ticks
        ''' </summary>
        Public Shared ReadOnly Property SecondsToTicks(ByVal seconds As Double) As Long
            Get
                Return seconds * 10000000L
            End Get
        End Property

        ''' <summary>
        ''' Returns the number of seconds in the local timezone, including fractional seconds, since that have elapsed since 12:00:00 midnight, January 1, 0001
        ''' </summary>
        Public Shared ReadOnly Property SystemTimer() As Double
            Get
                Return TicksToSeconds(Date.Now.Ticks)
            End Get
        End Property

        ''' <summary>
        ''' Returns the number of seconds in the Universally coordinated timezone, including fractional seconds, since that have elapsed since 12:00:00 midnight, January 1, 0001
        ''' </summary>
        Public Shared ReadOnly Property UtcSystemTimer() As Double
            Get
                Return TicksToSeconds(Date.UtcNow.Ticks)
            End Get
        End Property

        ''' <summary>
        ''' <para>Removes any milliseconds from a timestamp value to baseline the time at the bottom of the second</para>
        ''' </summary>
        ''' <param name="timestamp">Timestamp to baseline</param>
        Public Shared Function BaselinedTimestamp(ByVal timestamp As Date) As Date

            With timestamp
                Return New Date(.Year, .Month, .Day, .Hour, .Minute, .Second, 0)
            End With

        End Function

        ''' <summary>
        ''' <para>Turns given number of seconds into textual representation of years, days, hours, minutes and whole integer seconds</para>
        ''' </summary>
        ''' <param name="seconds">Seconds to be converted </param>
        Public Shared Function SecondsToText(ByVal seconds As Double) As String

            Return SecondsToText(seconds, 0)

        End Function

        ''' <summary>
        ''' <para>Turns number of given seconds into textual representation of years, days, hours, minutes and seconds</para>
        ''' </summary>
        ''' <remarks>
        ''' <para>Set second precision to -1 to suppress seconds display</para>
        ''' </remarks>
        ''' <param name="seconds">Seconds to be converted </param>
        ''' <param name="secondPrecision">Number of fractional digits to display for seconds</param>
        Public Shared Function SecondsToText(ByVal seconds As Double, ByVal secondPrecision As Integer) As String

            If secondPrecision < 0 Then
                Return SecondsToText(seconds, secondPrecision, m_standardTimeNamesWithoutSeconds)
            Else
                Return SecondsToText(seconds, secondPrecision, m_standardTimeNames)
            End If

        End Function

        ''' <summary>
        ''' <para>Turns number of given seconds into textual representation of years, days, hours, minutes and seconds given string array of time names - need one for each TimeName enum item</para>
        ''' </summary>
        ''' <remarks>
        ''' <para>Set second precision to -1 to suppress seconds display</para>
        ''' <para>Time names array needs one string entry per element in <see cref="TimeName">TimeName</see> enumeration.</para>
        ''' <para>Example timeNames array: "Year", "Years", "Day", "Days", "Hour", "Hours", "Minute", "Minutes", "Second", "Seconds", "Less Than 60 Seconds", "0 Seconds"</para>
        ''' </remarks>
        ''' <param name="seconds">Seconds to be converted</param>
        ''' <param name="secondPrecision">Number of fractional digits to display for seconds</param>
        ''' <param name="timeNames">Time names array to use during textal conversion</param>
        Public Shared Function SecondsToText(ByVal seconds As Double, ByVal secondPrecision As Integer, ByVal timeNames As String()) As String

            With New StringBuilder
                Dim years As Integer    ' 1 year   = 365.2425 days or 31556952 seconds
                Dim days As Integer     ' 1 day    = 86400 seconds
                Dim hours As Integer    ' 1 hour   = 3600 seconds
                Dim minutes As Integer  ' 1 minute = 60 seconds

                ' See if number of seconds ranges in years
                years = seconds \ 31556952

                If years >= 1 Then
                    ' Remove whole years from remaining seconds
                    seconds = seconds - years * 31556952

                    ' Append textual representation of years
                    .Append(years)
                    .Append(" "c)
                    If years = 1 Then
                        .Append(timeNames(TimeName.Year))
                    Else
                        .Append(timeNames(TimeName.Years))
                    End If
                End If

                ' See if remaining number of seconds ranges in days
                days = seconds \ 86400
                If days >= 1 Then
                    ' Remove whole days from remaining seconds
                    seconds = seconds - days * 86400

                    ' Append textual representation of days
                    .Append(" "c)
                    .Append(days)
                    .Append(" "c)
                    If days = 1 Then
                        .Append(timeNames(TimeName.Day))
                    Else
                        .Append(timeNames(TimeName.Days))
                    End If
                End If

                ' See if remaining number of seconds ranges in hours
                hours = seconds \ 3600
                If hours >= 1 Then
                    ' Remove whole hours from remaining seconds
                    seconds = seconds - hours * 3600

                    ' Append textual representation of hours
                    .Append(" "c)
                    .Append(hours)
                    .Append(" "c)
                    If hours = 1 Then
                        .Append(timeNames(TimeName.Hour))
                    Else
                        .Append(timeNames(TimeName.Hours))
                    End If
                End If

                ' See if remaining number of seconds ranges in minutes
                minutes = seconds \ 60
                If minutes >= 1 Then
                    ' Remove whole minutes from remaining seconds
                    seconds = seconds - minutes * 60

                    ' Append textual representation of minutes
                    .Append(" "c)
                    .Append(minutes)
                    .Append(" "c)
                    If minutes = 1 Then
                        .Append(timeNames(TimeName.Minute))
                    Else
                        .Append(timeNames(TimeName.Minutes))
                    End If
                End If

                ' Handle remaining seconds
                If secondPrecision = 0 Then
                    ' No fractonal seconds requested, round seconds to nearest integer
                    Dim wholeSeconds As Integer = Convert.ToInt32(System.Math.Round(seconds))

                    If wholeSeconds > 0 Then
                        ' Append textual representation of whole seconds
                        .Append(" "c)
                        .Append(wholeSeconds)
                        .Append(" "c)
                        If wholeSeconds = 1 Then
                            .Append(timeNames(TimeName.Second))
                        Else
                            .Append(timeNames(TimeName.Seconds))
                        End If
                    End If
                Else
                    ' Handle fractional seconds request
                    If seconds > 0 Then
                        If secondPrecision < 0 Then
                            ' If second display has been disabled and less than 60 seconds remain we still need to show something
                            If .Length = 0 Then .Append(timeNames(TimeName.LessThan60Seconds))
                        Else
                            ' Append textual representation of fractional seconds
                            .Append(" "c)
                            .Append(seconds.ToString("0." & (New String("0", secondPrecision))))
                            .Append(" "c)
                            If seconds = 1 Then
                                .Append(timeNames(TimeName.Second))
                            Else
                                .Append(timeNames(TimeName.Seconds))
                            End If
                        End If
                    End If
                End If

                ' Handle zero seconds display
                If .Length = 0 Then .Append(timeNames(TimeName.NoSeconds))

                Return .ToString.Trim()
            End With

        End Function

        ''' <summary>
        ''' <para>Returns 3 letter month abbreviation for given month number (1-12)</para>
        ''' </summary>
        ''' <remarks>
        ''' <para>Month abbreviations are English only.</para>
        ''' </remarks>
        ''' <param name="monthNumber">Numeric month number (1-12)</param>
        Public Shared ReadOnly Property ShortMonthName(ByVal monthNumber As Integer) As String
            Get
                Select Case monthNumber
                    Case 1
                        Return "Jan"
                    Case 2
                        Return "Feb"
                    Case 3
                        Return "Mar"
                    Case 4
                        Return "Apr"
                    Case 5
                        Return "May"
                    Case 6
                        Return "Jun"
                    Case 7
                        Return "Jul"
                    Case 8
                        Return "Aug"
                    Case 9
                        Return "Sep"
                    Case 10
                        Return "Oct"
                    Case 11
                        Return "Nov"
                    Case 12
                        Return "Dec"
                    Case Else
                        Throw New ArgumentOutOfRangeException("monthNumber", "Invalid month number """ & monthNumber & """ specified - expected a value between 1 and 12")
                End Select
            End Get
        End Property

        ''' <summary>
        ''' <para>Returns full month name for given month number (1-12)</para>
        ''' </summary>
        ''' <remarks>
        ''' <para>Month names are English only.</para>
        ''' </remarks>
        ''' <param name="monthNumber">Numeric month number (1-12)</param>
        Public Shared ReadOnly Property LongMonthName(ByVal monthNumber As Integer) As String
            Get
                Select Case monthNumber
                    Case 1
                        Return "January"
                    Case 2
                        Return "February"
                    Case 3
                        Return "March"
                    Case 4
                        Return "April"
                    Case 5
                        Return "May"
                    Case 6
                        Return "June"
                    Case 7
                        Return "July"
                    Case 8
                        Return "August"
                    Case 9
                        Return "September"
                    Case 10
                        Return "October"
                    Case 11
                        Return "November"
                    Case 12
                        Return "December"
                    Case Else
                        Throw New ArgumentOutOfRangeException("monthNumber", "Invalid month number """ & monthNumber & """ specified - expected a value between 1 and 12")
                End Select
            End Get
        End Property

        ' JRC - These functions added to make time zone management classes easier to use...

        '''<summary>
        ''' Returns the specified Win32 time zone using its standard name
        '''</summary>
        '''<param name="standardName">Standard name for desired Win32 time zone</param>
        Public Shared Function GetWin32TimeZone(ByVal standardName As String) As Win32TimeZone

            Return GetWin32TimeZone(TimeZoneName.StandardName, standardName)

        End Function

        ''' <summary>
        ''' <para>Returns the specified Win32 time zone using specified name</para>
        ''' </summary>
        ''' <param name="lookupBy">Type of name used for time zone lookup</param>
        ''' <param name="name">Value of name used for time zone lookup</param>
        Public Shared Function GetWin32TimeZone(ByVal lookupBy As TimeZoneName, ByVal name As String) As Win32TimeZone

            For Each timeZone As Win32TimeZone In TimeZones.GetTimeZones
                With timeZone
                    Select Case lookupBy
                        Case TimeZoneName.DaylightAbbreviation
                            If String.Compare(.DaylightAbbreviation, name, True) = 0 Then
                                Return timeZone
                            End If
                        Case TimeZoneName.DaylightName
                            If String.Compare(.DaylightName, name, True) = 0 Then
                                Return timeZone
                            End If
                        Case TimeZoneName.DisplayName
                            If String.Compare(.DisplayName, name, True) = 0 Then
                                Return timeZone
                            End If
                        Case TimeZoneName.StandardAbbreviation
                            If String.Compare(.StandardAbbreviation, name, True) = 0 Then
                                Return timeZone
                            End If
                        Case TimeZoneName.StandardName
                            If String.Compare(.StandardName, name, True) = 0 Then
                                Return timeZone
                            End If
                    End Select
                End With
            Next

            Throw New ArgumentException("Windows time zone with " & [Enum].GetName(GetType(TimeZoneName), lookupBy) & " of """ & name & """ was not found!")

        End Function

        ''' <summary>
        ''' <para>Universally Coordinated Time Zone (a.k.a., Greenwich Meridian Time Zone)</para>
        ''' </summary>
        Public Shared ReadOnly Property UniversalTimeZone() As Win32TimeZone
            Get
                If m_universalTimeZone Is Nothing Then m_universalTimeZone = GetWin32TimeZone("GMT Standard Time")
                Return m_universalTimeZone
            End Get
        End Property

        ''' <summary>
        ''' <para>Eastern Time Zone</para>
        ''' </summary>
        Public Shared ReadOnly Property EasternTimeZone() As Win32TimeZone
            Get
                If m_easternTimeZone Is Nothing Then m_easternTimeZone = GetWin32TimeZone("Eastern Standard Time")
                Return m_easternTimeZone
            End Get
        End Property

        ''' <summary>
        ''' <para>Central Time Zone</para>
        ''' </summary>
        Public Shared ReadOnly Property CentralTimeZone() As Win32TimeZone
            Get
                If m_centralTimeZone Is Nothing Then m_centralTimeZone = GetWin32TimeZone("Central Standard Time")
                Return m_centralTimeZone
            End Get
        End Property

        ''' <summary>
        ''' <para>Mountain Time Zone</para>
        ''' </summary>
        Public Shared ReadOnly Property MountainTimeZone() As Win32TimeZone
            Get
                If m_mountainTimeZone Is Nothing Then m_mountainTimeZone = GetWin32TimeZone("Mountain Standard Time")
                Return m_mountainTimeZone
            End Get
        End Property

        ''' <summary>
        ''' <para>Pacific Standard Time Zone</para>
        ''' </summary>
        Public Shared ReadOnly Property PacificTimeZone() As Win32TimeZone
            Get
                If m_pacificTimeZone Is Nothing Then m_pacificTimeZone = GetWin32TimeZone("Pacific Standard Time")
                Return m_pacificTimeZone
            End Get
        End Property

        ''' <summary>
        ''' <para>Converts given local time to Eastern time</para>
        ''' </summary>
        ''' <param name="localTimestamp">Timestamp in local time to be converted to Eastern time</param>
        ''' <returns>
        ''' <para>Timestamp in Eastern time</para>
        ''' </returns>
        Public Shared Function LocalTimeToEasternTime(ByVal localTimeStamp As Date) As Date

            Return LocalTimeTo(localTimeStamp, EasternTimeZone)

        End Function

        ''' <summary>
        ''' <para>Converts given local time to Central time</para>
        ''' </summary>
        ''' <param name="localTimestamp">Timestamp in local time to be converted to Central time</param>
        ''' <returns>
        ''' <para>Timestamp in Central time</para>
        ''' </returns>
        Public Shared Function LocalTimeToCentralTime(ByVal localTimeStamp As Date) As Date

            Return LocalTimeTo(localTimeStamp, CentralTimeZone)

        End Function

        ''' <summary>
        ''' <para>Converts given local time to Mountain time</para>
        ''' </summary>
        ''' <param name="localTimestamp">Timestamp in local time to be converted to Mountain time</param>
        ''' <returns>
        ''' <para>Timestamp in Mountain time</para>
        ''' </returns>
        Public Shared Function LocalTimeToMountainTime(ByVal localTimeStamp As Date) As Date

            Return LocalTimeTo(localTimeStamp, MountainTimeZone)

        End Function

        ''' <summary>
        ''' <para>Converts given local time to Pacific time</para>
        ''' </summary>
        ''' <param name="localTimestamp">Timestamp in local time to be converted to Pacific time</param>
        ''' <returns>
        ''' <para>Timestamp in Pacific time</para>
        ''' </returns>
        Public Shared Function LocalTimeToPacificTime(ByVal localTimeStamp As Date) As Date

            Return LocalTimeTo(localTimeStamp, PacificTimeZone)

        End Function

        ''' <summary>
        ''' <para>Converts given local time to Universally Coordinated Time (a.k.a., Greenwich Meridian Time)</para>
        ''' </summary>
        ''' <remarks>
        ''' <para>This function is only provided for the sake of completeness - all it does is call the "ToUniversalTime" property on the given timestamp.</para>
        ''' </remarks>
        ''' <param name="localTimestamp">Timestamp in local time to be converted to Universal time</param>
        ''' <returns>
        ''' <para>Timestamp in UniversalTime (a.k.a., GMT)</para>
        ''' </returns>
        Public Shared Function LocalTimeToUniversalTime(ByVal localTimestamp As Date) As Date

            Return localTimestamp.ToUniversalTime()

        End Function

        ''' <summary>
        ''' <para>Converts given local time to time in specified time zone</para>
        ''' </summary>
        ''' <param name="localTimestamp">Timestamp in local time to be converted to time in specified time zone</param>
        ''' <param name="destinationTimeZoneStandardName">Standard name of desired end time zone for given timestamp</param>
        ''' <returns>
        ''' <para>Timestamp in specified time zone</para>
        ''' </returns>
        Public Shared Function LocalTimeTo(ByVal localTimestamp As Date, ByVal destinationTimeZoneStandardName As String) As Date

            Return LocalTimeTo(localTimestamp, GetWin32TimeZone(destinationTimeZoneStandardName))

        End Function

        ''' <summary>
        ''' <para>Converts given local time to time in specified time zone</para>
        ''' </summary>
        ''' <param name="localTimestamp">Timestamp in local time to be converted to time in specified time zone</param>
        ''' <param name="destinationTimeZone">Desired end time zone for given timestamp</param>
        ''' <returns>
        ''' <para>Timestamp in specified time zone</para>
        ''' </returns>
        Public Shared Function LocalTimeTo(ByVal localTimestamp As Date, ByVal destinationTimeZone As Win32TimeZone) As Date

            Dim destOffset As Double

            ' Calculate exact UTC offset of destination time zone in hours
            With destinationTimeZone.GetUtcOffset(localTimestamp)
                destOffset = .Hours + .Minutes / 60
            End With

            Return localTimestamp.ToUniversalTime().AddHours(destOffset)

        End Function

        ''' <summary>
        ''' <para>Converts given timestamp from one time zone to another using standard names for time zones</para>
        ''' </summary>
        ''' <param name="timestamp">Timestamp in source time zone to be converted to time in destination time zone</param>
        ''' <param name="sourceTimeZoneStandardName">Standard name of time zone for given source timestamp</param>
        ''' <param name="destinationTimeZoneStandardName">Standard name of desired end time zone for given source timestamp</param>
        ''' <returns>
        ''' <para>Timestamp in destination time zone</para>
        ''' </returns>
        Public Shared Function TimeZoneToTimeZone(ByVal timestamp As Date, ByVal sourceTimeZoneStandardName As String, ByVal destinationTimeZoneStandardName As String) As Date

            Return TimeZoneToTimeZone(timestamp, GetWin32TimeZone(sourceTimeZoneStandardName), GetWin32TimeZone(destinationTimeZoneStandardName))

        End Function

        ''' <summary>
        ''' <para>Converts given timestamp from one time zone to another</para>
        ''' </summary>
        ''' <param name="timestamp">Timestamp in source time zone to be converted to time in destination time zone</param>
        ''' <param name="sourceTimeZone">Time zone for given source timestamp</param>
        ''' <param name="destinationTimeZone">Desired end time zone for given source timestamp</param>
        ''' <returns>
        ''' <para>Timestamp in destination time zone</para>
        ''' </returns>
        Public Shared Function TimeZoneToTimeZone(ByVal timestamp As Date, ByVal sourceTimeZone As Win32TimeZone, ByVal destinationTimeZone As Win32TimeZone) As Date

            Dim destOffset As Double

            ' Calculate exact UTC offset of destination time zone in hours
            With destinationTimeZone.GetUtcOffset(timestamp)
                destOffset = .Hours + .Minutes / 60
            End With

            Return sourceTimeZone.ToUniversalTime(timestamp).AddHours(destOffset)

        End Function

    End Class

End Namespace
