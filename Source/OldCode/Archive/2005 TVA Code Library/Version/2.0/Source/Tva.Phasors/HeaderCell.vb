'*******************************************************************************************************
'  HeaderCell.vb - Header cell class
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
'  01/14/2005 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Buffer
Imports Tva.Interop
Imports Tva.Phasors.Common

' This class represents the protocol independent common implementation of an element of header frame data that can be received from a PMU.
<CLSCompliant(False)> _
Public Class HeaderCell

    Inherits ChannelCellBase
    Implements IHeaderCell

    Private m_character As Byte

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

    Public Overrides ReadOnly Property InheritedType() As System.Type
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

End Class
