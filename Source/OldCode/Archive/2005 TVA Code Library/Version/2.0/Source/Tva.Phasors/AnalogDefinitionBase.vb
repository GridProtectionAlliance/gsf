'*******************************************************************************************************
'  AnalogDefinitionBase.vb - Analog value definition base class
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
'  02/18/2005 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

' This class represents the common implementation of the protocol independent definition of an analog value.
<CLSCompliant(False)> _
Public MustInherit Class AnalogDefinitionBase

    Inherits ChannelDefinitionBase
    Implements IAnalogDefinition

    Protected Sub New(ByVal parent As IConfigurationCell)

        MyBase.New(parent)

    End Sub

    Protected Sub New(ByVal parent As IConfigurationCell, ByVal dataFormat As DataFormat, ByVal index As Integer, ByVal label As String, ByVal scale As Integer, ByVal offset As Double)

        MyBase.New(parent, dataFormat, index, label, scale, offset)

    End Sub

    ' Derived classes are expected to expose a Public Sub New(ByVal analogDefinition As IAnalogDefinition)
    Protected Sub New(ByVal analogDefinition As IAnalogDefinition)

        MyClass.New(analogDefinition.Parent, analogDefinition.DataFormat, analogDefinition.Index, analogDefinition.Label, _
            analogDefinition.ScalingFactor, analogDefinition.Offset)

    End Sub

End Class
