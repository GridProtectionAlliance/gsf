'*******************************************************************************************************
'  ConfigurationCell.vb - FNet Cconfiguration cell
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
'  02/08/2007 - J. Ritchie Carroll & Jian (Ryan) Zuo
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Runtime.Serialization
Imports System.Text
Imports PhasorProtocols.Common
Imports PhasorProtocols.FNet.Common
Imports TVA.DateTime.Common

Namespace FNet

    <CLSCompliant(False), Serializable()> _
    Public Class ConfigurationCell

        Inherits ConfigurationCellBase

        Private m_ticksOffset As Long
        Private m_longitude As Single
        Private m_latitude As Single
        Private m_numberOfSatellites As Integer = 1 ' We'll initially assume synchronization is good until told otherwise

        Protected Sub New()
        End Sub

        ''' <summary>
        ''' Deserialize the configuration cell. Retrieve the Longitude, Latitude and the number of satellite
        ''' </summary>
        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            MyBase.New(info, context)

            ' Deserialize configuration cell
            m_ticksOffset = info.GetInt64("ticksOffset")
            m_longitude = info.GetSingle("longitude")
            m_latitude = info.GetSingle("latitude")
            m_numberOfSatellites = info.GetInt32("numberOfSatellites")

        End Sub

        Public Sub New(ByVal parent As ConfigurationFrame, ByVal nominalFrequency As LineFrequency, ByVal ticksOffset As Long)

            MyBase.New(parent, False, 0, nominalFrequency, MaximumPhasorValues, MaximumAnalogValues, MaximumDigitalValues)
            m_ticksOffset = ticksOffset

        End Sub

        Public Sub New(ByVal configurationCell As IConfigurationCell)

            MyBase.New(configurationCell)

        End Sub

        ' FNet supports no configuration frame in the data stream - so there will be nothing to parse

        '' This constructor satisfies ChannelCellBase class requirement:
        ''   Final dervived classes must expose Public Sub New(ByVal parent As IChannelFrame, ByVal state As IChannelFrameParsingState, ByVal index As Int32, ByVal binaryImage As Byte(), ByVal startIndex As Int32)
        'Public Sub New(ByVal parent As IConfigurationFrame, ByVal state As IConfigurationFrameParsingState, ByVal index As Int32, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

        '    '' We pass in defaults for id code and nominal frequency since these will be parsed out later
        '    'MyBase.New(parent, False, MaximumPhasorValues, MaximumAnalogValues, MaximumDigitalValues, _
        '    '    New ConfigurationCellParsingState( _
        '    '        AddressOf FNet.PhasorDefinition.CreateNewPhasorDefintion, _
        '    '        AddressOf FNet.FrequencyDefinition.CreateNewFrequencyDefintion, _
        '    '        Nothing, _
        '    '        Nothing), _
        '    '    binaryImage, startIndex)

        'End Sub

        'Friend Shared Function CreateNewConfigurationCell(ByVal parent As IChannelFrame, ByVal state As IChannelFrameParsingState(Of IConfigurationCell), ByVal index As Int32, ByVal binaryImage As Byte(), ByVal startIndex As Int32) As IConfigurationCell

        '    Return New ConfigurationCell(parent, state, index, binaryImage, startIndex)

        'End Function

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

        ' FNet protocol only allows one device, so we share ID code with parent frame...
        Public Overrides Property IDCode() As UInt16
            Get
                Return Parent.IDCode
            End Get
            Set(ByVal value As UInt16)
                Parent.IDCode = value
            End Set
        End Property

        Public Property TicksOffset() As Long
            Get
                Return m_ticksOffset
            End Get
            Set(ByVal value As Long)
                m_ticksOffset = value
            End Set
        End Property

        Public Property Longitude() As Single
            Get
                Return m_longitude
            End Get
            Set(ByVal value As Single)
                m_longitude = value
            End Set
        End Property

        Public Property Latitude() As Single
            Get
                Return m_latitude
            End Get
            Set(ByVal value As Single)
                m_latitude = value
            End Set
        End Property

        Public Property NumberOfSatellites() As Integer
            Get
                Return m_numberOfSatellites
            End Get
            Set(ByVal value As Integer)
                m_numberOfSatellites = value
            End Set
        End Property

        Public Overrides ReadOnly Property MaximumStationNameLength() As Int32
            Get
                ' The station name is defined external to the protocol, so there is no set limit
                Return Int32.MaxValue
            End Get
        End Property

        ' FNet only supports floating point data
        Public Overrides Property PhasorDataFormat() As DataFormat
            Get
                Return DataFormat.FloatingPoint
            End Get
            Set(ByVal value As DataFormat)
                If value <> DataFormat.FloatingPoint Then Throw New NotSupportedException("FNet only supports floating point data")
            End Set
        End Property

        Public Overrides Property PhasorCoordinateFormat() As CoordinateFormat
            Get
                Return CoordinateFormat.Polar
            End Get
            Set(ByVal value As CoordinateFormat)
                If value <> CoordinateFormat.Polar Then Throw New NotSupportedException("FNet only supports polar coordinates")
            End Set
        End Property

        Public Overrides Property FrequencyDataFormat() As DataFormat
            Get
                Return DataFormat.FloatingPoint
            End Get
            Set(ByVal value As DataFormat)
                If value <> DataFormat.FloatingPoint Then Throw New NotSupportedException("FNet only supports floating point data")
            End Set
        End Property

        Public Overrides Property AnalogDataFormat() As DataFormat
            Get
                Return DataFormat.FloatingPoint
            End Get
            Set(ByVal value As DataFormat)
                If value <> DataFormat.FloatingPoint Then Throw New NotSupportedException("FNet only supports floating point data")
            End Set
        End Property

        ''' <summary>
        ''' Serialize the parameters of Longitude, Latitude and numberOfSatellite
        ''' </summary>
        Public Overrides Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext)

            MyBase.GetObjectData(info, context)

            ' Serialize configuration cell
            info.AddValue("ticksOffset", m_ticksOffset)
            info.AddValue("longitude", m_longitude)
            info.AddValue("latitude", m_latitude)
            info.AddValue("numberOfSatellites", m_numberOfSatellites)

        End Sub

        ''' <summary>
        ''' Retrieve the attribute include Longitude, Latitude and NumberOfSatellite
        ''' </summary>
        Public Overrides ReadOnly Property Attributes() As Dictionary(Of String, String)
            Get
                Dim baseAttributes As Dictionary(Of String, String) = MyBase.Attributes

                baseAttributes.Add("Time Offset (ticks)", m_ticksOffset)
                baseAttributes.Add("Time Offset (seconds)", m_ticksOffset / TicksPerSecond)
                baseAttributes.Add("Longitude", Longitude)
                baseAttributes.Add("Latitude", Latitude)
                baseAttributes.Add("Number of Satellites", NumberOfSatellites)

                Return baseAttributes
            End Get
        End Property

    End Class

End Namespace