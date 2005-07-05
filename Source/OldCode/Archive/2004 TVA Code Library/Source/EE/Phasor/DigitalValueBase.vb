'*******************************************************************************************************
'  DigitalValueBase.vb - Digital value base class
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
'  02/18/2005 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.ComponentModel
Imports TVA.Interop

Namespace EE.Phasor

    ' This class represents the common implementation of the protocol independent representation of a digital value.
    Public MustInherit Class DigitalValueBase

        Inherits ChannelValueBase
        Implements IDigitalValue

        Private m_value As Int16

        Protected Sub New(ByVal parent As IDataCell)

            MyBase.New(parent)

        End Sub

        ' Derived classes are expected expose a Public Sub New(ByVal parent As IDataCell, ByVal digitalDefinition As IDigitalDefinition, ByVal value As Int16)
        Protected Sub New(ByVal parent As IDataCell, ByVal digitalDefinition As IDigitalDefinition, ByVal value As Int16)

            MyBase.New(parent, digitalDefinition)

            m_value = value

        End Sub

        ' Derived classes are expected expose a Public Sub New(ByVal parent As IDataCell, ByVal digitalDefinition As IDigitalDefinition, ByVal binaryImage As Byte(), ByVal startIndex As Integer)
        Protected Sub New(ByVal parent As IDataCell, ByVal digitalDefinition As IDigitalDefinition, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            MyBase.New(parent, digitalDefinition)

            m_value = EndianOrder.ReverseToInt16(binaryImage, startIndex)

        End Sub

        ' Derived classes are expected to expose a Public Sub New(ByVal digitalValue As IDigitalValue)
        Protected Sub New(ByVal digitalValue As IDigitalValue)

            Me.New(digitalValue.Parent, digitalValue.Definition, digitalValue.Value)

        End Sub

        Public Overridable Shadows Property Definition() As IDigitalDefinition Implements IDigitalValue.Definition
            Get
                Return MyBase.Definition
            End Get
            Set(ByVal Value As IDigitalDefinition)
                MyBase.Definition = Value
            End Set
        End Property

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

        Protected Overrides ReadOnly Property BodyLength() As Int16
            Get
                Return 2
            End Get
        End Property

        Protected Overrides ReadOnly Property BodyImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BodyLength)

                EndianOrder.SwapCopyBytes(m_value, buffer, 0)

                Return buffer
            End Get
        End Property

    End Class

End Namespace