'*******************************************************************************************************
'  IConfigurationCellParsingState.vb - Configuration cell parsing state interface
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
'  04/16/2005 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

' This interface represents the protocol independent parsing state of a set of configuration related data settings (typically related to a PMU).
Public Interface IConfigurationCellParsingState

    Inherits IChannelCellParsingState

    ReadOnly Property PhasorDefinitionType() As Type

    ReadOnly Property FrequencyDefinitionType() As Type

    ReadOnly Property AnalogDefinitionType() As Type

    ReadOnly Property DigitalDefinitionType() As Type

End Interface
