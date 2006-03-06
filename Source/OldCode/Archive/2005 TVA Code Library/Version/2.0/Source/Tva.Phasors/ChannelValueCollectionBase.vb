'*******************************************************************************************************
'  ChannelValueCollectionBase.vb - Channel data value collection base class
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

' This class represents the common implementation of the protocol independent representation of a collection of any kind of data value.
<CLSCompliant(False)> _
Public MustInherit Class ChannelValueCollectionBase(Of TDefinition As IChannelDefinition, TValue As IChannelValue(Of TDefinition))

    Inherits ChannelCollectionBase(Of TValue)

    Private m_fixedCount As Integer
    Private m_floatCount As Integer

    Protected Sub New(ByVal maximumCount As Integer)

        MyBase.New(maximumCount)

    End Sub

    Public Shadows Sub Add(ByVal value As TValue)

        ' In typical usage, all channel values will be of the same data type - but we can't anticipate all
        ' possible uses of collection, so we track totals of each data type so we can quickly ascertain if
        ' all the items in the collection are of the same data type
        If value.DataFormat = Phasors.DataFormat.FixedInteger Then
            m_fixedCount += 1
        Else
            m_floatCount += 1
        End If

        MyBase.Add(value)

    End Sub

    Public Overrides ReadOnly Property BinaryLength() As Int16
        Get
            If Count > 0 Then
                If m_fixedCount = 0 Or m_floatCount = 0 Then
                    ' Data types in list are consistent, a simple calculation will derive total binary length
                    Return Item(0).BinaryLength * Count
                Else
                    ' List has items of different data types, will have to traverse list to calculate total binary length
                    Dim length As Integer

                    For x As Integer = 0 To Count - 1
                        length += Item(x).BinaryLength
                    Next

                    Return length
                End If
            Else
                Return 0
            End If
        End Get
    End Property

    Public Overridable ReadOnly Property AllValuesAreEmpty() As Boolean
        Get
            Dim allEmpty As Boolean = True

            For x As Integer = 0 To Count - 1
                If Not Item(x).IsEmpty Then
                    allEmpty = False
                    Exit For
                End If
            Next

            Return allEmpty
        End Get
    End Property

    Public Shadows Sub Clear()

        MyBase.Clear()
        m_fixedCount = 0
        m_floatCount = 0

    End Sub

End Class

