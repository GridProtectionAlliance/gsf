'***********************************************************************
'  PMUDefinition.vb - PMU definition
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

Namespace EE.Phasor.PDCstream

    Public Class PMUDefinition

        Implements IComparable

        Public ID As String
        Public Name As String
        Public Index As Integer
        Public Phasors As PhasorDefinition()
        Public Frequency As FrequencyDefinition
        Public Digital1Label As String
        Public Digital2Label As String
        Public SampleRate As Integer
        Public Offset As Integer

        Public Sub New(ByVal phasorCount As Integer)

            Phasors = Array.CreateInstance(GetType(PhasorDefinition), phasorCount)

        End Sub

        Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

            ' We sort PMU's by index
            If TypeOf obj Is PMUDefinition Then
                Return Index.CompareTo(DirectCast(obj, PMUDefinition).Index)
            Else
                Throw New ArgumentException("PMUDefinition can only be compared to other PMUDefinitions")
            End If

        End Function

    End Class

End Namespace