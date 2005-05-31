'***********************************************************************
'  IFrequencyDefinition.vb - Frequency and df/dt value definition interface
'  Copyright © 2005 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
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

    ' This interface represents a protocol independent frequency and df/dt value definition.
    Public Interface IFrequencyDefinition

        Inherits IChannelDefinition

        ReadOnly Property NominalFrequency() As LineFrequency

        Property DfDtOffset() As Double

        Property DfDtScalingFactor() As Integer

        ReadOnly Property MaximumDfDtScalingFactor() As Integer

    End Interface

End Namespace