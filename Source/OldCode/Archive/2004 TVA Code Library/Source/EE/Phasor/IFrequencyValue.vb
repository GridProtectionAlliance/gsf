'***********************************************************************
'  IFrequencyValue.vb - Frequency value interface
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

    ' This class represents the protocol independent interface of a frequency value.
    Public Interface IFrequencyValue

        Inherits IChannelValue

        ReadOnly Property Definition() As IFrequencyDefinition

        Property Frequency() As Double

        Property DfDt() As Double

        Property UnscaledFrequency() As Int16

        Property UnscaledDfDt() As Int16

    End Interface

End Namespace