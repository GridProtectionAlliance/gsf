'***********************************************************************
'  ChannelDefinitionBase.vb - Channel data definition base class
'  Copyright © 2004 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [WESTAFF]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  3/7/2005 - James R Carroll
'       Initial version of source generated
'
'***********************************************************************

Imports System.Text

Namespace EE.Phasor

    ' This class represents the common implementation of the protocol independent definition of any kind of data.
    Public MustInherit Class ChannelDefinitionBase

        Implements IChannelDefinition

        Protected m_index As Integer
        Protected m_label As String
        Protected m_scale As Double

        Protected Sub New()

            m_index = 0
            m_label = ""
            m_scale = 1.0

        End Sub

        Protected Sub New(ByVal index As Integer, ByVal label As String, ByVal scale As Double)

            m_index = index
            m_label = label
            m_scale = scale

        End Sub

        Public Overridable Property ScalingFactor() As Double Implements IChannelDefinition.ScalingFactor
            Get
                Return m_scale
            End Get
            Set(ByVal Value As Double)
                m_scale = Value
            End Set
        End Property

        Public Overridable Property Label() As String Implements IChannelDefinition.Label
            Get
                Return m_label
            End Get
            Set(ByVal Value As String)
                If Len(Value) > MaximumLabelLength Then
                    Throw New OverflowException("Label length cannot exceed " & MaximumLabelLength)
                Else
                    m_label = Trim(Replace(Value, Chr(20), " "))
                End If
            End Set
        End Property

        Public Overridable ReadOnly Property LabelImage() As Byte() Implements IChannelDefinition.LabelImage
            Get
                Return Encoding.ASCII.GetBytes(m_label.PadRight(MaximumLabelLength))
            End Get
        End Property

        Public MustOverride ReadOnly Property BinaryLength() As Integer Implements IChannelDefinition.BinaryLength

        Public MustOverride ReadOnly Property BinaryImage() As Byte() Implements IChannelDefinition.BinaryImage

        Public Overridable Property Index() As Integer Implements IChannelDefinition.Index
            Get
                Return m_index
            End Get
            Set(ByVal Value As Integer)
                m_index = Value
            End Set
        End Property

        Public Overridable ReadOnly Property MaximumLabelLength() As Integer Implements IChannelDefinition.MaximumLabelLength
            Get
                Return 16
            End Get
        End Property

        Public Overridable Function CompareTo(ByVal obj As Object) As Integer Implements IComparable.CompareTo

            ' We sort phasor defintions by index
            If TypeOf obj Is IChannelDefinition Then
                Return Index.CompareTo(DirectCast(obj, IChannelDefinition).Index)
            Else
                Throw New ArgumentException("PhasorDefinition can only be compared to other PhasorDefinitions")
            End If

        End Function

    End Class

End Namespace
