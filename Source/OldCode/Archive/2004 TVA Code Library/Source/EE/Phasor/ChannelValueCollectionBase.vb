'*******************************************************************************************************
'  ChannelValueCollectionBase.vb - Channel data value collection base class
'  Copyright © 2004 - TVA, all rights reserved - Gbtc
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

Namespace EE.Phasor

    ' This class represents the common implementation of the protocol independent representation of a collection of any kind of data value.
    Public MustInherit Class ChannelValueCollectionBase

        Inherits ChannelCollectionBase

        Private m_fixedCount As Integer
        Private m_floatCount As Integer

        Protected Sub New(ByVal maximumCount As Integer)

            MyBase.New(maximumCount)

        End Sub

        Public Shadows Sub Add(ByVal value As IChannelValue)

            ' In typical usage, all channel values will be of the same data type - but we can't anticipate all
            ' possible uses of collection, so we track totals of each data type so we can quickly ascertain if
            ' all the items in the collection are of the same data type
            If value.DataFormat = Phasor.DataFormat.FixedInteger Then
                m_fixedCount += 1
            Else
                m_floatCount += 1
            End If

            MyBase.Add(value)

        End Sub

        Default Public Shadows ReadOnly Property Item(ByVal index As Integer) As IChannelValue
            Get
                Return MyBase.Item(index)
            End Get
        End Property

        Public Overrides ReadOnly Property BinaryLength() As Int16
            Get
                If List.Count > 0 Then
                    If m_fixedCount = 0 Or m_floatCount = 0 Then
                        ' Data types in list are consistent, a simple calculation will derive total binary length
                        Return Item(0).BinaryLength * List.Count
                    Else
                        ' List has items of different data types, will have to traverse list to calculate total binary length
                        Dim length As Integer

                        For x As Integer = 0 To List.Count - 1
                            length += Item(x).BinaryLength
                        Next

                        Return length
                    End If
                Else
                    Return 0
                End If
            End Get
        End Property

        Public ReadOnly Property IsEmpty() As Boolean
            Get
                Dim empty As Boolean = True

                For x As Integer = 0 To List.Count - 1
                    If Not Item(x).IsEmpty Then
                        empty = False
                        Exit For
                    End If
                Next

                Return empty
            End Get
        End Property

        Protected Overrides Sub OnClearComplete()

            m_fixedCount = 0
            m_floatCount = 0

        End Sub

    End Class

End Namespace
