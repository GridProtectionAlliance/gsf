'***********************************************************************
'  DatAWare.PMUServerPoints.vb - DatAWare PMU Server Point Definition
'  Copyright © 2004 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [WESTAFF]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  11/23/2004 - James R Carroll
'       These are the classes that correlate a DatAWare server and
'       database index to a specific piece of PMU data, i.e., this
'       is where the PDCstream classes and DatAWare classes are
'       sewn together...
'
'***********************************************************************

Imports DatAWarePDC.PDCstream

Namespace DatAWare

    ' This class loads all the database points for a given connection and creates a PMU point definition for each.
    ' The point definitions are stored in a hashtable so they can be looked up by database index.
    ' We use a hashtable over an array because DatAWare database indexes do not have to be contiguous
    Public Class PMUServerPoints

        Private m_concentrator As PDCstream.Concentrator
        Private m_connection As DatAWare.Connection
        Private m_points As Hashtable
        Private m_emptyPoint As PMUDataPoint

        Public Sub New(ByVal concentrator As PDCstream.Concentrator, ByVal connection As DatAWare.Connection, ByVal userName As String, ByVal password As String)

            Dim openedConnection As Boolean
            Dim dataPoint As PMUDataPoint

            m_concentrator = concentrator
            m_connection = connection
            m_points = New Hashtable

            ' Open connection to DatAWare server if it's not open already
            If Not connection.IsOpen Then
                connection.Open(userName, password)
                openedConnection = True
            End If

            ' Get the point defintion (i.e., the database structure) for each defined point and
            ' create a new PMU point definition based on the information in the synonym field
            For x As Integer = 1 To connection.Points.Count
                Try
                    With connection.Points.Definition(x)
                        If Len(.Synonym1) > 0 Then
                            dataPoint = DefinePMUDataPoint(concentrator.ConfigFile, .Synonym1)

                            ' If data point is received on change, load initial value into point definition
                            If dataPoint.ReceivedOnChange Then dataPoint.Value = connection.Points(x).Value

                            m_points.Add(.Index, dataPoint)
                        End If
                    End With
                Catch ex As Exception
                    concentrator.UpdateStatus("Failed to load point definition for DatAWare server " & connection.Server & "/" & connection.PlantCode & " database index " & x & " due to exception: " & ex.Message)
                End Try
            Next

            ' If we opened the DatAWare connection we'll close it such that we leave the connection
            ' in the same state as we found it...
            If openedConnection Then
                connection.Close()
            End If

        End Sub

        Public ReadOnly Property Concentrator() As PDCstream.Concentrator
            Get
                Return m_concentrator
            End Get
        End Property

        Public ReadOnly Property Connection() As DatAWare.Connection
            Get
                Return m_connection
            End Get
        End Property

        Default Public Property Point(ByVal databaseIndex As Integer) As PMUDataPoint
            Get
                Try
                    Return DirectCast(m_points(databaseIndex), PMUDataPoint)
                Catch ex As Exception
                    Return m_emptyPoint
                End Try
            End Get
            Set(ByVal Value As PMUDataPoint)
                m_points(databaseIndex) = Value
            End Set
        End Property

        Public ReadOnly Property PointExists(ByVal databaseIndex As Integer) As Boolean
            Get
                Return m_points.ContainsKey(databaseIndex)
            End Get
        End Property

        ' This combines a lookup and return in a single step...
        Public Function GetPoint(ByVal databaseIndex As Integer, ByRef dataPoint As PMUDataPoint) As Boolean

            Dim point As Object = m_points(databaseIndex)

            If point Is Nothing Then
                Return False
            Else
                dataPoint = DirectCast(point, PMUDataPoint)
                Return True
            End If

        End Function

        Public ReadOnly Property Count() As Integer
            Get
                Return m_points.Count
            End Get
        End Property

        Public Function GetEnumerator() As System.Collections.IEnumerator

            Return m_points.Values.GetEnumerator

        End Function

        ' DatAWare database index tied to PMU data based on specially formatted synonym field.
        '   See document: DatAWare Synonym Field Format for Phasor Data
        Public Shared Function DefinePMUDataPoint(ByVal configFile As ConfigFile, ByVal synonym As String) As PDCstream.PMUDataPoint

            If Len(synonym) = 0 Then Throw New ArgumentNullException("Unable to create PointDefinition, synonym was null")

            Dim dataPoint As PMUDataPoint
            Dim descriptor As String = synonym
            Dim pmuID As String
            Dim dashIndex As Integer = descriptor.IndexOf("-"c)
            Dim charIndex As Integer

            If dashIndex = -1 Then Throw New InvalidOperationException("Unable to create PointDefinition, invalid synonym declaration: " & synonym)

            With dataPoint
                ' Lookup PMU definition
                pmuID = descriptor.Substring(0, dashIndex).ToUpper()
                .PMU = configFile.PMU(pmuID)
                If .PMU Is Nothing Then Throw New InvalidOperationException("Unable to create PointDefinition: failed to find PMU ID """ & pmuID & """ in config file.  Synonym declaration: " & synonym)

                Select Case descriptor.Substring(dashIndex + 1, 2).ToUpper()
                    Case "PM"
                        .Type = PointType.PhasorMagnitude
                    Case "PA"
                        .Type = PointType.PhasorAngle
                    Case "FQ"
                        .Type = PointType.Frequency
                    Case "DF"
                        .Type = PointType.DfDt
                    Case "DV"
                        .Type = PointType.DigitalValue
                    Case "SF"
                        .Type = PointType.StatusFlags
                    Case Else
                        Throw New InvalidOperationException("Unable to create PointDefinition, point type could not be derived from synonym declaration: " & synonym)
                End Select

                If dashIndex + 3 < descriptor.Length Then
                    descriptor = descriptor.Substring(dashIndex + 3)
                Else
                    descriptor = ""
                End If

                Do While charIndex < descriptor.Length
                    If Char.IsDigit(descriptor, charIndex) Then
                        charIndex += 1
                    Else
                        Exit Do
                    End If
                Loop

                If charIndex > 0 Then
                    .Index = Convert.ToInt32(descriptor.Substring(0, charIndex))
                    If charIndex < descriptor.Length Then
                        descriptor = descriptor.Substring(charIndex)
                    Else
                        descriptor = ""
                    End If
                End If

                If descriptor.Length >= 3 Then
                    Dim dotIndex As Integer = descriptor.IndexOf("."c)

                    If dotIndex > -1 Then
                        If dotIndex + 1 < descriptor.Length Then
                            .ReceivedOnChange = (String.Compare(descriptor.Substring(dotIndex + 1).ToUpper, "ROC") = 0)
                        End If

                        If dotIndex > 0 Then
                            descriptor = descriptor.Substring(0, dotIndex)
                        Else
                            descriptor = ""
                        End If
                    End If

                    If descriptor.Length > 0 Then
                        Select Case descriptor.ToUpper()
                            Case "AVG"
                                .ArchiveStyle = PointArchiveStyle.Average
                            Case "MIN"
                                .ArchiveStyle = PointArchiveStyle.Minimum
                            Case "MAX"
                                .ArchiveStyle = PointArchiveStyle.Maximum
                            Case Else
                                Throw New InvalidOperationException("Unable to create PointDefinition, point archive style could not be derived from synonym declaration: " & synonym)
                        End Select
                    Else
                        .ArchiveStyle = PointArchiveStyle.Raw
                    End If
                Else
                    .ArchiveStyle = PointArchiveStyle.Raw
                End If
            End With

            Return dataPoint

        End Function

    End Class

End Namespace
