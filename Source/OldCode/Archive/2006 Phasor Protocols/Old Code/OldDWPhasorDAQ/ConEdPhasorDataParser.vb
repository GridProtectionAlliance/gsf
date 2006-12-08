Imports Tva.Phasors
Imports Tva.DateTime.Common

<CLSCompliant(False)> _
Public Class ConEdPhasorDataParser

    Inherits PhasorDataParser

    Public Sub New(ByVal frameParser As FrameParser, ByVal pmuIDs As List(Of String), ByVal measurementIDs As Dictionary(Of String, Integer))

        MyBase.New(frameParser, pmuIDs, measurementIDs)

    End Sub

    Protected Overrides Sub MapDataFrameMeasurements(ByVal frame As Tva.Phasors.IDataFrame)

        ' ConEd is on Eastern Time - this will corrcet issue
        frame.Ticks = EasternTimeZone.ToUniversalTime(frame.Timestamp).Ticks

        ' Allow base class to map data frame measurement instances to their associated point ID's
        MyBase.MapDataFrameMeasurements(frame)

    End Sub

End Class
