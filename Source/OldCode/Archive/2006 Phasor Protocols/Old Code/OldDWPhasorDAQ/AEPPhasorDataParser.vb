Imports Tva.Phasors
Imports Tva.DateTime.Common

<CLSCompliant(False)> _
Public Class AEPPhasorDataParser

    Inherits PhasorDataParser

    Public Sub New(ByVal frameParser As FrameParser, ByVal pmuIDs As List(Of String), ByVal measurementIDs As Dictionary(Of String, Integer))

        MyBase.New(frameParser, pmuIDs, measurementIDs)

    End Sub

    Protected Overrides Sub MapDataFrameMeasurements(ByVal frame As Tva.Phasors.IDataFrame)

        ' AEP's configur frame is bad - right we only try to correct frequenct offsets
        With frame
            Dim x, y As Integer

            ' Loop through each parsed PMU data cell
            For x = 0 To .Cells.Count - 1
                With .Cells(x)
                    With .PhasorValues
                        For y = 0 To .Count - 1
                            With .Item(y)
                                ' Adjust magnitude
                                .Magnitude = .Magnitude * 100000
                            End With
                        Next
                    End With

                    With .FrequencyValue
                        ' Adjust frequency
                        .Frequency = .Frequency + 10
                    End With
                End With
            Next
        End With

        ' Allow base class to map data frame measurement instances to their associated point ID's
        MyBase.MapDataFrameMeasurements(frame)

    End Sub

End Class
