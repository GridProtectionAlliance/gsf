'*******************************************************************************************************
'  Tva.DateTime.Win32TimeZone.vb - Win32 Time Zone Classes
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  06/10/2004 - James R Carroll
'       Integrated external source for Michael R. Brumm's TimeZone management into TVA.Shared.DateTime
'  12/21/2005 - James R Carroll
'       2.0 version of source code imorted from 1.1 source (TVA.Shared.DateTime)
'*******************************************************************************************************

Imports System.Globalization
Imports Microsoft.Win32

Namespace DateTime

    ' *************************************************************************************************************
    '
    ' Classes and code for TimeZone management follow.  Original code was written by Michael R. Brumm.
    ' For updates and more information, visit: http://www.michaelbrumm.com/simpletimezone.html
    ' or contact me@michaelbrumm.com
    '
    ' Integrated into TVA code library on June 10th, 2004
    ' 
    ' *************************************************************************************************************

    ' IMPORTANT:
    ' This class is immutable, and any derived classes
    ' should also be immutable.
    <Serializable()> _
    Public Class DaylightTimeChange


        Private Const NUM_DAYS_IN_WEEK As Int32 = 7

        Private _month As Int32
        Private _dayOfWeek As DayOfWeek
        Private _dayOfWeekIndex As Int32
        Private _timeOfDay As TimeSpan
        ' Constructor without parameters is not allowed.
        Private Sub New()
        End Sub

        ' Constructor allows the definition of a time change
        ' for most time zones using daylight saving time. These
        ' time zones often define the start or end of daylight
        ' saving as "the first Sunday of April, at 2:00am". This
        ' would be constructed as:
        '
        ' New DaylightTimeChange( _
        '   4, _                      ' 4th month: April
        '   DayOfWeek.Sunday, 0, _    ' 1st Sunday
        '   New TimeSpan(2, 0, 0) _   ' at 2:00am
        ' )
        '
        ' "The last Sunday of October, at 2:00am" would be
        ' constructed as:
        '
        ' New DaylightTimeChange( _
        '   10, _                     ' 10th month: October
        '   DayOfWeek.Sunday, 4, _    ' 5th (last) Sunday
        '   New TimeSpan(2, 0, 0) _   ' at 2:00am
        ' )
        '
        Public Sub New( _
          ByVal month As Int32, _
          ByVal dayOfWeek As DayOfWeek, _
          ByVal dayOfWeekIndex As Int32, _
          ByVal timeOfDay As TimeSpan _
        )

            ' Parameter checking
            If ((month < 1) OrElse (month > 12)) Then
                Throw New ArgumentOutOfRangeException("month", month, "The month must be between 1 and 12, inclusive.")
            End If

            If ( _
                (dayOfWeek < dayOfWeek.Sunday) OrElse _
                (dayOfWeek > dayOfWeek.Saturday) _
            ) Then
                Throw New ArgumentOutOfRangeException("dayOfWeek", dayOfWeek, "The day of week must be between Sunday and Saturday.")
            End If

            ' 0 = 1st
            ' 1 = 2nd
            ' 2 = 3rd
            ' 3 = 4th
            ' 4 = 5th (last)
            If ((dayOfWeekIndex < 0) OrElse (dayOfWeekIndex > 4)) Then
                Throw New ArgumentOutOfRangeException("dayOfWeekIndex", dayOfWeekIndex, "The day of week index must be between 0 and 4, inclusive.")
            End If

            If ( _
                (timeOfDay.Ticks < 0) OrElse _
                (timeOfDay.Ticks >= TimeSpan.TicksPerDay) _
            ) Then
                Throw New ArgumentOutOfRangeException("timeOfDay", timeOfDay, "The time of the day must be less than one day, and not negative.")
            End If

            ' Initialize private storage
            _month = month
            _dayOfWeek = dayOfWeek
            _dayOfWeekIndex = dayOfWeekIndex
            _timeOfDay = timeOfDay

        End Sub
        '''<summary>
        ''' <para>
        '''  Returns the time and date of the daylight saving change for a particular year. For example:"the 1st Sunday of April at 2:00am" for the year "2000"  is "2000/04/02 02:00"
        ''' </para>
        ''' </summary>
        ''' <param name="year">Year</param>


        Public Overridable Function GetDate( _
          ByVal year As Int32 _
        ) As System.DateTime

            If ((year < 1) OrElse (year > System.DateTime.MaxValue.Year)) Then
                Throw New ArgumentOutOfRangeException("year")
            End If

            ' Get the first day of the change month for the specified year.
            Dim resultDate As New System.DateTime(year, _month, 1)

            ' Get the first day of the month that falls on the
            ' day of the week for this change.
            If (resultDate.DayOfWeek > _dayOfWeek) Then
                resultDate = resultDate.AddDays(NUM_DAYS_IN_WEEK - (resultDate.DayOfWeek - _dayOfWeek))
            ElseIf (resultDate.DayOfWeek < _dayOfWeek) Then
                resultDate = resultDate.AddDays(_dayOfWeek - resultDate.DayOfWeek)
            End If

            ' Get the nth weekday (3rd Tuesday, for example)
            resultDate = resultDate.AddDays(NUM_DAYS_IN_WEEK * _dayOfWeekIndex)

            ' If the date has passed the month, then go back a week. This allows
            ' the 5th weekday to always be the last weekday.
            While (resultDate.Month > _month)
                resultDate = resultDate.AddDays(-NUM_DAYS_IN_WEEK)
            End While

            ' Add the time of day that daylight saving begins.
            resultDate = resultDate.Add(_timeOfDay)

            ' Return the date and time of the change.
            Return resultDate

        End Function


    End Class

    <Serializable()> _
    Public Class SimpleTimeZone

        Inherits TimeZone


        Private _standardAlways As Boolean
        Private _daylightAlwaysWithinStandard As Boolean
        Private _standardAlwaysWithinDaylight As Boolean

        Private _standardOffset As TimeSpan
        Private _standardName As String
        Private _standardAbbreviation As String

        Private _daylightDelta As TimeSpan
        Private _daylightOffset As TimeSpan
        Private _daylightName As String
        Private _daylightAbbreviation As String
        Private _daylightTimeChangeStart As DaylightTimeChange
        Private _daylightTimeChangeEnd As DaylightTimeChange


        ' Constructor without parameters is not allowed.
        Private Sub New()
        End Sub
        '''<summary>
        ''' <para>
        '''  Constructor for time zone without daylight saving time.
        ''' </para>
        ''' </summary>

        Public Sub New( _
          ByVal standardOffset As TimeSpan, _
          ByVal standardName As String, _
          ByVal standardAbbreviation As String _
        )

            ' Initialize private storage
            _standardAlways = True

            _standardOffset = standardOffset
            _standardName = standardName
            _standardAbbreviation = standardAbbreviation

        End Sub
        '''<summary>
        ''' <para>
        '''  Constructor for time zone with or without daylight saving time.
        ''' </para>
        ''' </summary>

        Public Sub New( _
          ByVal standardOffset As TimeSpan, _
          ByVal standardName As String, _
          ByVal standardAbbreviation As String, _
          ByVal daylightDelta As TimeSpan, _
          ByVal daylightName As String, _
          ByVal daylightAbbreviation As String, _
          ByVal daylightTimeChangeStart As DaylightTimeChange, _
          ByVal daylightTimeChangeEnd As DaylightTimeChange _
        )

            ' Allow non-daylight saving time zones to be created
            ' using this constructor.
            If ( _
              (daylightTimeChangeStart Is Nothing) AndAlso _
              (daylightTimeChangeEnd Is Nothing) _
              ) Then

                ' Initialize private storage
                _standardAlways = True

                _standardOffset = standardOffset
                _standardName = standardName
                _standardAbbreviation = standardAbbreviation

                Exit Sub

            End If

            ' If the time zone has a start OR an end, then it
            ' must have a start AND an end.
            If (daylightTimeChangeStart Is Nothing) Then
                Throw New ArgumentNullException("daylightTimeChangeStart")
            End If

            If (daylightTimeChangeEnd Is Nothing) Then
                Throw New ArgumentNullException("daylightTimeChangeEnd")
            End If

            ' Initialize private storage
            _standardAlways = False

            _standardOffset = standardOffset
            _standardName = standardName
            _standardAbbreviation = standardAbbreviation

            _daylightDelta = daylightDelta
            _daylightOffset = _standardOffset.Add(daylightDelta)
            _daylightName = daylightName
            _daylightAbbreviation = daylightAbbreviation

            ' These referance types are immutable, so they cannot be
            ' changed outside this class' scope, and thus can be
            ' permanently referenced.
            _daylightTimeChangeStart = daylightTimeChangeStart
            _daylightTimeChangeEnd = daylightTimeChangeEnd

        End Sub
        ''' <summary>
        ''' Gets the Standard Name
        ''' </summary>
        ''' <value>
        ''' Standard Name
        ''' </value>
        ''' <remarks>
        ''' Value must be string
        ''' </remarks>

        Public Overrides ReadOnly Property StandardName() As String
            Get
                Return _standardName
            End Get
        End Property

        ''' <summary>
        ''' Gets the Standard Abbreviation
        ''' </summary>
        ''' <value>
        ''' Standard Abbreviation
        ''' </value>
        ''' <remarks>
        ''' Value must be string
        ''' </remarks>
        Public Overridable ReadOnly Property StandardAbbreviation() As String
            Get
                Return _standardAbbreviation
            End Get
        End Property
        ''' <summary>
        ''' Gets the daylight name
        ''' </summary>
        ''' <value>
        ''' daylight name
        ''' </value>
        ''' <remarks>
        ''' Value must be string
        ''' </remarks>

        Public Overrides ReadOnly Property DaylightName() As String
            Get
                Return _daylightName
            End Get
        End Property

        ''' <summary>
        ''' Gets the daylight abbreviation
        ''' </summary>
        ''' <value>
        ''' daylight abbreviation
        ''' </value>
        ''' <remarks>
        ''' Value must be string
        ''' </remarks>
        Public Overridable ReadOnly Property DaylightAbbreviation() As String
            Get
                Return _daylightAbbreviation
            End Get
        End Property
        '''<summary>
        ''' <para>
        ''' The name is dependent on whether the time zone is in daylight
        ''' saving time or not. This method can be ambiguous during
        ''' daylight changes.
        ''' </para>
        ''' </summary>

        Public Overridable Function GetNameLocalTime( _
          ByVal time As System.DateTime _
        ) As String

            If (_standardAlways) Then
                Return _standardName
            ElseIf (IsDaylightSavingTime(time)) Then
                Return _daylightName
            Else
                Return _standardName
            End If

        End Function
        '''<summary>
        ''' <para>
        ''' This function is unambiguous during daylight changes.
        ''' </para>
        ''' </summary>

        Public Overridable Function GetNameUniversalTime( _
          ByVal time As System.DateTime _
        ) As String

            If (IsDaylightSavingTimeUniversalTime(time)) Then
                Return _daylightName
            Else
                Return _standardName
            End If

        End Function

        '''<summary>
        ''' <para>
        ''' The abbreviation is dependant on whether the time zone is in
        ''' daylight saving time or not. This function can be ambiguous during
        ''' daylight changes.
        ''' </para>
        ''' </summary>

        Public Overridable Function GetAbbreviationLocalTime( _
          ByVal time As System.DateTime _
        ) As String

            If (_standardAlways) Then
                Return _standardAbbreviation
            ElseIf (IsDaylightSavingTime(time)) Then
                Return _daylightAbbreviation
            Else
                Return _standardAbbreviation
            End If

        End Function
        '''<summary>
        ''' <para>
        ''' This function is unambiguous during daylight changes.
        ''' </para>
        ''' </summary>

        Public Overridable Function GetAbbreviationUniversalTime( _
          ByVal time As System.DateTime _
        ) As String

            If (IsDaylightSavingTimeUniversalTime(time)) Then
                Return _daylightAbbreviation
            Else
                Return _standardAbbreviation
            End If

        End Function
        '''<summary>
        ''' <para>
        ''' Returns daylight changes 
        ''' </para>
        ''' </summary>
        ''' <param name="year">Year to get the daylight changes</param>

        Public Overrides Function GetDaylightChanges( _
          ByVal year As System.Int32 _
        ) As DaylightTime

            If ((year < 1) OrElse (year > System.DateTime.MaxValue.Year)) Then
                Throw New ArgumentOutOfRangeException("year")
            End If

            If (_standardAlways) Then
                Return Nothing

            Else
                Return New DaylightTime( _
                 _daylightTimeChangeStart.GetDate(year), _
                 _daylightTimeChangeEnd.GetDate(year), _
                 _daylightDelta _
                )
            End If

        End Function
        '''<summary>
        ''' <para>
        '''  This method can be ambiguous during daylight changes.
        ''' </para>
        ''' </summary>

        Public Overloads Overrides Function IsDaylightSavingTime( _
          ByVal time As System.DateTime _
        ) As Boolean

            Return IsDaylightSavingTime(time, False)

        End Function
        '''<summary>
        ''' <para>
        '''  This method is unambiguous during daylight changes.
        ''' </para>
        ''' </summary>

        Public Overridable Function IsDaylightSavingTimeUniversalTime( _
          ByVal time As System.DateTime _
        ) As Boolean

            time = time.Add(_standardOffset)
            Return IsDaylightSavingTime(time, True)

        End Function
        '''<summary>
        ''' <para>
        ''' Return whether the time is within the daylight saving time for this year.
        ''' </para>
        ''' </summary>

        Private Overloads Function IsDaylightSavingTime( _
          ByVal time As System.DateTime, _
          ByVal fromUtcTime As Boolean _
        ) As Boolean

            ' If this time zone is never in daylight saving, then
            ' return false.
            If (_standardAlways) Then
                Return False
            End If

            ' Get the daylight saving time start and end for this
            ' time's year.
            Dim daylightTimes As DaylightTime
            daylightTimes = GetDaylightChanges(time.Year)
            'Return whether the time is within the daylight saving time for this year.

            Return IsDaylightSavingTime(time, daylightTimes, fromUtcTime)

        End Function
        '''<summary>
        ''' <para>
        ''' Return a boolean value if the time is within daylighttime
        ''' </para>
        ''' </summary>
        ''' <param name="time"> Required.Time</param>
        ''' <param name="daylightTimes"> Required.DayLightTime</param>
        ''' <value>
        ''' Must be boolean
        ''' </value>

        Public Overloads Shared Function IsDaylightSavingTime( _
           ByVal time As System.DateTime, _
           ByVal daylightTimes As DaylightTime _
        ) As Boolean

            Return IsDaylightSavingTime(time, daylightTimes, False)

        End Function


        Private Overloads Shared Function IsDaylightSavingTime( _
          ByVal time As System.DateTime, _
          ByVal daylightTimes As DaylightTime, _
          ByVal fromUtcTime As Boolean _
        ) As Boolean


            If (daylightTimes Is Nothing) Then
                Return False
            End If

            Dim daylightStart As System.DateTime
            Dim daylightEnd As System.DateTime
            Dim daylightDelta As TimeSpan
            daylightStart = daylightTimes.Start
            daylightEnd = daylightTimes.End
            daylightDelta = daylightTimes.Delta

            ' If the time came from a utc time, then the delta must be
            ' removed from the end time, because the end of daylight
            ' saving time is described using using a local time (which
            ' is currently in daylight saving time).
            If (fromUtcTime) Then
                daylightEnd = daylightEnd.Subtract(daylightDelta)
            End If

            ' Northern hemisphere (normally)
            ' The daylight saving time of the year falls between the
            ' start and the end dates.
            If (daylightStart < daylightEnd) Then

                ' The daylight saving time of the year falls between the
                ' start and the end dates.
                If ( _
                  (time >= daylightStart) AndAlso _
                  (time < daylightEnd) _
                  ) Then

                    ' If the time was taken from a UTC time, then do not apply
                    ' the backward compatibility.
                    If (fromUtcTime) Then
                        Return True

                        ' Backward compatiblity with .NET Framework TimeZone.
                        ' If the daylight saving delta is positive, then there is a
                        ' period of time which does not exist (between 2am and 3am in
                        ' most daylight saving time zones) at the beginning of the
                        ' daylight saving. This period of non-existant time should be 
                        ' considered standard time (not daylight saving).
                    Else

                        If (daylightDelta.Ticks > 0) Then
                            If (time < (daylightStart.Add(daylightDelta))) Then
                                Return False
                            Else
                                Return True
                            End If
                        Else
                            Return True
                        End If

                    End If

                    ' Otherwise, the time and date is not within daylight
                    ' saving time.
                Else

                    ' If the time was taken from a UTC time, then do not apply
                    ' the backward compatibility.
                    If (fromUtcTime) Then
                        Return False

                        ' Backward compatiblity with .NET Framework TimeZone.
                        ' If the daylight saving delta is negative (which shouldn't
                        ' happen), then there is a period of time which does not exist
                        ' (between 2am and 3am in most daylight saving time zones).
                        ' at the end of daylight saving. This period of
                        ' non-existant time should be considered daylight saving.
                    Else

                        If (daylightDelta.Ticks < 0) Then

                            If ( _
                              (time >= daylightEnd) AndAlso _
                              (time < daylightEnd.Subtract(daylightDelta)) _
                              ) Then
                                Return True
                            Else
                                Return False
                            End If

                        Else
                            Return False
                        End If

                    End If

                End If

                ' Southern hemisphere (normally)
                ' The daylight saving time of the year is after the start,
                ' or before the end, but not between the two dates.
            Else

                ' The daylight saving time of the year is after the start,
                ' or before the end, but not between the two dates.
                If (time >= daylightStart) Then

                    ' If the time was taken from a UTC time, then do not apply
                    ' the backward compatibility.
                    If (fromUtcTime) Then
                        Return True

                        ' Backward compatiblity with .NET Framework TimeZone.
                        ' If the daylight saving delta is positive, then there is a
                        ' period of time which does not exist (between 2am and 3am in
                        ' most daylight saving time zones) at the beginning of the
                        ' daylight saving. This period of non-existant time should be 
                        ' considered standard time (not daylight saving).
                    Else

                        If (daylightDelta.Ticks > 0) Then
                            If (time < (daylightStart.Add(daylightDelta))) Then
                                Return False
                            Else
                                Return True
                            End If
                        Else
                            Return True
                        End If

                    End If

                    ' The current time is before the end of daylight saving, so
                    ' it is during daylight saving.
                ElseIf (time < daylightEnd) Then
                    Return True

                    ' Otherwise, the time and date is not within daylight
                    ' saving time.
                Else

                    ' If the time was taken from a UTC time, then do not apply
                    ' the backward compatibility.
                    If (fromUtcTime) Then
                        Return False

                        ' Backward compatiblity with .NET Framework TimeZone.
                        ' If the daylight saving delta is negative (which shouldn't
                        ' happen), then there is a period of time which does not exist
                        ' (between 2am and 3am in most daylight saving time zones).
                        ' at the end of daylight saving. This period of
                        ' non-existant time should be considered daylight saving.
                    Else

                        If (daylightDelta.Ticks < 0) Then

                            If ( _
                              (time >= daylightEnd) AndAlso _
                              (time < daylightEnd.Subtract(daylightDelta)) _
                              ) Then
                                Return True
                            Else
                                Return False
                            End If

                        Else
                            Return False
                        End If

                    End If

                End If

            End If

        End Function
        '''<summary>
        ''' <para>
        ''' Return whether the time is within the ambiguous time for this year
        ''' </para>
        ''' </summary>
        ''' <param name="time"> Required.Time</param>
        ''' <param name="daylightTimes"> Required.DayLightTime</param>
        ''' <value>
        ''' Must be boolean
        ''' </value>
        Public Overridable Function IsAmbiguous( _
           ByVal time As System.DateTime _
         ) As Boolean

            ' If this time zone is never in daylight saving, then
            ' return false.
            If (_standardAlways) Then
                Return False
            End If

            ' Get the daylight saving time start and end for this
            ' time's year.
            Dim daylightTimes As DaylightTime
            daylightTimes = GetDaylightChanges(time.Year)

            ' Return whether the time is within the ambiguous
            ' time for this year.
            Return IsAmbiguous(time, daylightTimes)

        End Function

        '''<summary>
        ''' <para>
        ''' Returns a boolean value if it is the ambiguous time at the start and end of daylight savings or not.        
        ''' </para>
        ''' </summary>
        ''' <param name="time"> Required.Time</param>
        ''' <param name="daylightTimes"> Required.DayLightTime</param>
        ''' <value>
        ''' Must be boolean
        ''' </value>


        Public Shared Function IsAmbiguous( _
          ByVal time As System.DateTime, _
          ByVal daylightTimes As DaylightTime _
        ) As Boolean

            ' Mirrors .NET Framework TimeZone functionality, which 
            ' does not throw an exception.
            If (daylightTimes Is Nothing) Then
                Return False
            End If

            Dim daylightStart As System.DateTime
            Dim daylightEnd As System.DateTime
            Dim daylightDelta As TimeSpan
            daylightStart = daylightTimes.Start
            daylightEnd = daylightTimes.End
            daylightDelta = daylightTimes.Delta

            ' The ambiguous time is at the end of the daylight
            ' saving time when the delta is positive.
            If (daylightDelta.Ticks > 0) Then

                If ( _
                  (time < daylightEnd) AndAlso _
                  (daylightEnd.Subtract(daylightDelta) <= time) _
                  ) Then
                    Return True
                End If

                ' The ambiguous time is at the start of the daylight
                ' saving time when the delta is negative.
            ElseIf (daylightDelta.Ticks < 0) Then

                If ( _
                  (time < daylightStart) AndAlso _
                  (daylightStart.Add(daylightDelta) <= time) _
                  ) Then
                    Return True
                End If

            End If

            Return False

        End Function
        '''<summary>
        ''' <para>
        ''' Returns standard offset if the timezone is never in daylight savings         
        ''' </para>
        ''' </summary>
        ''' <param name="time"> Required.Time</param>

        Public Overrides Function GetUtcOffset( _
          ByVal time As System.DateTime _
        ) As TimeSpan

            ' If this time zone is never in daylight saving, then
            ' return the standard offset.
            If (_standardAlways) Then
                Return _standardOffset

                ' If the time zone is in daylight saving, then return
                ' the daylight saving offset.
            ElseIf (IsDaylightSavingTime(time)) Then
                Return _daylightOffset

                ' Otherwise, return the standard offset.
            Else
                Return _standardOffset
            End If

        End Function
        '''<summary>
        ''' <para>
        ''' Returns time with daylight savings          
        ''' </para>
        ''' </summary>
        ''' <param name="time"> Required.Time to add daylight savings to</param>


        Public Overrides Function ToLocalTime( _
          ByVal time As System.DateTime _
        ) As System.DateTime

            time = time.Add(_standardOffset)

            If (Not (_standardAlways)) Then
                If (IsDaylightSavingTime(time, True)) Then
                    time = time.Add(_daylightDelta)
                End If
            End If

            Return time

        End Function

        '''<summary>
        ''' <para>
        ''' This can return an incorrect time during the time change between standard and daylight saving time, because
        ''' times near the daylight saving switch can be ambiguous.
        ''' </para>
        ''' </summary>
        ''' <remarks>
        ''' <para>
        '''  For example, if daylight saving ends at: "2000/10/29 02:00", and fall back an hour, then is:
        ''' "2000/10/29 01:30", during daylight saving, or not?
        ''' </para>
        ''' <para>
        ''' Consequently, this function is provided for backwards compatiblity only, and should be deprecated and replaced
        ''' with the overload that allows daylight saving to be specified.
        ''' </para>
        ''' </remarks>
        Public Overloads Overrides Function ToUniversalTime( _
          ByVal time As System.DateTime _
        ) As System.DateTime

            If (_standardAlways) Then
                Return time.Subtract(_standardOffset)

            Else

                If (IsDaylightSavingTime(time)) Then
                    Return time.Subtract(_daylightOffset)
                Else
                    Return time.Subtract(_standardOffset)
                End If

            End If


        End Function
        '''<summary>
        ''' <para>
        '''' This overload allows the status of daylight saving to be specified along with the time. This conversion
        ''' is unambiguous and always correct.
        ''' </para>
        ''' </summary>


        Public Overloads Function ToUniversalTime( _
          ByVal time As System.DateTime, _
          ByVal daylightSaving As Boolean _
        ) As System.DateTime

            If (_standardAlways) Then
                Return time.Subtract(_standardOffset)

            Else

                If (daylightSaving) Then
                    Return time.Subtract(_daylightOffset)
                Else
                    Return time.Subtract(_standardOffset)
                End If

            End If

        End Function

    End Class

    Public NotInheritable Class TimeZones

        Private Const VALUE_INDEX As String = "Index"
        Private Const VALUE_DISPLAY_NAME As String = "Display"
        Private Const VALUE_STANDARD_NAME As String = "Std"
        Private Const VALUE_DAYLIGHT_NAME As String = "Dlt"
        Private Const VALUE_ZONE_INFO As String = "TZI"

        Private Const LENGTH_ZONE_INFO As Int32 = 44
        Private Const LENGTH_DWORD As Int32 = 4
        Private Const LENGTH_WORD As Int32 = 2
        Private Const LENGTH_SYSTEMTIME As Int32 = 16



        Private Shared REG_KEYS_TIME_ZONES As String() = { _
          "SOFTWARE\Microsoft\Windows\CurrentVersion\Time Zones", _
          "SOFTWARE\Microsoft\Windows NT\CurrentVersion\Time Zones" _
        }


        Private Shared nameRegKeyTimeZones As String


        Private Class TZREGReader


            Public Bias As Int32
            Public StandardBias As Int32
            Public DaylightBias As Int32
            Public StandardDate As SYSTEMTIMEReader
            Public DaylightDate As SYSTEMTIMEReader
            ''' <summary>
            '''Initializes a new instance of the TZREGReader class . 
            ''' </summary>
            '''<param name="bytes"> Required.</param>

            Public Sub New(ByVal bytes As Byte())

                Dim index As Int32
                index = 0

                Bias = BitConverter.ToInt32(bytes, index)
                index = index + LENGTH_DWORD

                StandardBias = BitConverter.ToInt32(bytes, index)
                index = index + LENGTH_DWORD

                DaylightBias = BitConverter.ToInt32(bytes, index)
                index = index + LENGTH_DWORD

                StandardDate = New SYSTEMTIMEReader(bytes, index)
                index = index + LENGTH_SYSTEMTIME

                DaylightDate = New SYSTEMTIMEReader(bytes, index)

            End Sub


        End Class


        Private Class SYSTEMTIMEReader


            Public Year As Int16
            Public Month As Int16
            Public DayOfWeek As Int16
            Public Day As Int16
            Public Hour As Int16
            Public Minute As Int16
            Public Second As Int16
            Public Milliseconds As Int16
            ''' <summary>
            '''Initializes a new instance of the SYSTEMTIMEReader class . 
            ''' </summary>
            '''<param name="bytes"> Required.</param>
            ''' <param name="index"> Required.</param>

            Public Sub New(ByVal bytes As Byte(), ByVal index As Int32)

                Year = BitConverter.ToInt16(bytes, index)
                index = index + LENGTH_WORD

                Month = BitConverter.ToInt16(bytes, index)
                index = index + LENGTH_WORD

                DayOfWeek = BitConverter.ToInt16(bytes, index)
                index = index + LENGTH_WORD

                Day = BitConverter.ToInt16(bytes, index)
                index = index + LENGTH_WORD

                Hour = BitConverter.ToInt16(bytes, index)
                index = index + LENGTH_WORD

                Minute = BitConverter.ToInt16(bytes, index)
                index = index + LENGTH_WORD

                Second = BitConverter.ToInt16(bytes, index)
                index = index + LENGTH_WORD

                Milliseconds = BitConverter.ToInt16(bytes, index)

            End Sub


        End Class
        ''' <summary>
        '''Initializes a new instance of the SYSTEMTIMEReader class . 
        ''' </summary>

        Shared Sub New()

            With Registry.LocalMachine

                Dim currentNameRegKey As String
                For Each currentNameRegKey In REG_KEYS_TIME_ZONES

                    If (Not (.OpenSubKey(currentNameRegKey) Is Nothing)) Then
                        nameRegKeyTimeZones = currentNameRegKey
                        Exit Sub
                    End If

                Next

            End With

        End Sub
        '''<summary>
        '''   <para>Returns an abbreviation for a name.</para>
        '''</summary>
        ''' <param name="name"> Required.  Name </param>

        Private Shared Function GetAbbreviation( _
          ByVal name As String() _
          ) As String

            Dim abbreviation As String = ""

            Dim nameChars As Char()
            nameChars = name.ToCharArray

            Dim currentChar As Char
            For Each currentChar In nameChars
                If (Char.IsUpper(currentChar)) Then
                    abbreviation = abbreviation & currentChar
                End If
            Next

            Return abbreviation

        End Function
        '''<summary>
        '''   <para>Returns Win32TimeZone for a registrykey timezone.</para>
        '''</summary>
        ''' <param name="regKeyTimeZone"> Required. Timezone </param>
        Private Shared Function LoadTimeZone( _
          ByVal regKeyTimeZone As RegistryKey _
          ) As Win32TimeZone

            Dim timeZoneIndex As Int32
            Dim displayName As String
            Dim standardName As String
            Dim daylightName As String
            Dim timeZoneData As Byte()

            With regKeyTimeZone
                timeZoneIndex = DirectCast(.GetValue(VALUE_INDEX), Int32)
                displayName = DirectCast(.GetValue(VALUE_DISPLAY_NAME), String)
                standardName = DirectCast(.GetValue(VALUE_STANDARD_NAME), String)
                daylightName = DirectCast(.GetValue(VALUE_DAYLIGHT_NAME), String)
                timeZoneData = DirectCast(.GetValue(VALUE_ZONE_INFO), Byte())
            End With

            If (timeZoneData.Length <> LENGTH_ZONE_INFO) Then
                Return Nothing
            End If

            Dim timeZoneInfo As New TZREGReader(timeZoneData)

            Dim standardOffset As New TimeSpan( _
              0, _
              -(timeZoneInfo.Bias + timeZoneInfo.StandardBias), _
              0 _
              )

            Dim daylightDelta As New TimeSpan( _
              0, _
              -(timeZoneInfo.DaylightBias), _
              0 _
              )

            If ( _
              (daylightDelta.Ticks = 0) Or _
              (timeZoneInfo.StandardDate.Month = 0) Or _
              (timeZoneInfo.DaylightDate.Month = 0) _
              ) Then
                Return New Win32TimeZone( _
                  timeZoneIndex, _
                  displayName, _
                  standardOffset, _
                  standardName, _
                  GetAbbreviation(standardName) _
                  )
            End If

            If ( _
              (timeZoneInfo.StandardDate.Year <> 0) Or _
              (timeZoneInfo.DaylightDate.Year <> 0) _
              ) Then
                Return Nothing
            End If

            Dim daylightSavingsStart As DaylightTimeChange
            Dim daylightSavingsEnd As DaylightTimeChange

            With timeZoneInfo.DaylightDate
                daylightSavingsStart = New DaylightTimeChange( _
                  .Month, _
                  CType(.DayOfWeek, DayOfWeek), _
                  (.Day - 1), _
                  New TimeSpan(0, .Hour, .Minute, .Second, .Milliseconds) _
                )
            End With

            With timeZoneInfo.StandardDate
                daylightSavingsEnd = New DaylightTimeChange( _
                  .Month, _
                  CType(.DayOfWeek, DayOfWeek), _
                  (.Day - 1), _
                  New TimeSpan(0, .Hour, .Minute, .Second, .Milliseconds) _
                )
            End With

            Return New Win32TimeZone( _
              timeZoneIndex, _
              displayName, _
              standardOffset, _
              standardName, _
              GetAbbreviation(standardName), _
              daylightDelta, _
              daylightName, _
              GetAbbreviation(daylightName), _
              daylightSavingsStart, _
              daylightSavingsEnd _
              )

        End Function
        '''<summary>
        '''   <para>Returns Timezone for a particular index.</para>
        '''</summary>
        ''' <param name="index"> Required. An integer index. </param>
        Public Shared Function GetTimeZone(ByVal index As Int32) As Win32TimeZone

            If (nameRegKeyTimeZones Is Nothing) Then
                Return Nothing
            End If

            Dim regKeyTimeZones As RegistryKey
            Try
                regKeyTimeZones = Registry.LocalMachine.OpenSubKey(nameRegKeyTimeZones)
            Catch
            End Try

            If (regKeyTimeZones Is Nothing) Then
                Return Nothing
            End If

            Dim result As Win32TimeZone

            Dim currentNameSubKey As String
            Dim namesSubKeys As String()
            namesSubKeys = regKeyTimeZones.GetSubKeyNames()

            Dim currentSubKey As RegistryKey

            Dim currentTimeZone As Win32TimeZone
            Dim timeZoneIndex As Int32

            For Each currentNameSubKey In namesSubKeys

                Try
                    currentSubKey = regKeyTimeZones.OpenSubKey(currentNameSubKey)
                Catch
                    currentSubKey = Nothing
                End Try

                If (Not (currentSubKey Is Nothing)) Then

                    Try

                        timeZoneIndex = DirectCast(currentSubKey.GetValue(VALUE_INDEX), Int32)

                        If (timeZoneIndex = index) Then
                            result = LoadTimeZone(currentSubKey)
                            currentSubKey.Close()
                            Exit For
                        End If

                    Catch
                    End Try

                    currentSubKey.Close()

                End If

            Next

            regKeyTimeZones.Close()

            Return result

        End Function
        '''<summary>
        '''   <para>Returns an array of Timezones.</para>
        '''</summary>

        Public Shared Function GetTimeZones() As Win32TimeZone()

            If (nameRegKeyTimeZones Is Nothing) Then
                Return New Win32TimeZone() {}
            End If

            Dim regKeyTimeZones As RegistryKey
            Try
                regKeyTimeZones = Registry.LocalMachine.OpenSubKey(nameRegKeyTimeZones)
            Catch
            End Try

            If (regKeyTimeZones Is Nothing) Then
                Return New Win32TimeZone() {}
            End If

            Dim results As New ArrayList

            Dim currentNameSubKey As String
            Dim namesSubKeys As String()
            namesSubKeys = regKeyTimeZones.GetSubKeyNames()

            Dim currentSubKey As RegistryKey

            Dim currentTimeZone As Win32TimeZone

            For Each currentNameSubKey In namesSubKeys

                Try
                    currentSubKey = regKeyTimeZones.OpenSubKey(currentNameSubKey)
                Catch
                    currentSubKey = Nothing
                End Try

                If (Not (currentSubKey Is Nothing)) Then

                    Try

                        currentTimeZone = LoadTimeZone(currentSubKey)

                        If (Not (currentTimeZone Is Nothing)) Then
                            results.Add(currentTimeZone)
                        End If

                    Catch
                    End Try

                    currentSubKey.Close()

                End If

            Next

            regKeyTimeZones.Close()

            Return DirectCast(results.ToArray(GetType(Win32TimeZone)), Win32TimeZone())

        End Function

    End Class

    <Serializable()> _
    Public Class Win32TimeZone

        Inherits SimpleTimeZone


        Private _index As Int32
        Private _displayName As String


        Public Sub New( _
          ByVal index As Int32, _
          ByVal displayName As String, _
          ByVal standardOffset As TimeSpan, _
          ByVal standardName As String, _
          ByVal standardAbbreviation As String _
        )

            MyBase.New( _
              standardOffset, _
              standardName, _
              standardAbbreviation _
              )

            _index = index
            _displayName = displayName

        End Sub
        ''' <summary>
        ''' Summary:
        '''Initializes a new instance of the Win32TimeZoneClass. 
        ''' </summary>

        Public Sub New( _
          ByVal index As Int32, _
          ByVal displayName As String, _
          ByVal standardOffset As TimeSpan, _
          ByVal standardName As String, _
          ByVal standardAbbreviation As String, _
          ByVal daylightDelta As TimeSpan, _
          ByVal daylightName As String, _
          ByVal daylightAbbreviation As String, _
          ByVal daylightTimeChangeStart As DaylightTimeChange, _
          ByVal daylightTimeChangeEnd As DaylightTimeChange _
        )

            MyBase.New( _
              standardOffset, _
              standardName, _
              standardAbbreviation, _
              daylightDelta, _
              daylightName, _
              daylightAbbreviation, _
              daylightTimeChangeStart, _
              daylightTimeChangeEnd _
              )

            _index = index
            _displayName = displayName

        End Sub
        ''' <summary>
        ''' Gets the Index
        ''' </summary>
        ''' <value>
        ''' Index 
        ''' </value>
        ''' <remarks>
        ''' Value must be integer
        ''' </remarks>

        Public ReadOnly Property Index() As Int32
            Get
                Return _index
            End Get
        End Property
        ''' <summary>
        ''' Gets the Display name
        ''' </summary>
        ''' <value>
        ''' Display name
        ''' </value>
        ''' <remarks>
        ''' Value must be String
        ''' </remarks>

        Public ReadOnly Property DisplayName() As String
            Get
                Return _displayName
            End Get
        End Property

        ''' <summary>
        ''' Returns the Display name
        ''' </summary>
        Public Overrides Function ToString() As String
            Return _displayName
        End Function

    End Class

End Namespace
