'*******************************************************************************************************
'  IFrequencyValue.vb - Frequency value interface
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

' This class represents the protocol independent interface of a frequency value.
Public Interface IFrequencyValue

    Inherits IChannelValue(Of IFrequencyDefinition)

    Property Frequency() As Double

    Property DfDt() As Double

    Property UnscaledFrequency() As Int16

    Property UnscaledDfDt() As Int16

End Interface
