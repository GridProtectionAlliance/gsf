'*******************************************************************************************************
'  ConfigurationFrame.vb - FNet Configuration Frame
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
Imports TVA.DateTime
Imports PhasorProtocols.Common
Imports TVA.IO.Compression.Common

Namespace FNet

    <CLSCompliant(False), Serializable()> _
    Public Class ConfigurationFrame

        Inherits ConfigurationFrameBase

        Protected Sub New()
        End Sub

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            MyBase.New(info, context)

        End Sub

        Public Sub New(ByVal idCode As UInt16, ByVal ticks As Long, ByVal frameRate As Int16, ByVal nominalFrequency As LineFrequency, ByVal stationName As String)

            MyBase.New(idCode, New ConfigurationCellCollection, ticks, frameRate)

            ' FNet protocol sends data for one device
            Cells.Add(New ConfigurationCell(Me, nominalFrequency))

            With Cells(0)
                ' Assign station name
                If String.IsNullOrEmpty(stationName) Then
                    .StationName = "FNET Unit-" & idCode
                Else
                    .StationName = stationName
                End If

                ' Add a single frequency definition
                .FrequencyDefinition = New FrequencyDefinition(DirectCast(.This, ConfigurationCell))

                ' Add a single phasor definition
                .PhasorDefinitions.Add(New PhasorDefinition(DirectCast(.This, ConfigurationCell)))

                With .PhasorDefinitions(0)
                    .Label = "120V Phasor"
                    .Type = PhasorType.Voltage
                End With
            End With

        End Sub

        ' FNet supports no configuration frame in the data stream - so there will be nothing to parse
        'Public Sub New(ByVal binaryImage As Byte(), ByVal startIndex As Int32)

        '    MyBase.New(New ConfigurationFrameParsingState(New ConfigurationCellCollection, 0, _
        '            AddressOf FNet.ConfigurationCell.CreateNewConfigurationCell), binaryImage, startIndex)

        'End Sub

        Public Sub New(ByVal configurationFrame As IConfigurationFrame)

            MyBase.New(configurationFrame)

        End Sub

        Public Overrides ReadOnly Property DerivedType() As System.Type
            Get
                Return Me.GetType
            End Get
        End Property

        Public Shadows ReadOnly Property Cells() As ConfigurationCellCollection
            Get
                Return MyBase.Cells
            End Get
        End Property

        ' Since FNet only supports a single device there will only be one cell, so we just share nominal frequency, longitude,
        ' latitude and number of satellites with our only child and expose the value at the parent level for convience
        Public Property NominalFrequency() As LineFrequency
            Get
                Return Cells(0).NominalFrequency
            End Get
            Set(ByVal value As LineFrequency)
                Cells(0).NominalFrequency = value
            End Set
        End Property

        Public Property Longitude() As Single
            Get
                Return Cells(0).Longitude
            End Get
            Set(ByVal value As Single)
                Cells(0).Longitude = value
            End Set
        End Property

        Public Property Latitude() As Single
            Get
                Return Cells(0).Latitude
            End Get
            Set(ByVal value As Single)
                Cells(0).Latitude = value
            End Set
        End Property

        Public Property NumberOfSatellites() As Integer
            Get
                Return Cells(0).NumberOfSatellites
            End Get
            Set(ByVal value As Integer)
                Cells(0).NumberOfSatellites = value
            End Set
        End Property

    End Class

End Namespace