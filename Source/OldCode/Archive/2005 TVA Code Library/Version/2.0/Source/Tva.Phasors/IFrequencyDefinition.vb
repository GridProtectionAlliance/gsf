'*******************************************************************************************************
'  IFrequencyDefinition.vb - Frequency and df/dt value definition interface
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
'  02/18/2005 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

' This interface represents a protocol independent frequency and df/dt value definition.
Public Interface IFrequencyDefinition

    Inherits IChannelDefinition

    ReadOnly Property NominalFrequency() As LineFrequency

    Property DfDtOffset() As Double

    Property DfDtScalingFactor() As Integer

    ReadOnly Property MaximumDfDtScalingFactor() As Integer

End Interface
