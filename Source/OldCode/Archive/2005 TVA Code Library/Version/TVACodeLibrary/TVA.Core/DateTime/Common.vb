'*******************************************************************************************************
'  TVA.DateTime.Common.vb - Common Date/Time Functions
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  02/23/2003 - J. Ritchie Carroll
'       Generated original version of source code.
'  06/10/2004 - J. Ritchie Carroll
'       Added SecondsToText overload to allow custom time names, e.g., 1 Min 2 Secs.
'  01/05/2005 - J. Ritchie Carroll
'       Added BaselinedTimestamp function.
'  12/21/2005 - J. Ritchie Carroll
'       Migrated 2.0 version of source code from 1.1 source (TVA.Shared.DateTime).
'  08/28/2006 - J. Ritchie Carroll
'       Added TimeIsValid, LocalTimeIsValid and UtcTimeIsValid functions.
'  09/15/2006 - J. Ritchie Carroll
'       Updated BaselinedTimestamp function to support multiple time intervals.
'  09/18/2006 - J. Ritchie Carroll
'       Added TicksBeyondSecond function to support high-resolution timestamp intervals.
'  07/17/2007 - J. Ritchie Carroll
'       Exposed TicksPerSecond as public shared constant.
'  08/31/2007 - Darrell Zuercher
'       Edited code comments.
'
'*******************************************************************************************************

Imports System.Text

Namespace DateTime

    ''' <summary>Defines common global functions related to Date/Time manipulation.</summary>
    Public NotInheritable Class Common

        ''' <summary>Number of 100-nanosecond ticks in one second.</summary>
        Public Const TicksPerSecond As Long = 10000000L

        ''' <summary>Standard time names used by SecondsToText function.</summary>
        Private Shared m_standardTimeNames As String() = New String() {"Year", "Years", "Day", "Days", "Hour", "Hours", "Minute", "Minutes", "Second", "Seconds", "Less Than 60 Seconds", "0 Seconds"}

        ''' <summary>Standard time names, without seconds, used by SecondsToText function.</summary>
        Private Shared m_standardTimeNamesWithoutSeconds As String() = New String() {"Year", "Years", "Day", "Days", "Hour", "Hours", "Minute", "Minutes", "Second", "Seconds", "Less Than 1 Minute", "0 Minutes"}

        ' We define a few common timezones for convenience.
        Private Shared m_universalTimeZone As Win32TimeZone
        Private Shared m_easternTimeZone As Win32TimeZone
        Private Shared m_centralTimeZone As Win32TimeZone
        Private Shared m_mountainTimeZone As Win32TimeZone
        Private Shared m_pacificTimeZone As Win32TimeZone

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        ''' <summary>Converts 100-nanosecond tick intervals to seconds.</summary>
        Public Shared ReadOnly Property TicksToSeconds(ByVal ticks As Long) As Double
            Get
                Return ticks / TicksPerSecond
            End Get
        End Property

        ''' <summary>Converts seconds to 100-nanosecond tick intervals.</summary>
        Public Shared ReadOnly Property SecondsToTicks(ByVal seconds As Double) As Long
            Get
                Return Convert.ToInt64(seconds * TicksPerSecond)
            End Get
        End Property

        ''' <summary>Determines if the specified UTC time is valid, by comparing it to the system clock.</summary>
        ''' <param name="utcTime">UTC time to test for validity.</param>
        ''' <param name="lagTime">The allowed lag time, in seconds, before assuming time is too old to be valid.</param>
        ''' <param name="leadTime">The allowed lead time, in seconds, before assuming time is too advanced to be 
        ''' valid.</param>
        ''' <returns>True, if time is within the specified range.</returns>
        ''' <remarks>
        ''' <para>Time is considered valid if it exists within the specified lag time/lead time range of current 
        ''' time.</para>
        ''' <para>Note that lag time and lead time must be greater than zero, but can be set to sub-second 
        ''' intervals.</para>
        ''' </remarks>
        ''' <exception cref="ArgumentOutOfRangeException">LagTime and LeadTime must be greater than zero, but can 
        ''' be less than one.</exception>
        Public Shared Function UtcTimeIsValid(ByVal utcTime As Date, ByVal lagTime As Double, ByVal leadTime As Double) As Boolean

            Return UtcTimeIsValid(utcTime.Ticks, lagTime, leadTime)

        End Function

        ''' <summary>Determines if the specified UTC time ticks are valid, by comparing them to the system clock.</summary>
        ''' <param name="utcTicks">Ticks of time to test for validity.</param>
        ''' <param name="lagTime">The allowed lag time, in seconds, before assuming time is too old to be valid.</param>
        ''' <param name="leadTime">The allowed lead time, in seconds, before assuming time is too advanced to be 
        ''' valid.</param>
        ''' <returns>True, if time is within the specified range.</returns>
        ''' <remarks>
        ''' <para>Time is considered valid if it exists within the specified lag time/lead time range of current 
        ''' time.</para>
        ''' <para>Note that lag time and lead time must be greater than zero, but can be set to sub-second 
        ''' intervals.</para>
        ''' </remarks>
        ''' <exception cref="ArgumentOutOfRangeException">LagTime and LeadTime must be greater than zero, but can 
        ''' be less than one.</exception>
        Public Shared Function UtcTimeIsValid(ByVal utcTicks As Long, ByVal lagTime As Double, ByVal leadTime As Double) As Boolean

            Return TimeIsValid(Date.UtcNow.Ticks, utcTicks, lagTime, leadTime)

        End Function

        ''' <summary>Determines if the specified local time is valid, by comparing it to the system clock.</summary>
        ''' <param name="localTime">Time to test for validity.</param>
        ''' <param name="lagTime">The allowed lag time, in seconds, before assuming time is too old to be valid.</param>
        ''' <param name="leadTime">The allowed lead time, in seconds, before assuming time is too advanced to be 
        ''' valid.</param>
        ''' <returns>True, if time is within the specified range.</returns>
        ''' <remarks>
        ''' <para>Time is considered valid if it exists within the specified lag time/lead time range of current 
        ''' time.</para>
        ''' <para>Note that lag time and lead time must be greater than zero, but can be set to sub-second 
        ''' intervals.</para>
        ''' </remarks>
        ''' <exception cref="ArgumentOutOfRangeException">LagTime and LeadTime must be greater than zero, but can 
        ''' be less than one.</exception>
        Public Shared Function LocalTimeIsValid(ByVal localTime As Date, ByVal lagTime As Double, ByVal leadTime As Double) As Boolean

            Return LocalTimeIsValid(localTime.Ticks, lagTime, leadTime)

        End Function

        ''' <summary>Determines if the specified local time ticks are valid, by comparing them to the system clock.</summary>
        ''' <param name="localTicks">Ticks of time to test for validity.</param>
        ''' <param name="lagTime">The allowed lag time, in seconds, before assuming time is too old to be valid.</param>
        ''' <param name="leadTime">The allowed lead time, in seconds, before assuming time is too advanced to be 
        ''' valid.</param>
        ''' <returns>True, if time is within the specified range.</returns>
        ''' <remarks>
        ''' <para>Time is considered valid if it exists within the specified lag time/lead time range of current 
        ''' time.</para>
        ''' <para>Note that lag time and lead time must be greater than zero, but can be set to sub-second 
        ''' intervals.</para>
        ''' </remarks>
        ''' <exception cref="ArgumentOutOfRangeException">LagTime and LeadTime must be greater than zero, but can 
        ''' be less than one.</exception>
        Public Shared Function LocalTimeIsValid(ByVal localTicks As Long, ByVal lagTime As Double, ByVal leadTime As Double) As Boolean

            Return TimeIsValid(Date.Now.Ticks, localTicks, lagTime, leadTime)

        End Function

        ''' <summary>Determines if time is valid, by comparing it to the specified current time.</summary>
        ''' <param name="currentTime">Specified current time (e.g., could be Date.Now or Date.UtcNow).</param>
        ''' <param name="testTime">Time to test for validity.</param>
        ''' <param name="lagTime">The allowed lag time, in seconds, before assuming time is too old to be valid.</param>
        ''' <param name="leadTime">The allowed lead time, in seconds, before assuming time is too advanced to be 
        ''' valid.</param>
        ''' <returns>True, if time is within the specified range.</returns>
        ''' <remarks>
        ''' <para>Time is considered valid if it exists within the specified lag time/lead time range of current 
        ''' time.</para>
        ''' <para>Note that lag time and lead time must be greater than zero, but can be set to sub-second 
        ''' intervals.</para>
        ''' </remarks>
        ''' <exception cref="ArgumentOutOfRangeException">LagTime and LeadTime must be greater than zero, but can 
        ''' be less than one.</exception>
        Public Shared Function TimeIsValid(ByVal currentTime As Date, ByVal testTime As Date, ByVal lagTime As Double, ByVal leadTime As Double) As Boolean

            Return TimeIsValid(currentTime.Ticks, testTime.Ticks, lagTime, leadTime)

        End Function

        ''' <summary>Determines if time is valid, by comparing it to the specified current time.</summary>
        ''' <param name="currentTicks">Specified ticks of current time (e.g., could be Date.Now.Ticks or 
        ''' Date.UtcNow.Ticks).</param>
        ''' <param name="testTicks">Ticks of time to test for validity.</param>
        ''' <param name="lagTime">The allowed lag time, in seconds, before assuming time is too old to be valid.</param>
        ''' <param name="leadTime">The allowed lead time, in seconds, before assuming time is too advanced to be 
        ''' valid.</param>
        ''' <returns>True, if time is within the specified range.</returns>
        ''' <remarks>
        ''' <para>Time is considered valid if it exists within the specified lag time/lead time range of current 
        ''' time.</para>
        ''' <para>Note that lag time and lead time must be greater than zero, but can be set to sub-second 
        ''' intervals.</para>
        ''' </remarks>
        ''' <exception cref="ArgumentOutOfRangeException">LagTime and LeadTime must be greater than zero, but can 
        ''' be less than one.</exception>
        Public Shared Function TimeIsValid(ByVal currentTicks As Long, ByVal testTicks As Long, ByVal lagTime As Double, ByVal leadTime As Double) As Boolean

            If lagTime <= 0 Then Throw New ArgumentOutOfRangeException("lagTime", "lagTime must be greater than zero, but it can be less than one")
            If leadTime <= 0 Then Throw New ArgumentOutOfRangeException("leadTime", "leadTime must be greater than zero, but it can be less than one")

            Dim distance As Double = TicksToSeconds(currentTicks - testTicks)
            Return (distance >= -leadTime AndAlso distance <= lagTime)

        End Function

        ''' <summary>Gets  the number of seconds in the local timezone, including fractional seconds, that have 
        ''' elapsed since 12:00:00 midnight, January 1, 0001.</summary>
        Public Shared ReadOnly Property SystemTimer() As Double
            Get
                Return TicksToSeconds(Date.Now.Ticks)
            End Get
        End Property

        ''' <summary>Gets the number of seconds in the universally coordinated timezone, including fractional 
        ''' seconds, that have elapsed since 12:00:00 midnight, January 1, 0001.</summary>
        Public Shared ReadOnly Property UtcSystemTimer() As Double
            Get
                Return TicksToSeconds(Date.UtcNow.Ticks)
            End Get
        End Property

        ''' <summary>Gets the distance, in ticks, beyond the top of the timestamp second.</summary>
        ''' <param name="ticks">Ticks of timestamp to evaluate.</param>
        ''' <returns>Timestamp's tick distance from the top of the second.</returns>
        Public Shared ReadOnly Property TicksBeyondSecond(ByVal ticks As Long) As Long
            Get
                Return ticks - BaselinedTimestamp(New Date(ticks), BaselineTimeInterval.Second).Ticks
            End Get
        End Property

        ''' <summary>Gets the distance, in ticks, beyond the top of the timestamp second.</summary>
        ''' <param name="timestamp">Timestamp to evaluate.</param>
        ''' <returns>Timestamp's tick distance from the top of the second.</returns>
        Public Shared ReadOnly Property TicksBeyondSecond(ByVal timestamp As Date) As Long
            Get
                Return timestamp.Ticks - BaselinedTimestamp(timestamp, BaselineTimeInterval.Second).Ticks
            End Get
        End Property

        ''' <summary>Removes any milliseconds from a timestamp value, to baseline the time at the bottom of the 
        ''' second.</summary>
        ''' <param name="ticks">Ticks of timestamp to baseline.</param>
        ''' <param name="baselineTo">Time interval to which timestamp should be baselined.</param>
        Public Shared Function BaselinedTimestamp(ByVal ticks As Long, ByVal baselineTo As BaselineTimeInterval) As Date

            Return BaselinedTimestamp(New Date(ticks), baselineTo)

        End Function

        ''' <summary>Creates a baselined timestamp which begins at the specified time interval.</summary>
        ''' <param name="timestamp">Timestamp to baseline.</param>
        ''' <param name="baselineTo">Time interval to which timestamp should be baselined.</param>
        ''' <returns>Baselined timestamp which begins at the specified time interval.</returns>
        ''' <remarks>
        ''' <para>Baselining to the second would return the timestamp starting at zero milliseconds.</para>
        ''' <para>Baselining to the minute would return the timestamp starting at zero seconds and milliseconds.</para>
        ''' <para>Baselining to the hour would return the timestamp starting at zero minutes, seconds and 
        ''' milliseconds.</para>
        ''' <para>Baselining to the day would return the timestamp starting at zero hours, minutes, seconds and 
        ''' milliseconds.</para>
        ''' <para>Baselining to the month would return the timestamp starting at day one, zero hours, minutes, 
        ''' seconds and milliseconds.</para>
        ''' <para>Baselining to the year would return the timestamp starting at month one, day one, zero hours, 
        ''' minutes, seconds and milliseconds.</para>
        ''' </remarks>
        Public Shared Function BaselinedTimestamp(ByVal timestamp As Date, ByVal baselineTo As BaselineTimeInterval) As Date

            With timestamp
                Select Case baselineTo
                    Case BaselineTimeInterval.Second
                        Return New Date(.Year, .Month, .Day, .Hour, .Minute, .Second, 0)
                    Case BaselineTimeInterval.Minute
                        Return New Date(.Year, .Month, .Day, .Hour, .Minute, 0, 0)
                    Case BaselineTimeInterval.Hour
                        Return New Date(.Year, .Month, .Day, .Hour, 0, 0, 0)
                    Case BaselineTimeInterval.Day
                        Return New Date(.Year, .Month, .Day, 0, 0, 0, 0)
                    Case BaselineTimeInterval.Month
                        Return New Date(.Year, .Month, 1, 0, 0, 0, 0)
                    Case BaselineTimeInterval.Year
                        Return New Date(.Year, 1, 1, 0, 0, 0, 0)
                    Case Else
                        Return timestamp
                End Select
            End With

        End Function

        ''' <summary>Turns the given number of seconds into textual representation of years, days, hours, minutes 
        ''' and whole integer seconds.</summary>
        ''' <param name="seconds">Seconds to be converted.</param>
        Public Shared Function SecondsToText(ByVal seconds As Double) As String

            Return SecondsToText(seconds, 0)

        End Function

        ''' <summary>Turns the given number of seconds into textual representation of years, days, hours, minutes 
        ''' and seconds.</summary>
        ''' <param name="seconds">Seconds to be converted.</param>
        ''' <param name="secondPrecision">Number of fractional digits to display for seconds.</param>
        ''' <remarks>Set second precision to -1 to suppress seconds display.</remarks>
        Public Shared Function SecondsToText(ByVal seconds As Double, ByVal secondPrecision As Integer) As String

            If secondPrecision < 0 Then
                Return SecondsToText(seconds, secondPrecision, m_standardTimeNamesWithoutSeconds)
            Else
                Return SecondsToText(seconds, secondPrecision, m_standardTimeNames)
            End If

        End Function

        ''' <summary>Turns the given number of seconds into textual representation of years, days, hours, minutes 
        ''' and seconds given string array of time names. Need one for each TimeName enum item.</summary>
        ''' <param name="seconds">Seconds to be converted.</param>
        ''' <param name="secondPrecision">Number of fractional digits to display for seconds.</param>
        ''' <param name="timeNames">Time names array to use during textal conversion.</param>
        ''' <remarks>
        ''' <para>Set second precision to -1 to suppress seconds display.</para>
        ''' <para>Time names array needs one string entry per element in <see cref="TimeName">TimeName</see> 
        ''' enumeration.</para>
        ''' <para>Example timeNames array: "Year", "Years", "Day", "Days", "Hour", "Hours", "Minute", "Minutes", 
        ''' "Second", "Seconds", "Less Than 60 Seconds", "0 Seconds".</para>
        ''' </remarks>
        Public Shared Function SecondsToText(ByVal seconds As Double, ByVal secondPrecision As Integer, ByVal timeNames As String()) As String

            With New StringBuilder
                Dim years As Integer    ' 1 year   = 365.2425 days or 31556952 seconds
                Dim days As Integer     ' 1 day    = 86400 seconds
                Dim hours As Integer    ' 1 hour   = 3600 seconds
                Dim minutes As Integer  ' 1 minute = 60 seconds

                ' checks if number of seconds ranges in years.
                years = seconds \ 31556952

                If years >= 1 Then
                    ' Removes whole years from remaining seconds.
                    seconds = seconds - years * 31556952

                    ' Appends textual representation of years.
                    .Append(years)
                    .Append(" "c)
                    If years = 1 Then
                        .Append(timeNames(TimeName.Year))
                    Else
                        .Append(timeNames(TimeName.Years))
                    End If
                End If

                ' Checks if remaining number of seconds ranges in days.
                days = seconds \ 86400
                If days >= 1 Then
                    ' Removes whole days from remaining seconds.
                    seconds = seconds - days * 86400

                    ' Appends textual representation of days.
                    .Append(" "c)
                    .Append(days)
                    .Append(" "c)
                    If days = 1 Then
                        .Append(timeNames(TimeName.Day))
                    Else
                        .Append(timeNames(TimeName.Days))
                    End If
                End If

                ' Checks if remaining number of seconds ranges in hours.
                hours = seconds \ 3600
                If hours >= 1 Then
                    ' Removes whole hours from remaining seconds.
                    seconds = seconds - hours * 3600

                    ' Appends textual representation of hours.
                    .Append(" "c)
                    .Append(hours)
                    .Append(" "c)
                    If hours = 1 Then
                        .Append(timeNames(TimeName.Hour))
                    Else
                        .Append(timeNames(TimeName.Hours))
                    End If
                End If

                ' Checks if remaining number of seconds ranges in minutes.
                minutes = seconds \ 60
                If minutes >= 1 Then
                    ' Removes whole minutes from remaining seconds.
                    seconds = seconds - minutes * 60

                    ' Appends textual representation of minutes.
                    .Append(" "c)
                    .Append(minutes)
                    .Append(" "c)
                    If minutes = 1 Then
                        .Append(timeNames(TimeName.Minute))
                    Else
                        .Append(timeNames(TimeName.Minutes))
                    End If
                End If

                ' Handles remaining seconds.
                If secondPrecision = 0 Then
                    ' No fractional seconds requested. Rounds seconds to nearest integer.
                    Dim wholeSeconds As Integer = Convert.ToInt32(System.Math.Round(seconds))

                    If wholeSeconds > 0 Then
                        ' Appends textual representation of whole seconds.
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
                    ' Handles fractional seconds request.
                    If seconds > 0 Then
                        If secondPrecision < 0 Then
                            ' If second display has been disabled and less than 60 seconds remain, we still need 
                            ' to show something.
                            If .Length = 0 Then .Append(timeNames(TimeName.LessThan60Seconds))
                        Else
                            ' Appends textual representation of fractional seconds.
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

                ' Handles zero seconds display.
                If .Length = 0 Then .Append(timeNames(TimeName.NoSeconds))

                Return .ToString.Trim()
            End With

        End Function

        ' JRC - These functions were added to make time zone management classes easier to use.

        ''' <summary>Returns the specified Win32 time zone, using its standard name.</summary>
        ''' <param name="standardName">Standard name for desired Win32 time zone.</param>
        Public Shared Function GetWin32TimeZone(ByVal standardName As String) As Win32TimeZone

            Return GetWin32TimeZone(standardName, TimeZoneName.StandardName)

        End Function

        ''' <summary>Returns the specified Win32 time zone, using specified name.</summary>
        ''' <param name="name">Value of name used for time zone lookup.</param>
        ''' <param name="lookupBy">Type of name used for time zone lookup.</param>
        Public Shared Function GetWin32TimeZone(ByVal name As String, ByVal lookupBy As TimeZoneName) As Win32TimeZone

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

        ''' <summary>Gets Universally Coordinated Time Zone (a.k.a., Greenwich Meridian Time Zone).</summary>
        Public Shared ReadOnly Property UniversalTimeZone() As Win32TimeZone
            Get
                If m_universalTimeZone Is Nothing Then m_universalTimeZone = GetWin32TimeZone("GMT Standard Time")
                Return m_universalTimeZone
            End Get
        End Property

        ''' <summary>Gets Eastern Time Zone.</summary>
        Public Shared ReadOnly Property EasternTimeZone() As Win32TimeZone
            Get
                If m_easternTimeZone Is Nothing Then m_easternTimeZone = GetWin32TimeZone("Eastern Standard Time")
                Return m_easternTimeZone
            End Get
        End Property

        ''' <summary>Gets Central Time Zone.</summary>
        Public Shared ReadOnly Property CentralTimeZone() As Win32TimeZone
            Get
                If m_centralTimeZone Is Nothing Then m_centralTimeZone = GetWin32TimeZone("Central Standard Time")
                Return m_centralTimeZone
            End Get
        End Property

        ''' <summary>Gets Mountain Time Zone.</summary>
        Public Shared ReadOnly Property MountainTimeZone() As Win32TimeZone
            Get
                If m_mountainTimeZone Is Nothing Then m_mountainTimeZone = GetWin32TimeZone("Mountain Standard Time")
                Return m_mountainTimeZone
            End Get
        End Property

        ''' <summary>Gets Pacific Standard Time Zone.</summary>
        Public Shared ReadOnly Property PacificTimeZone() As Win32TimeZone
            Get
                If m_pacificTimeZone Is Nothing Then m_pacificTimeZone = GetWin32TimeZone("Pacific Standard Time")
                Return m_pacificTimeZone
            End Get
        End Property

        ''' <summary>Converts given local time to Eastern time.</summary>
        ''' <param name="localTimestamp">Timestamp in local time to be converted to Eastern time.</param>
        ''' <returns>
        ''' <para>Timestamp in Eastern time.</para>
        ''' </returns>
        Public Shared Function LocalTimeToEasternTime(ByVal localTimeStamp As Date) As Date

            Return LocalTimeTo(localTimeStamp, EasternTimeZone)

        End Function

        ''' <summary>Converts given local time to Central time.</summary>
        ''' <param name="localTimestamp">Timestamp in local time to be converted to Central time.</param>
        ''' <returns>
        ''' <para>Timestamp in Central time.</para>
        ''' </returns>
        Public Shared Function LocalTimeToCentralTime(ByVal localTimeStamp As Date) As Date

            Return LocalTimeTo(localTimeStamp, CentralTimeZone)

        End Function

        ''' <summary>Converts given local time to Mountain time.</summary>
        ''' <param name="localTimestamp">Timestamp in local time to be converted to Mountain time.</param>
        ''' <returns>
        ''' <para>Timestamp in Mountain time.</para>
        ''' </returns>
        Public Shared Function LocalTimeToMountainTime(ByVal localTimeStamp As Date) As Date

            Return LocalTimeTo(localTimeStamp, MountainTimeZone)

        End Function

        ''' <summary>Converts given local time to Pacific time.</summary>
        ''' <param name="localTimestamp">Timestamp in local time to be converted to Pacific time.</param>
        ''' <returns>
        ''' <para>Timestamp in Pacific time.</para>
        ''' </returns>
        Public Shared Function LocalTimeToPacificTime(ByVal localTimeStamp As Date) As Date

            Return LocalTimeTo(localTimeStamp, PacificTimeZone)

        End Function

        ''' <summary>Converts given local time to Universally Coordinated Time (a.k.a., Greenwich Meridian Time).</summary>
        ''' <remarks>This function is only provided for the sake of completeness. All it does is call the 
        ''' "ToUniversalTime" property on the given timestamp.</remarks>
        ''' <param name="localTimestamp">Timestamp in local time to be converted to Universal time.</param>
        ''' <returns>
        ''' <para>Timestamp in UniversalTime (a.k.a., GMT).</para>
        ''' </returns>
        Public Shared Function LocalTimeToUniversalTime(ByVal localTimestamp As Date) As Date

            Return localTimestamp.ToUniversalTime()

        End Function

        ''' <summary>Converts given local time to time in specified time zone.</summary>
        ''' <param name="localTimestamp">Timestamp in local time to be converted to time in specified time zone.</param>
        ''' <param name="destinationTimeZoneStandardName">Standard name of desired end time zone for given 
        ''' timestamp.</param>
        ''' <returns>
        ''' <para>Timestamp in specified time zone.</para>
        ''' </returns>
        Public Shared Function LocalTimeTo(ByVal localTimestamp As Date, ByVal destinationTimeZoneStandardName As String) As Date

            Return LocalTimeTo(localTimestamp, GetWin32TimeZone(destinationTimeZoneStandardName))

        End Function

        ''' <summary>Converts given local time to time in specified time zone.</summary>
        ''' <param name="localTimestamp">Timestamp in local time to be converted to time in specified time zone.</param>
        ''' <param name="destinationTimeZone">Desired end time zone for given timestamp.</param>
        ''' <returns>
        ''' <para>Timestamp in specified time zone.</para>
        ''' </returns>
        Public Shared Function LocalTimeTo(ByVal localTimestamp As Date, ByVal destinationTimeZone As Win32TimeZone) As Date

            Dim destOffset As Double

            ' Calculates exact UTC offset of destination time zone in hours.
            With destinationTimeZone.GetUtcOffset(localTimestamp)
                destOffset = .Hours + .Minutes / 60
            End With

            Return localTimestamp.ToUniversalTime().AddHours(destOffset)

        End Function

        ''' <summary>
        ''' Converts the specified Universally Coordinated Time timestamp to Eastern time timestamp.
        ''' </summary>
        ''' <param name="universalTimestamp">The Universally Coordinated Time timestamp that is to be converted.</param>
        ''' <returns>The timestamp in Eastern time.</returns>
        Public Shared Function UniversalTimeToEasternTime(ByVal universalTimestamp) As Date

            Return UniversalTimeTo(universalTimestamp, EasternTimeZone)

        End Function

        ''' <summary>
        ''' Converts the specified Universally Coordinated Time timestamp to Central time timestamp.
        ''' </summary>
        ''' <param name="universalTimestamp">The Universally Coordinated Time timestamp that is to be converted.</param>
        ''' <returns>The timestamp in Central time.</returns>
        Public Shared Function UniversalTimeToCentralTime(ByVal universalTimestamp) As Date

            Return UniversalTimeTo(universalTimestamp, CentralTimeZone)

        End Function

        ''' <summary>
        ''' Converts the specified Universally Coordinated Time timestamp to Mountain time timestamp.
        ''' </summary>
        ''' <param name="universalTimestamp">The Universally Coordinated Time timestamp that is to be converted.</param>
        ''' <returns>The timestamp in Mountain time.</returns>
        Public Shared Function UniversalTimeToMountainTime(ByVal universalTimestamp) As Date

            Return UniversalTimeTo(universalTimestamp, MountainTimeZone)

        End Function

        ''' <summary>
        ''' Converts the specified Universally Coordinated Time timestamp to Pacific time timestamp.
        ''' </summary>
        ''' <param name="universalTimestamp">The Universally Coordinated Time timestamp that is to be converted.</param>
        ''' <returns>The timestamp in Pacific time.</returns>
        Public Shared Function UniversalTimeToPacificTime(ByVal universalTimestamp) As Date

            Return UniversalTimeTo(universalTimestamp, PacificTimeZone)

        End Function

        ''' <summary>
        ''' Converts the specified Universally Coordinated Time timestamp to timestamp in specified time zone.
        ''' </summary>
        ''' <param name="universalTimestamp">The Universally Coordinated Time timestamp that is to be converted.</param>
        ''' <param name="destinationTimeZoneStandardName">The time zone standard name to which the Universally 
        ''' Coordinated Time timestamp is to be converted to.</param>
        ''' <returns>The timestamp in the specified time zone.</returns>
        Public Shared Function UniversalTimeTo(ByVal universalTimestamp As Date, ByVal destinationTimeZoneStandardName As String) As Date

            Return UniversalTimeTo(universalTimestamp, GetWin32TimeZone(destinationTimeZoneStandardName))

        End Function

        ''' <summary>
        ''' Converts the specified Universally Coordinated Time timestamp to timestamp in specified time zone.
        ''' </summary>
        ''' <param name="universalTimestamp">The Universally Coordinated Time timestamp that is to be converted.</param>
        ''' <param name="destinationTimeZone">The time zone to which the Universally Coordinated Time timestamp 
        ''' is to be converted to.</param>
        ''' <returns>The timestamp in the specified time zone.</returns>
        Public Shared Function UniversalTimeTo(ByVal universalTimestamp As Date, ByVal destinationTimeZone As Win32TimeZone) As Date

            Return destinationTimeZone.ToLocalTime(universalTimestamp)

        End Function

        ''' <summary>Converts given timestamp from one time zone to another using standard names for time zones.</summary>
        ''' <param name="timestamp">Timestamp in source time zone to be converted to time in destination time zone.</param>
        ''' <param name="sourceTimeZoneStandardName">Standard name of time zone for given source timestamp.</param>
        ''' <param name="destinationTimeZoneStandardName">Standard name of desired end time zone for given source 
        ''' timestamp.</param>
        ''' <returns>
        ''' <para>Timestamp in destination time zone.</para>
        ''' </returns>
        Public Shared Function TimeZoneToTimeZone(ByVal timestamp As Date, ByVal sourceTimeZoneStandardName As String, ByVal destinationTimeZoneStandardName As String) As Date

            Return TimeZoneToTimeZone(timestamp, GetWin32TimeZone(sourceTimeZoneStandardName), GetWin32TimeZone(destinationTimeZoneStandardName))

        End Function

        ''' <summary>Converts given timestamp from one time zone to another.</summary>
        ''' <param name="timestamp">Timestamp in source time zone to be converted to time in destination time 
        ''' zone.</param>
        ''' <param name="sourceTimeZone">Time zone for given source timestamp.</param>
        ''' <param name="destinationTimeZone">Desired end time zone for given source timestamp.</param>
        ''' <returns>
        ''' <para>Timestamp in destination time zone.</para>
        ''' </returns>
        Public Shared Function TimeZoneToTimeZone(ByVal timestamp As Date, ByVal sourceTimeZone As Win32TimeZone, ByVal destinationTimeZone As Win32TimeZone) As Date

            Dim destOffset As Double

            ' Calculates exact UTC offset of destination time zone in hours.
            With destinationTimeZone.GetUtcOffset(timestamp)
                destOffset = .Hours + .Minutes / 60
            End With

            Return sourceTimeZone.ToUniversalTime(timestamp).AddHours(destOffset)

        End Function

        ''' <summary>Gets the 3-letter month abbreviation for given month number (1-12).</summary>
        ''' <param name="monthNumber">Numeric month number (1-12).</param>
        ''' <remarks>Month abbreviations are English only.</remarks>
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

        ''' <summary>Gets the full month name for given month number (1-12).</summary>
        ''' <param name="monthNumber">Numeric month number (1-12).</param>
        ''' <remarks>Month names are English only.</remarks>
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

    End Class

End Namespace
