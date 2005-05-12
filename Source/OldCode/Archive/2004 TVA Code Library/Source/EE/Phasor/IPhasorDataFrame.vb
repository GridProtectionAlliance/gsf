'***********************************************************************
'  IPhasorDataFrame.vb - Phasor data frame interface
'  Copyright © 2004 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [WESTAFF]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  01/14/2005 - James R Carroll
'       Initial version of source generated
'
'***********************************************************************

Imports TVA.Interop
Imports TVA.Shared.Bit
Imports TVA.Shared.DateTime
Imports TVA.Compression.Common

Namespace EE.Phasor

    ' This interface represents the protocol independent representation of any frame of phasor data.
    Public Interface IPhasorDataFrame

        Inherits IChannelFrame

        Property PhasorFormat() As PhasorFormat

        Property StatusFlags() As Int16

        ReadOnly Property PhasorValues() As PhasorValueCollection

        ReadOnly Property FrequencyValue() As IFrequencyValue

        ReadOnly Property DigitalValues() As DigitalValueCollection

    End Interface

End Namespace