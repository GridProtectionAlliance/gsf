'*******************************************************************************************************
'  ChannelValueCollectionBase.vb - Channel data value collection base class
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

''' <summary>This class represents the common implementation of the protocol independent representation of a collection of any kind of data value.</summary>
<CLSCompliant(False), Serializable()> _
Public MustInherit Class ChannelValueCollectionBase(Of TDefinition As IChannelDefinition, TValue As IChannelValue(Of TDefinition))

    Inherits ChannelCollectionBase(Of TValue)

    Private m_fixedCount As Int32
    Private m_floatCount As Int32

    Protected Sub New()
    End Sub

    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

        MyBase.New(info, context)

    End Sub

    Protected Sub New(ByVal maximumCount As Int32)

        MyBase.New(maximumCount)

    End Sub

    Public Overridable Shadows Sub Add(ByVal value As TValue)

        ' In typical usage, all channel values will be of the same data type - but we can't anticipate all
        ' possible uses of collection, so we track totals of each data type so we can quickly ascertain if
        ' all the items in the collection are of the same data type
        If value.DataFormat = PhasorProtocols.DataFormat.FixedInteger Then
            m_fixedCount += 1
        Else
            m_floatCount += 1
        End If

        MyBase.Add(value)

    End Sub

    Public Overrides ReadOnly Property BinaryLength() As UInt16
        Get
            If Count > 0 Then
                If m_fixedCount = 0 OrElse m_floatCount = 0 Then
                    ' Data types in list are consistent, a simple calculation will derive total binary length
                    Return Item(0).BinaryLength * Count
                Else
                    ' List has items of different data types, will have to traverse list to calculate total binary length
                    Dim length As UInt16

                    For x As Int32 = 0 To Count - 1
                        length += Item(x).BinaryLength
                    Next

                    Return length
                End If
            Else
                Return 0
            End If
        End Get
    End Property

    Public Overridable ReadOnly Property AllValuesAssigned() As Boolean
        Get
            Dim allAssigned As Boolean = True

            For x As Int32 = 0 To Count - 1
                If Item(x).IsEmpty Then
                    allAssigned = False
                    Exit For
                End If
            Next

            Return allAssigned
        End Get
    End Property

    Public Overridable Shadows Sub Clear()

        MyBase.Clear()
        m_fixedCount = 0
        m_floatCount = 0

    End Sub

    Public Overrides ReadOnly Property Attributes() As Dictionary(Of String, String)
        Get
            Dim baseAttributes As Dictionary(Of String, String) = MyBase.Attributes

            baseAttributes.Add("Fixed Count", m_fixedCount)
            baseAttributes.Add("Float Count", m_floatCount)
            baseAttributes.Add("All Values Assigned", AllValuesAssigned)

            Return baseAttributes
        End Get
    End Property

End Class
