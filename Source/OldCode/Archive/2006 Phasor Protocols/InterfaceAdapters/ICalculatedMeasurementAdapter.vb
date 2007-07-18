'*******************************************************************************************************
'  ICalculatedMeasurementAdapter.vb - Abstract calculated measurement adpater interface
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

Public Interface ICalculatedMeasurementAdapter

    Inherits IAdapter

    Event NewCalculatedMeasurements(ByVal measurements As IList(Of IMeasurement))

    Event CalculationException(ByVal source As String, ByVal ex As Exception)

    Sub Initialize( _
        ByVal calculationName As String, _
        ByVal configurationSection As String, _
        ByVal outputMeasurements As IMeasurement(), _
        ByVal inputMeasurementKeys As MeasurementKey(), _
        ByVal minimumMeasurementsToUse As Integer, _
        ByVal expectedMeasurementsPerSecond As Integer, _
        ByVal lagTime As Double, _
        ByVal leadTime As Double)

    Sub Start()

    Sub [Stop]()

    Property ConfigurationSection() As String

    Sub QueueMeasurementForCalculation(ByVal measurement As IMeasurement)

    Sub QueueMeasurementsForCalculation(ByVal measurements As ICollection(Of IMeasurement))

    Property OutputMeasurements() As IMeasurement()

    Property InputMeasurementKeys() As MeasurementKey()

    Property MinimumMeasurementsToUse() As Integer

End Interface
