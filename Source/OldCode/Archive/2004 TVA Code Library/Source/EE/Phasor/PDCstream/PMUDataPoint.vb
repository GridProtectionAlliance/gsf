'***********************************************************************
'  PMUDataPoint.vb - PDCstream PMUDataPoint structure
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
'  11/12/2004 - James R Carroll
'       Initial version of source generated
'
'***********************************************************************

Namespace EE.Phasor.PDCstream

    Public Structure PMUDataPoint

        Public PMU As PMUDefinition
        Public Type As PointType
        Public ArchiveStyle As PointArchiveStyle
        Public ReceivedOnChange As Boolean
        Public Index As Integer
        Public Timestamp As DateTime
        Public Value As Double

    End Structure

End Namespace
