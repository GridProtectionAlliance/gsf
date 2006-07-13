'*******************************************************************************************************
'  CommandCell.vb - Command cell class
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
Imports System.Buffer
Imports Tva.Phasors.Common

''' <summary>This class represents the protocol independent common implementation of an element of extended frame data of a command frame that can be received from a PMU.</summary>
<CLSCompliant(False), Serializable()> _
Public Class CommandCell

    Inherits ChannelCellBase
    Implements ICommandCell

    Private m_extendedDataByte As Byte

    Protected Sub New()
    End Sub

    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

        MyBase.New(info, context)

        ' Deserialize command cell value
        m_extendedDataByte = info.GetByte("extendedDataByte")

    End Sub

    Public Sub New(ByVal parent As ICommandFrame)

        MyBase.New(parent, False)

    End Sub

    Public Sub New(ByVal parent As ICommandFrame, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

        MyClass.New(parent)
        ParseBinaryImage(Nothing, binaryImage, startIndex)

    End Sub

    Public Sub New(ByVal parent As ICommandFrame, ByVal extendedDataByte As Byte)

        MyBase.New(parent, False)
        m_extendedDataByte = extendedDataByte

    End Sub

    Public Sub New(ByVal headerCell As IHeaderCell)

        MyClass.New(headerCell.Parent, headerCell.Character)

    End Sub

    Friend Shared Function CreateNewCommandCell(ByVal parent As IChannelFrame, ByVal state As IChannelFrameParsingState(Of ICommandCell), ByVal index As Int32, ByVal binaryImage As Byte(), ByVal startIndex As Int32) As ICommandCell

        Return New CommandCell(parent, binaryImage, startIndex)

    End Function

    Public Overrides ReadOnly Property InheritedType() As System.Type
        Get
            Return Me.GetType()
        End Get
    End Property

    Public Overridable Shadows ReadOnly Property Parent() As ICommandFrame Implements ICommandCell.Parent
        Get
            Return MyBase.Parent
        End Get
    End Property

    Public Overridable Property ExtendedDataByte() As Byte Implements ICommandCell.ExtendedDataByte
        Get
            Return m_extendedDataByte
        End Get
        Set(ByVal value As Byte)
            m_extendedDataByte = value
        End Set
    End Property

    Protected Overrides ReadOnly Property BodyLength() As UInt16
        Get
            Return 1
        End Get
    End Property

    Protected Overrides ReadOnly Property BodyImage() As Byte()
        Get
            Return New Byte() {m_extendedDataByte}
        End Get
    End Property

    Protected Overrides Sub ParseBodyImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

        m_extendedDataByte = binaryImage(startIndex)

    End Sub

    Public Overrides Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext)

        MyBase.GetObjectData(info, context)

        ' Serialize command cell value
        info.AddValue("extendedDataByte", m_extendedDataByte)

    End Sub

End Class
