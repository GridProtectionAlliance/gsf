'*******************************************************************************************************
'  Tva.Measurements.IMeasurement.vb - Abstract measurement interface
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  12/8/2004 - James R Carroll
'       Initial version of source generated for Real-Time Frequency Monitor
'  02/02/2006 - james R Carroll
'       Integrated into code library
'
'*******************************************************************************************************

Public Interface IMeasurement

    ReadOnly Property This() As IMeasurement
    Property Index() As Integer
    Property Value() As Double
    Property Timestamp() As Date

End Interface
