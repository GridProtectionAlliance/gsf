'*******************************************************************************************************
'  PhasorDefinition.vb - PDCstream Phasor definition
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
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
    Public Class PhasorDefinition

        Inherits PhasorDefinitionBase

        Private m_ratio As Single
        Private m_calFactor As Single
        Private m_shunt As Single
        Private m_voltageReferenceIndex As Integer

        Public Sub New(ByVal parent As ConfigurationCell)

            MyBase.New(parent)

        End Sub

        Public Sub New(ByVal parent As ConfigurationCell, ByVal index As Integer, ByVal entryValue As String)

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
            If entry.Length > 2 Then CalFactor = CDbl(Trim(entry(2))) Else CalFactor = defaultPhasor.CalFactor
            If entry.Length > 3 Then Offset = CDbl(Trim(entry(3))) Else Offset = defaultPhasor.Offset
            If entry.Length > 4 Then Shunt = CDbl(Trim(entry(4))) Else Shunt = defaultPhasor.Shunt
            If entry.Length > 5 Then VoltageReferenceIndex = Convert.ToInt32(Trim(entry(5))) Else VoltageReferenceIndex = defaultPhasor.VoltageReferenceIndex
            If entry.Length > 6 Then Label = Trim(entry(6)) Else Label = defaultPhasor.Label

            Me.Index = index

        End Sub

        Public Sub New(ByVal phasorDefinition As IPhasorDefinition)

            MyBase.New(phasorDefinition)

        End Sub

        Public Overrides ReadOnly Property InheritedType() As System.Type
            Get
                Return Me.GetType
            End Get
        End Property

        Public Property Ratio() As Single
            Get
                Return m_ratio
            End Get
            Set(ByVal Value As Single)
                m_ratio = Value
            End Set
        End Property

        Public Property CalFactor() As Single
            Get
                Return m_calFactor
            End Get
            Set(ByVal Value As Single)
                m_calFactor = Value
            End Set
        End Property

        Public Property Shunt() As Single
            Get
                Return m_shunt
            End Get
            Set(ByVal Value As Single)
                m_shunt = Value
            End Set
        End Property

        Public Property VoltageReferenceIndex() As Integer
            Get
                Return m_voltageReferenceIndex
            End Get
            Set(ByVal Value As Integer)
                m_voltageReferenceIndex = Value
            End Set
        End Property

        Public Overloads Shared ReadOnly Property ScalingFactor(ByVal phasor As PhasorDefinition) As Single
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

        Public Overrides ReadOnly Property MaximumLabelLength() As Integer
            Get
                Return Integer.MaxValue
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

    End Class

End Namespace
