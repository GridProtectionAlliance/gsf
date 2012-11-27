//*******************************************************************************************************
//  FrameParser.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/14/2005 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//
//*******************************************************************************************************

#region [ TVA Open Source Agreement ]
/*

 THIS OPEN SOURCE AGREEMENT ("AGREEMENT") DEFINES THE RIGHTS OF USE,REPRODUCTION, DISTRIBUTION,
 MODIFICATION AND REDISTRIBUTION OF CERTAIN COMPUTER SOFTWARE ORIGINALLY RELEASED BY THE
 TENNESSEE VALLEY AUTHORITY, A CORPORATE AGENCY AND INSTRUMENTALITY OF THE UNITED STATES GOVERNMENT
 ("GOVERNMENT AGENCY"). GOVERNMENT AGENCY IS AN INTENDED THIRD-PARTY BENEFICIARY OF ALL SUBSEQUENT
 DISTRIBUTIONS OR REDISTRIBUTIONS OF THE SUBJECT SOFTWARE. ANYONE WHO USES, REPRODUCES, DISTRIBUTES,
 MODIFIES OR REDISTRIBUTES THE SUBJECT SOFTWARE, AS DEFINED HEREIN, OR ANY PART THEREOF, IS, BY THAT
 ACTION, ACCEPTING IN FULL THE RESPONSIBILITIES AND OBLIGATIONS CONTAINED IN THIS AGREEMENT.

 Original Software Designation: openPDC
 Original Software Title: The TVA Open Source Phasor Data Concentrator
 User Registration Requested. Please Visit https://naspi.tva.com/Registration/
 Point of Contact for Original Software: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>

 1. DEFINITIONS

 A. "Contributor" means Government Agency, as the developer of the Original Software, and any entity
 that makes a Modification.

 B. "Covered Patents" mean patent claims licensable by a Contributor that are necessarily infringed by
 the use or sale of its Modification alone or when combined with the Subject Software.

 C. "Display" means the showing of a copy of the Subject Software, either directly or by means of an
 image, or any other device.

 D. "Distribution" means conveyance or transfer of the Subject Software, regardless of means, to
 another.

 E. "Larger Work" means computer software that combines Subject Software, or portions thereof, with
 software separate from the Subject Software that is not governed by the terms of this Agreement.

 F. "Modification" means any alteration of, including addition to or deletion from, the substance or
 structure of either the Original Software or Subject Software, and includes derivative works, as that
 term is defined in the Copyright Statute, 17 USC § 101. However, the act of including Subject Software
 as part of a Larger Work does not in and of itself constitute a Modification.

 G. "Original Software" means the computer software first released under this Agreement by Government
 Agency entitled openPDC, including source code, object code and accompanying documentation, if any.

 H. "Recipient" means anyone who acquires the Subject Software under this Agreement, including all
 Contributors.

 I. "Redistribution" means Distribution of the Subject Software after a Modification has been made.

 J. "Reproduction" means the making of a counterpart, image or copy of the Subject Software.

 K. "Sale" means the exchange of the Subject Software for money or equivalent value.

 L. "Subject Software" means the Original Software, Modifications, or any respective parts thereof.

 M. "Use" means the application or employment of the Subject Software for any purpose.

 2. GRANT OF RIGHTS

 A. Under Non-Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor,
 with respect to its own contribution to the Subject Software, hereby grants to each Recipient a
 non-exclusive, world-wide, royalty-free license to engage in the following activities pertaining to
 the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Modification

 5. Redistribution

 6. Display

 B. Under Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor, with
 respect to its own contribution to the Subject Software, hereby grants to each Recipient under Covered
 Patents a non-exclusive, world-wide, royalty-free license to engage in the following activities
 pertaining to the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Sale

 5. Offer for Sale

 C. The rights granted under Paragraph B. also apply to the combination of a Contributor's Modification
 and the Subject Software if, at the time the Modification is added by the Contributor, the addition of
 such Modification causes the combination to be covered by the Covered Patents. It does not apply to
 any other combinations that include a Modification. 

 D. The rights granted in Paragraphs A. and B. allow the Recipient to sublicense those same rights.
 Such sublicense must be under the same terms and conditions of this Agreement.

 3. OBLIGATIONS OF RECIPIENT

 A. Distribution or Redistribution of the Subject Software must be made under this Agreement except for
 additions covered under paragraph 3H. 

 1. Whenever a Recipient distributes or redistributes the Subject Software, a copy of this Agreement
 must be included with each copy of the Subject Software; and

 2. If Recipient distributes or redistributes the Subject Software in any form other than source code,
 Recipient must also make the source code freely available, and must provide with each copy of the
 Subject Software information on how to obtain the source code in a reasonable manner on or through a
 medium customarily used for software exchange.

 B. Each Recipient must ensure that the following copyright notice appears prominently in the Subject
 Software:

          No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.

 C. Each Contributor must characterize its alteration of the Subject Software as a Modification and
 must identify itself as the originator of its Modification in a manner that reasonably allows
 subsequent Recipients to identify the originator of the Modification. In fulfillment of these
 requirements, Contributor must include a file (e.g., a change log file) that describes the alterations
 made and the date of the alterations, identifies Contributor as originator of the alterations, and
 consents to characterization of the alterations as a Modification, for example, by including a
 statement that the Modification is derived, directly or indirectly, from Original Software provided by
 Government Agency. Once consent is granted, it may not thereafter be revoked.

 D. A Contributor may add its own copyright notice to the Subject Software. Once a copyright notice has
 been added to the Subject Software, a Recipient may not remove it without the express permission of
 the Contributor who added the notice.

 E. A Recipient may not make any representation in the Subject Software or in any promotional,
 advertising or other material that may be construed as an endorsement by Government Agency or by any
 prior Recipient of any product or service provided by Recipient, or that may seek to obtain commercial
 advantage by the fact of Government Agency's or a prior Recipient's participation in this Agreement.

 F. In an effort to track usage and maintain accurate records of the Subject Software, each Recipient,
 upon receipt of the Subject Software, is requested to register with Government Agency by visiting the
 following website: https://naspi.tva.com/Registration/. Recipient's name and personal information
 shall be used for statistical purposes only. Once a Recipient makes a Modification available, it is
 requested that the Recipient inform Government Agency at the web site provided above how to access the
 Modification.

 G. Each Contributor represents that that its Modification does not violate any existing agreements,
 regulations, statutes or rules, and further that Contributor has sufficient rights to grant the rights
 conveyed by this Agreement.

 H. A Recipient may choose to offer, and to charge a fee for, warranty, support, indemnity and/or
 liability obligations to one or more other Recipients of the Subject Software. A Recipient may do so,
 however, only on its own behalf and not on behalf of Government Agency or any other Recipient. Such a
 Recipient must make it absolutely clear that any such warranty, support, indemnity and/or liability
 obligation is offered by that Recipient alone. Further, such Recipient agrees to indemnify Government
 Agency and every other Recipient for any liability incurred by them as a result of warranty, support,
 indemnity and/or liability offered by such Recipient.

 I. A Recipient may create a Larger Work by combining Subject Software with separate software not
 governed by the terms of this agreement and distribute the Larger Work as a single product. In such
 case, the Recipient must make sure Subject Software, or portions thereof, included in the Larger Work
 is subject to this Agreement.

 J. Notwithstanding any provisions contained herein, Recipient is hereby put on notice that export of
 any goods or technical data from the United States may require some form of export license from the
 U.S. Government. Failure to obtain necessary export licenses may result in criminal liability under
 U.S. laws. Government Agency neither represents that a license shall not be required nor that, if
 required, it shall be issued. Nothing granted herein provides any such export license.

 4. DISCLAIMER OF WARRANTIES AND LIABILITIES; WAIVER AND INDEMNIFICATION

 A. No Warranty: THE SUBJECT SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTY OF ANY KIND, EITHER
 EXPRESSED, IMPLIED, OR STATUTORY, INCLUDING, BUT NOT LIMITED TO, ANY WARRANTY THAT THE SUBJECT
 SOFTWARE WILL CONFORM TO SPECIFICATIONS, ANY IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 PARTICULAR PURPOSE, OR FREEDOM FROM INFRINGEMENT, ANY WARRANTY THAT THE SUBJECT SOFTWARE WILL BE ERROR
 FREE, OR ANY WARRANTY THAT DOCUMENTATION, IF PROVIDED, WILL CONFORM TO THE SUBJECT SOFTWARE. THIS
 AGREEMENT DOES NOT, IN ANY MANNER, CONSTITUTE AN ENDORSEMENT BY GOVERNMENT AGENCY OR ANY PRIOR
 RECIPIENT OF ANY RESULTS, RESULTING DESIGNS, HARDWARE, SOFTWARE PRODUCTS OR ANY OTHER APPLICATIONS
 RESULTING FROM USE OF THE SUBJECT SOFTWARE. FURTHER, GOVERNMENT AGENCY DISCLAIMS ALL WARRANTIES AND
 LIABILITIES REGARDING THIRD-PARTY SOFTWARE, IF PRESENT IN THE ORIGINAL SOFTWARE, AND DISTRIBUTES IT
 "AS IS."

 B. Waiver and Indemnity: RECIPIENT AGREES TO WAIVE ANY AND ALL CLAIMS AGAINST GOVERNMENT AGENCY, ITS
 AGENTS, EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT. IF RECIPIENT'S USE
 OF THE SUBJECT SOFTWARE RESULTS IN ANY LIABILITIES, DEMANDS, DAMAGES, EXPENSES OR LOSSES ARISING FROM
 SUCH USE, INCLUDING ANY DAMAGES FROM PRODUCTS BASED ON, OR RESULTING FROM, RECIPIENT'S USE OF THE
 SUBJECT SOFTWARE, RECIPIENT SHALL INDEMNIFY AND HOLD HARMLESS  GOVERNMENT AGENCY, ITS AGENTS,
 EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT, TO THE EXTENT PERMITTED BY
 LAW.  THE FOREGOING RELEASE AND INDEMNIFICATION SHALL APPLY EVEN IF THE LIABILITIES, DEMANDS, DAMAGES,
 EXPENSES OR LOSSES ARE CAUSED, OCCASIONED, OR CONTRIBUTED TO BY THE NEGLIGENCE, SOLE OR CONCURRENT, OF
 GOVERNMENT AGENCY OR ANY PRIOR RECIPIENT.  RECIPIENT'S SOLE REMEDY FOR ANY SUCH MATTER SHALL BE THE
 IMMEDIATE, UNILATERAL TERMINATION OF THIS AGREEMENT.

 5. GENERAL TERMS

 A. Termination: This Agreement and the rights granted hereunder will terminate automatically if a
 Recipient fails to comply with these terms and conditions, and fails to cure such noncompliance within
 thirty (30) days of becoming aware of such noncompliance. Upon termination, a Recipient agrees to
 immediately cease use and distribution of the Subject Software. All sublicenses to the Subject
 Software properly granted by the breaching Recipient shall survive any such termination of this
 Agreement.

 B. Severability: If any provision of this Agreement is invalid or unenforceable under applicable law,
 it shall not affect the validity or enforceability of the remainder of the terms of this Agreement.

 C. Applicable Law: This Agreement shall be subject to United States federal law only for all purposes,
 including, but not limited to, determining the validity of this Agreement, the meaning of its
 provisions and the rights, obligations and remedies of the parties.

 D. Entire Understanding: This Agreement constitutes the entire understanding and agreement of the
 parties relating to release of the Subject Software and may not be superseded, modified or amended
 except by further written agreement duly executed by the parties.

 E. Binding Authority: By accepting and using the Subject Software under this Agreement, a Recipient
 affirms its authority to bind the Recipient to all terms and conditions of this Agreement and that
 Recipient hereby agrees to all terms and conditions herein.

 F. Point of Contact: Any Recipient contact with Government Agency is to be directed to the designated
 representative as follows: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>.

*/
#endregion

using System;
using System.Text;
using GSF.Parsing;
using GSF;

namespace GSF.PhasorProtocols.IeeeC37_118
{
    /// <summary>
    /// Represents a frame parser for an IEEE C37.118 binary data stream and returns parsed data via events.
    /// </summary>
    /// <remarks>
    /// Frame parser is implemented as a write-only stream - this way data can come from any source.
    /// </remarks>
    public class FrameParser : FrameParserBase<FrameType>
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Occurs when an IEEE C37.118 <see cref="ConfigurationFrame1"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="ConfigurationFrame1"/> that was received.
        /// </remarks>
        public event EventHandler<EventArgs<ConfigurationFrame1>> ReceivedConfigurationFrame1;

        /// <summary>
        /// Occurs when an IEEE C37.118 <see cref="ConfigurationFrame2"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="ConfigurationFrame2"/> that was received.
        /// </remarks>
        public event EventHandler<EventArgs<ConfigurationFrame2>> ReceivedConfigurationFrame2;

        /// <summary>
        /// Occurs when an IEEE C37.118 <see cref="DataFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="DataFrame"/> that was received.
        /// </remarks>
        public new event EventHandler<EventArgs<DataFrame>> ReceivedDataFrame;

        /// <summary>
        /// Occurs when an IEEE C37.118 <see cref="HeaderFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="HeaderFrame"/> that was received.
        /// </remarks>
        public new event EventHandler<EventArgs<HeaderFrame>> ReceivedHeaderFrame;

        /// <summary>
        /// Occurs when an IEEE C37.118 <see cref="CommandFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="CommandFrame"/> that was received.
        /// <para>
        /// Command frames are normally sent, not received, but there is nothing that prevents this.
        /// </para>
        /// </remarks>
        public new event EventHandler<EventArgs<CommandFrame>> ReceivedCommandFrame;

        // Fields
        private DraftRevision m_draftRevision;
        private ConfigurationFrame2 m_configurationFrame2;
        private bool m_configurationChangeHandled;
        private long m_unexpectedCommandFrames;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="FrameParser"/> from specified parameters.
        /// </summary>
        /// <param name="draftRevision">The <see cref="IeeeC37_118.DraftRevision"/> of this <see cref="FrameParser"/>.</param>
        public FrameParser(DraftRevision draftRevision)
        {
            // Initialize protocol synchronization bytes for this frame parser
            base.ProtocolSyncBytes = new byte[] { GSF.PhasorProtocols.Common.SyncByte };

            m_draftRevision = draftRevision;
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
            get
            {
                return m_configurationFrame2;
            }
            set
            {
                m_configurationFrame2 = CastToDerivedConfigurationFrame(value, m_draftRevision);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IeeeC37_118.DraftRevision"/> of this <see cref="FrameParser"/>.
        /// </summary>
        public DraftRevision DraftRevision
        {
            get
            {
                return m_draftRevision;
            }
            set
            {
                m_draftRevision = value;
            }
        }

        /// <summary>
        /// Gets the IEEE C37.118 resolution of fractional timestamps of the current <see cref="ConfigurationFrame"/>, if one has been parsed.
        /// </summary>
        public uint Timebase
        {
            get
            {
                if (m_configurationFrame2 == null)
                    return 0;
                else
                    return m_configurationFrame2.Timebase;
            }
        }

        /// <summary>
        /// Gets flag that determines if this protocol parsing implementation uses synchronization bytes.
        /// </summary>
        public override bool ProtocolUsesSyncBytes
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets current descriptive status of the <see cref="FrameParser"/>.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.Append("IEEE C37.118-2005 revision: ");
                status.Append(m_draftRevision);
                status.AppendLine();
                status.Append("         Current time base: ");
                status.Append(Timebase);
                status.AppendLine();
                status.Append(" Unexpected command frames: ");
                status.Append(m_unexpectedCommandFrames);
                status.AppendLine();
                status.Append(base.Status);

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Start the data parser.
        /// </summary>
        public override void Start()
        {
            m_configurationChangeHandled = false;
            m_unexpectedCommandFrames = 0;

            // We narrow down parsing types to just those needed...
            switch (m_draftRevision)
            {
                case DraftRevision.Draft6:
                    base.Start(new Type[] { typeof(DataFrame), typeof(ConfigurationFrame1Draft6), typeof(ConfigurationFrame2Draft6), typeof(HeaderFrame) });
                    break;
                case DraftRevision.Draft7:
                    base.Start(new Type[] { typeof(DataFrame), typeof(ConfigurationFrame1), typeof(ConfigurationFrame2), typeof(HeaderFrame) });
                    break;
                case DraftRevision.Draft8:
                    base.Start(new Type[] { typeof(DataFrame), typeof(ConfigurationFrame1), typeof(ConfigurationFrame2), typeof(ConfigurationFrame3), typeof(HeaderFrame) });
                    break;
                default:
                    break;
            }
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
            if (length >= CommonFrameHeader.FixedLength)
            {
                // Parse common frame header
                CommonFrameHeader parsedFrameHeader = new CommonFrameHeader(m_configurationFrame2, buffer, offset);

                // As an optimization, we also make sure entire frame buffer image is available to be parsed - by doing this
                // we eliminate the need to validate length on all subsequent data elements that comprise the frame
                if (length >= parsedFrameHeader.FrameLength)
                {
                    // Expose the frame buffer image in case client needs this data for any reason
                    OnReceivedFrameBufferImage(parsedFrameHeader.FrameType, buffer, offset, parsedFrameHeader.FrameLength);

                    // Handle special parsing states
                    switch (parsedFrameHeader.TypeID)
                    {
                        case FrameType.DataFrame:
                            // Assign data frame parsing state
                            parsedFrameHeader.State = new DataFrameParsingState(parsedFrameHeader.FrameLength, m_configurationFrame2, DataCell.CreateNewCell);
                            break;
                        case FrameType.ConfigurationFrame1:
                        case FrameType.ConfigurationFrame2:
                            // Assign configuration frame parsing state
                            parsedFrameHeader.State = new ConfigurationFrameParsingState(parsedFrameHeader.FrameLength, ConfigurationCell.CreateNewCell);
                            break;
                        case FrameType.ConfigurationFrame3:
                            // parsedFrameHeader.State = new ConfigurationFrameParsingState(parsedFrameHeader.FrameLength, ConfigurationCell3.CreateNewCell);
                            break;
                        case FrameType.HeaderFrame:
                            // Assign header frame parsing state
                            parsedFrameHeader.State = new HeaderFrameParsingState(parsedFrameHeader.FrameLength, parsedFrameHeader.DataLength);
                            break;
                    }

                    return parsedFrameHeader;
                }
            }

            return null;
        }

        /// <summary>
        /// Raises the <see cref="FrameParserBase{TypeIndentifier}.ReceivedConfigurationFrame"/> event.
        /// </summary>
        /// <param name="frame"><see cref="IConfigurationFrame"/> to send to <see cref="FrameParserBase{TypeIndentifier}.ReceivedConfigurationFrame"/> event.</param>
        protected override void OnReceivedConfigurationFrame(IConfigurationFrame frame)
        {
            // We override this method so we can cache configuration 2 frame when it's received
            base.OnReceivedConfigurationFrame(frame);

            // Cache new configuration frame for parsing subsequent data frames...
            ConfigurationFrame2 configurationFrame2 = frame as ConfigurationFrame2;

            if (configurationFrame2 != null)
                m_configurationFrame2 = configurationFrame2;
            //FIXME: Add cf3

        }

        /// <summary>
        /// Raises the <see cref="FrameParserBase{TypeIndentifier}.ReceivedDataFrame"/> event.
        /// </summary>
        /// <param name="frame"><see cref="IDataFrame"/> to send to <see cref="FrameParserBase{TypeIndentifier}.ReceivedDataFrame"/> event.</param>
        protected override void OnReceivedDataFrame(IDataFrame frame)
        {
            // We override this method so we can detect and respond to a configuration change notification
            base.OnReceivedDataFrame(frame);

            bool configurationChangeDetected = false;

            DataCellCollection dataCells = frame.Cells as DataCellCollection;

            if (dataCells != null)
            {
                // Check for a configuration change notification from any data cell
                for (int x = 0; x < dataCells.Count; x++)
                {
                    // Configuration change is only relevant if raw status flags <> FF (status undefined)
                    if (dataCells[x].ConfigurationChangeDetected && !dataCells[x].DeviceError && ((DataCellBase)dataCells[x]).StatusFlags != ushort.MaxValue)
                    {
                        configurationChangeDetected = true;

                        // Configuration change detection flag should terminate after one minute, but
                        // we only want to send a single notification
                        if (!m_configurationChangeHandled)
                        {
                            m_configurationChangeHandled = true;
                            base.OnConfigurationChanged();
                        }

                        break;
                    }
                }
            }

            if (!configurationChangeDetected)
                m_configurationChangeHandled = false;
        }

        /// <summary>
        /// Casts the parsed <see cref="IChannelFrame"/> to its specific implementation (i.e., <see cref="DataFrame"/>, <see cref="ConfigurationFrame"/>, <see cref="CommandFrame"/> or <see cref="HeaderFrame"/>).
        /// </summary>
        /// <param name="frame"><see cref="IChannelFrame"/> that was parsed by <see cref="FrameImageParserBase{TTypeIdentifier,TOutputType}"/> that implements protocol specific common frame header interface.</param>
        protected override void OnReceivedChannelFrame(IChannelFrame frame)
        {
            // Raise abstract channel frame events as a priority (i.e., IDataFrame, IConfigurationFrame, etc.)
            base.OnReceivedChannelFrame(frame);

            // Raise IEEE C37.118 specific channel frame events, if any have been subscribed
            if (frame != null && (ReceivedDataFrame != null || ReceivedConfigurationFrame2 != null || ReceivedConfigurationFrame1 != null || ReceivedHeaderFrame != null || ReceivedCommandFrame != null))
            {
                DataFrame dataFrame = frame as DataFrame;

                if (dataFrame != null)
                {
                    if (ReceivedDataFrame != null)
                        ReceivedDataFrame(this, new EventArgs<DataFrame>(dataFrame));
                }
                else
                {
                    // Configuration frame type 2 is more specific than type 1 (and more common), so we check it first
                    ConfigurationFrame2 configFrame2 = frame as ConfigurationFrame2;

                    if (configFrame2 != null)
                    {
                        if (ReceivedConfigurationFrame2 != null)
                            ReceivedConfigurationFrame2(this, new EventArgs<ConfigurationFrame2>(configFrame2));
                    }
                    else
                    {
                        ConfigurationFrame1 configFrame1 = frame as ConfigurationFrame1;

                        if (configFrame1 != null)
                        {
                            if (ReceivedConfigurationFrame1 != null)
                                ReceivedConfigurationFrame1(this, new EventArgs<ConfigurationFrame1>(configFrame1));
                        }
                        else
                        {
                            HeaderFrame headerFrame = frame as HeaderFrame;

                            if (headerFrame != null)
                            {
                                if (ReceivedHeaderFrame != null)
                                    ReceivedHeaderFrame(this, new EventArgs<HeaderFrame>(headerFrame));
                            }
                            else
                            {
                                CommandFrame commandFrame = frame as CommandFrame;

                                if (commandFrame != null)
                                {
                                    if (ReceivedCommandFrame != null)
                                        ReceivedCommandFrame(this, new EventArgs<CommandFrame>(commandFrame));
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles unknown frame types.
        /// </summary>
        /// <param name="frameType">Unknown frame ID.</param>
        protected override void OnUnknownFrameTypeEncountered(FrameType frameType)
        {
            // It is unusual, but not all that uncommon, for a device to send a command frame to its host. Normally the host sends to commands
            // to the device. However, since some devices seem to do this frequently we suppress reporting that the frame is "undefined".
            // Technically the frame is defined, but it is not part of the valid set of frames intended for reception.
            if (frameType != FrameType.CommandFrame)
                base.OnUnknownFrameTypeEncountered(frameType);
            else
                m_unexpectedCommandFrames++;
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Attempts to cast given frame into an IEEE C37.118 configuration frame - theoretically this will
        // allow the same configuration frame to be used for any protocol implementation
        internal static ConfigurationFrame2 CastToDerivedConfigurationFrame(IConfigurationFrame sourceFrame, DraftRevision draftRevision)
        {
            // See if frame is already an IEEE C37.118 configuration frame, type 2 (if so, we don't need to do any work)
            ConfigurationFrame2 derivedFrame = sourceFrame as ConfigurationFrame2;

            if (derivedFrame == null)
            {
                // Create a new IEEE C37.118 configuration frame converted from equivalent configuration information
                ConfigurationCell derivedCell;
                IFrequencyDefinition sourceFrequency;

                // Assuming configuration frame 2 and timebase = 100000
                if (draftRevision == DraftRevision.Draft7)
                    derivedFrame = new ConfigurationFrame2(100000, sourceFrame.IDCode, sourceFrame.Timestamp, sourceFrame.FrameRate);
                else
                    derivedFrame = new ConfigurationFrame2Draft6(100000, sourceFrame.IDCode, sourceFrame.Timestamp, sourceFrame.FrameRate);

                foreach (IConfigurationCell sourceCell in sourceFrame.Cells)
                {
                    // Create new derived configuration cell
                    derivedCell = new ConfigurationCell(derivedFrame, sourceCell.IDCode, sourceCell.NominalFrequency);

                    string stationName = sourceCell.StationName;
                    string idLabel = sourceCell.IDLabel;

                    if (!string.IsNullOrWhiteSpace(stationName))
                        derivedCell.StationName = stationName.TruncateLeft(derivedCell.MaximumStationNameLength);

                    if (!string.IsNullOrWhiteSpace(idLabel))
                        derivedCell.IDLabel = idLabel.TruncateLeft(derivedCell.IDLabelLength);

                    // Create equivalent derived phasor definitions
                    foreach (IPhasorDefinition sourcePhasor in sourceCell.PhasorDefinitions)
                    {
                        derivedCell.PhasorDefinitions.Add(new PhasorDefinition(derivedCell, sourcePhasor.Label, sourcePhasor.ScalingValue, sourcePhasor.Offset, sourcePhasor.PhasorType, null));
                    }

                    // Create equivalent dervied frequency definition
                    sourceFrequency = sourceCell.FrequencyDefinition;

                    if (sourceFrequency != null)
                        derivedCell.FrequencyDefinition = new FrequencyDefinition(derivedCell, sourceFrequency.Label);

                    // Create equivalent dervied analog definitions (assuming analog type = SinglePointOnWave)
                    foreach (IAnalogDefinition sourceAnalog in sourceCell.AnalogDefinitions)
                    {
                        derivedCell.AnalogDefinitions.Add(new AnalogDefinition(derivedCell, sourceAnalog.Label, sourceAnalog.ScalingValue, sourceAnalog.Offset, sourceAnalog.AnalogType));
                    }

                    // Create equivalent dervied digital definitions
                    foreach (IDigitalDefinition sourceDigital in sourceCell.DigitalDefinitions)
                    {
                        derivedCell.DigitalDefinitions.Add(new DigitalDefinition(derivedCell, sourceDigital.Label, 0, 0));
                    }

                    // Add cell to frame
                    derivedFrame.Cells.Add(derivedCell);
                }
            }

            return derivedFrame;
        }

        #endregion
    }
}