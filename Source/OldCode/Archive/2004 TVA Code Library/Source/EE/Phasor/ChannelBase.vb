'***********************************************************************
'  ChannelBase.vb - Channel data base class
'  Copyright © 2004 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  3/7/2005 - James R Carroll
'       Initial version of source generated
'
'***********************************************************************

Imports System.Buffer

Namespace EE.Phasor

    ' This class represents the common implementation of the protocol independent definition of any kind of data.
    Public MustInherit Class ChannelBase

        Implements IChannel

        Public MustOverride ReadOnly Property InheritedType() As Type Implements IChannel.InheritedType

        Public Overridable ReadOnly Property This() As IChannel Implements IChannel.This
            Get
                Return Me
            End Get
        End Property

        Public MustOverride ReadOnly Property BinaryLength() As Int16 Implements IChannel.BinaryLength

        Public MustOverride ReadOnly Property BinaryImage() As Byte() Implements IChannel.BinaryImage

        ' This is a common optimized block copy function for any kind of data
        Public Sub CopyImage(ByVal channel As IChannel, ByVal buffer As Byte(), ByRef index As Integer) Implements IChannel.CopyImage

            With channel
                Dim length As Int16 = .BinaryLength
                BlockCopy(.BinaryImage, 0, buffer, index, length)
                index += length
            End With

        End Sub

    End Class

End Namespace
