'*******************************************************************************************************
'  IListeningAdapter.vb - Abstract incoming data adpater interface
'  Copyright © 2008 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2008
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  06/01/2006 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports TVA.Measurements

Public Interface IListeningAdapter

    Inherits IAdapter

    Event NewMeasurements(ByVal measurements As ICollection(Of IMeasurement))

    Sub Initialize(ByVal connectionString As String)

    Sub Connect()

    Sub Disconnect()

End Interface
