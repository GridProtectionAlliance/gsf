'*******************************************************************************************************
'  ChannelFrameCollectionBase.vb - Channel data frame collection base class
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
'  01/14/2005 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

' This class represents the protocol independent common implementation of a collection of any frame of data that can be sent or received from a PMU.
<CLSCompliant(False)> _
Public MustInherit Class ChannelFrameCollectionBase(Of T As IChannelFrame)

    Inherits ChannelCollectionBase(Of T)

    Protected Sub New(ByVal maximumCount As Int32)

        MyBase.New(maximumCount)

    End Sub

    Public Overrides ReadOnly Property BinaryLength() As UInt16
        Get
            ' Frames will be different lengths, so we must manually sum lengths
            Dim length As UInt16

            For x As Int32 = 0 To Count - 1
                length += Item(x).BinaryLength
            Next

            Return length
        End Get
    End Property

End Class
