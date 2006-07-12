'*******************************************************************************************************
'  Tva.DateTime.TimeTagBase.vb - Base class for alternate time tag implementations
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
'  07/12/2006 - J. Ritchie Carroll
'       Original version of source generated
'
'*******************************************************************************************************

Imports System.Runtime.Serialization
Imports Tva.DateTime.Common

Namespace DateTime

    ''' <summary>Base class for alternate time tag implementations</summary>
    Public MustInherit Class TimeTagBase

        Implements IComparable, ISerializable

        Private m_baseDateOffsetTicks As Long
        Private m_seconds As Double

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            ' Deserialize time tag
            m_baseDateOffsetTicks = info.GetInt64("baseDateOffsetTicks")
            m_seconds = info.GetDouble("seconds")

        End Sub

        ''' <summary>Creates new Unix timetag given number of seconds since 1/1/1900</summary>
        ''' <param name="seconds">Number of seconds since 1/1/1970</param>
        Protected Sub New(ByVal baseDateOffsetTicks As Long, ByVal seconds As Double)

            m_baseDateOffsetTicks = baseDateOffsetTicks
            Value = seconds

        End Sub

        ''' <summary>Creates new Unix timetag given standard .NET DateTime</summary>
        ''' <param name="timestamp">.NET DateTime to create Unix timetag from (minimum valid date is 1/1/1970)</param>
        Protected Sub New(ByVal baseDateOffsetTicks As Long, ByVal timestamp As Date)

            ' Zero base 100-nanosecond ticks from 1/1/1970 and convert to seconds
            m_baseDateOffsetTicks = baseDateOffsetTicks
            Value = TicksToSeconds(timestamp.Ticks - m_baseDateOffsetTicks)

        End Sub

        ''' <summary>Value represents number of seconds since 1/1/1970</summary>
        Public Overridable Property Value() As Double
            Get
                Return m_seconds
            End Get
            Set(ByVal val As Double)
                m_seconds = val
                If m_seconds < 0 Then m_seconds = 0
            End Set
        End Property

        ''' <summary>Returns standard .NET DateTime representation for timetag</summary>
        Public Overridable Function ToDateTime() As Date

            ' Convert m_seconds to 100-nanosecond ticks and add the 1/1/1970 offset
            Return New Date(SecondsToTicks(m_seconds) + m_baseDateOffsetTicks)

        End Function

        ''' <summary>Returns basic textual representation for timetag</summary>
        ''' <remarks>Format is "yyyy-MM-dd HH:mm:ss.fff" so that textual representation can be sorted in the correct chronological order</remarks>
        Public Overrides Function ToString() As String

            Return ToDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff")

        End Function

        ''' <summary>Ticks representing the absolute minimum time of this timetag implementation</summary>
        Public Overridable ReadOnly Property BaseDateOffsetTicks() As Long
            Get
                Return m_baseDateOffsetTicks
            End Get
        End Property

        ''' <summary>Compares this timetag to another one</summary>
        Public Overridable Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

            If TypeOf obj Is TimeTagBase Then
                Return m_seconds.CompareTo(DirectCast(obj, TimeTagBase).Value)
            ElseIf TypeOf obj Is Double Then
                Return m_seconds.CompareTo(CDbl(obj))
            Else
                Throw New ArgumentException("Timetag can only be compared with other timetags...")
            End If

        End Function

        Public Overrides Function Equals(ByVal obj As Object) As Boolean

            Return (CompareTo(obj) = 0)

        End Function

        Public Overrides Function GetHashCode() As Integer

            Return Convert.ToInt32(m_seconds * 1000)

        End Function

        Public Overridable Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext) Implements System.Runtime.Serialization.ISerializable.GetObjectData

            ' Serialize time tag
            info.AddValue("baseDateOffsetTicks", m_baseDateOffsetTicks)
            info.AddValue("seconds", m_seconds)

        End Sub

    End Class

End Namespace
