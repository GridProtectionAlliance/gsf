'***********************************************************************
'  PDCstream.FrequencyDefinition.vb - Frequency definition
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

    Public Class FrequencyDefinition

        Public Scale As Double
        Public Offset As Double
        Public DfDtScale As Double
        Public DfDtOffset As Double
        Public Dummy As Integer
        Public Label As String

        Public Sub New(ByVal configFile As ConfigFile, ByVal entryValue As String)

            Dim entry As String() = entryValue.Split(","c)

            ' First entry is an F - we just ignore this
            If entry.Length > 1 Then Scale = CDbl(Trim(entry(1))) Else Scale = configFile.DefaultFrequency.Scale
            If entry.Length > 2 Then Offset = CDbl(Trim(entry(2))) Else Offset = configFile.DefaultFrequency.Offset
            If entry.Length > 3 Then DfDtScale = CDbl(Trim(entry(3))) Else DfDtScale = configFile.DefaultFrequency.DfDtScale
            If entry.Length > 4 Then DfDtOffset = CDbl(Trim(entry(4))) Else DfDtOffset = configFile.DefaultFrequency.DfDtOffset
            If entry.Length > 5 Then Dummy = CInt(Trim(entry(5))) Else Dummy = configFile.DefaultFrequency.Dummy
            If entry.Length > 6 Then Label = Trim(entry(6)) Else Label = configFile.DefaultFrequency.Label

        End Sub

    End Class

End Namespace