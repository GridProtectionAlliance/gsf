'***********************************************************************
'  AnalogValueBase.vb - Analog value base class
'  Copyright © 2005 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
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

Imports TVA.Interop

Namespace EE.Phasor

    ' This class represents the common implementation of the protocol independent representation of an analog value.
    Public MustInherit Class AnalogValueBase

        Inherits ChannelValueBase
        Implements IAnalogValue

        Private m_value As Double

        Protected Sub New(ByVal parent As IDataCell)

            MyBase.New(parent)

        End Sub

        ' Dervied classes are expected expose a Public Sub New(ByVal parent As IDataCell, ByVal analogDefinition As IAnalogDefinition, ByVal value As Double)
        Protected Sub New(ByVal parent As IDataCell, ByVal analogDefinition As IAnalogDefinition, ByVal value As Double)

            MyBase.New(parent, analogDefinition)

            m_value = value

        End Sub

        ' Dervied classes are expected expose a Public Sub New(ByVal parent As IDataCell, ByVal analogDefinition As IAnalogDefinition, ByVal unscaledValue As Int16)
        Protected Sub New(ByVal parent As IDataCell, ByVal analogDefinition As IAnalogDefinition, ByVal unscaledValue As Int16)

            Me.New(parent, analogDefinition, unscaledValue / analogDefinition.ScalingFactor)

        End Sub

        ' Dervied classes are expected expose a Public Sub New(ByVal parent As IDataCell, ByVal analogDefinition As IAnalogDefinition, ByVal binaryImage As Byte(), ByVal startIndex As Integer)
        Protected Sub New(ByVal parent As IDataCell, ByVal analogDefinition As IAnalogDefinition, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            MyBase.New(parent, analogDefinition)

            If DataFormat = DataFormat.FixedInteger Then
                UnscaledValue = EndianOrder.ReverseToInt16(binaryImage, startIndex)
            Else
                m_value = EndianOrder.ReverseToSingle(binaryImage, startIndex)
            End If

        End Sub

        ' Dervied classes are expected expose a Public Sub New(ByVal analogValue As IAnalogValue)
        Protected Sub New(ByVal analogValue As IAnalogValue)

            Me.New(analogValue.Parent, analogValue.Definition, analogValue.Value)

        End Sub

        Public Overridable Shadows Property Definition() As IAnalogDefinition Implements IAnalogValue.Definition
            Get
                Return MyBase.Definition
            End Get
            Set(ByVal Value As IAnalogDefinition)
                MyBase.Definition = Value
            End Set
        End Property

        Public Overridable Property Value() As Double Implements IAnalogValue.Value
            Get
                Return m_value
            End Get
            Set(ByVal Value As Double)
                m_value = Value
            End Set
        End Property

        Public Overridable Property UnscaledValue() As Int16 Implements IAnalogValue.UnscaledValue
            Get
                Return Convert.ToInt16(m_value * Definition.ScalingFactor)
            End Get
            Set(ByVal Value As Int16)
                m_value = Value / Definition.ScalingFactor
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

        Public Overrides ReadOnly Property BinaryLength() As Int16
            Get
                If DataFormat = DataFormat.FixedInteger Then
                    Return 2
                Else
                    Return 4
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property BinaryImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)

                If DataFormat = DataFormat.FixedInteger Then
                    EndianOrder.SwapCopyBytes(UnscaledValue, buffer, 0)
                Else
                    EndianOrder.SwapCopyBytes(Convert.ToSingle(m_value), buffer, 0)
                End If

                Return buffer
            End Get
        End Property

    End Class

End Namespace