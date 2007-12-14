'*******************************************************************************************************
'  ChannelCollectionBase.vb - Channel data collection base class
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
'  3/7/2005 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Runtime.Serialization
Imports System.ComponentModel
Imports TVA.Parsing
Imports PhasorProtocols.Common

''' <summary>This class represents the common implementation of the protocol independent representation of a collection of any kind of data.</summary>
''' <remarks>By having our collections implement IChannel (inherited via IChannelCollection), we have the benefit of providing a binary image of the entire collection</remarks>
<CLSCompliant(False), Serializable()> _
Public MustInherit Class ChannelCollectionBase(Of T As IChannel)

    Inherits List(Of T)
    Implements IChannelCollection(Of T)

    Private m_maximumCount As Int32
    Private m_attributes As Dictionary(Of String, String)
    Private m_tag As Object

    Protected Sub New()
    End Sub

    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

        ' Deserialize collection
        m_maximumCount = info.GetInt32("maximumCount")

        For x As Integer = 0 To info.GetInt32("count") - 1
            Add(info.GetValue("item" & x, GetType(T)))
        Next

    End Sub

    Protected Sub New(ByVal maximumCount As Int32)

        m_maximumCount = maximumCount

    End Sub

    Public Overridable Shadows Sub Add(ByVal value As T)

        ' Note: Maximum count is much easier to specify by using <value>.MaxValue which runs from 0 to MaxValue (i.e., MaxValue + 1)
        ' so we allow one extra item in the following check to keep from having to add 1 to all maximum count specifications
        If Count > m_maximumCount Then Throw New OverflowException("Maximum " & DerivedType.Name & " item limit reached")
        MyBase.Add(value)

    End Sub

    Public MustOverride ReadOnly Property DerivedType() As Type Implements IChannelCollection(Of T).DerivedType

    Public Overridable ReadOnly Property This() As IChannel Implements IChannel.This
        Get
            Return Me
        End Get
    End Property

    Public Overridable Property MaximumCount() As Int32
        Get
            Return m_maximumCount
        End Get
        Set(ByVal value As Int32)
            m_maximumCount = value
        End Set
    End Property

    <EditorBrowsable(EditorBrowsableState.Never)> _
    Public Overridable Sub ParseBinaryImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Int32) Implements IChannelCollection(Of T).ParseBinaryImage

        Throw New NotImplementedException("Binary images are not intended to be parsed at a collection level")

    End Sub

    Public Overridable ReadOnly Property BinaryLength() As UInt16 Implements IChannel.BinaryLength
        Get
            If Count > 0 Then
                Return Item(0).BinaryLength * Count
            Else
                Return 0
            End If
        End Get
    End Property

    Private ReadOnly Property IBinaryDataProviderBinaryLength() As Integer Implements IBinaryDataProvider.BinaryLength
        Get
            Return BinaryLength
        End Get
    End Property

    Public Overridable ReadOnly Property BinaryImage() As Byte() Implements IBinaryDataProvider.BinaryImage
        Get
            Dim buffer As Byte() = CreateArray(Of Byte)(BinaryLength)
            Dim index As Int32

            For x As Int32 = 0 To Count - 1
                CopyImage(Item(x), buffer, index)
            Next

            Return buffer
        End Get
    End Property

    Public Overridable Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext) Implements System.Runtime.Serialization.ISerializable.GetObjectData

        ' Serialize collection
        info.AddValue("maximumCount", m_maximumCount)
        info.AddValue("count", Count)

        For x As Integer = 0 To Count - 1
            info.AddValue("item" & x, Item(x), GetType(T))
        Next

    End Sub

    Public Overridable ReadOnly Property Attributes() As Dictionary(Of String, String) Implements IChannel.Attributes
        Get
            ' Create a new attributes dictionary or clear the contents of any existing one
            If m_attributes Is Nothing Then
                m_attributes = New Dictionary(Of String, String)
            Else
                m_attributes.Clear()
            End If

            m_attributes.Add("Derived Type", DerivedType.Name)
            m_attributes.Add("Binary Length", BinaryLength)
            m_attributes.Add("Maximum Count", MaximumCount)
            m_attributes.Add("Current Count", Count)

            Return m_attributes
        End Get
    End Property

    Public Overridable Property Tag() As Object Implements IChannel.Tag
        Get
            Return m_tag
        End Get
        Set(ByVal value As Object)
            m_tag = value
        End Set

    End Property

End Class