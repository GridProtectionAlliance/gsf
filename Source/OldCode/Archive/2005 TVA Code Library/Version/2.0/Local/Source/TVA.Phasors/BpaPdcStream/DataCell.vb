'*******************************************************************************************************
'  DataCell.vb - PDCstream PMU Data Cell
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
Imports TVA.Phasors.BpaPdcStream.Common

Namespace Phasors.BpaPdcStream

    ' This data cell represents what most might call a "field" in table of rows - it is a single unit of data for a specific PMU
    <CLSCompliant(False), Serializable()> _
    Public Class DataCell

        Inherits DataCellBase

        Private m_flags As ChannelFlags
        Private m_reservedFlags As ReservedFlags
        Private m_sampleNumber As Int16
        Private m_dataRate As Byte
        Private m_pdcBlockPmuCount As Byte
        Private m_isPdcBlockPmu As Boolean
        Private m_isPdcBlockHeader As Boolean
        Private m_pdcBlockLength As Integer

        Protected Sub New()
        End Sub

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            MyBase.New(info, context)

            ' Deserialize data cell
            m_flags = info.GetValue("flags", GetType(ChannelFlags))
            m_reservedFlags = info.GetValue("reservedFlags", GetType(ReservedFlags))
            m_sampleNumber = info.GetInt16("sampleNumber")

        End Sub

        Public Sub New(ByVal parent As IDataFrame, ByVal configurationCell As IConfigurationCell, ByVal sampleNumber As Int16)

            MyBase.New(parent, True, configurationCell, MaximumPhasorValues, MaximumAnalogValues, MaximumDigitalValues)

            Dim x As Int32

            m_sampleNumber = sampleNumber

            ' Initialize phasor values and frequency value with an empty value
            For x = 0 To configurationCell.PhasorDefinitions.Count - 1
                PhasorValues.Add(New PhasorValue(Me, configurationCell.PhasorDefinitions(x), 0, 0))
            Next

            ' Initialize frequency and df/dt
            FrequencyValue = New FrequencyValue(Me, configurationCell.FrequencyDefinition, 0, 0)

            ' Initialize analog values
            For x = 0 To configurationCell.AnalogDefinitions.Count - 1
                AnalogValues.Add(New AnalogValue(Me, configurationCell.AnalogDefinitions(x), 0))
            Next

            ' Initialize any digital values
            For x = 0 To configurationCell.DigitalDefinitions.Count - 1
                DigitalValues.Add(New DigitalValue(Me, configurationCell.DigitalDefinitions(x), 0))
            Next

        End Sub

        Public Sub New(ByVal dataCell As IDataCell)

            MyBase.New(dataCell)

        End Sub

        ' This constructor satisfies ChannelCellBase class requirement:
        '   ' Final dervived classes must expose Public Sub New(ByVal parent As IChannelFrame, ByVal state As IChannelFrameParsingState, ByVal index As Int32, ByVal binaryImage As Byte(), ByVal startIndex As Int32)
        Public Sub New(ByVal parent As IDataFrame, ByVal state As DataFrameParsingState, ByVal index As Int32, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            MyBase.New(parent, True, MaximumPhasorValues, MaximumAnalogValues, MaximumDigitalValues, _
                New DataCellParsingState(state.ConfigurationFrame.Cells(index), _
                    AddressOf BpaPdcStream.PhasorValue.CreateNewPhasorValue, _
                    AddressOf BpaPdcStream.FrequencyValue.CreateNewFrequencyValue, _
                    AddressOf BpaPdcStream.AnalogValue.CreateNewAnalogValue, _
                    AddressOf BpaPdcStream.DigitalValue.CreateNewDigitalValue, _
                    index), _
                binaryImage, startIndex)

        End Sub

        ' This overload allows construction of PMU's that exist within a PDCxchng block
        Public Sub New(ByVal parent As IDataFrame, ByVal configurationCell As ConfigurationCell, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            MyBase.New(parent, False, MaximumPhasorValues, MaximumAnalogValues, MaximumDigitalValues, _
                New DataCellParsingState(configurationCell, _
                    AddressOf BpaPdcStream.PhasorValue.CreateNewPhasorValue, _
                    AddressOf BpaPdcStream.FrequencyValue.CreateNewFrequencyValue, _
                    AddressOf BpaPdcStream.AnalogValue.CreateNewAnalogValue, _
                    AddressOf BpaPdcStream.DigitalValue.CreateNewDigitalValue, _
                    True), _
                binaryImage, startIndex)

        End Sub

        Friend Shared Function CreateNewDataCell(ByVal parent As IChannelFrame, ByVal state As IChannelFrameParsingState(Of IDataCell), ByVal index As Int32, ByVal binaryImage As Byte(), ByVal startIndex As Int32) As IDataCell

            Return New DataCell(parent, DirectCast(state, DataFrameParsingState), index, binaryImage, startIndex)

        End Function

        Public Overrides ReadOnly Property DerivedType() As System.Type
            Get
                Return Me.GetType()
            End Get
        End Property

        Public Shadows ReadOnly Property Parent() As DataFrame
            Get
                Return MyBase.Parent
            End Get
        End Property

        Public Shadows Property ConfigurationCell() As ConfigurationCell
            Get
                Return MyBase.ConfigurationCell
            End Get
            Set(ByVal value As ConfigurationCell)
                MyBase.ConfigurationCell = value
            End Set
        End Property

        ' Note: this is only the first byte of the channel flag word
        Public Property ChannelFlags() As ChannelFlags
            Get
                Return m_flags
            End Get
            Set(ByVal value As ChannelFlags)
                m_flags = value
            End Set
        End Property

        Public Property ReservedFlags() As ReservedFlags
            Get
                Return m_reservedFlags
            End Get
            Set(ByVal value As ReservedFlags)
                m_reservedFlags = value
            End Set
        End Property

        Public Property DataRate() As Byte
            Get
                If Parent.ConfigurationFrame.RevisionNumber >= RevisionNumber.Revision2 Then
                    Return Parent.ConfigurationFrame.FrameRate
                Else
                    Return m_dataRate
                End If
            End Get
            Set(ByVal value As Byte)
                m_dataRate = value
            End Set
        End Property

        Public Property IEEEFormatFlags() As IEEEFormatFlags
            Get
                Return ConfigurationCell.IEEEFormatFlags
            End Get
            Set(ByVal value As IEEEFormatFlags)
                ConfigurationCell.IEEEFormatFlags = value
            End Set
        End Property

        Public Property SampleNumber() As Int16
            Get
                Return m_sampleNumber
            End Get
            Set(ByVal value As Int16)
                m_sampleNumber = value
            End Set
        End Property

        ' These properties make it easier to manage channel flags
        Public Property ReservedFlag0IsSet() As Boolean
            Get
                Return ((m_reservedFlags And ReservedFlags.Reserved0) > 0)
            End Get
            Set(ByVal value As Boolean)
                If value Then
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
            Set(ByVal value As Boolean)
                If value Then
                    m_reservedFlags = m_reservedFlags Or ReservedFlags.Reserved1
                Else
                    m_reservedFlags = m_reservedFlags And Not ReservedFlags.Reserved1
                End If
            End Set
        End Property

        Public Overrides Property DataIsValid() As Boolean
            Get
                Return ((m_flags And ChannelFlags.DataIsValid) = 0)
            End Get
            Set(ByVal value As Boolean)
                If value Then
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
            Set(ByVal value As Boolean)
                If value Then
                    m_flags = m_flags Or ChannelFlags.TransmissionErrors
                Else
                    m_flags = m_flags And Not ChannelFlags.TransmissionErrors
                End If
            End Set
        End Property

        Public Overrides Property SynchronizationIsValid() As Boolean
            Get
                Return ((m_flags And ChannelFlags.PMUSynchronized) = 0)
            End Get
            Set(ByVal value As Boolean)
                If value Then
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
            Set(ByVal value As Boolean)
                If value Then
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
            Set(ByVal value As Boolean)
                If value Then
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
            Set(ByVal value As Boolean)
                If value Then
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
            Set(ByVal value As Boolean)
                If value Then
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
            Set(ByVal value As Boolean)
                If value Then
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
            Set(ByVal value As Boolean)
                If value Then
                    m_flags = m_flags And Not ChannelFlags.TimestampIncluded
                Else
                    m_flags = m_flags Or ChannelFlags.TimestampIncluded
                End If
            End Set
        End Property

        Public ReadOnly Property IsPdcBlockPmu() As Boolean
            Get
                Return m_isPdcBlockPmu
            End Get
        End Property

        Public ReadOnly Property PdcBlockPmuCount() As Byte
            Get
                Return m_pdcBlockPmuCount
            End Get
        End Property

        Public ReadOnly Property PdcBlockLength() As Integer
            Get
                Return m_pdcBlockLength
            End Get
        End Property

        Protected Overrides ReadOnly Property HeaderLength() As UInt16
            Get
                If m_isPdcBlockPmu Then
                    Return 2
                Else
                    Return 6
                End If
            End Get
        End Property

        Protected Overrides ReadOnly Property HeaderImage() As Byte()
            Get
                Dim buffer As Byte() = CreateArray(Of Byte)(HeaderLength)

                ' Add PDCstream specific image - note that although this stream will
                ' correctly parse a PDCxchng style stream - we will not produce one.
                ' Only a fully formatted stream will ever be produced
                buffer(0) = (m_flags And Not ChannelFlags.PDCExchangeFormat)

                If Parent.ConfigurationFrame.RevisionNumber >= RevisionNumber.Revision2 Then
                    buffer(1) = (Convert.ToByte(AnalogValues.Count) Or m_reservedFlags)
                    buffer(2) = (Convert.ToByte(DigitalValues.Count) Or IEEEFormatFlags)
                    buffer(3) = Convert.ToByte(PhasorValues.Count)
                Else
                    buffer(1) = m_dataRate
                    buffer(2) = Convert.ToByte(DigitalValues.Count)
                    buffer(3) = Convert.ToByte(PhasorValues.Count)
                End If

                EndianOrder.BigEndian.CopyBytes(m_sampleNumber, buffer, 4)

                Return buffer
            End Get
        End Property

        Protected Overrides Sub ParseHeaderImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            Dim parsingState As DataCellParsingState = DirectCast(state, DataCellParsingState)
            Dim revision As RevisionNumber = Parent.ConfigurationFrame.RevisionNumber
            Dim analogs As Byte = binaryImage(startIndex + 1)
            Dim digitals As Byte
            Dim phasors As Byte

            ' Get data cell flags
            m_flags = binaryImage(startIndex)

            ' Parse PDCstream specific header image
            If revision >= RevisionNumber.Revision2 AndAlso Not parsingState.IsPdcBlockPmu Then
                ' Strip off reserved flags
                m_reservedFlags = (analogs And Not ReservedFlags.AnalogWordsMask)

                ' Leave analog word count
                analogs = (analogs And ReservedFlags.AnalogWordsMask)
            Else
                ' Older revisions didn't allow analogs
                m_dataRate = analogs
                analogs = 0
            End If

            If parsingState.IsPdcBlockPmu Then
                ' PDC Block PMU's contain exactly 2 phasors, 0 analogs and 1 digital
                phasors = 2
                analogs = 0
                digitals = 1
                m_isPdcBlockPmu = True  ' Have to take note of our smaller size for HeaderLength calculation!
                UsingPDCExchangeFormat = True
            Else
                ' Parse number of digitals and phasors for normal PMU cells
                digitals = binaryImage(startIndex + 2)
                phasors = binaryImage(startIndex + 3)

                If revision >= RevisionNumber.Revision2 Then
                    ' Strip off IEEE flags
                    IEEEFormatFlags = (digitals And Not IEEEFormatFlags.DigitalWordsMask)

                    ' Leave digital word count
                    digitals = (digitals And IEEEFormatFlags.DigitalWordsMask)
                End If

                ' Check for PDC exchange format
                If UsingPDCExchangeFormat Then
                    ' In cases where we are using PDC exchange the phasor count is the number of PMU's in the PDC block
                    m_pdcBlockPmuCount = phasors

                    ' This PDC block header has no data values of its own (only PMU's) - so we cancel
                    ' data parsing for any other elements (see ParseBodyImage override below)
                    m_isPdcBlockHeader = True

                    ' Parse PMU's from PDC block...
                    Dim parentFrame As DataFrame = Parent
                    Dim index As Integer = parsingState.Index
                    Dim cellLength As UInt16

                    ' Account for channel flags in PDC block header
                    m_pdcBlockLength = 4
                    startIndex += 4

                    For x As Integer = 0 To m_pdcBlockPmuCount - 1
                        parentFrame.Cells.Add(New DataCell(parentFrame, parentFrame.ConfigurationFrame.Cells(index + x), binaryImage, startIndex))
                        cellLength = parentFrame.Cells(index + x).BinaryLength
                        startIndex += cellLength
                        m_pdcBlockLength += cellLength
                    Next
                Else
                    ' Parse PMU's sample number
                    m_sampleNumber = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 4)
                End If
            End If

            ' Algorithm Case: Determine best course of action when stream counts don't match counts defined in the
            ' external INI based configuration file.  Think about what *will* happen when new data appears in the
            ' stream that's not in the config file - you could raise an event notifying consumer about the mismatch
            ' instead of raising an exception - could even make a boolean property that would allow either case.
            ' The important thing to consider is that to parse the cell images you have to have a defined
            ' definition (see base class "Phasors.DataCellBase.ParseBodyImage").  If you have more items defined
            ' in the stream than you do in the config file then you won't get the new value, too few items and you
            ' don't have enough definitions to correctly interpret the data (that would be bad) - either way the
            ' definitions won't line up with the appropriate data value and you won't know which one is missing or
            ' added.  I can't change the protocol so this is enough argument to just raise an error for config
            ' file/stream mismatch.  So for now we'll just throw an exception and deal with consequences :)
            ' Note that this only applies to PDCstream protocol.

            ' Addendum: After running this with several protocol implementations I noticed that if a device wasn't
            ' reporting, the phasor count dropped to zero even if there were phasors defined in the configuration
            ' file, so the only time an exception is thrown is if there are more phasors defined in the the stream
            ' than there are defined in the INI file...

            ' At least this number of phasors should be already defined in BPA PDCstream configuration file
            If phasors > ConfigurationCell.PhasorDefinitions.Count Then
                Throw New InvalidOperationException("Stream/Config File Mismatch: Phasor value count in stream (" & phasors & ") does not match defined count in configuration file (" & ConfigurationCell.PhasorDefinitions.Count & ") for " & ConfigurationCell.IDLabel)
            End If

            ' If analog values get a clear definition in INI file at some point, we can validate the number in the stream to the number in the config file...
            'If analogWords > ConfigurationCell.AnalogDefinitions.Count Then
            '    Throw New InvalidOperationException("Stream/Config File Mismatch: Analog value count in stream (" analogWords & ") does not match defined count in configuration file (" & ConfigurationCell.AnalogDefinitions.Count & ")")
            'End If

            ' If digital values get a clear definition in INI file at some point, we can validate the number in the stream to the number in the config file...
            'If digitalWords > ConfigurationCell.DigitalDefinitions.Count Then
            '    Throw New InvalidOperationException("Stream/Config File Mismatch: Digital value count in stream (" digitalWords & ") does not match defined count in configuration file (" & ConfigurationCell.DigitalDefinitions.Count & ")")
            'End If

            ' Dyanmically add analog definitions to configuration cell as needed (they are only defined in data frame of BPA PDCstream)
            With ConfigurationCell.AnalogDefinitions
                If analogs > .Count Then
                    For x As Integer = .Count To analogs - 1
                        .Add(New BpaPdcStream.AnalogDefinition(ConfigurationCell, x, "Analog " & (x + 1), 1, 0.0F))
                    Next
                End If
            End With

            ' Dyanmically add digital definitions to configuration cell as needed (they are only defined in data frame of BPA PDCstream)
            With ConfigurationCell.DigitalDefinitions
                If digitals > .Count Then
                    For x As Integer = .Count To digitals - 1
                        .Add(New BpaPdcStream.DigitalDefinition(ConfigurationCell, x, "Digital Word " & (x + 1)))
                    Next
                End If
            End With

            ' Unlike most all other protocols the counts defined for phasors, analogs and digitals in the data frame
            ' may not exactly match what's defined in the configuration frame as these values are defined in an external
            ' INI file for BPA PDCstream.  As a result, we manually assign the counts to the parsing state so that these
            ' will be the counts used to parse values from data frame in the base class ParseBodyImage method
            parsingState.PhasorCount = phasors
            parsingState.AnalogCount = analogs
            parsingState.DigitalCount = digitals

            ' Status flags and remaining data elements will parsed by base class in the ParseBodyImage method

        End Sub

        Protected Overrides Sub ParseBodyImage(ByVal state As IChannelParsingState, ByVal binaryImage() As Byte, ByVal startIndex As Integer)

            ' PDC block headers have no body elements to parse other than children and they will have already been parsed at this point
            If Not m_isPdcBlockHeader Then MyBase.ParseBodyImage(state, binaryImage, startIndex)

        End Sub

        Protected Overrides ReadOnly Property BodyLength() As UShort
            Get
                ' PDC block headers have no body elements - so we return a zero length
                If m_isPdcBlockHeader Then
                    Return 0
                Else
                    Return MyBase.BodyLength
                End If
            End Get
        End Property

        Public Overrides Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext)

            MyBase.GetObjectData(info, context)

            ' Serialize data cell
            info.AddValue("flags", m_flags, GetType(ChannelFlags))
            info.AddValue("reservedFlags", m_reservedFlags, GetType(ReservedFlags))
            info.AddValue("sampleNumber", m_sampleNumber)

        End Sub

        Public Overrides ReadOnly Property Attributes() As System.Collections.Generic.Dictionary(Of String, String)
            Get
                Dim baseAttributes As Dictionary(Of String, String) = MyBase.Attributes

                baseAttributes.Add("Channel Flags", m_flags)
                baseAttributes.Add("Reserved Flags", m_reservedFlags)
                baseAttributes.Add("Sample Number", m_sampleNumber)
                baseAttributes.Add("Reserved Flag 0 Is Set", ReservedFlag0IsSet)
                baseAttributes.Add("Reserved Flag 1 Is Set", ReservedFlag1IsSet)
                baseAttributes.Add("Transmission Errors", TransmissionErrors)
                baseAttributes.Add("Data Is Sorted By Arrival", DataIsSortedByArrival)
                baseAttributes.Add("Data Is Sorted By Timestamp", ((m_flags And ChannelFlags.DataSortedByTimestamp) = 0))
                baseAttributes.Add("Using PDC Exchange Format", UsingPDCExchangeFormat)
                baseAttributes.Add("Using Macrodyne Format", UsingMacrodyneFormat)
                baseAttributes.Add("Using IEEE Format", UsingIEEEFormat)
                baseAttributes.Add("Timestamp Is Included", ((m_flags And ChannelFlags.TimestampIncluded) = 0))
                baseAttributes.Add("PMU Parsed From PDC Block", m_isPdcBlockPmu)

                Return baseAttributes
            End Get
        End Property

    End Class

End Namespace