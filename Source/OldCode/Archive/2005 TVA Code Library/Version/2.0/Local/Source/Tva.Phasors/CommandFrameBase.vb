'*******************************************************************************************************
'  CommandFrameBase.vb - Command frame base class
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  01/14/2005 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Runtime.Serialization
Imports System.Text
Imports Tva.Phasors.Common

''' <summary>This class represents the protocol independent common implementation of a command frame that can be sent or received from a PMU.</summary>
<CLSCompliant(False), Serializable()> _
Public MustInherit Class CommandFrameBase

    Inherits ChannelFrameBase(Of ICommandCell)
    Implements ICommandFrame

    Private m_command As DeviceCommand

    Protected Sub New()
    End Sub

    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

        MyBase.New(info, context)

        ' Deserialize command frame
        m_command = info.GetValue("command", GetType(DeviceCommand))

    End Sub

    Protected Sub New(ByVal cells As CommandCellCollection, ByVal command As DeviceCommand)

        MyBase.New(cells)
        m_command = command

    End Sub

    ' Derived classes are expected to expose a Public Sub New(ByVal binaryImage As Byte(), ByVal startIndex As Int32)
    ' and automatically pass in parsing state
    Protected Sub New(ByVal state As ICommandFrameParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

        MyBase.New(state, binaryImage, startIndex)

    End Sub

    ' Derived classes are expected to expose a Public Sub New(ByVal commandFrame As ICommandFrame)
    Protected Sub New(ByVal commandFrame As ICommandFrame)

        MyClass.New(commandFrame.Cells, commandFrame.Command)

    End Sub

    Protected Overrides ReadOnly Property FundamentalFrameType() As FundamentalFrameType
        Get
            Return Phasors.FundamentalFrameType.CommandFrame
        End Get
    End Property

    Public Overridable Shadows ReadOnly Property Cells() As CommandCellCollection Implements ICommandFrame.Cells
        Get
            Return MyBase.Cells
        End Get
    End Property

    Public Overridable Property Command() As DeviceCommand Implements ICommandFrame.Command
        Get
            Return m_command
        End Get
        Set(ByVal value As DeviceCommand)
            m_command = value
        End Set
    End Property

    Public Overridable Property ExtendedData() As Byte() Implements ICommandFrame.ExtendedData
        Get
            Return Cells.BinaryImage
        End Get
        Set(ByVal value As Byte())
            Cells.Clear()
            MyBase.ParseBodyImage(New CommandFrameParsingState(Cells, 0, value.Length), value, 0)
        End Set
    End Property

    Protected Overrides ReadOnly Property BodyLength() As UInt16
        Get
            Return MyBase.BodyLength + 2
        End Get
    End Property

    Protected Overrides ReadOnly Property BodyImage() As Byte()
        Get
            Dim buffer As Byte() = CreateArray(Of Byte)(BodyLength)
            Dim index As Int32 = 2

            EndianOrder.BigEndian.CopyBytes(m_command, buffer, 0)
            CopyImage(MyBase.BodyImage, buffer, index, MyBase.BodyLength)

            Return buffer
        End Get
    End Property

    Protected Overrides Sub ParseBodyImage(ByVal state As IChannelParsingState, ByVal binaryImage() As Byte, ByVal startIndex As Int32)

        m_command = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex)
        MyBase.ParseBodyImage(state, binaryImage, startIndex + 2)

    End Sub

    Public Overrides Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext)

        MyBase.GetObjectData(info, context)

        ' Serialize command frame
        info.AddValue("command", m_command, GetType(DeviceCommand))

    End Sub

    Public Overrides ReadOnly Property Attributes() As Dictionary(Of String, String)
        Get
            With MyBase.Attributes
                .Add("Device Command", Command & ": " & [Enum].GetName(GetType(DeviceCommand), Command))

                If Cells.Count > 0 Then
                    .Add("Extended Data", ByteEncoding.Hexadecimal.GetString(Cells.BinaryImage, 0, Cells.Count))
                Else
                    .Add("Extended Data", "<null>")
                End If
            End With

            Return MyBase.Attributes
        End Get
    End Property

End Class
