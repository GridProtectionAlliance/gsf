'*******************************************************************************************************
'  PhasorDefinition.vb - Phasor definition
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

Namespace FNet

    <CLSCompliant(False), Serializable()> _
    Public Class DigitalDefinition

        Inherits DigitalDefinitionBase

        Private m_statusFlags As Int16

        Protected Sub New()
        End Sub

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            MyBase.New(info, context)

            ' Deserialize digital definition
            m_statusFlags = info.GetInt16("statusFlags")

        End Sub

        Public Sub New(ByVal parent As ConfigurationCell)

            MyBase.New(parent)

        End Sub

        Public Sub New(ByVal parent As ConfigurationCell, ByVal index As Int32, ByVal label As String)

            MyBase.New(parent, index, label)

        End Sub

        Public Sub New(ByVal parent As ConfigurationCell, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            MyBase.New(parent, binaryImage, startIndex)

        End Sub

        Public Sub New(ByVal digitalDefinition As IDigitalDefinition)

            MyBase.New(digitalDefinition)

        End Sub

        Friend Shared Function CreateNewDigitalDefintion(ByVal parent As IConfigurationCell, ByVal binaryImage As Byte(), ByVal startIndex As Int32) As IDigitalDefinition

            Return New DigitalDefinition(parent, binaryImage, startIndex)

        End Function

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

        Public Property NormalStatus() As Int16
            Get
                Return m_statusFlags And Bit4
            End Get
            Set(ByVal value As Int16)
                If value > 0 Then
                    m_statusFlags = m_statusFlags Or Bit4
                Else
                    m_statusFlags = m_statusFlags And Not Bit4
                End If
            End Set
        End Property

        Public Property ValidInput() As Int16
            Get
                Return m_statusFlags And Bit0
            End Get
            Set(ByVal value As Int16)
                If value > 0 Then
                    m_statusFlags = m_statusFlags Or Bit0
                Else
                    m_statusFlags = m_statusFlags And Not Bit0
                End If
            End Set
        End Property

        Friend Shared ReadOnly Property ConversionFactorLength() As Int32
            Get
                Return 2
            End Get
        End Property

        Friend ReadOnly Property ConversionFactorImage() As Byte()
            Get
                Return EndianOrder.BigEndian.GetBytes(m_statusFlags)
            End Get
        End Property

        Friend Sub ParseConversionFactor(ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            m_statusFlags = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex)

        End Sub

        Public Overrides Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext)

            MyBase.GetObjectData(info, context)

            ' Serialize digital definition
            info.AddValue("statusFlags", m_statusFlags)

        End Sub

    End Class

End Namespace
