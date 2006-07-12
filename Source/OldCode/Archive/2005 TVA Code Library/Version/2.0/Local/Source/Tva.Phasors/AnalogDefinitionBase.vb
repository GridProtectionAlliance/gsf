'*******************************************************************************************************
'  AnalogDefinitionBase.vb - Analog value definition base class
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

' This class represents the common implementation of the protocol independent definition of an analog value.
<CLSCompliant(False)> _
Public MustInherit Class AnalogDefinitionBase

    Inherits ChannelDefinitionBase
    Implements IAnalogDefinition

    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

        MyBase.New(info, context)

    End Sub

    Protected Sub New(ByVal parent As IConfigurationCell)

        MyBase.New(parent)

    End Sub

    Protected Sub New(ByVal parent As IConfigurationCell, ByVal index As Int32, ByVal label As String, ByVal scale As Int32, ByVal offset As Single)

        MyBase.New(parent, index, label, scale, offset)

    End Sub

    Protected Sub New(ByVal parent As IConfigurationCell, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

        MyBase.New(parent, binaryImage, startIndex)

    End Sub

    ' Derived classes are expected to expose a Public Sub New(ByVal analogDefinition As IAnalogDefinition)
    Protected Sub New(ByVal analogDefinition As IAnalogDefinition)

        MyClass.New(analogDefinition.Parent, analogDefinition.Index, analogDefinition.Label, _
            analogDefinition.ScalingFactor, analogDefinition.Offset)

    End Sub

    Public Overrides ReadOnly Property DataFormat() As DataFormat
        Get
            Return Parent.AnalogDataFormat
        End Get
    End Property

End Class
