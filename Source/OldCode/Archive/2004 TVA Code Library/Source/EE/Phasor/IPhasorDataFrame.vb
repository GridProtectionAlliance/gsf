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

Namespace EE.Phasor

    ' This interface represents the protocol independent representation of any frame of phasor data.
    Public Interface IPhasorDataFrame

        Inherits IChannelFrame

        ReadOnly Property PhasorDataCells() As PhasorDataCellCollection

    End Interface

End Namespace