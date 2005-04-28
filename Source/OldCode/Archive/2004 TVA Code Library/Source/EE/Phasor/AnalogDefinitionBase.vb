'***********************************************************************
'  AnalogDefinitionBase.vb - Analog value definition base class
'  Copyright © 2005 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [WESTAFF]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  02/18/2005 - James R Carroll
'       Initial version of source generated
'
'***********************************************************************

Namespace EE.Phasor

    ' This class represents the common implementation of the protocol independent definition of an analog value.
    Public MustInherit Class AnalogDefinitionBase

        Inherits ChannelDefinitionBase
        Implements IAnalogDefinition

        ' Create analog definition from other analog definition
        ' Note: This method is expected to be implemented as a public shared method in derived class automatically passing in analogDefinitionType
        ' Dervied class must expose a Public Sub New(ByVal analogDefinition As IAnalogDefinition)
        Protected Shared Shadows Function CreateFrom(ByVal analogDefinitionType As Type, ByVal analogDefinition As IAnalogDefinition) As IAnalogDefinition

            Return CType(Activator.CreateInstance(analogDefinitionType, New Object() {analogDefinition}), IAnalogDefinition)

        End Function

        Protected Sub New(ByVal index As Integer, ByVal label As String, ByVal scale As Integer, ByVal offset As Double)

            MyBase.New(index, label, scale, offset)

        End Sub

        Protected Sub New(ByVal analogDefinition As IAnalogDefinition)

            Me.New(analogDefinition.Index, analogDefinition.Label, analogDefinition.ScalingFactor, analogDefinition.Offset)

        End Sub

    End Class

End Namespace