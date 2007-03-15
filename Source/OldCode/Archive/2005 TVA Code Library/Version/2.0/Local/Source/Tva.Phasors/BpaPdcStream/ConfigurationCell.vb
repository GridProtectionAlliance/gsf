'*******************************************************************************************************
'  ConfigurationCell.vb - PDCstream PMU configuration cell
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
Imports Tva.Phasors.BpaPdcStream.Common

Namespace BpaPdcStream

    <CLSCompliant(False), Serializable()> _
    Public Class ConfigurationCell

        Inherits ConfigurationCellBase

        Private m_configurationFileCell As ConfigurationCell
        Private m_ieeeFormatFlags As IEEEFormatFlags
        Private m_offset As UInt16
        Private m_reserved As Int16

        Protected Sub New()
        End Sub

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            MyBase.New(info, context)

            ' Deserialize configuration cell
            m_ieeeFormatFlags = info.GetValue("IEEEFormatFlags", GetType(IEEEFormatFlags))
            m_offset = info.GetUInt16("offset")
            m_reserved = info.GetUInt16("reserved")

        End Sub

        Public Sub New(ByVal parent As IConfigurationFrame, ByVal idCode As UInt16, ByVal nominalFrequency As LineFrequency)

            MyBase.New(parent, True, idCode, nominalFrequency, MaximumPhasorValues, MaximumAnalogValues, MaximumDigitalValues)

        End Sub

        Public Sub New(ByVal configurationCell As IConfigurationCell)

            MyBase.New(configurationCell)

        End Sub

        ' This constructor satisfies ChannelCellBase class requirement:
        '   Final dervived classes must expose Public Sub New(ByVal parent As IChannelFrame, ByVal state As IChannelFrameParsingState, ByVal index As Int32, ByVal binaryImage As Byte(), ByVal startIndex As Int32)
        Public Sub New(ByVal parent As IConfigurationFrame, ByVal state As IConfigurationFrameParsingState, ByVal index As Int32, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            ' We don't pass in a ConfigurationCellParsingState here because it is not needed for PDCstream (see ParseBodyImage below)
            MyBase.New(parent, True, MaximumPhasorValues, MaximumAnalogValues, MaximumDigitalValues, Nothing, binaryImage, startIndex)

        End Sub

        Friend Shared Function CreateNewConfigurationCell(ByVal parent As IChannelFrame, ByVal state As IChannelFrameParsingState(Of IConfigurationCell), ByVal index As Int32, ByVal binaryImage As Byte(), ByVal startIndex As Int32) As IConfigurationCell

            Return New ConfigurationCell(parent, state, index, binaryImage, startIndex)

        End Function

        Public Overrides ReadOnly Property DerivedType() As System.Type
            Get
                Return Me.GetType
            End Get
        End Property

        Public Shadows ReadOnly Property Parent() As ConfigurationFrame
            Get
                Return MyBase.Parent
            End Get
        End Property

        Public Property ConfigurationFileCell() As ConfigurationCell
            Get
                Return m_configurationFileCell
            End Get
            Set(ByVal value As ConfigurationCell)
                m_configurationFileCell = value
            End Set
        End Property

        ' We use phasor definitions, station name, ID code and frequency definition of associated configuration cell that was read from INI file
        Public Overrides ReadOnly Property PhasorDefinitions() As PhasorDefinitionCollection
            Get
                If m_configurationFileCell Is Nothing Then
                    Return MyBase.PhasorDefinitions
                Else
                    Return m_configurationFileCell.PhasorDefinitions
                End If
            End Get
        End Property

        Public Overrides Property StationName() As String
            Get
                If m_configurationFileCell Is Nothing Then
                    Return MyBase.StationName
                Else
                    Return m_configurationFileCell.StationName
                End If
            End Get
            Set(ByVal value As String)
                If m_configurationFileCell Is Nothing Then
                    MyBase.StationName = value
                Else
                    m_configurationFileCell.StationName = value
                End If
            End Set
        End Property

        Public Overrides Property IDCode() As UInt16
            Get
                If m_configurationFileCell Is Nothing Then
                    Return MyBase.IDCode
                Else
                    Return m_configurationFileCell.IDCode
                End If
            End Get
            Set(ByVal value As UInt16)
                If m_configurationFileCell Is Nothing Then
                    MyBase.IDCode = value
                Else
                    m_configurationFileCell.IDCode = value
                End If
            End Set
        End Property

        Public Overrides Property FrequencyDefinition() As IFrequencyDefinition
            Get
                If m_configurationFileCell Is Nothing Then
                    Return MyBase.FrequencyDefinition
                Else
                    Return m_configurationFileCell.FrequencyDefinition
                End If
            End Get
            Set(ByVal value As IFrequencyDefinition)
                If m_configurationFileCell Is Nothing Then
                    MyBase.FrequencyDefinition = value
                Else
                    m_configurationFileCell.FrequencyDefinition = value
                End If
            End Set
        End Property

        Public Overrides ReadOnly Property MaximumStationNameLength() As Int32
            Get
                ' The station name in the PDCstream is read from an INI file, so there is no set limit
                Return Int32.MaxValue
            End Get
        End Property

        ' The PDCstream descriptor maintains offsets for cell data in data packet
        Public Property Offset() As UInt16
            Get
                Return m_offset
            End Get
            Set(ByVal value As UInt16)
                m_offset = value
            End Set
        End Property

        Public Property Reserved() As Int16
            Get
                Return m_reserved
            End Get
            Set(ByVal value As Int16)
                m_reserved = value
            End Set
        End Property

        ' These flags are defined in the data cell in the BPA PDCstream
        Public Property IEEEFormatFlags() As IEEEFormatFlags
            Get
                Return m_ieeeFormatFlags
            End Get
            Set(ByVal value As IEEEFormatFlags)
                m_ieeeFormatFlags = value
            End Set
        End Property

        Public Overrides Property PhasorCoordinateFormat() As CoordinateFormat
            Get
                Return IIf((m_ieeeFormatFlags And IEEEFormatFlags.Coordinates) > 0, CoordinateFormat.Polar, CoordinateFormat.Rectangular)
            End Get
            Set(ByVal value As CoordinateFormat)
                If value = CoordinateFormat.Polar Then
                    m_ieeeFormatFlags = m_ieeeFormatFlags Or IEEEFormatFlags.Coordinates
                Else
                    m_ieeeFormatFlags = m_ieeeFormatFlags And Not IEEEFormatFlags.Coordinates
                End If
            End Set
        End Property

        Public Overrides Property PhasorDataFormat() As DataFormat
            Get
                Return IIf((m_ieeeFormatFlags And IEEEFormatFlags.Phasors) > 0, DataFormat.FloatingPoint, DataFormat.FixedInteger)
            End Get
            Set(ByVal value As DataFormat)
                If value = DataFormat.FloatingPoint Then
                    m_ieeeFormatFlags = m_ieeeFormatFlags Or IEEEFormatFlags.Phasors
                Else
                    m_ieeeFormatFlags = m_ieeeFormatFlags And Not IEEEFormatFlags.Phasors
                End If
            End Set
        End Property

        Public Overrides Property FrequencyDataFormat() As DataFormat
            Get
                Return IIf((m_ieeeFormatFlags And IEEEFormatFlags.Frequency) > 0, DataFormat.FloatingPoint, DataFormat.FixedInteger)
            End Get
            Set(ByVal value As DataFormat)
                If value = DataFormat.FloatingPoint Then
                    m_ieeeFormatFlags = m_ieeeFormatFlags Or IEEEFormatFlags.Frequency
                Else
                    m_ieeeFormatFlags = m_ieeeFormatFlags And Not IEEEFormatFlags.Frequency
                End If
            End Set
        End Property

        Public Overrides Property AnalogDataFormat() As DataFormat
            Get
                Return IIf((m_ieeeFormatFlags And IEEEFormatFlags.Analog) > 0, DataFormat.FloatingPoint, DataFormat.FixedInteger)
            End Get
            Set(ByVal value As DataFormat)
                If value = DataFormat.FloatingPoint Then
                    m_ieeeFormatFlags = m_ieeeFormatFlags Or IEEEFormatFlags.Analog
                Else
                    m_ieeeFormatFlags = m_ieeeFormatFlags And Not IEEEFormatFlags.Analog
                End If
            End Set
        End Property

        ' The descriptor cell broadcasted by PDCstream only includes PMUID and offset, all
        ' other metadata is defined in an external INI based configuration file - so we
        ' override the base class image implementations which attempt to generate and
        ' parse data based on a common nature of configuration frames
        Protected Overrides ReadOnly Property HeaderLength() As UShort
            Get
                Return 0
            End Get
        End Property

        Protected Overrides ReadOnly Property HeaderImage() As Byte()
            Get
                Return Nothing
            End Get
        End Property

        Protected Overrides Sub ParseHeaderImage(ByVal state As IChannelParsingState, ByVal binaryImage() As Byte, ByVal startIndex As Integer)

            ' BPA PDC stream doesn't use standard configuration cell header like IEEE do - so we override this function to do nothing

        End Sub

        Protected Overrides ReadOnly Property BodyLength() As UInt16
            Get
                Return 8
            End Get
        End Property

        Protected Overrides ReadOnly Property BodyImage() As Byte()
            Get
                Dim buffer As Byte() = CreateArray(Of Byte)(BodyLength)
                Dim index As Int32

                CopyImage(IDLabelImage, buffer, index, IDLabelLength)       ' PMUID
                EndianOrder.BigEndian.CopyBytes(Reserved, buffer, index)    ' Reserved
                EndianOrder.BigEndian.CopyBytes(Offset, buffer, index + 2)  ' Offset

                Return buffer
            End Get
        End Property

        Protected Overrides Sub ParseBodyImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            IDLabel = Encoding.ASCII.GetString(binaryImage, startIndex, 4)
            Reserved = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 4)
            Offset = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 6)

        End Sub

        Protected Overrides ReadOnly Property FooterLength() As UShort
            Get
                Return 0
            End Get
        End Property

        Protected Overrides ReadOnly Property FooterImage() As Byte()
            Get
                Return Nothing
            End Get
        End Property

        Protected Overrides Sub ParseFooterImage(ByVal state As IChannelParsingState, ByVal binaryImage() As Byte, ByVal startIndex As Integer)

            ' BPA PDC stream doesn't use standard configuration cell footer like IEEE do - so we override this function to do nothing

        End Sub

        Public Overrides Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext)

            MyBase.GetObjectData(info, context)

            ' Serialize configuration cell
            info.AddValue("IEEEFormatFlags", m_ieeeFormatFlags, GetType(IEEEFormatFlags))
            info.AddValue("offset", m_offset)
            info.AddValue("reserved", m_reserved)

        End Sub

        Public Overrides ReadOnly Property Attributes() As System.Collections.Generic.Dictionary(Of String, String)
            Get
                Dim baseAttributes As Dictionary(Of String, String) = MyBase.Attributes

                baseAttributes.Add("Offset", m_offset)
                baseAttributes.Add("Reserved", m_reserved)

                Return baseAttributes
            End Get
        End Property

    End Class

End Namespace