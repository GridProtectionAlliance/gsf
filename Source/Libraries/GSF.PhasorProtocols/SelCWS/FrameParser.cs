//******************************************************************************************************
//  FrameParser.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  02/08/2007 - J. Ritchie Carroll & Jian Ryan Zuo
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using GSF.Parsing;
using GSF.Units.EE;

namespace GSF.PhasorProtocols.SelCWS;

/// <summary>
/// Represents a frame parser for a SEL CWS text based data stream that returns parsed data via events.
/// </summary>
/// <remarks>
/// Frame parser is implemented as a write-only stream - this way data can come from any source.
/// </remarks>
public class FrameParser : FrameParserBase<FrameType>
{
    #region [ Members ]

    // Events

    /// <summary>
    /// Occurs when a SEL CWS <see cref="ConfigurationFrame"/> has been received.
    /// </summary>
    /// <remarks>
    /// <see cref="EventArgs{T}.Argument"/> is the <see cref="ConfigurationFrame"/> that was received.
    /// </remarks>
    public new event EventHandler<EventArgs<ConfigurationFrame>> ReceivedConfigurationFrame;

    /// <summary>
    /// Occurs when a SEL CWS <see cref="DataFrame"/> has been received.
    /// </summary>
    /// <remarks>
    /// <see cref="EventArgs{T}.Argument"/> is the <see cref="DataFrame"/> that was received.
    /// </remarks>
    public new event EventHandler<EventArgs<DataFrame>> ReceivedDataFrame;

    // Fields
    private ConfigurationFrame m_configurationFrame;
    private DataFrame m_initialDataFrame;
    private RollingPhaseEstimator m_phaseEstimator;

    #endregion

    #region [ Constructors ]

    /// <summary>
    /// Creates a new <see cref="FrameParser"/>.
    /// </summary>
    /// <param name="checkSumValidationFrameTypes">Frame types that should perform check-sum validation; default to <see cref="CheckSumValidationFrameTypes.AllFrames"/></param>
    /// <param name="trustHeaderLength">Determines if header lengths should be trusted over parsed byte count.</param>
    public FrameParser(CheckSumValidationFrameTypes checkSumValidationFrameTypes = CheckSumValidationFrameTypes.AllFrames, bool trustHeaderLength = true)
        : base(checkSumValidationFrameTypes, trustHeaderLength)
    {
        CalculatePhaseEstimates = SelCWS.ConnectionParameters.DefaultCalculatePhaseEstimates;
        NominalFrequency = Common.DefaultNominalFrequency;
        FrameRate = Common.DefaultFrameRate;
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets or sets current <see cref="IConfigurationFrame"/> used for parsing <see cref="IDataFrame"/>'s encountered in the data stream from a device.
    /// </summary>
    /// <remarks>
    /// If a <see cref="IConfigurationFrame"/> has been parsed, this will return a reference to the parsed frame.  Consumer can manually assign a
    /// <see cref="IConfigurationFrame"/> to start parsing data if one has not been encountered in the stream.
    /// </remarks>
    public override IConfigurationFrame ConfigurationFrame
    {
        get => m_configurationFrame!;
        set => throw new NotImplementedException("Type casting configuration frame is not implemented.");
    }

    /// <summary>
    /// Gets flag that determines if SEL CWS protocol parsing implementation uses synchronization bytes.
    /// </summary>
    public override bool ProtocolUsesSyncBytes => false;

    /// <summary>
    /// Gets flag that determines if phase estimates should be calculated for phasor measurements.
    /// </summary>
    public bool CalculatePhaseEstimates { get; set; }

    /// <summary>
    /// Gets the nominal <see cref="LineFrequency"/> of the SEL CWS device.
    /// </summary>
    public LineFrequency NominalFrequency { get; set; }

    /// <summary>
    /// Gets or sets the configured frame rate for the SEL CWS device.
    /// </summary>
    public ushort FrameRate { get; set; }

    #endregion

    #region [ Methods ]

    /// <summary>
    /// Start the data parser.
    /// </summary>
    public override void Start()
    {
        // We narrow down parsing types to just those needed...
        base.Start([typeof(ConfigurationFrame), typeof(DataFrame)]);

        // Make sure we mark stream an initialized even though base class doesn't think we use sync-bytes
        StreamInitialized = false;
    }

    /// <summary>
    /// Parses a common header instance that implements <see cref="ICommonHeader{TTypeIdentifier}"/> for the output type represented
    /// in the binary image.
    /// </summary>
    /// <param name="buffer">Buffer containing data to parse.</param>
    /// <param name="offset">Offset index into buffer that represents where to start parsing.</param>
    /// <param name="length">Maximum length of valid data from offset.</param>
    /// <returns>The <see cref="ICommonHeader{TTypeIdentifier}"/> which includes a type ID for the <see cref="Type"/> to be parsed.</returns>
    /// <remarks>
    /// <para>
    /// Derived classes need to provide a common header instance (i.e., class that implements <see cref="ICommonHeader{TTypeIdentifier}"/>)
    /// for the output types; this will primarily include an ID of the <see cref="Type"/> that the data image represents.  This parsing is
    /// only for common header information, actual parsing will be handled by output type via its <see cref="ISupportBinaryImage.ParseBinaryImage"/>
    /// method. This header image should also be used to add needed complex state information about the output type being parsed if needed.
    /// </para>
    /// <para>
    /// If there is not enough buffer available to parse common header (as determined by <paramref name="length"/>), return null.  Also, if
    /// the protocol allows frame length to be determined at the time common header is being parsed and there is not enough buffer to parse
    /// the entire frame, it will be optimal to prevent further parsing by returning null.
    /// </para>
    /// </remarks>
    protected override ICommonHeader<FrameType> ParseCommonHeader(byte[] buffer, int offset, int length)
    {
        // See if there is enough data in the buffer to parse the common frame header.
        if (length < CommonFrameHeader.FixedLength)
            return null;

        // Parse common frame header
        CommonFrameHeader parsedFrameHeader = new(buffer, offset, length);

        // Expose the frame buffer image in case client needs this data for any reason
        OnReceivedFrameBufferImage(parsedFrameHeader.FundamentalFrameType, buffer, offset, parsedFrameHeader.FrameLength);

        // As an optimization, we also make sure entire frame buffer image is available to be parsed - by doing this
        // we eliminate the need to validate length on all subsequent data elements that comprise the frame
        if (length < parsedFrameHeader.FrameLength)
            return null;

        // Handle special parsing states
        parsedFrameHeader.State = parsedFrameHeader.TypeID switch
        {
            FrameType.DataFrame =>
                new DataFrameParsingState(parsedFrameHeader.FrameLength, ConfigurationFrame, DataCell.CreateNewCell, TrustHeaderLength, ValidateDataFrameCheckSum),
            FrameType.ConfigurationFrame =>
                new ConfigurationFrameParsingState(parsedFrameHeader.FrameLength, null, TrustHeaderLength, ValidateConfigurationFrameCheckSum),
            _ => parsedFrameHeader.State
        };

        m_initialDataFrame = null;

        return parsedFrameHeader;
    }

    /// <inheritdoc/>
    public override void Parse(SourceChannel source, byte[] buffer, int offset, int count)
    {
        const byte DataFrameByte = (byte)FrameType.DataFrame;
        const byte ConfigFrameByte = (byte)FrameType.ConfigurationFrame;

        // Since the SEL CWS implementation supports both 0x00 and 0x01 as sync-bytes, we manually check for both
        // during stream initialization, base class handles a single set of sync-bytes, not variable.
        if (!Enabled)
            return;

        if (StreamInitialized)
        {
            base.Parse(source, buffer, offset, count);
        }
        else
        {
            // Initial stream may technically be anywhere in the middle of a frame, so we attempt to
            // locate sync-bytes to "line-up" data stream,

            // First we look for data frame sync-byte:
            int syncBytePosition = buffer.IndexOfSequence([DataFrameByte], offset, count);

            if (syncBytePosition > -1)
            {
                StreamInitialized = true;
                base.Parse(source, buffer, syncBytePosition, count - (syncBytePosition - offset));
            }
            else
            {
                // Second we look for configuration frame sync-byte:
                syncBytePosition = buffer.IndexOfSequence([ConfigFrameByte], offset, count);

                if (syncBytePosition > -1)
                {
                    StreamInitialized = true;
                    base.Parse(source, buffer, syncBytePosition, count - (syncBytePosition - offset));
                }
            }
        }
    }

    /// <inheritdoc/>
    protected override int ParseFrame(byte[] buffer, int offset, int length)
    {
        int parsedLength = base.ParseFrame(buffer, offset, length);

        if (buffer[offset] != (byte)FrameType.DataFrame || m_initialDataFrame is null)
            return parsedLength;

        long nanosecondsPerFrame = FrameRate / 50;
        long nanosecondsTimestamp = m_initialDataFrame.NanosecondTimestamp;

        // Move offset past initial data frame which includes 64-bit nanosecond timestamp
        offset = 32;
        length -= 32;

        // In the case of data frames in CWS, the source buffer has 49 more frames to parse after the first
        for (int i = 0; i < 49; i++)
        {
            nanosecondsTimestamp += nanosecondsPerFrame;

            DataFrame dataFrame = new(nanosecondsTimestamp, m_initialDataFrame.ConfigurationFrame)
            {
                CommonHeader = m_initialDataFrame.CommonHeader
            };

            dataFrame.ParseBinaryImage(buffer, offset, length);

            offset += 24;
            length -= 24;

            ApplyEstimatedPhases(dataFrame);
            OnReceivedDataFrame(dataFrame);

            // If event for native data frame is subscribed, raise it also
            ReceivedDataFrame?.Invoke(this, new EventArgs<DataFrame>(dataFrame));
        }

        return parsedLength;
    }

    private void ApplyEstimatedPhases(DataFrame dataFrame)
    {
        if (!CalculatePhaseEstimates || dataFrame.Cells.Count == 0)
            return;

        long timestamp = dataFrame.NanosecondTimestamp;

        if (timestamp <= 0)
            return;

        DataCell cell = dataFrame.Cells[0];

        if (cell.AnalogValues.Count != 6)
            return;

        // Expected order defined by SEL CWS protocol:
        double ia = cell.AnalogValues[0].Value;
        double ib = cell.AnalogValues[1].Value;
        double ic = cell.AnalogValues[2].Value;
        double va = cell.AnalogValues[3].Value;
        double vb = cell.AnalogValues[4].Value;
        double vc = cell.AnalogValues[5].Value;

        // Ensure phase estimator is created
        m_phaseEstimator ??= new RollingPhaseEstimator(FrameRate, NominalFrequency);

        // Calculate next phase estimation, returns false if not enough data yet
        if (!m_phaseEstimator.Step(ia, ib, ic, va, vb, vc, timestamp, out PhaseEstimate? result))
            return;

        PhaseEstimate estimate = result.GetValueOrDefault();

        cell.FrequencyValue.Frequency = estimate.Frequency;
        cell.FrequencyValue.DfDt = estimate.dFdt;

        for (int i = 0; i < 6; i++)
        {
            PhasorValue phasorValue = (cell.PhasorValues[i] as PhasorValue)!;
            phasorValue.Angle = estimate.Angles[i];
            phasorValue.Magnitude = estimate.Magnitudes[i];
        }
    }

    /// <inheritdoc/>
    protected override void OnParsingException(Exception ex)
    {
        base.OnParsingException(ex);

        // At the first sign of an error, we need to reset stream initialization flag - could just be looping
        // a saved file source, or we missed some data, just need to re-sync to next 0x00 or 0x01 byte...
        StreamInitialized = false;
    }

    /// <summary>
    /// Raises the <see cref="FrameParserBase{TypeIndentifier}.ReceivedConfigurationFrame"/> event.
    /// </summary>
    /// <param name="frame"><see cref="IConfigurationFrame"/> to send to <see cref="FrameParserBase{TypeIndentifier}.ReceivedConfigurationFrame"/> event.</param>
    protected override void OnReceivedConfigurationFrame(IConfigurationFrame frame)
    {
        // Cache new configuration frame for parsing subsequent data frames...
        m_configurationFrame = frame as ConfigurationFrame;

        base.OnReceivedConfigurationFrame(frame);
    }

    /// <summary>
    /// Casts the parsed <see cref="IChannelFrame"/> to its specific implementation (i.e., <see cref="DataFrame"/> or <see cref="ConfigurationFrame"/>).
    /// </summary>
    /// <param name="frame"><see cref="IChannelFrame"/> that was parsed by <see cref="FrameImageParserBase{TTypeIdentifier,TOutputType}"/> that implements protocol specific common frame header interface.</param>
    protected override void OnReceivedChannelFrame(IChannelFrame frame)
    {
        // Keep reference to initial data frame (1 of 50 frames in SEL CWS)
        if (frame is DataFrame initialDataFrame)
        {
            if (m_configurationFrame is not null && initialDataFrame.CommonHeader.ChannelID != m_configurationFrame.CommonHeader.ChannelID)
                throw new InvalidOperationException("Data frame channel ID does not match that of the current configuration frame.");

            ApplyEstimatedPhases(initialDataFrame);
            m_initialDataFrame = initialDataFrame;
        }

        // Raise abstract channel frame events as a priority (i.e., IDataFrame, IConfigurationFrame, etc.)
        base.OnReceivedChannelFrame(frame);

        // Raise SEL CWS specific channel frame events, if any have been subscribed
        if (ReceivedDataFrame is null && ReceivedConfigurationFrame is null)
            return;

        switch (frame)
        {
            case DataFrame dataFrame:
            {
                ReceivedDataFrame?.Invoke(this, new EventArgs<DataFrame>(dataFrame));
                break;
            }
            case ConfigurationFrame configFrame:
            {
                ReceivedConfigurationFrame?.Invoke(this, new EventArgs<ConfigurationFrame>(configFrame));
                break;
            }
        }
    }

    /// <summary>
    /// Gets or sets any connection specific <see cref="IConnectionParameters"/> that may be needed for parsing.
    /// </summary>
    public override IConnectionParameters ConnectionParameters
    {
        get => base.ConnectionParameters;
        set
        {
            if (value is not ConnectionParameters parameters)
                return;

            base.ConnectionParameters = parameters;

            // Assign new incoming connection parameter values
            CalculatePhaseEstimates = parameters.CalculatePhaseEstimates;
            FrameRate = parameters.FrameRate;
            NominalFrequency = parameters.NominalFrequency;
        }
    }

    #endregion
}