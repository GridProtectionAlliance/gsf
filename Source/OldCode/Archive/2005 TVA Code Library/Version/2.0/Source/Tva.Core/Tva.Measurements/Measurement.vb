'*******************************************************************************************************
'  Tva.Measurements.Measurement.vb - Basic measurement implementation
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  This class represents a basic measured value
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  12/8/2005 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Namespace Measurements

    Public Class Measurement

        Implements IMeasurement

        Private m_index As Integer
        Private m_value As Double
        Private m_ticks As Long

        Public Sub New()

            MyClass.New(-1, Double.NaN, 0)

        End Sub

        Public Sub New(ByVal index As Integer, ByVal value As Double, ByVal timestamp As Date)

            MyClass.New(index, value, timestamp.Ticks)

        End Sub

        Public Sub New(ByVal index As Integer, ByVal value As Double, ByVal ticks As Long)

            m_index = index
            m_value = value
            m_ticks = ticks

        End Sub

        ''' <summary>Handy instance reference to self</summary>
        Public Overridable ReadOnly Property This() As IMeasurement Implements IMeasurement.This
            Get
                Return Me
            End Get
        End Property

        ''' <summary>Gets or sets index or ID of this measurement</summary>
        Public Overridable Property Index() As Integer Implements IMeasurement.Index
            Get
                Return m_index
            End Get
            Set(ByVal value As Integer)
                m_index = value
            End Set
        End Property

        ''' <summary>Gets or sets numeric value of this measurement</summary>
        Public Overridable Property Value() As Double Implements IMeasurement.Value
            Get
                Return m_value
            End Get
            Set(ByVal value As Double)
                m_value = value
            End Set
        End Property

        ''' <summary>Gets or sets exact timestamp of the data represented by this measurement</summary>
        ''' <remarks>The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001</remarks>
        Public Overridable Property Ticks() As Long Implements IMeasurement.Ticks
            Get
                Return m_ticks
            End Get
            Set(ByVal value As Long)
                m_ticks = value
            End Set
        End Property

        ''' <summary>Closest date representation of ticks of this measurement</summary>
        Public Overridable ReadOnly Property Timestamp() As Date Implements IMeasurement.Timestamp
            Get
                Return New Date(m_ticks)
            End Get
        End Property

    End Class

End Namespace
