'*******************************************************************************************************
'  DataCell.vb - IEEE C37.118  PMU Data Cell
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
Imports TVA.Interop
Imports TVA.EE.Phasor.IEEEC37_118.Common

Namespace EE.Phasor.IEEEC37_118

    ' This data cell represents what most might call a "field" in table of rows - it is a single unit of data for a specific PMU
    Public Class DataCell

        Inherits DataCellBase

        Private m_sampleNumber As Int16

        Public Sub New(ByVal parent As IDataFrame, ByVal configurationCell As IConfigurationCell, ByVal sampleNumber As Int16)

            MyBase.New(parent, False, configurationCell, MaximumPhasorValues, MaximumAnalogValues, MaximumDigitalValues)

            Dim x As Integer

            With configurationCell
                m_sampleNumber = sampleNumber

                ' Initialize phasor values and frequency value with an empty value
                For x = 0 To .PhasorDefinitions.Count - 1
                    PhasorValues.Add(New PhasorValue(Me, .PhasorDefinitions(x), 0, 0))
                Next

                ' Initialize frequency and df/dt
                FrequencyValue = New FrequencyValue(Me, .FrequencyDefinition, 0, 0)

                ' Initialize analog values
                For x = 0 To .AnalogDefinitions.Count - 1
                    AnalogValues.Add(New AnalogValue(Me, .AnalogDefinitions(x), 0))
                Next

                ' Initialize any digital values
                For x = 0 To .DigitalDefinitions.Count - 1
                    DigitalValues.Add(New DigitalValue(Me, .DigitalDefinitions(x), 0))
                Next
            End With

        End Sub

        Public Sub New(ByVal dataCell As IDataCell)

            MyBase.New(dataCell)

        End Sub

        ' This constructor satisfies ChannelCellBase class requirement:
        '   ' Final dervived classes must expose Public Sub New(ByVal parent As IChannelFrame, ByVal state As IChannelFrameParsingState, ByVal index As Integer, ByVal binaryImage As Byte(), ByVal startIndex As Integer)
        Public Sub New(ByVal parent As IDataFrame, ByVal state As DataFrameParsingState, ByVal index As Integer, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            MyBase.New(parent, False, MaximumPhasorValues, MaximumAnalogValues, MaximumDigitalValues, New DataCellParsingState( _
                GetType(PhasorValue), GetType(FrequencyValue), GetType(AnalogValue), GetType(DigitalValue), state.ConfigurationFrame.Cells(index)), _
                binaryImage, startIndex)

        End Sub

        Public Overrides ReadOnly Property InheritedType() As System.Type
            Get
                Return Me.GetType()
            End Get
        End Property

        Public Property SampleNumber() As Int16
            Get
                Return m_sampleNumber
            End Get
            Set(ByVal Value As Int16)
                m_sampleNumber = Value
            End Set
        End Property

        Protected Overrides ReadOnly Property HeaderImage() As Byte()
            Get
                'Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), HeaderLength)

                '' Add PDCstream specific image
                'buffer(0) = m_flags
                'buffer(1) = (Convert.ToByte(AnalogValues.Count) Or m_reservedFlags)
                'buffer(2) = (Convert.ToByte(DigitalValues.Count) Or m_iEEEFormatFlags)
                'buffer(3) = Convert.ToByte(PhasorValues.Count)
                'EndianOrder.BigEndian.CopyBytes(m_sampleNumber, buffer, 4)

                'Return buffer
            End Get
        End Property

        Protected Overrides Sub ParseHeaderImage(ByVal state As IChannelParsingState, ByVal binaryImage() As Byte, ByVal startIndex As Integer)


            ' Parse PDCstream specific header image
            'm_flags = binaryImage(startIndex)

            'Dim analogWords As Byte = binaryImage(startIndex + 1)
            'Dim digitalWords As Byte = binaryImage(startIndex + 2)
            'Dim phasorWords As Byte = binaryImage(startIndex + 3)

            '' Strip off IEEE flags
            'm_reservedFlags = (analogWords And Not ReservedFlags.AnalogWordsMask)
            'm_iEEEFormatFlags = (digitalWords And Not IEEEFormatFlags.DigitalWordsMask)

            '' Leave word counts
            'analogWords = (analogWords And ReservedFlags.AnalogWordsMask)
            'digitalWords = (digitalWords And IEEEFormatFlags.DigitalWordsMask)

            '' Algorithm Case: Determine best course of action when stream counts don't match
            '' configuration file.  Think about what *will* happen when new data appears in
            '' the stream that's not in the config file - you could raise an event notifying
            '' consumer about the mismatch instead of raising an exception - could even make
            '' a boolean property that would allow either case.  The important thing to consider
            '' is that to parse the cell images you have to have a defined definition (see base
            '' class "Phasor.DataCellBase.ParseDataCell" - more in stream than in config file
            '' and you won't get the new value, too few and you don't have enough definitions -
            '' that would be bad - either way the definitions won't line up with the appropriate
            '' data value and you won't know which one is missing or added.  I can't change the
            '' protocol so this is enough argument to just raise an error for config file/stream
            '' mismatch.  So for now we'll just throw an exception and deal with consequences :)
            '' Note that this only applies to PDCstream protocol

            'If phasorWords <> ConfigurationCell.PhasorDefinitions.Count Then
            '    Throw New InvalidOperationException("Stream/Config File Mismatch: Phasor value count in stream (" & phasorWords & ") does not match defined count in configuration file (" & ConfigurationCell.PhasorDefinitions.Count & ")")
            'End If

            '' TODO: If analog values get a clear definition in INI file at some point, we can validate the number in the stream to the number in the config file...
            ''If analogWords <> ConfigurationCell.AnalogDefinitions.Count Then
            ''    Throw New InvalidOperationException("Stream/Config File Mismatch: Analog value count in stream (" analogWords & ") does not match defined count in configuration file (" & ConfigurationCell.AnalogDefinitions.Count & ")")
            ''End If

            '' TODO: If digital values get a clear definition in INI file at some point, we can validate the number in the stream to the number in the config file...
            ''If digitalWords <> ConfigurationCell.DigitalDefinitions.Count Then
            ''    Throw New InvalidOperationException("Stream/Config File Mismatch: Digital value count in stream (" digitalWords & ") does not match defined count in configuration file (" & ConfigurationCell.DigitalDefinitions.Count & ")")
            ''End If

            'm_sampleNumber = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 4)

        End Sub

    End Class

End Namespace