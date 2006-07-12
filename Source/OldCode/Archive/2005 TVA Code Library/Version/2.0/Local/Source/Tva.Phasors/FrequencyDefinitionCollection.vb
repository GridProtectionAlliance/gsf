'*******************************************************************************************************
'  FrequencyDefinitionCollection.vb - Frequency and df/dt value definition collection class
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
'  02/18/2005 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Runtime.Serialization

' This class represents the common implementation collection of the protocol independent definitions of frequency and df/dt values.
<CLSCompliant(False)> _
Public Class FrequencyDefinitionCollection

    Inherits ChannelDefinitionCollectionBase(Of IFrequencyDefinition)

    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

        MyBase.New(info, context)

    End Sub

    Public Sub New(ByVal maximumCount As Int32)

        MyBase.New(maximumCount)

    End Sub

    Public Overrides ReadOnly Property InheritedType() As Type
        Get
            Return Me.GetType()
        End Get
    End Property

End Class
