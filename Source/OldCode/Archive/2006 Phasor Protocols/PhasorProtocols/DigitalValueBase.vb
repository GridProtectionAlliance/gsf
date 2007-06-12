'*******************************************************************************************************
'  DigitalValueBase.vb - Digital value base class
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
Imports System.ComponentModel

''' <summary>This class represents the common implementation of the protocol independent representation of a digital value.</summary>
<CLSCompliant(False), Serializable()> _
Public MustInherit Class DigitalValueBase

    Inherits ChannelValueBase(Of IDigitalDefinition)
    Implements IDigitalValue

    Private m_value As Int16

    Protected Sub New()
    End Sub

    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

        MyBase.New(info, context)

        ' Deserialize digital value
        m_value = info.GetInt16("value")

    End Sub

    Protected Sub New(ByVal parent As IDataCell)

        MyBase.New(parent)

    End Sub

    ' Derived classes are expected expose a Public Sub New(ByVal parent As IDataCell, ByVal digitalDefinition As IDigitalDefinition, ByVal value As Int16)
    Protected Sub New(ByVal parent As IDataCell, ByVal digitalDefinition As IDigitalDefinition, ByVal value As Int16)

        MyBase.New(parent, digitalDefinition)

        m_value = value

    End Sub

    ' Derived classes are expected expose a Public Sub New(ByVal parent As IDataCell, ByVal digitalDefinition As IDigitalDefinition, ByVal binaryImage As Byte(), ByVal startIndex As Int32)
    Protected Sub New(ByVal parent As IDataCell, ByVal digitalDefinition As IDigitalDefinition, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

        MyBase.New(parent, digitalDefinition)
        ParseBinaryImage(Nothing, binaryImage, startIndex)

    End Sub

    ' Derived classes are expected to expose a Public Sub New(ByVal digitalValue As IDigitalValue)
    Protected Sub New(ByVal digitalValue As IDigitalValue)

        MyClass.New(digitalValue.Parent, digitalValue.Definition, digitalValue.Value)

    End Sub

    <EditorBrowsable(EditorBrowsableState.Never)> _
    Public NotOverridable Overrides ReadOnly Property DataFormat() As DataFormat
        Get
            Return MyBase.DataFormat
        End Get
    End Property

    Public Overridable Property Value() As Int16 Implements IDigitalValue.Value
        Get
            Return m_value
        End Get
        Set(ByVal value As Int16)
            m_value = value
        End Set
    End Property

    Default Public Overrides Property CompositeValue(ByVal index As Integer) As Single
        Get
            Return Convert.ToSingle(m_value)
        End Get
        Set(ByVal value As Single)
            Try
                m_value = Convert.ToInt16(value)
            Catch ex As OverflowException
                m_value = Int16.MinValue
            End Try
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
            Return 2
        End Get
    End Property

    Protected Overrides ReadOnly Property BodyImage() As Byte()
        Get
            Dim buffer As Byte() = CreateArray(Of Byte)(BodyLength)

            EndianOrder.BigEndian.CopyBytes(m_value, buffer, 0)

            Return buffer
        End Get
    End Property

    Protected Overrides Sub ParseBodyImage(ByVal state As IChannelParsingState, ByVal binaryImage() As Byte, ByVal startIndex As Integer)

        m_value = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex)

    End Sub

    Public Overrides Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext)

        MyBase.GetObjectData(info, context)

        ' Serialize digital value
        info.AddValue("value", m_value)

    End Sub

    Public Overrides ReadOnly Property Attributes() As Dictionary(Of String, String)
        Get
            Dim baseAttributes As Dictionary(Of String, String) = MyBase.Attributes
            Dim valueBytes As Byte() = BitConverter.GetBytes(Value)

            baseAttributes.Add("Digital Value", Value)
            baseAttributes.Add("Digital Value (Big Endian Bits)", ByteEncoding.BigEndianBinary.GetString(valueBytes))
            baseAttributes.Add("Digital Value (Hexadecimal)", "0x" & ByteEncoding.Hexadecimal.GetString(valueBytes))

            Return baseAttributes
        End Get
    End Property

End Class