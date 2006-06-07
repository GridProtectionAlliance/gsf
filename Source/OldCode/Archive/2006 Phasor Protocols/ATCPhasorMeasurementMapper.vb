'*******************************************************************************************************
'  ATCPhasorMeasurementMapper.vb - ATC frequency and magnitude data corrective measurement mapper
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

<CLSCompliant(False)> _
Public Class ATCPhasorMeasurementMapper

    Inherits PhasorMeasurementMapper

    Public Sub New(ByVal frameParser As FrameParser, ByVal source As String, ByVal pmuIDs As List(Of String), ByVal measurementIDs As Dictionary(Of String, Integer))

        MyBase.New(frameParser, source, pmuIDs, measurementIDs)

    End Sub

    Protected Overrides Sub MapDataFrameMeasurements(ByVal frame As IDataFrame)

        ' ATC's phasor's are 120 out of sync with TVA, so we correct here...
        With frame
            Dim x, y As Integer

            ' Loop through each parsed PMU data cell
            For x = 0 To .Cells.Count - 1
                With .Cells(x)
                    With .PhasorValues
                        For y = 0 To .Count - 1
                            With .Item(y)
                                ' Adjust angles...
                                .Angle += 120
                            End With
                        Next
                    End With
                End With
            Next
        End With

        ' Allow base class to map data frame measurement instances to their associated point ID's
        MyBase.MapDataFrameMeasurements(frame)

    End Sub

End Class
