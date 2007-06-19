'*******************************************************************************************************
'  DigitalDefinition.vb - Protocol Independent Digital definition
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
Imports System.ComponentModel
Imports PhasorProtocols

<CLSCompliant(False), Serializable()> _
Public Class DigitalDefinition

    Inherits DigitalDefinitionBase

    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

        MyBase.New(info, context)

    End Sub

    Public Sub New(ByVal parent As ConfigurationCell)

        MyBase.New(parent)

    End Sub

    Public Sub New(ByVal parent As ConfigurationCell, ByVal index As Int32, ByVal label As String)

        MyBase.New(parent, index, label)

    End Sub

    Public Sub New(ByVal digitalDefinition As IDigitalDefinition)

        MyBase.New(digitalDefinition)

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
