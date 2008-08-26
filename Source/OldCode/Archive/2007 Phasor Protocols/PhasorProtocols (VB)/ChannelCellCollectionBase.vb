'*******************************************************************************************************
'  ChannelCellCollectionBase.vb - Channel data cell collection base class
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

''' <summary>This class represents the common implementation of the protocol independent representation of a collection of any kind of data cell.</summary>
<CLSCompliant(False), Serializable()> _
Public MustInherit Class ChannelCellCollectionBase(Of T As IChannelCell)

    Inherits ChannelCollectionBase(Of T)
    Implements IChannelCellCollection(Of T)

    Private m_constantCellLength As Boolean

    Protected Sub New()
    End Sub

    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

        MyBase.New(info, context)

        ' Deserialize extra elements
        m_constantCellLength = info.GetBoolean("constantCellLength")

    End Sub

    Protected Sub New(ByVal maximumCount As Int32, ByVal constantCellLength As Boolean)

        MyBase.New(maximumCount)

        m_constantCellLength = constantCellLength

    End Sub

    Public Overrides ReadOnly Property BinaryLength() As UInt16
        Get
            If m_constantCellLength Then
                ' Cells will be constant length, so we can quickly calculate lengths
                Return MyBase.BinaryLength
            Else
                ' Cells will be different lengths, so we must manually sum lengths
                Dim length As UInt16

                For x As Int32 = 0 To Count - 1
                    length += Item(x).BinaryLength
                Next

                Return length
            End If
        End Get
    End Property

    Public Overrides Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext)

        MyBase.GetObjectData(info, context)

        ' Serialize extra elements
        info.AddValue("constantCellLength", m_constantCellLength)

    End Sub

    Public Overrides ReadOnly Property Attributes() As Dictionary(Of String, String)
        Get
            Dim baseAttributes As Dictionary(Of String, String) = MyBase.Attributes

            baseAttributes.Add("Constant Cell Length", m_constantCellLength)

            Return baseAttributes
        End Get
    End Property

End Class