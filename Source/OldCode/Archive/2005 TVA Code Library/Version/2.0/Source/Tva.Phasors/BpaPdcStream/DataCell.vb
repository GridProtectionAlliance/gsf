'*******************************************************************************************************
'  DataCell.vb - PDCstream PMU Data Cell
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

Imports Tva.Interop
Imports Tva.Phasors.BpaPdcStream.Common

Namespace BpaPdcStream

    ' This data cell represents what most might call a "field" in table of rows - it is a single unit of data for a specific PMU
    Public Class DataCell

        Inherits DataCellBase

        Private m_flags As ChannelFlags
        Private m_reservedFlags As ReservedFlags
        Private m_iEEEFormatFlags As IEEEFormatFlags
        Private m_sampleNumber As Int16

        Public Sub New(ByVal parent As IDataFrame, ByVal configurationCell As IConfigurationCell, ByVal sampleNumber As Int16)

            MyBase.New(parent, True, configurationCell, MaximumPhasorValues, MaximumAnalogValues, MaximumDigitalValues)

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

            MyBase.New(parent, True, MaximumPhasorValues, MaximumAnalogValues, MaximumDigitalValues, New DataCellParsingState( _
                GetType(PhasorValue), GetType(FrequencyValue), GetType(AnalogValue), GetType(DigitalValue), state.ConfigurationFrame.Cells(index)), _
                binaryImage, startIndex)

        End Sub

        Public Overrides ReadOnly Property InheritedType() As System.Type
            Get
                Return Me.GetType()
            End Get
        End Property

        ' Note: this is only the first byte of the channel flag word
        Public Property ChannelFlags() As ChannelFlags
            Get
                Return m_flags
            End Get
            Set(ByVal Value As ChannelFlags)
                m_flags = Value
            End Set
        End Property

        Public Property ReservedFlags() As ReservedFlags
            Get
                Return m_reservedFlags
            End Get
            Set(ByVal Value As ReservedFlags)
                m_reservedFlags = Value
            End Set
        End Property

        Public Property IEEEFormatFlags() As IEEEFormatFlags
            Get
                Return m_iEEEFormatFlags
            End Get
            Set(ByVal Value As IEEEFormatFlags)
                m_iEEEFormatFlags = Value
            End Set
        End Property

        Public Property SampleNumber() As Int16
            Get
                Return m_sampleNumber
            End Get
            Set(ByVal Value As Int16)
                m_sampleNumber = Value
            End Set
        End Property

        ' These properties make it easier to manage channel flags
        Public Property ReservedFlag0IsSet() As Boolean
            Get
                Return ((m_reservedFlags And ReservedFlags.Reserved0) > 0)
            End Get
            Set(ByVal Value As Boolean)
                If Value Then
                    m_reservedFlags = m_reservedFlags Or ReservedFlags.Reserved0
                Else
                    m_reservedFlags = m_reservedFlags And Not ReservedFlags.Reserved0
                End If
            End Set
        End Property

        Public Property ReservedFlag1IsSet() As Boolean
            Get
                Return ((m_reservedFlags And ReservedFlags.Reserved1) > 0)
            End Get
            Set(ByVal Value As Boolean)
                If Value Then
                    m_reservedFlags = m_reservedFlags Or ReservedFlags.Reserved1
                Else
                    m_reservedFlags = m_reservedFlags And Not ReservedFlags.Reserved1
                End If
            End Set
        End Property

        Public Property FrequencyDataFormat() As DataFormat
            Get
                Return IIf((m_iEEEFormatFlags And IEEEFormatFlags.Frequency) > 0, DataFormat.FloatingPoint, DataFormat.FixedInteger)
            End Get
            Set(ByVal Value As DataFormat)
                If Value = DataFormat.FloatingPoint Then
                    m_iEEEFormatFlags = m_iEEEFormatFlags Or IEEEFormatFlags.Frequency
                Else
                    m_iEEEFormatFlags = m_iEEEFormatFlags And Not IEEEFormatFlags.Frequency
                End If

                ConfigurationCell.FrequencyDefinition.DataFormat = Value
            End Set
        End Property

        Public Property AnalogDataFormat() As DataFormat
            Get
                Return IIf((m_iEEEFormatFlags And IEEEFormatFlags.Analog) > 0, DataFormat.FloatingPoint, DataFormat.FixedInteger)
            End Get
            Set(ByVal Value As DataFormat)
                If Value = DataFormat.FloatingPoint Then
                    m_iEEEFormatFlags = m_iEEEFormatFlags Or IEEEFormatFlags.Analog
                Else
                    m_iEEEFormatFlags = m_iEEEFormatFlags And Not IEEEFormatFlags.Analog
                End If

                ConfigurationCell.AnalogDefinitions.SetDataFormat(Value)
            End Set
        End Property

        Public Property PhasorDataFormat() As DataFormat
            Get
                Return IIf((m_iEEEFormatFlags And IEEEFormatFlags.Phasors) > 0, DataFormat.FloatingPoint, DataFormat.FixedInteger)
            End Get
            Set(ByVal Value As DataFormat)
                If Value = DataFormat.FloatingPoint Then
                    m_iEEEFormatFlags = m_iEEEFormatFlags Or IEEEFormatFlags.Phasors
                Else
                    m_iEEEFormatFlags = m_iEEEFormatFlags And Not IEEEFormatFlags.Phasors
                End If

                ConfigurationCell.PhasorDefinitions.SetDataFormat(Value)
            End Set
        End Property

        Public Property PhasorCoordinateFormat() As CoordinateFormat
            Get
                Return IIf((m_iEEEFormatFlags And IEEEFormatFlags.Coordinates) > 0, CoordinateFormat.Polar, CoordinateFormat.Rectangular)
            End Get
            Set(ByVal Value As CoordinateFormat)
                If Value = CoordinateFormat.Polar Then
                    m_iEEEFormatFlags = m_iEEEFormatFlags Or IEEEFormatFlags.Coordinates
                Else
                    m_iEEEFormatFlags = m_iEEEFormatFlags And Not IEEEFormatFlags.Coordinates
                End If

                ConfigurationCell.PhasorDefinitions.SetCoordinateFormat(Value)
            End Set
        End Property

        Public Property DataIsValid() As Boolean
            Get
                Return ((m_flags And ChannelFlags.DataIsValid) = 0)
            End Get
            Set(ByVal Value As Boolean)
                If Value Then
                    m_flags = m_flags And Not ChannelFlags.DataIsValid
                Else
                    m_flags = m_flags Or ChannelFlags.DataIsValid
                End If
            End Set
        End Property

        Public Property TransmissionErrors() As Boolean
            Get
                Return ((m_flags And ChannelFlags.TransmissionErrors) > 0)
            End Get
            Set(ByVal Value As Boolean)
                If Value Then
                    m_flags = m_flags Or ChannelFlags.TransmissionErrors
                Else
                    m_flags = m_flags And Not ChannelFlags.TransmissionErrors
                End If
            End Set
        End Property

        Public Property PMUSynchronized() As Boolean
            Get
                Return ((m_flags And ChannelFlags.PMUSynchronized) = 0)
            End Get
            Set(ByVal Value As Boolean)
                If Value Then
                    m_flags = m_flags And Not ChannelFlags.PMUSynchronized
                Else
                    m_flags = m_flags Or ChannelFlags.PMUSynchronized
                End If
            End Set
        End Property

        Public Property DataIsSortedByArrival() As Boolean
            Get
                Return ((m_flags And ChannelFlags.DataSortedByArrival) > 0)
            End Get
            Set(ByVal Value As Boolean)
                If Value Then
                    m_flags = m_flags Or ChannelFlags.DataSortedByArrival
                Else
                    m_flags = m_flags And Not ChannelFlags.DataSortedByArrival
                End If
            End Set
        End Property

        <Obsolete("This bit definition is for obsolete uses that is no longer needed.", False)> _
        Public Property DataIsSortedByTimestamp() As Boolean
            Get
                Return ((m_flags And ChannelFlags.DataSortedByTimestamp) = 0)
            End Get
            Set(ByVal Value As Boolean)
                If Value Then
                    m_flags = m_flags And Not ChannelFlags.DataSortedByTimestamp
                Else
                    m_flags = m_flags Or ChannelFlags.DataSortedByTimestamp
                End If
            End Set
        End Property

        Public Property UsingPDCExchangeFormat() As Boolean
            Get
                Return ((m_flags And ChannelFlags.PDCExchangeFormat) > 0)
            End Get
            Set(ByVal Value As Boolean)
                If Value Then
                    m_flags = m_flags Or ChannelFlags.PDCExchangeFormat
                Else
                    m_flags = m_flags And Not ChannelFlags.PDCExchangeFormat
                End If
            End Set
        End Property

        Public Property UsingMacrodyneFormat() As Boolean
            Get
                Return ((m_flags And ChannelFlags.MacrodyneFormat) > 0)
            End Get
            Set(ByVal Value As Boolean)
                If Value Then
                    m_flags = m_flags Or ChannelFlags.MacrodyneFormat
                Else
                    m_flags = m_flags And Not ChannelFlags.MacrodyneFormat
                End If
            End Set
        End Property

        Public Property UsingIEEEFormat() As Boolean
            Get
                Return ((m_flags And ChannelFlags.MacrodyneFormat) = 0)
            End Get
            Set(ByVal Value As Boolean)
                If Value Then
                    m_flags = m_flags And Not ChannelFlags.MacrodyneFormat
                Else
                    m_flags = m_flags Or ChannelFlags.MacrodyneFormat
                End If
            End Set
        End Property

        <Obsolete("This bit definition is for obsolete uses that is no longer needed.", False)> _
        Public Property TimestampIsIncluded() As Boolean
            Get
                Return ((m_flags And ChannelFlags.TimestampIncluded) = 0)
            End Get
            Set(ByVal Value As Boolean)
                If Value Then
                    m_flags = m_flags And Not ChannelFlags.TimestampIncluded
                Else
                    m_flags = m_flags Or ChannelFlags.TimestampIncluded
                End If
            End Set
        End Property

        Protected Overrides ReadOnly Property HeaderLength() As Int16
            Get
                Return 6
            End Get
        End Property

        Protected Overrides ReadOnly Property HeaderImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), HeaderLength)

                ' Add PDCstream specific image
                buffer(0) = m_flags
                buffer(1) = (Convert.ToByte(AnalogValues.Count) Or m_reservedFlags)
                buffer(2) = (Convert.ToByte(DigitalValues.Count) Or m_iEEEFormatFlags)
                buffer(3) = Convert.ToByte(PhasorValues.Count)
                EndianOrder.BigEndian.CopyBytes(m_sampleNumber, buffer, 4)

                Return buffer
            End Get
        End Property

        Protected Overrides Sub ParseHeaderImage(ByVal state As IChannelParsingState, ByVal binaryImage() As Byte, ByVal startIndex As Integer)

            ' Parse PDCstream specific header image
            m_flags = binaryImage(startIndex)

            Dim analogWords As Byte = binaryImage(startIndex + 1)
            Dim digitalWords As Byte = binaryImage(startIndex + 2)
            Dim phasorWords As Byte = binaryImage(startIndex + 3)

            ' Strip off IEEE flags
            m_reservedFlags = (analogWords And Not ReservedFlags.AnalogWordsMask)
            m_iEEEFormatFlags = (digitalWords And Not IEEEFormatFlags.DigitalWordsMask)

            ' Leave word counts
            analogWords = (analogWords And ReservedFlags.AnalogWordsMask)
            digitalWords = (digitalWords And IEEEFormatFlags.DigitalWordsMask)

            ' Algorithm Case: Determine best course of action when stream counts don't match
            ' configuration file.  Think about what *will* happen when new data appears in
            ' the stream that's not in the config file - you could raise an event notifying
            ' consumer about the mismatch instead of raising an exception - could even make
            ' a boolean property that would allow either case.  The important thing to consider
            ' is that to parse the cell images you have to have a defined definition (see base
            ' class "Phasor.DataCellBase.ParseDataCell" - more in stream than in config file
            ' and you won't get the new value, too few and you don't have enough definitions -
            ' that would be bad - either way the definitions won't line up with the appropriate
            ' data value and you won't know which one is missing or added.  I can't change the
            ' protocol so this is enough argument to just raise an error for config file/stream
            ' mismatch.  So for now we'll just throw an exception and deal with consequences :)
            ' Note that this only applies to PDCstream protocol

            If phasorWords <> ConfigurationCell.PhasorDefinitions.Count Then
                Throw New InvalidOperationException("Stream/Config File Mismatch: Phasor value count in stream (" & phasorWords & ") does not match defined count in configuration file (" & ConfigurationCell.PhasorDefinitions.Count & ")")
            End If

            ' TODO: If analog values get a clear definition in INI file at some point, we can validate the number in the stream to the number in the config file...
            'If analogWords <> ConfigurationCell.AnalogDefinitions.Count Then
            '    Throw New InvalidOperationException("Stream/Config File Mismatch: Analog value count in stream (" analogWords & ") does not match defined count in configuration file (" & ConfigurationCell.AnalogDefinitions.Count & ")")
            'End If

            ' TODO: If digital values get a clear definition in INI file at some point, we can validate the number in the stream to the number in the config file...
            'If digitalWords <> ConfigurationCell.DigitalDefinitions.Count Then
            '    Throw New InvalidOperationException("Stream/Config File Mismatch: Digital value count in stream (" digitalWords & ") does not match defined count in configuration file (" & ConfigurationCell.DigitalDefinitions.Count & ")")
            'End If

            m_sampleNumber = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 4)

        End Sub

    End Class

End Namespace