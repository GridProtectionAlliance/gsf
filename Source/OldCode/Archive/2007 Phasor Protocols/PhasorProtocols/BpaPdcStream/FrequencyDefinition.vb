'*******************************************************************************************************
'  FrequencyDefinition.vb - PDCstream Frequency definition
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  11/12/2004 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Runtime.Serialization
Imports System.Text

Namespace BpaPdcStream

    <CLSCompliant(False), Serializable()> _
    Public Class FrequencyDefinition

        Inherits FrequencyDefinitionBase

        Private m_dummy As Int32
        Private m_frequencyOffset As Single

        Protected Sub New()
        End Sub

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            MyBase.New(info, context)

        End Sub

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
            If entry.Length > 1 Then ScalingFactor = CInt(Trim(entry(1))) Else ScalingFactor = defaultFrequency.ScalingFactor
            If entry.Length > 2 Then Offset = CSng(Trim(entry(2))) Else Offset = defaultFrequency.Offset
            If entry.Length > 3 Then DfDtScalingFactor = CInt(Trim(entry(3))) Else DfDtScalingFactor = defaultFrequency.DfDtScalingFactor
            If entry.Length > 4 Then DfDtOffset = CSng(Trim(entry(4))) Else DfDtOffset = defaultFrequency.DfDtOffset
            If entry.Length > 5 Then m_dummy = CInt(Trim(entry(5))) Else m_dummy = defaultFrequency.m_dummy
            If entry.Length > 6 Then Label = Trim(entry(6)) Else Label = defaultFrequency.Label

        End Sub

        Public Sub New(ByVal parent As ConfigurationCell, ByVal frequencyDefinition As IFrequencyDefinition)

            MyBase.New(parent, frequencyDefinition)

        End Sub

        Public Overrides ReadOnly Property DerivedType() As System.Type
            Get
                Return Me.GetType
            End Get
        End Property

        Public Shadows ReadOnly Property Parent() As ConfigurationCell
            Get
                Return MyBase.Parent
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
                        frequency.m_dummy & "," & _
                        frequency.Label)

                    Return .ToString()
                End With
            End Get
        End Property

        Public Overrides Property Offset() As Single
            Get
                If Parent Is Nothing Then
                    Return m_frequencyOffset
                Else
                    Return MyBase.Offset
                End If
            End Get
            Set(ByVal value As Single)
                If Parent Is Nothing Then
                    ' Store local value for default frequency definition
                    m_frequencyOffset = value
                Else
                    ' Frequency offset is stored as nominal frequency of parent cell
                    If value >= 60.0F Then
                        Parent.NominalFrequency = LineFrequency.Hz60
                    Else
                        Parent.NominalFrequency = LineFrequency.Hz50
                    End If
                End If
            End Set
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