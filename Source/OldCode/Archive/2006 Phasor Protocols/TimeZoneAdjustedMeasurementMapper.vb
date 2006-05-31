'*******************************************************************************************************
'  TimeZoneAdjustedMeasurementMapper.vb - Timezone data corrective measurement mapper
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
Imports Tva.DateTime
Imports Tva.DateTime.Common

<CLSCompliant(False)> _
Public Class TimeZoneAdjustedMeasurementMapper

    Inherits PhasorMeasurementMapper

    Private m_sourceTimeZone As Win32TimeZone

    Public Sub New(ByVal sourceTimeZone As Win32TimeZone, ByVal frameParser As FrameParser, ByVal source As String, ByVal pmuIDs As List(Of String), ByVal measurementIDs As Dictionary(Of String, MeasurementDefinition))

        MyBase.New(frameParser, source, pmuIDs, measurementIDs)
        m_sourceTimeZone = sourceTimeZone

    End Sub

    Protected Overrides Sub MapDataFrameMeasurements(ByVal frame As Tva.Phasors.IDataFrame)

        ' Adjust time to be on UTC based on source PDC/PMU time zone
        ' For example, ConEd PMU is on Eastern Time - this will correct the issue
        frame.Ticks = m_sourceTimeZone.ToUniversalTime(frame.Timestamp).Ticks

        ' Allow base class to map data frame measurement instances to their associated point ID's
        MyBase.MapDataFrameMeasurements(frame)

    End Sub

End Class
