'***********************************************************************
'  IConfigurationCell.vb - Configuration cell interface
'  Copyright © 2004 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  04/16/2005 - James R Carroll
'       Initial version of source generated
'
'***********************************************************************

Namespace EE.Phasor

    ' This interface represents the protocol independent representation of a set of configuration related data settings.
    Public Interface IConfigurationCell

        Inherits IChannelCell

        Property StationName() As String

        ReadOnly Property StationNameImage() As Byte()

        ReadOnly Property MaximumStationNameLength() As Integer

        Property IDCode() As Int16

        ReadOnly Property PhasorDefinitions() As PhasorDefinitionCollection

        Property FrequencyDefinition() As IFrequencyDefinition

        ReadOnly Property AnalogDefinitions() As AnalogDefinitionCollection

        ReadOnly Property DigitalDefinitions() As DigitalDefinitionCollection

    End Interface

End Namespace