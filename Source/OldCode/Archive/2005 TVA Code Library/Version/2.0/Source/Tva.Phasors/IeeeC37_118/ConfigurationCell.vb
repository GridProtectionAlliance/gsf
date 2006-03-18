'*******************************************************************************************************
'  ConfigurationCell.vb - IEEE C37.118 PMU configuration cell
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
'  11/12/2004 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Text
Imports Tva.Interop
Imports Tva.Interop.Bit
Imports Tva.Collections.Common
Imports Tva.Phasors.Common
Imports Tva.Phasors.IeeeC37_118.Common

Namespace IeeeC37_118

    <CLSCompliant(False)> _
    Public Class ConfigurationCell

        Inherits ConfigurationCellBase

        Private m_formatFlags As FormatFlags

        Public Sub New(ByVal parent As ConfigurationFrame, ByVal idCode As UInt16, ByVal nominalFrequency As LineFrequency)

            MyBase.New(parent, False, idCode, nominalFrequency, MaximumPhasorValues, MaximumAnalogValues, MaximumDigitalValues)

        End Sub

        Public Sub New(ByVal configurationCell As IConfigurationCell)

            MyBase.New(configurationCell)

        End Sub

        ' This constructor satisfies ChannelCellBase class requirement:
        '   Final dervived classes must expose Public Sub New(ByVal parent As IChannelFrame, ByVal state As IChannelFrameParsingState, ByVal index As Integer, ByVal binaryImage As Byte(), ByVal startIndex As Integer)
        Public Sub New(ByVal parent As IConfigurationFrame, ByVal state As IConfigurationFrameParsingState, ByVal index As Integer, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            ' We pass in defaults for id code and nominal frequency since these will be parsed out later
            MyBase.New(parent, False, MaximumPhasorValues, MaximumAnalogValues, MaximumDigitalValues, _
                New ConfigurationCellParsingState( _
                    AddressOf IeeeC37_118.PhasorDefinition.CreateNewPhasorDefintion, _
                    AddressOf IeeeC37_118.FrequencyDefinition.CreateNewFrequencyDefintion, _
                    AddressOf IeeeC37_118.AnalogDefinition.CreateNewAnalogDefintion, _
                    AddressOf IeeeC37_118.DigitalDefinition.CreateNewDigitalDefintion), _
                binaryImage, startIndex)

        End Sub

        Friend Shared Function CreateNewConfigurationCell(ByVal parent As IChannelFrame, ByVal state As IChannelFrameParsingState(Of IConfigurationCell), ByVal index As Integer, ByVal binaryImage As Byte(), ByVal startIndex As Integer) As IConfigurationCell

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

                ' Propogate new settings to all configuration definitions...
                For Each phasorDefinition As IPhasorDefinition In PhasorDefinitions
                    phasorDefinition.DataFormat = IIf(value And IeeeC37_118.FormatFlags.Phasors > 0, DataFormat.FloatingPoint, DataFormat.FixedInteger)
                    phasorDefinition.CoordinateFormat = IIf(value And IeeeC37_118.FormatFlags.Coordinates > 0, CoordinateFormat.Polar, CoordinateFormat.Rectangular)
                Next

                FrequencyDefinition.DataFormat = IIf(value And IeeeC37_118.FormatFlags.Frequency > 0, DataFormat.FloatingPoint, DataFormat.FixedInteger)

                For Each analogDefinition As IAnalogDefinition In AnalogDefinitions
                    analogDefinition.DataFormat = IIf(value And IeeeC37_118.FormatFlags.Analog > 0, DataFormat.FloatingPoint, DataFormat.FixedInteger)
                Next
            End Set
        End Property

        Protected Overrides ReadOnly Property HeaderLength() As Int16
            Get
                Return MyBase.HeaderLength + 10
            End Get
        End Property

        Protected Overrides ReadOnly Property HeaderImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), HeaderLength)
                Dim index As Integer

                CopyImage(MyBase.HeaderImage, buffer, index, MyBase.HeaderLength)
                EndianOrder.BigEndian.CopyBytes(IDCode, buffer, index)
                EndianOrder.BigEndian.CopyBytes(m_formatFlags, buffer, index + 2)
                EndianOrder.BigEndian.CopyBytes(Convert.ToInt16(PhasorDefinitions.Count), buffer, index + 4)
                EndianOrder.BigEndian.CopyBytes(Convert.ToInt16(AnalogDefinitions.Count), buffer, index + 6)
                EndianOrder.BigEndian.CopyBytes(Convert.ToInt16(DigitalDefinitions.Count), buffer, index + 8)

                Return buffer
            End Get
        End Property

        Protected Overrides Sub ParseHeaderImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            Dim parsingState As IConfigurationCellParsingState = DirectCast(state, IConfigurationCellParsingState)

            ' Parse out station name
            MyBase.ParseHeaderImage(state, binaryImage, startIndex)
            startIndex += MyBase.HeaderLength

            IDCode = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex)
            m_formatFlags = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 2)

            With parsingState
                .PhasorCount = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 4)
                .AnalogCount = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 6)
                .DigitalCount = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 8)
            End With

        End Sub

        Protected Overrides ReadOnly Property FooterLength() As Int16
            Get
                Return MyBase.FooterLength + _
                    PhasorDefinitions.Count * PhasorDefinition.ConversionFactorLength + _
                    AnalogDefinitions.Count * AnalogDefinition.ConversionFactorLength + _
                    DigitalDefinitions.Count * DigitalDefinition.ConversionFactorLength
            End Get
        End Property

        Protected Overrides ReadOnly Property FooterImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), FooterLength)
                Dim x, index As Integer

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

                Return buffer
            End Get
        End Property

        Protected Overrides Sub ParseFooterImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            Dim x As Integer

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

        End Sub

    End Class

End Namespace