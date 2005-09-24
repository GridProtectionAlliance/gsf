'*******************************************************************************************************
'  ChannelCollectionBase.vb - Channel data collection base class
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
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
Imports TVA.EE.Phasor.Common

Namespace EE.Phasor

    ' This class represents the common implementation of the protocol independent representation of a collection of any kind of data.
    ' By having our collections implement IChannel (inherited via IChannelCollection), we have the benefit of providing a binary image
    ' of the entire collection
    Public MustInherit Class ChannelCollectionBase

        Inherits CollectionBase
        Implements IChannelCollection

        Private m_maximumCount As Integer

        Protected Sub New(ByVal maximumCount As Integer)

            m_maximumCount = maximumCount

        End Sub

        Public Sub Add(ByVal value As IChannel) Implements IChannelCollection.Add

            ' Note: Maximum count is much easier to specify by using <value>.MaxValue which runs from 0 to MaxValue (i.e., MaxValue + 1)
            ' so we allow one extra item in the following check to keep from having to add 1 to all maximum count specifications
            If List.Count > m_maximumCount Then Throw New OverflowException("Maximum " & InheritedType.Name & " item limit reached")
            List.Add(value)

        End Sub

        Default Public ReadOnly Property Item(ByVal index As Integer) As IChannel Implements IChannelCollection.Item
            Get
                Return List.Item(index)
            End Get
        End Property

        Public MustOverride ReadOnly Property InheritedType() As Type Implements IChannelCollection.InheritedType

        Public ReadOnly Property This() As IChannel Implements IChannel.This
            Get
                Return Me
            End Get
        End Property

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overridable Sub ParseBinaryImage(ByVal state As IChannelParsingState, ByVal binaryImage() As Byte, ByVal startIndex As Integer) Implements IChannelCollection.ParseBinaryImage

            Throw New NotImplementedException("Binary images are not intended to be parsed at a collection level")

        End Sub

        Public Overridable ReadOnly Property BinaryLength() As Int16 Implements IChannelCollection.BinaryLength
            Get
                If List.Count > 0 Then
                    Return Item(0).BinaryLength * List.Count
                Else
                    Return 0
                End If
            End Get
        End Property

        Public Overridable ReadOnly Property BinaryImage() As Byte() Implements IChannelCollection.BinaryImage
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)
                Dim index As Integer

                For x As Integer = 0 To List.Count - 1
                    CopyImage(Item(x), buffer, index)
                Next

                Return buffer
            End Get
        End Property

    End Class

End Namespace
