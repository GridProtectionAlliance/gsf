'*******************************************************************************************************
'  ConEdPhasorMeasurementMapper.vb - ConEdison timezone data corrective measurement mapper
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  05/08/2006 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports Tva.Phasors
Imports Tva.DateTime.Common

<CLSCompliant(False)> _
Public Class ConEdPhasorMeasurementMapper

    Inherits PhasorMeasurementMapper

    Public Sub New(ByVal frameParser As FrameParser, ByVal source As String, ByVal pmuIDs As List(Of String), ByVal measurementIDs As Dictionary(Of String, Integer))

        MyBase.New(frameParser, source, pmuIDs, measurementIDs)

    End Sub

    Protected Overrides Sub MapDataFrameMeasurements(ByVal frame As Tva.Phasors.IDataFrame)

        ' ConEd is on Eastern Time - this will correct the issue
        frame.Ticks = EasternTimeZone.ToUniversalTime(frame.Timestamp).Ticks

        ' Allow base class to map data frame measurement instances to their associated point ID's
        MyBase.MapDataFrameMeasurements(frame)

    End Sub

End Class
