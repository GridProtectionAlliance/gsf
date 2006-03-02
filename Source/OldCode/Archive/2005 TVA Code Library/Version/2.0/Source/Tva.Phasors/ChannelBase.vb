'*******************************************************************************************************
'  ChannelBase.vb - Channel data base class
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

Imports Tva.Phasors.Common

' This class represents the common implementation of the protocol independent definition of any kind of data.
Public MustInherit Class ChannelBase

    Implements IChannel

    ' This is expected to be overriden by the final derived class
    Public MustOverride ReadOnly Property InheritedType() As Type Implements IChannel.InheritedType

    Public Overridable ReadOnly Property This() As IChannel Implements IChannel.This
        Get
            Return Me
        End Get
    End Property

    Public Overridable ReadOnly Property BinaryLength() As Int16 Implements IChannel.BinaryLength
        Get
            Return HeaderLength + BodyLength + FooterLength
        End Get
    End Property

    Public Overridable ReadOnly Property BinaryImage() As Byte() Implements IChannel.BinaryImage
        Get
            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)
            Dim index As Integer

            ' Copy in header, body and footer images
            CopyImage(HeaderImage, buffer, index, HeaderLength)
            CopyImage(BodyImage, buffer, index, BodyLength)
            CopyImage(FooterImage, buffer, index, FooterLength)

            Return buffer
        End Get
    End Property

    Protected Overridable Sub ParseBinaryImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Integer) Implements IChannel.ParseBinaryImage

        ' Parse out header, body and footer images
        ParseHeaderImage(state, binaryImage, startIndex)
        startIndex += HeaderLength

        ParseBodyImage(state, binaryImage, startIndex)
        startIndex += BodyLength

        ParseFooterImage(state, binaryImage, startIndex)

    End Sub

    Protected Overridable Sub ParseHeaderImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Integer)
    End Sub

    Protected Overridable Sub ParseBodyImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Integer)
    End Sub

    Protected Overridable Sub ParseFooterImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Integer)
    End Sub

    Protected Overridable ReadOnly Property HeaderLength() As Int16
        Get
            Return 0
        End Get
    End Property

    Protected Overridable ReadOnly Property HeaderImage() As Byte()
        Get
            Throw New NotImplementedException("No header image specified for " & InheritedType.FullName)
        End Get
    End Property

    Protected Overridable ReadOnly Property BodyLength() As Int16
        Get
            Return 0
        End Get
    End Property

    Protected Overridable ReadOnly Property BodyImage() As Byte()
        Get
            Throw New NotImplementedException("No body image specified for " & InheritedType.FullName)
        End Get
    End Property

    Protected Overridable ReadOnly Property FooterLength() As Int16
        Get
            Return 0
        End Get
    End Property

    Protected Overridable ReadOnly Property FooterImage() As Byte()
        Get
            Throw New NotImplementedException("No footer image specified for " & InheritedType.FullName)
        End Get
    End Property

End Class

