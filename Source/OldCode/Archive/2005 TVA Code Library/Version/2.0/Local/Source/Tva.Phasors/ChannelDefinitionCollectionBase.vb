'*******************************************************************************************************
'  ChannelDefinitionCollectionBase.vb - Channel data definition collection base class
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
'  3/7/2005 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Runtime.Serialization

' This class represents the common implementation of the protocol independent collection of definitions of any kind of data.
<CLSCompliant(False)> _
Public MustInherit Class ChannelDefinitionCollectionBase(Of T As IChannelDefinition)

    Inherits ChannelCollectionBase(Of T)

    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

        MyBase.New(info, context)

    End Sub

    Protected Sub New(ByVal maximumCount As Int32)

        MyBase.New(maximumCount)

    End Sub

End Class

