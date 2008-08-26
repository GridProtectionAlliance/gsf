'*******************************************************************************************************
'  IConfigurationCellParsingState.vb - Configuration cell parsing state interface
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
'  04/16/2005 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

''' <summary>This interface represents the protocol independent parsing state of a set of configuration related data settings (typically related to a PMU).</summary>
<CLSCompliant(False)> _
Public Interface IConfigurationCellParsingState

    Inherits IChannelCellParsingState

    Delegate Function CreateNewDefinitionFunctionSignature(Of T As IChannelDefinition)(ByVal parent As IConfigurationCell, ByVal binaryImage As Byte(), ByVal startIndex As Int32) As T

    ReadOnly Property CreateNewPhasorDefintionFunction() As CreateNewDefinitionFunctionSignature(Of IPhasorDefinition)

    ReadOnly Property CreateNewFrequencyDefintionFunction() As CreateNewDefinitionFunctionSignature(Of IFrequencyDefinition)

    ReadOnly Property CreateNewAnalogDefintionFunction() As CreateNewDefinitionFunctionSignature(Of IAnalogDefinition)

    ReadOnly Property CreateNewDigitalDefintionFunction() As CreateNewDefinitionFunctionSignature(Of IDigitalDefinition)

End Interface