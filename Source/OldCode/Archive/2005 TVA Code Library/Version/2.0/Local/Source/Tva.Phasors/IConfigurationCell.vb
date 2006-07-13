'*******************************************************************************************************
'  IConfigurationCell.vb - Configuration cell interface
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

''' <summary>This interface represents the protocol independent representation of a set of configuration related data settings (typically related to a PMU).</summary>
<CLSCompliant(False)> _
Public Interface IConfigurationCell

    Inherits IChannelCell, IComparable

    Shadows ReadOnly Property Parent() As IConfigurationFrame

    Property StationName() As String

    ReadOnly Property StationNameImage() As Byte()

    ReadOnly Property MaximumStationNameLength() As Int32

    Property IDLabel() As String

    ReadOnly Property IDLabelImage() As Byte()

    ReadOnly Property IDLabelLength() As Int32

    ReadOnly Property PhasorDefinitions() As PhasorDefinitionCollection

    Property PhasorDataFormat() As DataFormat

    Property PhasorCoordinateFormat() As CoordinateFormat

    Property FrequencyDefinition() As IFrequencyDefinition

    Property FrequencyDataFormat() As DataFormat

    Property NominalFrequency() As LineFrequency

    ReadOnly Property AnalogDefinitions() As AnalogDefinitionCollection

    Property AnalogDataFormat() As DataFormat

    ReadOnly Property DigitalDefinitions() As DigitalDefinitionCollection

    ReadOnly Property FrameRate() As Int16

End Interface
