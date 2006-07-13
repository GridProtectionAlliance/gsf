'*******************************************************************************************************
'  ChannelFrameCollectionBase.vb - Channel data frame collection base class
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  01/14/2005 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Runtime.Serialization

''' <summary>This class represents the protocol independent common implementation of a collection of any frame of data that can be sent or received from a PMU.</summary>
<CLSCompliant(False), Serializable()> _
Public MustInherit Class ChannelFrameCollectionBase(Of T As IChannelFrame)

    Inherits ChannelCollectionBase(Of T)

    Protected Sub New()
    End Sub

    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

        MyBase.New(info, context)

    End Sub

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
