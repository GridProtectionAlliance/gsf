Imports System.IO
Imports TVA.Measurements
Imports OscillationMonitoringSystem

Module MainModule

    Sub Main()

        Dim calc As New FrequencyDomainDecomposition
        Dim dataFrame As Frame
        Dim columns As String()
        Dim rowCount As Integer

        Dim busMeasure0 As New MeasurementKey(1600, "P0")
        Dim busMeasure1 As New MeasurementKey(1601, "P0")
        Dim busMeasure2 As New MeasurementKey(1602, "P0")
        Dim busMeasure3 As New MeasurementKey(1603, "P0")

        calc.Initialize("DebugTest", "", Nothing, Nothing, -1, 30, 2, 2)
        rowCount = 0

        Using MyReader As New Microsoft.VisualBasic.FileIO.TextFieldParser("Data.txt")
            MyReader.TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited
            MyReader.Delimiters = New String() {vbTab}


            While Not MyReader.EndOfData
                Try
                    ' Parse columns from next row
                    columns = MyReader.ReadFields
                    rowCount = rowCount + 1
                    If rowCount > 10200 Then
                        Return
                    End If
                    ' Create new data frame
                    dataFrame = New Frame(Date.Now.Ticks)

                    If String.Compare(columns(0), "#") = 0 Then
                        ' # is used to simulate bad measurements, Add blank measuremetns
                        dataFrame.Measurements.Add(busMeasure0, New Measurement())
                        dataFrame.Measurements.Add(busMeasure1, New Measurement())
                        dataFrame.Measurements.Add(busMeasure2, New Measurement())
                        dataFrame.Measurements.Add(busMeasure3, New Measurement())
                    Else
                        ' Add measuremetns
                        dataFrame.Measurements.Add(busMeasure0, New Measurement(busMeasure0.ID, busMeasure0.Source, Convert.ToDouble(columns(0)), dataFrame.Ticks))
                        dataFrame.Measurements.Add(busMeasure1, New Measurement(busMeasure1.ID, busMeasure1.Source, Convert.ToDouble(columns(1)), dataFrame.Ticks))
                        dataFrame.Measurements.Add(busMeasure2, New Measurement(busMeasure2.ID, busMeasure2.Source, Convert.ToDouble(columns(2)), dataFrame.Ticks))
                        dataFrame.Measurements.Add(busMeasure3, New Measurement(busMeasure3.ID, busMeasure3.Source, Convert.ToDouble(columns(3)), dataFrame.Ticks))
                    End If

                    'calc.TestAlgorithm(dataFrame, 0)
                    calc.TestAlgorithm(dataFrame, rowCount)
                    dataFrame.Measurements.Clear()

                Catch ex As Microsoft.VisualBasic.FileIO.MalformedLineException
                    MsgBox("Line " & ex.Message & " is invalid.  Skipping")
                End Try
            End While
        End Using


    End Sub

End Module
