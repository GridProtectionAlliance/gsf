'*******************************************************************************************************
'  AnalogDefinition.vb - Protocol Independent Analog definition
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
Imports System.Text
Imports PhasorProtocols

<CLSCompliant(False), Serializable()> _
Public Class AnalogDefinition

    Inherits AnalogDefinitionBase

    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

        MyBase.New(info, context)

    End Sub

    Public Sub New(ByVal parent As ConfigurationCell)

        MyBase.New(parent)

    End Sub

    Public Sub New(ByVal parent As ConfigurationCell, ByVal index As Int32, ByVal label As String, ByVal scale As Int32, ByVal offset As Single)

        MyBase.New(parent, index, label, 1, 0)

    End Sub

    Public Sub New(ByVal parent As ConfigurationCell, ByVal analogDefinition As IAnalogDefinition)

        MyBase.New(parent, analogDefinition)

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
