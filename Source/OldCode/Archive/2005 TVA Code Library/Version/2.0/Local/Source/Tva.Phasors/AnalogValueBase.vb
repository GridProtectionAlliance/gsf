'*******************************************************************************************************
'  AnalogValueBase.vb - Analog value base class
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
'  02/18/2005 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Runtime.Serialization

''' <summary>This class represents the common implementation of the protocol independent representation of an analog value.</summary>
<CLSCompliant(False), Serializable()> _
Public MustInherit Class AnalogValueBase

    Inherits ChannelValueBase(Of IAnalogDefinition)
    Implements IAnalogValue

    Private m_value As Single

    Protected Sub New()
    End Sub

    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

        MyBase.New(info, context)

        ' Deserialize analog value
        m_value = info.GetSingle("value")

    End Sub

    Protected Sub New(ByVal parent As IDataCell)

        MyBase.New(parent)

    End Sub

    ' Derived classes are expected expose a Public Sub New(ByVal parent As IDataCell, ByVal analogDefinition As IAnalogDefinition, ByVal value As Single)
    Protected Sub New(ByVal parent As IDataCell, ByVal analogDefinition As IAnalogDefinition, ByVal value As Single)

        MyBase.New(parent, analogDefinition)

        m_value = value

    End Sub

    ' Derived classes are expected expose a Public Sub New(ByVal parent As IDataCell, ByVal analogDefinition As IAnalogDefinition, ByVal unscaledValue As Int16)
    Protected Sub New(ByVal parent As IDataCell, ByVal analogDefinition As IAnalogDefinition, ByVal integerValue As Int16)

        MyClass.New(parent, analogDefinition, Convert.ToSingle(integerValue))

    End Sub

    ' Derived classes are expected expose a Public Sub New(ByVal parent As IDataCell, ByVal analogDefinition As IAnalogDefinition, ByVal binaryImage As Byte(), ByVal startIndex As Int32)
    Protected Sub New(ByVal parent As IDataCell, ByVal analogDefinition As IAnalogDefinition, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

        MyBase.New(parent, analogDefinition)
        ParseBinaryImage(Nothing, binaryImage, startIndex)

    End Sub

    ' Derived classes are expected expose a Public Sub New(ByVal analogValue As IAnalogValue)
    Protected Sub New(ByVal analogValue As IAnalogValue)

        MyClass.New(analogValue.Parent, analogValue.Definition, analogValue.Value)

    End Sub

    Public Overridable Property Value() As Single Implements IAnalogValue.Value
        Get
            Return m_value
        End Get
        Set(ByVal value As Single)
            m_value = value
        End Set
    End Property

    Public Overridable Property IntegerValue() As Int16 Implements IAnalogValue.IntegerValue
        Get
            Return Convert.ToInt16(m_value)
        End Get
        Set(ByVal value As Int16)
            m_value = Convert.ToSingle(value)
        End Set
    End Property

    Default Public Overrides Property CompositeValue(ByVal index As Integer) As Single
        Get
            Return m_value
        End Get
        Set(ByVal value As Single)
            m_value = value
        End Set
    End Property

    Public Overrides ReadOnly Property CompositeValueCount() As Integer
        Get
            Return 1
        End Get
    End Property

    Public Overrides ReadOnly Property IsEmpty() As Boolean
        Get
            Return (m_value = 0)
        End Get
    End Property

    Protected Overrides ReadOnly Property BodyLength() As UInt16
        Get
            If DataFormat = Phasors.DataFormat.FixedInteger Then
                Return 2
            Else
                Return 4
            End If
        End Get
    End Property

    Protected Overrides ReadOnly Property BodyImage() As Byte()
        Get
            Dim buffer As Byte() = CreateArray(Of Byte)(BodyLength)

            If DataFormat = Phasors.DataFormat.FixedInteger Then
                EndianOrder.BigEndian.CopyBytes(IntegerValue, buffer, 0)
            Else
                EndianOrder.BigEndian.CopyBytes(m_value, buffer, 0)
            End If

            Return buffer
        End Get
    End Property

    Protected Overrides Sub ParseBodyImage(ByVal state As IChannelParsingState, ByVal binaryImage() As Byte, ByVal startIndex As Integer)

        If DataFormat = Phasors.DataFormat.FixedInteger Then
            IntegerValue = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex)
        Else
            m_value = EndianOrder.BigEndian.ToSingle(binaryImage, startIndex)
        End If

    End Sub

    Public Overrides Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext)

        MyBase.GetObjectData(info, context)

        ' Serialize analog value
        info.AddValue("value", m_value)

    End Sub

    Public Overrides ReadOnly Property Attributes() As Dictionary(Of String, String)
        Get
            With MyBase.Attributes
                .Add("Analog Value (Floating Point)", Value)
                .Add("Analog Value (Integer)", IntegerValue)
            End With

            Return MyBase.Attributes
        End Get
    End Property

End Class
