'*******************************************************************************************************
'  FrequencyDefinition.vb - PDCstream Frequency definition
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  11/12/2004 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Text

Namespace BpaPdcStream

    <CLSCompliant(False)> _
    Public Class FrequencyDefinition

        Inherits FrequencyDefinitionBase

        Public Dummy As Int32

        Public Sub New(ByVal parent As ConfigurationCell)

            MyBase.New(parent)

        End Sub

        Public Sub New(ByVal parent As ConfigurationCell, ByVal entryValue As String)

            MyBase.New(parent)

            Dim entry As String() = entryValue.Split(","c)
            Dim defaultFrequency As FrequencyDefinition

            If Not parent Is Nothing Then
                defaultFrequency = DirectCast(parent.Parent, ConfigurationFrame).DefaultFrequency
            Else
                defaultFrequency = New FrequencyDefinition(DirectCast(Nothing, ConfigurationCell))
            End If

            ' First entry is an F - we just ignore this
            If entry.Length > 1 Then ScalingFactor = System.Convert.ToInt32(Trim(entry(1))) Else ScalingFactor = defaultFrequency.ScalingFactor
            If entry.Length > 2 Then Offset = System.Convert.ToSingle(Trim(entry(2))) Else Offset = defaultFrequency.Offset
            If entry.Length > 3 Then DfDtScalingFactor = System.Convert.ToInt32(Trim(entry(3))) Else DfDtScalingFactor = defaultFrequency.DfDtScalingFactor
            If entry.Length > 4 Then DfDtOffset = System.Convert.ToSingle(Trim(entry(4))) Else DfDtOffset = defaultFrequency.DfDtOffset
            If entry.Length > 5 Then Dummy = System.Convert.ToInt32(Trim(entry(5))) Else Dummy = defaultFrequency.Dummy
            If entry.Length > 6 Then Label = Trim(entry(6)) Else Label = defaultFrequency.Label

        End Sub

        Public Sub New(ByVal frequencyDefinition As IFrequencyDefinition)

            MyBase.New(frequencyDefinition)

        End Sub

        Public Overrides ReadOnly Property InheritedType() As System.Type
            Get
                Return Me.GetType
            End Get
        End Property

        Public Shared ReadOnly Property ConfigFileFormat(ByVal frequency As FrequencyDefinition) As String
            Get
                With New StringBuilder
                    .Append("F," & _
                        frequency.ScalingFactor & "," & _
                        frequency.Offset & "," & _
                        frequency.DfDtScalingFactor & "," & _
                        frequency.DfDtOffset & "," & _
                        frequency.Dummy & "," & _
                        frequency.Label)

                    Return .ToString()
                End With
            End Get
        End Property

        Public Overrides ReadOnly Property MaximumLabelLength() As Int32
            Get
                Return Int32.MaxValue
            End Get
        End Property

        Protected Overrides ReadOnly Property BodyLength() As UInt16
            Get
                Return 0
            End Get
        End Property

        Protected Overrides ReadOnly Property BodyImage() As Byte()
            Get
                Throw New NotImplementedException("PDCstream does not include frequency definition in descriptor packet - must be defined in external INI file")
            End Get
        End Property

    End Class

End Namespace