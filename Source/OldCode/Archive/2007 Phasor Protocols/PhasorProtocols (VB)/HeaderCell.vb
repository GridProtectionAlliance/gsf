'*******************************************************************************************************
'  HeaderCell.vb - Header cell class
'  Copyright © 2008 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2008
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
Imports System.Text
Imports PhasorProtocols.Common

''' <summary>This class represents the protocol independent common implementation of an element of header frame data that can be received from a PMU.</summary>
<CLSCompliant(False), Serializable()> _
Public Class HeaderCell

    Inherits ChannelCellBase
    Implements IHeaderCell

    Private m_character As Byte

    Protected Sub New()
    End Sub

    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

        MyBase.New(info, context)

        ' Deserialize header cell value
        m_character = info.GetByte("character")

    End Sub

    Public Sub New(ByVal parent As IHeaderFrame)

        MyBase.New(parent, False)

    End Sub

    Public Sub New(ByVal parent As IHeaderFrame, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

        MyClass.New(parent)
        ParseBinaryImage(Nothing, binaryImage, startIndex)

    End Sub

    Public Sub New(ByVal parent As IHeaderFrame, ByVal character As Byte)

        MyBase.New(parent, False)
        m_character = character

    End Sub

    Public Sub New(ByVal headerCell As IHeaderCell)

        MyClass.New(headerCell.Parent, headerCell.Character)

    End Sub

    Friend Shared Function CreateNewHeaderCell(ByVal parent As IChannelFrame, ByVal state As IChannelFrameParsingState(Of IHeaderCell), ByVal index As Int32, ByVal binaryImage As Byte(), ByVal startIndex As Int32) As IHeaderCell

        Return New HeaderCell(parent, binaryImage, startIndex)

    End Function

    Public Overrides ReadOnly Property DerivedType() As System.Type
        Get
            Return Me.GetType()
        End Get
    End Property

    Public Overridable Shadows ReadOnly Property Parent() As IHeaderFrame Implements IHeaderCell.Parent
        Get
            Return MyBase.Parent
        End Get
    End Property

    Public Overridable Property Character() As Byte Implements IHeaderCell.Character
        Get
            Return m_character
        End Get
        Set(ByVal value As Byte)
            m_character = value
        End Set
    End Property

    Protected Overrides ReadOnly Property BodyLength() As UInt16
        Get
            Return 1
        End Get
    End Property

    Protected Overrides ReadOnly Property BodyImage() As Byte()
        Get
            Return New Byte() {m_character}
        End Get
    End Property

    Protected Overrides Sub ParseBodyImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

        m_character = binaryImage(startIndex)

    End Sub

    Public Overrides Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext)

        MyBase.GetObjectData(info, context)

        ' Serialize header cell value
        info.AddValue("character", m_character)

    End Sub

    Public Overrides ReadOnly Property Attributes() As Dictionary(Of String, String)
        Get
            Dim baseAttributes As Dictionary(Of String, String) = MyBase.Attributes

            baseAttributes.Add("Character", Encoding.ASCII.GetString(New Byte() {Character}))

            Return baseAttributes
        End Get
    End Property

End Class