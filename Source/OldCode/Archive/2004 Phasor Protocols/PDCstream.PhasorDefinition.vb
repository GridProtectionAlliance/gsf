'***********************************************************************
'  PDCstream.PhasorDefinition.vb - Phasor definition
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
'  11/12/2004 - James R Carroll
'       Initial version of source generated
'
'***********************************************************************

Namespace PDCstream

    Public Class PhasorDefinition

        Implements IComparable

        Public [Type] As PhasorType
        Public Index As Integer
        Public Ratio As Double
        Public CalFactor As Double
        Public Offset As Double
        Public Shunt As Double
        Public VoltageRef As Integer
        Public Label As String

        Public Sub New(ByVal configFile As ConfigFile, ByVal index As Integer, ByVal entryValue As String)

            Dim entry As String() = entryValue.Split(","c)
            Dim entryType As String = UCase(Left(Trim(entry(0)), 1))
            Dim defaultPhasor As PhasorDefinition

            If entryType = "V" Then
                [Type] = PhasorType.Voltage
                defaultPhasor = configFile.DefaultPhasorV
            ElseIf entryType = "I" Then
                [Type] = PhasorType.Current
                defaultPhasor = configFile.DefaultPhasorI
            Else
                [Type] = PhasorType.DontCare
                defaultPhasor = configFile.DefaultPhasorV
            End If

            If entry.Length > 1 Then Ratio = CDbl(Trim(entry(1))) Else Ratio = defaultPhasor.Ratio
            If entry.Length > 2 Then CalFactor = CDbl(Trim(entry(2))) Else CalFactor = defaultPhasor.CalFactor
            If entry.Length > 3 Then Offset = CDbl(Trim(entry(3))) Else Offset = defaultPhasor.Offset
            If entry.Length > 4 Then Shunt = CDbl(Trim(entry(4))) Else Shunt = defaultPhasor.Shunt
            If entry.Length > 5 Then VoltageRef = CInt(Trim(entry(5))) Else VoltageRef = defaultPhasor.VoltageRef
            If entry.Length > 6 Then Label = Trim(entry(6)) Else Label = defaultPhasor.Label

            Me.Index = index

        End Sub

        Public Shared ReadOnly Property ScalingFactor(ByVal phasor As PhasorDefinition) As Double
            Get
                With phasor
                    If .Type = PhasorType.Voltage Then
                        Return .CalFactor * .Ratio
                    Else
                        Return .CalFactor * .Ratio / .Shunt
                    End If
                End With
            End Get
        End Property

        Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

            ' We sort phasors by index
            If TypeOf obj Is PhasorDefinition Then
                Return Index.CompareTo(DirectCast(obj, PhasorDefinition).Index)
            Else
                Throw New ArgumentException("PhasorDefinition can only be compared to other PhasorDefinitions")
            End If

        End Function

    End Class

End Namespace
