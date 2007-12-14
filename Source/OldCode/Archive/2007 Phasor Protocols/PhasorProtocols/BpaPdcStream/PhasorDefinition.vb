'*******************************************************************************************************
'  PhasorDefinition.vb - PDCstream Phasor definition
'  Copyright © 2008 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2008
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
    Public Class PhasorDefinition

        Inherits PhasorDefinitionBase

        Private m_ratio As Single
        Private m_calFactor As Single
        Private m_shunt As Single
        Private m_voltageReferenceIndex As Int32

        Protected Sub New()
        End Sub

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            MyBase.New(info, context)

            ' Deserialize phasor definition
            m_ratio = info.GetSingle("ratio")
            m_calFactor = info.GetSingle("calFactor")
            m_shunt = info.GetSingle("shunt")
            m_voltageReferenceIndex = info.GetInt32("voltageReferenceIndex")

        End Sub

        Public Sub New(ByVal parent As ConfigurationCell)

            MyBase.New(parent)

        End Sub

        Public Sub New(ByVal parent As ConfigurationCell, ByVal index As Int32, ByVal entryValue As String)

            MyBase.New(parent)

            Dim entry As String() = entryValue.Split(","c)
            Dim entryType As String = UCase(Left(Trim(entry(0)), 1))
            Dim defaultPhasor As PhasorDefinition

            If Not parent Is Nothing Then
                Dim configFile As ConfigurationFrame = Me.Parent.Parent

                If entryType = "V" Then
                    [Type] = PhasorType.Voltage
                    defaultPhasor = configFile.DefaultPhasorV
                ElseIf entryType = "I" Then
                    [Type] = PhasorType.Current
                    defaultPhasor = configFile.DefaultPhasorI
                Else
                    [Type] = PhasorType.Voltage
                    defaultPhasor = configFile.DefaultPhasorV
                End If
            Else
                defaultPhasor = New PhasorDefinition(DirectCast(Nothing, ConfigurationCell))
            End If

            If entry.Length > 1 Then Ratio = CDbl(Trim(entry(1))) Else Ratio = defaultPhasor.Ratio
            If entry.Length > 2 Then CalFactor = CDbl(Trim(entry(2))) Else ConversionFactor = defaultPhasor.ConversionFactor
            If entry.Length > 3 Then Offset = CDbl(Trim(entry(3))) Else Offset = defaultPhasor.Offset
            If entry.Length > 4 Then Shunt = CDbl(Trim(entry(4))) Else Shunt = defaultPhasor.Shunt
            If entry.Length > 5 Then VoltageReferenceIndex = CInt(Trim(entry(5))) Else VoltageReferenceIndex = defaultPhasor.VoltageReferenceIndex
            If entry.Length > 6 Then Label = Trim(entry(6)) Else Label = defaultPhasor.Label

            Me.Index = index

        End Sub

        Public Sub New(ByVal parent As ConfigurationCell, ByVal phasorDefinition As IPhasorDefinition)

            MyBase.New(parent, phasorDefinition)

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

        ' BPA PDCstream uses a custom conversion factor (see shared overload below)
        Public Overrides Property ConversionFactor() As Single
            Get
                Return 1.0F
            End Get
            Set(ByVal value As Single)
                ' Ignore updates...
            End Set
        End Property

        Public Property Ratio() As Single
            Get
                Return m_ratio
            End Get
            Set(ByVal value As Single)
                m_ratio = value
            End Set
        End Property

        Public Property CalFactor() As Single
            Get
                Return m_calFactor
            End Get
            Set(ByVal value As Single)
                m_calFactor = value
            End Set
        End Property

        Public Property Shunt() As Single
            Get
                Return m_shunt
            End Get
            Set(ByVal value As Single)
                m_shunt = value
            End Set
        End Property

        Public Property VoltageReferenceIndex() As Int32
            Get
                Return m_voltageReferenceIndex
            End Get
            Set(ByVal value As Int32)
                m_voltageReferenceIndex = value
            End Set
        End Property

        Public Overloads Shared ReadOnly Property ConversionFactor(ByVal phasor As PhasorDefinition) As Single
            Get
                If phasor.Type = PhasorType.Voltage Then
                    Return phasor.CalFactor * phasor.Ratio
                Else
                    Return phasor.CalFactor * phasor.Ratio / phasor.Shunt
                End If
            End Get
        End Property

        Public Shared ReadOnly Property ConfigFileFormat(ByVal phasor As PhasorDefinition) As String
            Get
                With New StringBuilder
                    Select Case phasor.Type
                        Case PhasorType.Voltage
                            .Append("V"c)
                        Case PhasorType.Current
                            .Append("I"c)
                    End Select

                    .Append("," & _
                        phasor.Ratio & "," & _
                        phasor.CalFactor & "," & _
                        phasor.Offset & "," & _
                        phasor.Shunt & "," & _
                        phasor.VoltageReferenceIndex & "," & _
                        phasor.Label)

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
                Throw New NotImplementedException("PDCstream does not include phasor definition in descriptor packet - must be defined in external INI file")
            End Get
        End Property

        Public Overrides Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext)

            MyBase.GetObjectData(info, context)

            ' Serialize phasor definition
            info.AddValue("ratio", m_ratio)
            info.AddValue("calFactor", m_calFactor)
            info.AddValue("shunt", m_shunt)
            info.AddValue("voltageReferenceIndex", m_voltageReferenceIndex)

        End Sub

        Public Overrides ReadOnly Property Attributes() As Dictionary(Of String, String)
            Get
                Dim baseAttributes As Dictionary(Of String, String) = MyBase.Attributes

                baseAttributes.Add("Ratio", m_ratio)
                baseAttributes.Add("CalFactor", m_calFactor)
                baseAttributes.Add("Shunt", m_shunt)
                baseAttributes.Add("Voltage Reference Index", m_voltageReferenceIndex)

                Return baseAttributes
            End Get
        End Property

    End Class

End Namespace
