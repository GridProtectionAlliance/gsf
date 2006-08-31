'*******************************************************************************************************
'  Tva.Measurements.Frame.vb - Basic frame implementation
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  This class represents a basic measured value
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  6/22/2006 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Namespace Measurements

    ''' <summary>Implementation of a basic frame</summary>
    Public Class Frame

        Implements IFrame

        Private m_ticks As Long
        Private m_published As Boolean
        Private m_measurements As Dictionary(Of MeasurementKey, IMeasurement)

        Public Sub New(ByVal ticks As Long)

            m_ticks = ticks
            m_measurements = New Dictionary(Of MeasurementKey, IMeasurement)

        End Sub

        ''' <summary>Keyed measurements in this frame</summary>
        Public ReadOnly Property Measurements() As Dictionary(Of MeasurementKey, IMeasurement) Implements IFrame.Measurements
            Get
                Return m_measurements
            End Get
        End Property

        ''' <summary>Gets or sets published state of this frame</summary>
        Public Property Published() As Boolean Implements IFrame.Published
            Get
                Return m_published
            End Get
            Set(ByVal value As Boolean)
                m_published = value
            End Set
        End Property

        ''' <summary>Handy instance reference to self</summary>
        Public ReadOnly Property This() As IFrame Implements IFrame.This
            Get
                Return Me
            End Get
        End Property

        ''' <summary>Exact timestamp of the data represented in this frame</summary>
        ''' <remarks>The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001</remarks>
        Public Property Ticks() As Long Implements IFrame.Ticks
            Get
                Return m_ticks
            End Get
            Set(ByVal value As Long)
                m_ticks = value
            End Set
        End Property

        ''' <summary>Date representation of ticks of this frame</summary>
        Public ReadOnly Property Timestamp() As Date Implements IFrame.Timestamp
            Get
                Return New Date(m_ticks)
            End Get
        End Property

        ''' <summary>This implementation of a basic frame compares itself by timestamp</summary>
        Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

            If TypeOf obj Is IFrame Then
                Return m_ticks.CompareTo(DirectCast(obj, IFrame).Ticks)
            Else
                Throw New ArgumentException("Frame can only be compared with other IFrames...")
            End If

        End Function

    End Class

End Namespace