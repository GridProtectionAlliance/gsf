'*******************************************************************************************************
'  FrequencyDefinition.vb - Protocol Independent Frequency definition
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
'  11/12/2004 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Runtime.Serialization
Imports PhasorProtocols

<CLSCompliant(False), Serializable()> _
Public Class FrequencyDefinition

    Inherits FrequencyDefinitionBase

    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

        MyBase.New(info, context)

    End Sub

    Public Sub New(ByVal parent As ConfigurationCell)

        MyBase.New(parent)
        ScalingFactor = 1000
        DfDtScalingFactor = 100

    End Sub

    Public Sub New(ByVal parent As ConfigurationCell, ByVal label As String, ByVal scale As Int32, ByVal offset As Single, ByVal dfdtScale As Int32, ByVal dfdtOffset As Single)

        MyBase.New(parent, label, scale, offset, dfdtScale, dfdtOffset)

    End Sub

    Public Sub New(ByVal frequencyDefinition As IFrequencyDefinition)

        MyBase.New(frequencyDefinition)

    End Sub

    Public Overrides ReadOnly Property DerivedType() As System.Type
        Get
            Return Me.GetType
        End Get
    End Property

    Public Shadows ReadOnly Property Parent() As ConfigurationCell
        Get
            Return MyBase.Parent
        End Get
    End Property

End Class
