'*******************************************************************************************************
'  AnalogValueBase.vb - Analog value base class
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
'  02/18/2005 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports Tva.Interop

' This class represents the common implementation of the protocol independent representation of an analog value.
<CLSCompliant(False)> _
Public MustInherit Class AnalogValueBase

    Inherits ChannelValueBase(Of IAnalogDefinition)
    Implements IAnalogValue

    Private m_value As Single

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

    ' Derived classes are expected expose a Public Sub New(ByVal parent As IDataCell, ByVal analogDefinition As IAnalogDefinition, ByVal binaryImage As Byte(), ByVal startIndex As Integer)
    Protected Sub New(ByVal parent As IDataCell, ByVal analogDefinition As IAnalogDefinition, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

        MyBase.New(parent, analogDefinition)

        If DataFormat = Phasors.DataFormat.FixedInteger Then
            IntegerValue = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex)
        Else
            m_value = EndianOrder.BigEndian.ToSingle(binaryImage, startIndex)
        End If

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

    Public Overrides ReadOnly Property Values() As Single()
        Get
            Return New Single() {m_value}
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
            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BodyLength)

            If DataFormat = Phasors.DataFormat.FixedInteger Then
                EndianOrder.BigEndian.CopyBytes(IntegerValue, buffer, 0)
            Else
                EndianOrder.BigEndian.CopyBytes(m_value, buffer, 0)
            End If

            Return buffer
        End Get
    End Property

End Class
