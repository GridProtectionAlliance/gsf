'*******************************************************************************************************
'  ConfigurationCell.vb - IEEE C37.118 Configuration cell
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
Imports Tva.Phasors.IeeeC37_118.Common

Namespace IeeeC37_118

    <CLSCompliant(False), Serializable()> _
    Public Class ConfigurationCell

        Inherits ConfigurationCellBase

        ' Because the protocol doesn't include a version number that can account draft implementations, we must manually account for this where needed
        Private m_formatFlags As FormatFlags
        Private m_configurationCount As UInt16

        Protected Sub New()
        End Sub

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            MyBase.New(info, context)

            ' Deserialize configuration cell
            m_formatFlags = info.GetValue("formatFlags", GetType(FormatFlags))
            m_configurationCount = info.GetUInt16("configurationCount")

        End Sub

        Public Sub New(ByVal parent As ConfigurationFrame, ByVal idCode As UInt16, ByVal nominalFrequency As LineFrequency)

            MyBase.New(parent, False, idCode, nominalFrequency, MaximumPhasorValues, MaximumAnalogValues, MaximumDigitalValues)

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
                    AddressOf IeeeC37_118.PhasorDefinition.CreateNewPhasorDefintion, _
                    AddressOf IeeeC37_118.FrequencyDefinition.CreateNewFrequencyDefintion, _
                    AddressOf IeeeC37_118.AnalogDefinition.CreateNewAnalogDefintion, _
                    AddressOf IeeeC37_118.DigitalDefinition.CreateNewDigitalDefintion), _
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

        Public Property FormatFlags() As FormatFlags
            Get
                Return m_formatFlags
            End Get
            Set(ByVal value As FormatFlags)
                m_formatFlags = value
            End Set
        End Property

        Public Property ConfigurationCount() As UInt16
            Get
                Return m_configurationCount
            End Get
            Set(ByVal value As UInt16)
                m_configurationCount = value
            End Set
        End Property

        Public Overrides Property PhasorDataFormat() As DataFormat
            Get
                Return IIf((m_formatFlags And FormatFlags.Phasors) > 0, DataFormat.FloatingPoint, DataFormat.FixedInteger)
            End Get
            Set(ByVal value As DataFormat)
                If value = DataFormat.FloatingPoint Then
                    m_formatFlags = m_formatFlags Or FormatFlags.Phasors
                Else
                    m_formatFlags = m_formatFlags And Not FormatFlags.Phasors
                End If
            End Set
        End Property

        Public Overrides Property PhasorCoordinateFormat() As CoordinateFormat
            Get
                Return IIf((m_formatFlags And FormatFlags.Coordinates) > 0, CoordinateFormat.Polar, CoordinateFormat.Rectangular)
            End Get
            Set(ByVal value As CoordinateFormat)
                If value = CoordinateFormat.Polar Then
                    m_formatFlags = m_formatFlags Or FormatFlags.Coordinates
                Else
                    m_formatFlags = m_formatFlags And Not FormatFlags.Coordinates
                End If
            End Set
        End Property

        Public Overrides Property FrequencyDataFormat() As DataFormat
            Get
                Return IIf((m_formatFlags And FormatFlags.Frequency) > 0, DataFormat.FloatingPoint, DataFormat.FixedInteger)
            End Get
            Set(ByVal value As DataFormat)
                If value = DataFormat.FloatingPoint Then
                    m_formatFlags = m_formatFlags Or FormatFlags.Frequency
                Else
                    m_formatFlags = m_formatFlags And Not FormatFlags.Frequency
                End If
            End Set
        End Property

        Public Overrides Property AnalogDataFormat() As DataFormat
            Get
                Return IIf((m_formatFlags And FormatFlags.Analog) > 0, DataFormat.FloatingPoint, DataFormat.FixedInteger)
            End Get
            Set(ByVal value As DataFormat)
                If value = DataFormat.FloatingPoint Then
                    m_formatFlags = m_formatFlags Or FormatFlags.Analog
                Else
                    m_formatFlags = m_formatFlags And Not FormatFlags.Analog
                End If
            End Set
        End Property

        Protected Overrides ReadOnly Property HeaderLength() As UInt16
            Get
                Return MyBase.HeaderLength + 10
            End Get
        End Property

        Protected Overrides ReadOnly Property HeaderImage() As Byte()
            Get
                Dim buffer As Byte() = CreateArray(Of Byte)(HeaderLength)
                Dim index As Int32

                CopyImage(MyBase.HeaderImage, buffer, index, MyBase.HeaderLength)
                EndianOrder.BigEndian.CopyBytes(IDCode, buffer, index)
                EndianOrder.BigEndian.CopyBytes(m_formatFlags, buffer, index + 2)
                EndianOrder.BigEndian.CopyBytes(Convert.ToInt16(PhasorDefinitions.Count), buffer, index + 4)
                EndianOrder.BigEndian.CopyBytes(Convert.ToInt16(AnalogDefinitions.Count), buffer, index + 6)
                EndianOrder.BigEndian.CopyBytes(Convert.ToInt16(DigitalDefinitions.Count), buffer, index + 8)

                Return buffer
            End Get
        End Property

        Protected Overrides Sub ParseHeaderImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            Dim parsingState As IConfigurationCellParsingState = DirectCast(state, IConfigurationCellParsingState)

            ' Parse out station name
            MyBase.ParseHeaderImage(state, binaryImage, startIndex)
            startIndex += MyBase.HeaderLength

            IDCode = EndianOrder.BigEndian.ToUInt16(binaryImage, startIndex)
            m_formatFlags = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 2)

            With parsingState
                .PhasorCount = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 4)
                .AnalogCount = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 6)
                .DigitalCount = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 8)
            End With

        End Sub

        Protected Overrides ReadOnly Property FooterLength() As UInt16
            Get
                Return MyBase.FooterLength + _
                    PhasorDefinitions.Count * PhasorDefinition.ConversionFactorLength + _
                    AnalogDefinitions.Count * AnalogDefinition.ConversionFactorLength + _
                    DigitalDefinitions.Count * DigitalDefinition.ConversionFactorLength + _
                    IIf(Parent.DraftRevision = DraftRevision.Draft7, 2, 0)
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

                With AnalogDefinitions
                    For x = 0 To .Count - 1
                        CopyImage(DirectCast(.Item(x), AnalogDefinition).ConversionFactorImage, buffer, index, AnalogDefinition.ConversionFactorLength)
                    Next
                End With

                With DigitalDefinitions
                    For x = 0 To .Count - 1
                        CopyImage(DirectCast(.Item(x), DigitalDefinition).ConversionFactorImage, buffer, index, DigitalDefinition.ConversionFactorLength)
                    Next
                End With

                ' Include nominal frequency
                CopyImage(MyBase.FooterImage, buffer, index, MyBase.FooterLength)

                ' Include configuration count (new for version 7.0)
                If Parent.DraftRevision = DraftRevision.Draft7 Then EndianOrder.BigEndian.CopyBytes(m_configurationCount, buffer, index)

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

            With AnalogDefinitions
                For x = 0 To .Count - 1
                    DirectCast(.Item(x), AnalogDefinition).ParseConversionFactor(binaryImage, startIndex)
                    startIndex += AnalogDefinition.ConversionFactorLength
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

            ' Get configuration count (new for version 7.0)
            If Parent.DraftRevision = DraftRevision.Draft7 Then m_configurationCount = EndianOrder.BigEndian.ToUInt16(binaryImage, startIndex)

        End Sub

        Public Overrides Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext)

            MyBase.GetObjectData(info, context)

            ' Serialize configuration cell
            info.AddValue("formatFlags", m_formatFlags, GetType(FormatFlags))
            info.AddValue("configurationCount", m_configurationCount)

        End Sub

    End Class

End Namespace