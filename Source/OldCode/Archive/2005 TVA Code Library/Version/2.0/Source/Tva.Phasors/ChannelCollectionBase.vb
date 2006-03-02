'*******************************************************************************************************
'  ChannelCollectionBase.vb - Channel data collection base class
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

Imports System.ComponentModel
Imports Tva.Phasors.Common

' This class represents the common implementation of the protocol independent representation of a collection of any kind of data.
' By having our collections implement IChannel (inherited via IChannelCollection), we have the benefit of providing a binary image
' of the entire collection
Public MustInherit Class ChannelCollectionBase(Of T As IChannel)

    Inherits List(Of T)
    Implements IChannelCollection(Of T)

    Private m_maximumCount As Integer

    Protected Sub New(ByVal maximumCount As Integer)

        m_maximumCount = maximumCount

    End Sub

    Public Shadows Sub Add(ByVal value As T)

        ' Note: Maximum count is much easier to specify by using <value>.MaxValue which runs from 0 to MaxValue (i.e., MaxValue + 1)
        ' so we allow one extra item in the following check to keep from having to add 1 to all maximum count specifications
        If Count > m_maximumCount Then Throw New OverflowException("Maximum " & InheritedType.Name & " item limit reached")
        MyBase.Add(value)

    End Sub

    Private ReadOnly Property IChannelCollectionItem(ByVal index As Integer) As T
        Get
            Return Me(index)
        End Get
    End Property

    Public MustOverride ReadOnly Property InheritedType() As Type Implements IChannelCollection(Of T).InheritedType

    Public ReadOnly Property This() As IChannel Implements IChannel.This
        Get
            Return Me
        End Get
    End Property

    Public Property MaximumCount() As Integer
        Get
            Return m_maximumCount
        End Get
        Set(ByVal value As Integer)
            m_maximumCount = value
        End Set
    End Property

    <EditorBrowsable(EditorBrowsableState.Never)> _
    Public Overridable Sub ParseBinaryImage(ByVal state As IChannelParsingState, ByVal binaryImage() As Byte, ByVal startIndex As Integer) Implements IChannelCollection(Of T).ParseBinaryImage

        Throw New NotImplementedException("Binary images are not intended to be parsed at a collection level")

    End Sub

    Public Overridable ReadOnly Property BinaryLength() As Int16 Implements IChannelCollection(Of T).BinaryLength
        Get
            If Count > 0 Then
                Return Item(0).BinaryLength * Count
            Else
                Return 0
            End If
        End Get
    End Property

    Public Overridable ReadOnly Property BinaryImage() As Byte() Implements IChannelCollection(Of T).BinaryImage
        Get
            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)
            Dim index As Integer

            For x As Integer = 0 To Count - 1
                CopyImage(Item(x), buffer, index)
            Next

            Return buffer
        End Get
    End Property

End Class

