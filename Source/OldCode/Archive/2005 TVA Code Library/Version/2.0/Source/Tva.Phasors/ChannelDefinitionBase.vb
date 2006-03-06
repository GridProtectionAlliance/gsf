'*******************************************************************************************************
'  ChannelDefinitionBase.vb - Channel data definition base class
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
'  3/7/2005 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Text
Imports Tva.Phasors.Common

' This class represents the common implementation of the protocol independent definition of any kind of data.
<CLSCompliant(False)> _
Public MustInherit Class ChannelDefinitionBase

    Inherits ChannelBase
    Implements IChannelDefinition

    Private m_parent As IConfigurationCell
    Private m_dataFormat As DataFormat
    Private m_index As Integer
    Private m_label As String
    Private m_scale As Integer
    Private m_offset As Double

    Protected Sub New(ByVal parent As IConfigurationCell)

        MyBase.New()

        m_parent = parent
        m_dataFormat = DataFormat.FixedInteger
        m_label = "undefined"
        m_scale = 1

    End Sub

    Protected Sub New(ByVal parent As IConfigurationCell, ByVal dataFormat As DataFormat, ByVal index As Integer, ByVal label As String, ByVal scale As Integer, ByVal offset As Double)

        MyBase.New()

        m_parent = parent
        m_dataFormat = dataFormat
        m_index = index
        Me.Label = label
        m_scale = scale
        m_offset = offset

    End Sub

    ' Derived classes are expected to expose a Protected Sub New(ByVal channelDefinition As IChannelDefinition)
    Protected Sub New(ByVal channelDefinition As IChannelDefinition)

        MyClass.New(channelDefinition.Parent, channelDefinition.DataFormat, channelDefinition.Index, channelDefinition.Label, _
            channelDefinition.ScalingFactor, channelDefinition.Offset)

    End Sub

    Public Overridable ReadOnly Property Parent() As IConfigurationCell Implements IChannelDefinition.Parent
        Get
            Return m_parent
        End Get
    End Property

    Public Overridable Property DataFormat() As DataFormat Implements IChannelDefinition.DataFormat
        Get
            Return m_dataFormat
        End Get
        Set(ByVal value As DataFormat)
            m_dataFormat = value
        End Set
    End Property

    Public Overridable Property Index() As Integer Implements IChannelDefinition.Index
        Get
            Return m_index
        End Get
        Set(ByVal value As Integer)
            m_index = value
        End Set
    End Property

    Public Overridable Property Offset() As Double Implements IChannelDefinition.Offset
        Get
            Return m_offset
        End Get
        Set(ByVal value As Double)
            m_offset = value
        End Set
    End Property

    Public Overridable Property ScalingFactor() As Integer Implements IChannelDefinition.ScalingFactor
        Get
            Return m_scale
        End Get
        Set(ByVal value As Integer)
            If value > MaximumScalingFactor Then Throw New OverflowException("Scaling factor value cannot exceed " & MaximumScalingFactor)
            m_scale = value
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
        Set(ByVal value As String)
            If Len(Trim(value)) > MaximumLabelLength Then
                Throw New OverflowException("Label length cannot exceed " & MaximumLabelLength)
            Else
                m_label = Trim(Replace(value, Chr(20), " "))
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

    Public Overridable Function CompareTo(ByVal obj As Object) As Integer Implements IComparable.CompareTo

        ' We sort phasor defintions by index
        If TypeOf obj Is IChannelDefinition Then
            Return Index.CompareTo(DirectCast(obj, IChannelDefinition).Index)
        Else
            Throw New ArgumentException("PhasorDefinition can only be compared to other PhasorDefinitions")
        End If

    End Function

    Protected Overrides ReadOnly Property BodyLength() As Int16
        Get
            Return MaximumLabelLength
        End Get
    End Property

    Protected Overrides ReadOnly Property BodyImage() As Byte()
        Get
            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BodyLength)

            CopyImage(LabelImage, buffer, Index, MaximumLabelLength)

            Return buffer
        End Get
    End Property

End Class
