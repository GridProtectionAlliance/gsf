'*******************************************************************************************************
'  ConfigurationCell.vb - IEEE 1344 Cconfiguration cell
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
Imports System.Text
Imports Tva.Phasors.Common
Imports Tva.Phasors.Ieee1344.Common

Namespace Ieee1344

    <CLSCompliant(False), Serializable()> _
    Public Class ConfigurationCell

        Inherits ConfigurationCellBase

        Private m_coordinateFormat As CoordinateFormat
        Private m_statusFlags As Int16

        Protected Sub New()
        End Sub

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            MyBase.New(info, context)

            ' Deserialize configuration cell
            m_coordinateFormat = info.GetValue("coordinateFormat", GetType(CoordinateFormat))
            m_statusFlags = info.GetInt16("statusFlags")

        End Sub

        Public Sub New(ByVal parent As ConfigurationFrame, ByVal nominalFrequency As LineFrequency)

            MyBase.New(parent, False, 0, nominalFrequency, MaximumPhasorValues, MaximumAnalogValues, MaximumDigitalValues)

        End Sub

        Public Sub New(ByVal configurationCell As IConfigurationCell)

            MyBase.New(configurationCell)

        End Sub

        ' This constructor satisfies ChannelCellBase class requirement:
        '   Final dervived classes must expose Public Sub New(ByVal parent As IChannelFrame, ByVal state As IChannelFrameParsingState, ByVal index As Int32, ByVal binaryImage As Byte(), ByVal startIndex As Int32)
        Public Sub New(ByVal parent As IConfigurationFrame, ByVal state As IConfigurationFrameParsingState, ByVal index As Int32, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            ' We pass in defaults for id code and nominal frequency since these will be parsed out later
            MyBase.New(parent, False, MaximumPhasorValues, MaximumAnalogValues, MaximumDigitalValues, _
                New ConfigurationCellParsingState( _
                    AddressOf Ieee1344.PhasorDefinition.CreateNewPhasorDefintion, _
                    AddressOf Ieee1344.FrequencyDefinition.CreateNewFrequencyDefintion, _
                    Nothing, _
                    AddressOf Ieee1344.DigitalDefinition.CreateNewDigitalDefintion), _
                binaryImage, startIndex)

        End Sub

        Friend Shared Function CreateNewConfigurationCell(ByVal parent As IChannelFrame, ByVal state As IChannelFrameParsingState(Of IConfigurationCell), ByVal index As Int32, ByVal binaryImage As Byte(), ByVal startIndex As Int32) As IConfigurationCell

            Return New ConfigurationCell(parent, state, index, binaryImage, startIndex)

        End Function

        ' TODO: May want to shadow all parents in final derived classes - also go through code and make sure all MustInherit class properties are overridable
        Public Shadows ReadOnly Property Parent() As ConfigurationFrame
            Get
                Return MyBase.Parent
            End Get
        End Property

        Public Overrides ReadOnly Property InheritedType() As System.Type
            Get
                Return Me.GetType
            End Get
        End Property

        Public Property StatusFlags() As Int16
            Get
                Return m_statusFlags
            End Get
            Set(ByVal value As Int16)
                m_statusFlags = value
            End Set
        End Property

        Public Shadows Property IDCode() As UInt64
            Get
                ' IEEE 1344 only allows one PMU, so we share ID code with parent frame...
                Return Parent.IDCode
            End Get
            Set(ByVal value As UInt64)
                Parent.IDCode = value

                ' Base classes constrain maximum value to 65535
                If value > UInt16.MaxValue Then
                    MyBase.IDCode = UInt16.MaxValue
                Else
                    MyBase.IDCode = Convert.ToUInt16(value)
                End If
            End Set
        End Property

        Public Property SynchronizationIsValid() As Boolean
            Get
                Return (StatusFlags And Bit15) = 0
            End Get
            Set(ByVal value As Boolean)
                If value Then
                    StatusFlags = StatusFlags And Not Bit15
                Else
                    StatusFlags = StatusFlags Or Bit15
                End If
            End Set
        End Property

        Public Property DataIsValid() As Boolean
            Get
                Return (StatusFlags And Bit14) = 0
            End Get
            Set(ByVal value As Boolean)
                If value Then
                    StatusFlags = StatusFlags And Not Bit14
                Else
                    StatusFlags = StatusFlags Or Bit14
                End If
            End Set
        End Property

        Public Property TriggerStatus() As TriggerStatus
            Get
                Return StatusFlags And TriggerMask
            End Get
            Set(ByVal value As TriggerStatus)
                StatusFlags = (StatusFlags And Not TriggerMask) Or value
            End Set
        End Property

        ' IEEE 1344 only supports scaled data
        Public Overrides Property PhasorDataFormat() As DataFormat
            Get
                Return DataFormat.FixedInteger
            End Get
            Set(ByVal value As DataFormat)
                If value <> DataFormat.FixedInteger Then Throw New NotSupportedException("IEEE 1344 only supports scaled data")
            End Set
        End Property

        Public Overrides Property PhasorCoordinateFormat() As CoordinateFormat
            Get
                Return m_coordinateFormat
            End Get
            Set(ByVal value As CoordinateFormat)
                m_coordinateFormat = value
            End Set
        End Property

        Public Overrides Property FrequencyDataFormat() As DataFormat
            Get
                Return DataFormat.FixedInteger
            End Get
            Set(ByVal value As DataFormat)
                If value <> DataFormat.FixedInteger Then Throw New NotSupportedException("IEEE 1344 only supports scaled data")
            End Set
        End Property

        Public Overrides Property AnalogDataFormat() As DataFormat
            Get
                Return DataFormat.FixedInteger
            End Get
            Set(ByVal value As DataFormat)
                If value <> DataFormat.FixedInteger Then Throw New NotSupportedException("IEEE 1344 only supports scaled data")
            End Set
        End Property

        Protected Overrides ReadOnly Property HeaderLength() As UInt16
            Get
                Return MyBase.HeaderLength + 14
            End Get
        End Property

        Protected Overrides ReadOnly Property HeaderImage() As Byte()
            Get
                Dim buffer As Byte() = CreateArray(Of Byte)(HeaderLength)
                Dim index As Int32

                EndianOrder.BigEndian.CopyBytes(m_statusFlags, buffer, index)

                ' Copy in station name
                index += 2
                CopyImage(MyBase.HeaderImage, buffer, index, MyBase.HeaderLength)

                EndianOrder.BigEndian.CopyBytes(IDCode, buffer, index)
                EndianOrder.BigEndian.CopyBytes(Convert.ToInt16(PhasorDefinitions.Count), buffer, index + 8)
                EndianOrder.BigEndian.CopyBytes(Convert.ToInt16(DigitalDefinitions.Count), buffer, index + 10)

                Return buffer
            End Get
        End Property

        Protected Overrides Sub ParseHeaderImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            Dim parsingState As IConfigurationCellParsingState = DirectCast(state, IConfigurationCellParsingState)

            m_statusFlags = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex)
            startIndex += 2

            ' Parse out station name
            MyBase.ParseHeaderImage(state, binaryImage, startIndex)
            startIndex += MyBase.HeaderLength

            IDCode = EndianOrder.BigEndian.ToUInt64(binaryImage, startIndex)

            With parsingState
                .PhasorCount = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 8)
                .DigitalCount = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 10)
            End With

        End Sub

        Protected Overrides ReadOnly Property FooterLength() As UInt16
            Get
                Return MyBase.FooterLength + _
                    PhasorDefinitions.Count * PhasorDefinition.ConversionFactorLength + _
                    DigitalDefinitions.Count * DigitalDefinition.ConversionFactorLength
            End Get
        End Property

        Protected Overrides ReadOnly Property FooterImage() As Byte()
            Get
                Dim buffer As Byte() = CreateArray(Of Byte)(FooterLength)
                Dim x, index As Int32

                ' Include conversion factors in configuration cell footer
                With PhasorDefinitions
                    For x = 0 To .Count - 1
                        CopyImage(DirectCast(.Item(x), PhasorDefinition).ConversionFactorImage, buffer, index, PhasorDefinition.ConversionFactorLength)
                    Next
                End With

                With DigitalDefinitions
                    For x = 0 To .Count - 1
                        CopyImage(DirectCast(.Item(x), DigitalDefinition).ConversionFactorImage, buffer, index, DigitalDefinition.ConversionFactorLength)
                    Next
                End With

                ' Include nominal frequency
                CopyImage(MyBase.FooterImage, buffer, index, MyBase.FooterLength)

                Return buffer
            End Get
        End Property

        Protected Overrides Sub ParseFooterImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            Dim x As Int32

            ' Parse conversion factors from configuration cell footer
            With PhasorDefinitions
                For x = 0 To .Count - 1
                    DirectCast(.Item(x), PhasorDefinition).ParseConversionFactor(binaryImage, startIndex)
                    startIndex += PhasorDefinition.ConversionFactorLength
                Next
            End With

            With DigitalDefinitions
                For x = 0 To .Count - 1
                    DirectCast(.Item(x), DigitalDefinition).ParseConversionFactor(binaryImage, startIndex)
                    startIndex += DigitalDefinition.ConversionFactorLength
                Next
            End With

            ' Parse nominal frequency
            MyBase.ParseFooterImage(state, binaryImage, startIndex)

        End Sub

        Public Overrides Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext)

            MyBase.GetObjectData(info, context)

            ' Serialize configuration cell
            info.AddValue("coordinateFormat", m_coordinateFormat, GetType(CoordinateFormat))
            info.AddValue("statusFlags", m_statusFlags)

        End Sub

    End Class

End Namespace