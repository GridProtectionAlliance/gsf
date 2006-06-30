'*******************************************************************************************************
'  IDataCell.vb - Data cell interface
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

Imports Tva.Measurements

' This interface represents the protocol independent representation of a set of phasor related data values.
<CLSCompliant(False)> _
Public Interface IDataCell

    Inherits IChannelCell, IMeasurement

    Shadows ReadOnly Property Parent() As IDataFrame

    Shadows ReadOnly Property This() As IDataCell

    Property ConfigurationCell() As IConfigurationCell

    Property StatusFlags() As Int16

    ReadOnly Property AllValuesAreEmpty() As Boolean

    ReadOnly Property PhasorValues() As PhasorValueCollection

    Property FrequencyValue() As IFrequencyValue

    ReadOnly Property AnalogValues() As AnalogValueCollection

    ReadOnly Property DigitalValues() As DigitalValueCollection

    Property SynchronizationIsValid() As Boolean

    Property DataIsValid() As Boolean

End Interface
