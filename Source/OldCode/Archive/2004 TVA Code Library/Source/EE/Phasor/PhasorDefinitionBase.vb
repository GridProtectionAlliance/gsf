'***********************************************************************
'  PhasorDefinitionBase.vb - Phasor definition base class
'  Copyright © 2005 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [WESTAFF]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  02/18/2005 - James R Carroll
'       Initial version of source generated
'
'***********************************************************************

Imports System.Text

Namespace EE.Phasor

    ' This class represents the common implementation of the protocol independent definition of a phasor value definition.
    Public MustInherit Class PhasorDefinitionBase

        Implements IPhasorDefinition

        Protected m_label As String
        Protected m_type As PhasorType
        Protected m_voltageReference As IPhasorDefinition

        Protected Sub New()

            m_label = ""
            m_type = PhasorType.Voltage

        End Sub

        Public Overridable Property [Type]() As PhasorType Implements IPhasorDefinition.Type
            Get
                Return m_type
            End Get
            Set(ByVal Value As PhasorType)
                m_type = Value
            End Set
        End Property

        Public Overridable Property ScalingFactor() As Double Implements IPhasorDefinition.ScalingFactor
            Get
                Return CalFactor
            End Get
            Set(ByVal Value As Double)
                CalFactor = Value
            End Set
        End Property

        Public MustOverride Property CalFactor() As Double Implements IPhasorDefinition.CalFactor

        Public MustOverride ReadOnly Property MaximumCalFactor() As Int32 Implements IPhasorDefinition.MaximumCalFactor

        Public Overridable Property Label() As String Implements IPhasorDefinition.Label
            Get
                Return m_label
            End Get
            Set(ByVal Value As String)
                If Len(Value) > MaximumLabelLength Then
                    Throw New OverflowException("Label length cannot exceed " & MaximumLabelLength)
                Else
                    m_label = Trim(Replace(Value, Chr(20), " "))
                End If
            End Set
        End Property

        Public Overridable ReadOnly Property LabelImage() As Byte() Implements IPhasorDefinition.LabelImage
            Get
                Return Encoding.ASCII.GetBytes(m_label.PadRight(MaximumLabelLength))
            End Get
        End Property

        Public MustOverride ReadOnly Property BinaryLength() As Integer Implements IPhasorDefinition.BinaryLength

        Public MustOverride ReadOnly Property BinaryImage() As Byte() Implements IPhasorDefinition.BinaryImage

        Public MustOverride Property Index() As Integer Implements IPhasorDefinition.Index

        Public Overridable ReadOnly Property MaximumLabelLength() As Integer Implements IPhasorDefinition.MaximumLabelLength
            Get
                Return 16
            End Get
        End Property

        Public Overridable Property VoltageReference() As IPhasorDefinition Implements IPhasorDefinition.VoltageReference
            Get
                If m_type = PhasorType.Voltage Then
                    Return Me
                Else
                    Return m_voltageReference
                End If
            End Get
            Set(ByVal Value As IPhasorDefinition)
                If m_type = PhasorType.Voltage Then
                    If Not Value Is Me Then
                        Throw New NotImplementedException("Voltage phasors do not have a voltage reference")
                    End If
                Else
                    m_voltageReference = Value
                End If
            End Set
        End Property

        Public Overridable Function CompareTo(ByVal obj As Object) As Integer Implements IComparable.CompareTo

            ' We sort phasor defintions by index
            If TypeOf obj Is IPhasorDefinition Then
                Return Index.CompareTo(DirectCast(obj, IPhasorDefinition).Index)
            Else
                Throw New ArgumentException("PhasorDefinition can only be compared to other PhasorDefinitions")
            End If

        End Function

    End Class

End Namespace
