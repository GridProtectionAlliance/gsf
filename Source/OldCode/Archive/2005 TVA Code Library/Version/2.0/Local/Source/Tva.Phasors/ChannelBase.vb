'*******************************************************************************************************
'  ChannelBase.vb - Channel data base class
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
'  3/7/2005 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports Tva.Phasors.Common

''' <summary>This class represents the common implementation of the protocol independent definition of any kind of data.</summary>
<CLSCompliant(False), Serializable()> _
Public MustInherit Class ChannelBase

    Implements IChannel

    ' This is the attributes dictionary relevant to all channel properties.  This dictionary
    ' will only be instantiated with a call to "Attributes" property which will begin the
    ' enumeration of relevant system properties.  This is typically used for display purposes.
    ' For example, this information is displayed in a tree view on the the PMU Connection
    ' Tester to display attributes of data elements that may be protocol specific
    Private m_attributes As Dictionary(Of String, String)
    Private m_tag As Object

    ' This is expected to be overriden by the final derived class
    Public MustOverride ReadOnly Property DerivedType() As Type Implements IChannel.DerivedType

    Public Overridable ReadOnly Property This() As IChannel Implements IChannel.This
        Get
            Return Me
        End Get
    End Property

    ' This property is not typically overriden
    Public Overridable ReadOnly Property BinaryLength() As UInt16 Implements IChannel.BinaryLength
        Get
            Return HeaderLength + BodyLength + FooterLength
        End Get
    End Property

    ' Phasor classes use an unsigned 16-bit integer for data lengths - so we expose the IBinaryDataProvider binary length
    ' property as a private property
    Private ReadOnly Property IBinaryDataProviderBinaryLength() As Integer Implements IBinaryDataProvider.BinaryLength
        Get
            Return BinaryLength
        End Get
    End Property

    ' This property is not typically overriden
    Public Overridable ReadOnly Property BinaryImage() As Byte() Implements IBinaryDataProvider.BinaryImage
        Get
            Dim buffer As Byte() = CreateArray(Of Byte)(BinaryLength)
            Dim index As Int32

            ' Copy in header, body and footer images
            CopyImage(HeaderImage, buffer, index, HeaderLength)
            CopyImage(BodyImage, buffer, index, BodyLength)
            CopyImage(FooterImage, buffer, index, FooterLength)

            Return buffer
        End Get
    End Property

    ' This property is not typically overriden
    Protected Overridable Sub ParseBinaryImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Int32) Implements IChannel.ParseBinaryImage

        ' Parse out header, body and footer images
        ParseHeaderImage(state, binaryImage, startIndex)
        startIndex += HeaderLength

        ParseBodyImage(state, binaryImage, startIndex)
        startIndex += BodyLength

        ParseFooterImage(state, binaryImage, startIndex)

    End Sub

    Protected Overridable Sub ParseHeaderImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Int32)
    End Sub

    Protected Overridable Sub ParseBodyImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Int32)
    End Sub

    Protected Overridable Sub ParseFooterImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Int32)
    End Sub

    Protected Overridable ReadOnly Property HeaderLength() As UInt16
        Get
            Return 0
        End Get
    End Property

    Protected Overridable ReadOnly Property HeaderImage() As Byte()
        Get
            Return Nothing
        End Get
    End Property

    Protected Overridable ReadOnly Property BodyLength() As UInt16
        Get
            Return 0
        End Get
    End Property

    Protected Overridable ReadOnly Property BodyImage() As Byte()
        Get
            Return Nothing
        End Get
    End Property

    Protected Overridable ReadOnly Property FooterLength() As UInt16
        Get
            Return 0
        End Get
    End Property

    Protected Overridable ReadOnly Property FooterImage() As Byte()
        Get
            Return Nothing
        End Get
    End Property

    Public Overridable ReadOnly Property Attributes() As Dictionary(Of String, String) Implements IChannel.Attributes
        Get
            ' Create a new attributes dictionary or clear the contents of any existing one
            If m_attributes Is Nothing Then
                m_attributes = New Dictionary(Of String, String)
            Else
                m_attributes.Clear()
            End If

            m_attributes.Add("Derived Type", DerivedType.FullName)
            m_attributes.Add("Binary Length", BinaryLength)

            Return m_attributes
        End Get
    End Property

    ''' <summary>User definable tag used to hold a reference associated with channel data</summary>
    Public Overridable Property Tag() As Object Implements IChannel.Tag
        Get
            Return m_tag
        End Get
        Set(ByVal value As Object)
            m_tag = value
        End Set
    End Property

End Class