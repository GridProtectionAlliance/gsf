'*******************************************************************************************************
'  IHistorianAdapter.vb - Abstract historian adpater interface
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
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

Public Interface IHistorianAdapter

    Inherits IAdapter

    Event ArchivalException(ByVal source As String, ByVal ex As Exception)

    Event UnarchivedMeasurements(ByVal total As Integer)

    Sub Initialize(ByVal connectionString As String)

    Sub Connect()

    Sub QueueMeasurementForArchival(ByVal measurement As IMeasurement)

    Sub QueueMeasurementsForArchival(ByVal measurements As ICollection(Of IMeasurement))

    Function GetMeasurements(ByVal total As Integer) As IMeasurement()

    Sub Disconnect()

End Interface
