'*******************************************************************************************************
'  IDataCell.vb - Data cell interface
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

' This interface represents the protocol independent representation of a set of phasor related data values.
Public Interface IDataCell

    Inherits IChannelCell

    Shadows ReadOnly Property Parent() As IDataFrame

    Property ConfigurationCell() As IConfigurationCell

    Property StatusFlags() As Int16

    ReadOnly Property IsEmpty() As Boolean

    ReadOnly Property PhasorValues() As PhasorValueCollection

    Property FrequencyValue() As IFrequencyValue

    ReadOnly Property AnalogValues() As AnalogValueCollection

    ReadOnly Property DigitalValues() As DigitalValueCollection

End Interface
