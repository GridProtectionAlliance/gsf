'*******************************************************************************************************
'  FrequencyValueBase.vb - Frequency and DfDt value base class
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

''' <summary>This class represents the protocol independent a frequency and dfdt value.</summary>
<CLSCompliant(False), Serializable()> _
Public MustInherit Class FrequencyValueBase

    Inherits ChannelValueBase(Of IFrequencyDefinition)
    Implements IFrequencyValue

    Private m_frequency As Single
    Private m_dfdt As Single

    <Serializable()> _
    Public Enum CompositeValueType
        Frequency
        DfDt
    End Enum

    Protected Sub New()
    End Sub

    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

        MyBase.New(info, context)

        ' Deserialize frequency value
        m_frequency = info.GetSingle("frequency")
        m_dfdt = info.GetSingle("dfdt")

    End Sub

    Protected Sub New(ByVal parent As IDataCell)

        MyBase.New(parent)

    End Sub

    ' Derived classes are expected expose a Public Sub New(ByVal parent As IDataCell, ByVal frequencyDefinition As IFrequencyDefinition, ByVal frequency As Single, ByVal dfdt As Single)
    Protected Sub New(ByVal parent As IDataCell, ByVal frequencyDefinition As IFrequencyDefinition, ByVal frequency As Single, ByVal dfdt As Single)

        MyBase.New(parent, frequencyDefinition)

        m_frequency = frequency
        m_dfdt = dfdt

    End Sub

    ' Derived classes are expected expose a Public Sub New(ByVal parent As IDataCell, ByVal frequencyDefinition As IFrequencyDefinition, ByVal unscaledFrequency As Int16, ByVal unscaledDfDt As Int16)
    Protected Sub New(ByVal parent As IDataCell, ByVal frequencyDefinition As IFrequencyDefinition, ByVal unscaledFrequency As Int16, ByVal unscaledDfDt As Int16)

        MyClass.New(parent, frequencyDefinition, _
            Convert.ToSingle(unscaledFrequency / frequencyDefinition.ScalingFactor + frequencyDefinition.Offset), _
            Convert.ToSingle(unscaledDfDt / frequencyDefinition.DfDtScalingFactor + frequencyDefinition.DfDtOffset))

    End Sub

    ' Derived classes are expected expose a Public Sub New(ByVal parent As IDataCell, ByVal frequencyDefinition As IFrequencyDefinition, ByVal binaryImage As Byte(), ByVal startIndex As Int32)
    Protected Sub New(ByVal parent As IDataCell, ByVal frequencyDefinition As IFrequencyDefinition, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

        MyBase.New(parent, frequencyDefinition)
        ParseBinaryImage(Nothing, binaryImage, startIndex)

    End Sub

    ' Derived classes are expected to expose a Public Sub New(ByVal frequencyValue As IFrequencyValue)
    Protected Sub New(ByVal frequencyValue As IFrequencyValue)

        MyClass.New(frequencyValue.Parent, frequencyValue.Definition, frequencyValue.Frequency, frequencyValue.DfDt)

    End Sub

    Public Overridable Property Frequency() As Single Implements IFrequencyValue.Frequency
        Get
            Return m_frequency
        End Get
        Set(ByVal value As Single)
            m_frequency = value
        End Set
    End Property

    Public Overridable Property DfDt() As Single Implements IFrequencyValue.DfDt
        Get
            Return m_dfdt
        End Get
        Set(ByVal value As Single)
            m_dfdt = value
        End Set
    End Property

    Public Overridable Property UnscaledFrequency() As Int16 Implements IFrequencyValue.UnscaledFrequency
        Get
            With Definition
                Return Convert.ToInt16((m_frequency - .Offset) * .ScalingFactor)
            End With
        End Get
        Set(ByVal value As Int16)
            With Definition
                m_frequency = value / .ScalingFactor + .Offset
            End With
        End Set
    End Property

    Public Overridable Property UnscaledDfDt() As Int16 Implements IFrequencyValue.UnscaledDfDt
        Get
            With Definition
                Return Convert.ToInt16((m_dfdt - .DfDtOffset) * .DfDtScalingFactor)
            End With
        End Get
        Set(ByVal value As Int16)
            With Definition
                m_dfdt = value / .DfDtScalingFactor + .DfDtOffset
            End With
        End Set
    End Property

    Default Public Overrides Property CompositeValue(ByVal index As Integer) As Single
        Get
            Select Case index
                Case CompositeValueType.Frequency
                    Return m_frequency
                Case CompositeValueType.DfDt
                    Return m_dfdt
                Case Else
                    Throw New IndexOutOfRangeException("Specified frequency value composite index, " & index & ", is out of range - there are only two composite values for a frequency value: frequency (0) and df/dt (1)")
            End Select
        End Get
        Set(ByVal value As Single)
            Select Case index
                Case CompositeValueType.Frequency
                    m_frequency = value
                Case CompositeValueType.DfDt
                    m_dfdt = value
                Case Else
                    Throw New IndexOutOfRangeException("Specified frequency value composite index, " & index & ", is out of range - there are only two composite values for a frequency value: frequency (0) and df/dt (1)")
            End Select
        End Set
    End Property

    Public Overrides ReadOnly Property CompositeValueCount() As Integer
        Get
            Return 2
        End Get
    End Property

    Public Overrides ReadOnly Property IsEmpty() As Boolean
        Get
            Return (m_frequency = 0 And m_dfdt = 0)
        End Get
    End Property

    Protected Overrides ReadOnly Property BodyLength() As UInt16
        Get
            If DataFormat = Phasors.DataFormat.FixedInteger Then
                Return 4
            Else
                Return 8
            End If
        End Get
    End Property

    Protected Overrides ReadOnly Property BodyImage() As Byte()
        Get
            Dim buffer As Byte() = CreateArray(Of Byte)(BodyLength)

            If DataFormat = Phasors.DataFormat.FixedInteger Then
                EndianOrder.BigEndian.CopyBytes(UnscaledFrequency, buffer, 0)
                EndianOrder.BigEndian.CopyBytes(UnscaledDfDt, buffer, 2)
            Else
                EndianOrder.BigEndian.CopyBytes(m_frequency, buffer, 0)
                EndianOrder.BigEndian.CopyBytes(m_dfdt, buffer, 4)
            End If

            Return buffer
        End Get
    End Property

    Protected Overrides Sub ParseBodyImage(ByVal state As IChannelParsingState, ByVal binaryImage() As Byte, ByVal startIndex As Integer)

        If DataFormat = Phasors.DataFormat.FixedInteger Then
            UnscaledFrequency = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex)
            UnscaledDfDt = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 2)
        Else
            m_frequency = EndianOrder.BigEndian.ToSingle(binaryImage, startIndex)
            m_dfdt = EndianOrder.BigEndian.ToSingle(binaryImage, startIndex + 4)
        End If

    End Sub

    Public Overrides Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext)

        MyBase.GetObjectData(info, context)

        ' Serialize frequency value
        info.AddValue("frequency", m_frequency)
        info.AddValue("dfdt", m_dfdt)

    End Sub

    Public Overrides ReadOnly Property Attributes() As System.Collections.Generic.Dictionary(Of String, String)
        Get
            With MyBase.Attributes
                .Add("Frequency Value", Frequency)
                .Add("df/dt Value", DfDt)
                .Add("Unscaled Frequency Value", UnscaledFrequency)
                .Add("Unscaled df/dt Value", UnscaledDfDt)
            End With

            Return MyBase.Attributes
        End Get
    End Property

End Class
