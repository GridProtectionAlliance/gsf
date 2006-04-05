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
Imports Tva.Common
Imports Tva.Phasors.Common

' This class represents the common implementation of the protocol independent definition of any kind of data.
<CLSCompliant(False)> _
Public MustInherit Class ChannelDefinitionBase

    Inherits ChannelBase
    Implements IChannelDefinition

    Private m_parent As IConfigurationCell
    Private m_dataFormat As DataFormat
    Private m_index As Int32
    Private m_label As String
    Private m_scale As Int32
    Private m_offset As Single

    Protected Sub New(ByVal parent As IConfigurationCell)

        m_parent = parent
        m_dataFormat = DataFormat.FixedInteger
        m_label = "undefined"
        m_scale = 1

    End Sub

    Protected Sub New(ByVal parent As IConfigurationCell, ByVal dataFormat As DataFormat, ByVal index As Int32, ByVal label As String, ByVal scale As Int32, ByVal offset As Single)

        m_parent = parent
        m_dataFormat = dataFormat
        m_index = index
        Me.Label = label
        m_scale = scale
        m_offset = offset

    End Sub

    Protected Sub New(ByVal parent As IConfigurationCell, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

        m_parent = parent
        ParseBinaryImage(Nothing, binaryImage, startIndex)

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

    Public Overridable Property Index() As Int32 Implements IChannelDefinition.Index
        Get
            Return m_index
        End Get
        Set(ByVal value As Int32)
            m_index = value
        End Set
    End Property

    Public Overridable Property Offset() As Single Implements IChannelDefinition.Offset
        Get
            Return m_offset
        End Get
        Set(ByVal value As Single)
            m_offset = value
        End Set
    End Property

    Public Overridable Property ScalingFactor() As Int32 Implements IChannelDefinition.ScalingFactor
        Get
            Return m_scale
        End Get
        Set(ByVal value As Int32)
            If value > MaximumScalingFactor Then Throw New OverflowException("Scaling factor value cannot exceed " & MaximumScalingFactor)
            m_scale = value
        End Set
    End Property

    Public Overridable Property ConversionFactor() As Single Implements IChannelDefinition.ConversionFactor
        Get
            Return m_scale * ScalePerBit
        End Get
        Set(ByVal value As Single)
            ScalingFactor = Convert.ToInt32(value / ScalePerBit)
        End Set
    End Property

    Public Overridable ReadOnly Property ScalePerBit() As Single Implements IChannelDefinition.ScalePerBit
        Get
            ' Typical scale/bit is 10^-5
            Return 0.00001
        End Get
    End Property

    Public Overridable ReadOnly Property MaximumScalingFactor() As Int32 Implements IChannelDefinition.MaximumScalingFactor
        Get
            ' Typical scaling/conversion factors should fit within 3 bytes (i.e., 24 bits) of space
            Return &H1FFFFFF
        End Get
    End Property

    Public Overridable Property Label() As String Implements IChannelDefinition.Label
        Get
            Return m_label
        End Get
        Set(ByVal value As String)
            If String.IsNullOrEmpty(value) Then value = "undefined"

            If value.Trim().Length > MaximumLabelLength Then
                Throw New OverflowException("Label length cannot exceed " & MaximumLabelLength)
            Else
                m_label = value.Replace(Chr(20), " "c).Trim()
            End If
        End Set
    End Property

    Public Overridable ReadOnly Property LabelImage() As Byte() Implements IChannelDefinition.LabelImage
        Get
            Return Encoding.ASCII.GetBytes(m_label.PadRight(MaximumLabelLength))
        End Get
    End Property

    Public Overridable ReadOnly Property MaximumLabelLength() As Int32 Implements IChannelDefinition.MaximumLabelLength
        Get
            ' Typical label length is 16 characters
            Return 16
        End Get
    End Property

    Public Overridable Function CompareTo(ByVal obj As Object) As Int32 Implements IComparable.CompareTo

        ' We sort channel defintions by index
        If TypeOf obj Is IChannelDefinition Then
            Return Index.CompareTo(DirectCast(obj, IChannelDefinition).Index)
        Else
            Throw New ArgumentException("ChannelDefinition can only be compared to other IChannelDefinitions")
        End If

    End Function

    Protected Overrides ReadOnly Property BodyLength() As UInt16
        Get
            Return MaximumLabelLength
        End Get
    End Property

    Protected Overrides ReadOnly Property BodyImage() As Byte()
        Get
            Return LabelImage
        End Get
    End Property

    Protected Overrides Sub ParseBodyImage(ByVal state As IChannelParsingState, ByVal binaryImage() As Byte, ByVal startIndex As Int32)

        Dim length As Int32 = Array.IndexOf(binaryImage, Convert.ToByte(0), startIndex, MaximumLabelLength) - startIndex

        If length < 0 Then length = MaximumLabelLength

        Label = Encoding.ASCII.GetString(binaryImage, startIndex, length)

    End Sub

End Class
