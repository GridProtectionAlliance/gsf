'*******************************************************************************************************
'  PhasorDefinition.vb - Protocol Independent Phasor definition
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
'  11/12/2004 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Option Strict Off
Imports System.Runtime.Serialization
Imports PhasorProtocols

<CLSCompliant(False), Serializable()> _
Public Class PhasorDefinition

    Inherits PhasorDefinitionBase

    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

        MyBase.New(info, context)

    End Sub

    Public Sub New(ByVal parent As ConfigurationCell)

        MyBase.New(parent)

    End Sub

    Public Sub New(ByVal parent As ConfigurationCell, ByVal index As Int32, ByVal label As String, ByVal scale As Int32, ByVal offset As Single, ByVal type As PhasorType, ByVal voltageReference As PhasorDefinition)

        MyBase.New(parent, index, label, scale, offset, type, voltageReference)

    End Sub

    Public Sub New(ByVal parent As ConfigurationCell, ByVal phasorDefinition As IPhasorDefinition)

        MyBase.New(parent, phasorDefinition)

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
