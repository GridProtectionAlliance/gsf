'*******************************************************************************************************
'  FrequencyDefinition.vb - IEEE 1344 Frequency definition
'  Copyright © 2008 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2008
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

Namespace Ieee1344

    <CLSCompliant(False), Serializable()> _
    Public Class FrequencyDefinition

        Inherits FrequencyDefinitionBase

        Private m_statusFlags As Int16

        Protected Sub New()
        End Sub

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            MyBase.New(info, context)

            ' Deserialize frequency definition
            m_statusFlags = info.GetInt16("statusFlags")

        End Sub

        Public Sub New(ByVal parent As ConfigurationCell)

            MyBase.New(parent)
            ScalingFactor = 1000
            DfDtScalingFactor = 100

        End Sub

        Public Sub New(ByVal parent As ConfigurationCell, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            MyBase.New(parent, binaryImage, startIndex)
            ScalingFactor = 1000
            DfDtScalingFactor = 100

        End Sub

        Public Sub New(ByVal parent As ConfigurationCell, ByVal label As String, ByVal scale As Int32, ByVal offset As Single, ByVal dfdtScale As Int32, ByVal dfdtOffset As Single)

            MyBase.New(parent, label, scale, offset, dfdtScale, dfdtOffset)

        End Sub

        Public Sub New(ByVal parent As ConfigurationCell, ByVal frequencyDefinition As IFrequencyDefinition)

            MyBase.New(parent, frequencyDefinition)

        End Sub

        Friend Shared Function CreateNewFrequencyDefintion(ByVal parent As IConfigurationCell, ByVal binaryImage As Byte(), ByVal startIndex As Int32) As IFrequencyDefinition

            Return New FrequencyDefinition(parent, binaryImage, startIndex)

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

        Public Property FrequencyIsAvailable() As Boolean
            Get
                Return (m_statusFlags And Bit8) = 0
            End Get
            Set(ByVal value As Boolean)
                If value Then
                    m_statusFlags = m_statusFlags And Not Bit8
                Else
                    m_statusFlags = m_statusFlags Or Bit8
                End If
            End Set
        End Property

        Public Property DfDtIsAvailable() As Boolean
            Get
                Return (m_statusFlags And Bit9) = 0
            End Get
            Set(ByVal value As Boolean)
                If value Then
                    m_statusFlags = m_statusFlags And Not Bit9
                Else
                    m_statusFlags = m_statusFlags Or Bit9
                End If
            End Set
        End Property

        Protected Overrides ReadOnly Property BodyLength() As UInt16
            Get
                Return 2
            End Get
        End Property

        Protected Overrides ReadOnly Property BodyImage() As Byte()
            Get
                If NominalFrequency = LineFrequency.Hz50 Then
                    m_statusFlags = m_statusFlags Or Bit0
                Else
                    m_statusFlags = m_statusFlags And Not Bit0
                End If

                Return EndianOrder.BigEndian.GetBytes(m_statusFlags)
            End Get
        End Property

        Protected Overrides Sub ParseBodyImage(ByVal state As IChannelParsingState, ByVal binaryImage() As Byte, ByVal startIndex As Int32)

            m_statusFlags = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex)
            Parent.NominalFrequency = IIf((m_statusFlags And Bit0) > 0, LineFrequency.Hz50, LineFrequency.Hz60)

        End Sub

        Public Overrides Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext)

            MyBase.GetObjectData(info, context)

            ' Serialize frequency definition
            info.AddValue("statusFlags", m_statusFlags)

        End Sub

        Public Overrides ReadOnly Property Attributes() As Dictionary(Of String, String)
            Get
                Dim baseAttributes As Dictionary(Of String, String) = MyBase.Attributes

                baseAttributes.Add("Frequency Is Available", FrequencyIsAvailable)
                baseAttributes.Add("df/dt Is Available", DfDtIsAvailable)

                Return baseAttributes
            End Get
        End Property

    End Class

End Namespace