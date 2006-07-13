'*******************************************************************************************************
'  IDataCellParsingState.vb - Data cell parsing state interface
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
'  04/16/2005 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

''' <summary>This interface represents the protocol independent parsing state of a set of phasor related data values.</summary>
<CLSCompliant(False)> _
Public Interface IDataCellParsingState

    Inherits IChannelCellParsingState

    Delegate Function CreateNewValueFunctionSignature(Of TDefinition As IChannelDefinition, TValue As IChannelValue(Of TDefinition))(ByVal parent As IDataCell, ByVal definition As TDefinition, ByVal binaryImage As Byte(), ByVal startIndex As Int32) As TValue

    ReadOnly Property ConfigurationCell() As IConfigurationCell

    ReadOnly Property CreateNewPhasorValueFunction() As CreateNewValueFunctionSignature(Of IPhasorDefinition, IPhasorValue)

    ReadOnly Property CreateNewFrequencyValueFunction() As CreateNewValueFunctionSignature(Of IFrequencyDefinition, IFrequencyValue)

    ReadOnly Property CreateNewAnalogValueFunction() As CreateNewValueFunctionSignature(Of IAnalogDefinition, IAnalogValue)

    ReadOnly Property CreateNewDigitalValueFunction() As CreateNewValueFunctionSignature(Of IDigitalDefinition, IDigitalValue)

End Interface
