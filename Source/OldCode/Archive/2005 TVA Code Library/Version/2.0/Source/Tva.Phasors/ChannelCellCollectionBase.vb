'*******************************************************************************************************
'  ChannelCellCollectionBase.vb - Channel data cell collection base class
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

' This class represents the common implementation of the protocol independent representation of a collection of any kind of data cell.
Public MustInherit Class ChannelCellCollectionBase

    Inherits ChannelCollectionBase(Of IChannelCell)
    Implements IChannelCellCollection

    Private m_constantCellLength As Boolean

    Protected Sub New(ByVal maximumCount As Integer, ByVal constantCellLength As Boolean)

        MyBase.New(maximumCount)

        m_constantCellLength = constantCellLength

    End Sub

    Public Overrides ReadOnly Property BinaryLength() As Int16
        Get
            If m_constantCellLength Then
                ' Cells will be constant length, so we can quickly calculate lengths
                Return MyBase.BinaryLength
            Else
                ' Cells will be different lengths, so we must manually sum lengths
                Dim length As Int16

                For x As Integer = 0 To Count - 1
                    length += Item(x).BinaryLength
                Next

                Return length
            End If
        End Get
    End Property

End Class
