'***********************************************************************
'  DigitalValueBase.vb - Digital value base class
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

Imports System.ComponentModel
Imports TVA.Interop

Namespace EE.Phasor

    ' This class represents the common implementation of the protocol independent representation of a digital value.
    Public MustInherit Class DigitalValueBase

        Inherits ChannelValueBase
        Implements IDigitalValue

        Protected m_digitalDefinition As IDigitalDefinition
        Protected m_value As Int16

        Protected Sub New()

            MyBase.New(EE.Phasor.DataFormat.FixedInteger)

            m_digitalDefinition = Nothing
            m_value = 0

        End Sub

        Protected Sub New(ByVal digitalDefinition As IDigitalDefinition, ByVal value As Int16)

            MyBase.New(EE.Phasor.DataFormat.FixedInteger)

            m_digitalDefinition = digitalDefinition
            m_value = value

        End Sub

        Protected Sub New(ByVal digitalDefinition As IDigitalDefinition, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            MyBase.New(EE.Phasor.DataFormat.FixedInteger)

            m_digitalDefinition = digitalDefinition
            m_value = EndianOrder.ReverseToInt16(binaryImage, startIndex)

        End Sub

        Public Overridable ReadOnly Property Definition() As IDigitalDefinition Implements IDigitalValue.Definition
            Get
                Return m_digitalDefinition
            End Get
        End Property

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public NotOverridable Overrides Property DataFormat() As DataFormat
            Get
                Return m_dataFormat
            End Get
            Set(ByVal Value As DataFormat)
                If Value = EE.Phasor.DataFormat.FixedInteger Then
                    m_dataFormat = Value
                Else
                    Throw New NotImplementedException("Digital values represent bit flags and thus can only be fixed integers")
                End If
            End Set
        End Property

        Public Overridable Property Value() As Int16 Implements IDigitalValue.Value
            Get
                Return m_value
            End Get
            Set(ByVal Value As Int16)
                m_value = Value
            End Set
        End Property

        Public Overrides ReadOnly Property Values() As Double()
            Get
                Return New Double() {m_value}
            End Get
        End Property

        Public Overrides ReadOnly Property IsEmpty() As Boolean
            Get
                Return (m_value = 0)
            End Get
        End Property

        Public Overrides ReadOnly Property BinaryLength() As Integer
            Get
                Return 2
            End Get
        End Property

        Public Overrides ReadOnly Property BinaryImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)

                EndianOrder.SwapCopyBytes(m_value, buffer, 0)

                Return buffer
            End Get
        End Property

    End Class

End Namespace