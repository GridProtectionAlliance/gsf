Imports System.IO
Imports TVA.Measurements
Imports RealTimeEventDetection

Module MainModule

    Sub Main()

        Dim calc As New EventDetectionAlgorithm()
        Dim dataFrame As Frame
        Dim columns As String()

        Dim bus1VM As New MeasurementKey(1608, "P0")      ' TVA_CUMB-BUS1:ABBV
        Dim bus1VA As New MeasurementKey(1609, "P0")      ' TVA_CUMB-BUS1:ABBVH
        Dim bus2VM As New MeasurementKey(1610, "P0")      ' TVA_CUMB-BUS2:ABBV

        calc.Initialize("DebugTest", "", Nothing, Nothing, -1, 30, 2, 2)

        Using MyReader As New Microsoft.VisualBasic.FileIO.TextFieldParser("Data.txt")
            MyReader.TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited
            MyReader.Delimiters = New String() {vbTab}

            While Not MyReader.EndOfData
                Try
                    ' Parse columns from next row
                    columns = MyReader.ReadFields

                    ' Create new data frame
                    dataFrame = New Frame(Date.Now.Ticks)

                    ' Add measuremetns
                    dataFrame.Measurements.Add(bus1VM, New Measurement(bus1VM.ID, bus1VM.Source, Convert.ToDouble(columns(0)), dataFrame.Ticks))
                    dataFrame.Measurements.Add(bus1VA, New Measurement(bus1VA.ID, bus1VA.Source, Convert.ToDouble(columns(1)), dataFrame.Ticks))

                    calc.TestAlgorithm(dataFrame, 0)

                Catch ex As Microsoft.VisualBasic.FileIO.MalformedLineException
                    MsgBox("Line " & ex.Message & " is invalid.  Skipping")
                End Try
            End While
        End Using


    End Sub

End Module
