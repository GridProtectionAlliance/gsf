'***********************************************************************
'  ChannelDefinitionBase.vb - Channel data definition base class
'  Copyright © 2004 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
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

        Private m_dataFormat As DataFormat
        Private m_index As Integer
        Private m_label As String
        Private m_scale As Integer
        Private m_offset As Double

        Protected Sub New()

            m_dataFormat = DataFormat.FixedInteger
            m_label = "undefined"
            m_scale = 1

        End Sub

        Protected Sub New(ByVal dataFormat As DataFormat, ByVal index As Integer, ByVal label As String, ByVal scale As Integer, ByVal offset As Double)

            With Me
                .DataFormat = dataFormat
                .Index = index
                .Label = label
                .ScalingFactor = scale
                .Offset = offset
            End With

        End Sub

        ' Dervied classes are expected to expose a Protected Sub New(ByVal channelDefinition As IChannelDefinition)
        Protected Sub New(ByVal channelDefinition As IChannelDefinition)

            Me.New(channelDefinition.DataFormat, channelDefinition.Index, channelDefinition.Label, channelDefinition.ScalingFactor, channelDefinition.Offset)

        End Sub

        Public MustOverride ReadOnly Property InheritedType() As Type Implements IChannelDefinition.InheritedType

        Public Overridable ReadOnly Property This() As IChannel Implements IChannelDefinition.This
            Get
                Return Me
            End Get
        End Property

        Public Overridable Property DataFormat() As DataFormat Implements IChannelDefinition.DataFormat
            Get
                Return m_dataFormat
            End Get
            Set(ByVal Value As DataFormat)
                m_dataFormat = Value
            End Set
        End Property

        Public Overridable Property Index() As Integer Implements IChannelDefinition.Index
            Get
                Return m_index
            End Get
            Set(ByVal Value As Integer)
                m_index = Value
            End Set
        End Property

        Public Overridable Property Offset() As Double Implements IChannelDefinition.Offset
            Get
                Return m_offset
            End Get
            Set(ByVal Value As Double)
                m_offset = Value
            End Set
        End Property

        Public Overridable Property ScalingFactor() As Integer Implements IChannelDefinition.ScalingFactor
            Get
                Return m_scale
            End Get
            Set(ByVal Value As Integer)
                If Value > MaximumScalingFactor Then Throw New OverflowException("Scaling factor value cannot exceed " & MaximumScalingFactor)
                m_scale = Value
            End Set
        End Property

        Public Overridable ReadOnly Property MaximumScalingFactor() As Integer Implements IChannelDefinition.MaximumScalingFactor
            Get
                ' Typical scaling/conversion factors should fit within 3 bytes (i.e., 24 bits) of space
                Return 2 ^ 24
            End Get
        End Property

        Public Overridable Property Label() As String Implements IChannelDefinition.Label
            Get
                Return m_label
            End Get
            Set(ByVal Value As String)
                If Len(Trim(Value)) > MaximumLabelLength Then
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

        Public Overridable ReadOnly Property MaximumLabelLength() As Integer Implements IChannelDefinition.MaximumLabelLength
            Get
                ' Typical label length is 16 characters
                Return 16
            End Get
        End Property

        Public MustOverride ReadOnly Property BinaryLength() As Int16 Implements IChannelDefinition.BinaryLength

        Public MustOverride ReadOnly Property BinaryImage() As Byte() Implements IChannelDefinition.BinaryImage

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
