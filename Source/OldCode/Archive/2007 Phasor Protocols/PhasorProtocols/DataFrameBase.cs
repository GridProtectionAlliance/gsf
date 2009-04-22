//*******************************************************************************************************
//  DataFrameBase.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/14/2005 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace PCS.PhasorProtocols
{
    /// <summary>
    /// Represents the protocol independent common implementation of any <see cref="IDataFrame"/> that can be sent or received.
    /// </summary>
    [Serializable()]
    public abstract class DataFrameBase : ChannelFrameBase<IDataCell>, IDataFrame
    {
        #region [ Members ]

        // Fields
        private IConfigurationFrame m_configurationFrame;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DataFrameBase"/> from specified parameters.
        /// </summary>
        /// <param name="cells">The reference to the collection of cells for this <see cref="DataFrameBase"/>.</param>
        /// <param name="timestamp">The exact timestamp, in <see cref="Ticks"/>, of the data represented by this <see cref="DataFrameBase"/>.</param>
        /// <param name="configurationFrame">The <see cref="IConfigurationFrame"/> associated with this <see cref="DataFrameBase"/>.</param>
        protected DataFrameBase(DataCellCollection cells, Ticks timestamp, IConfigurationFrame configurationFrame)
            : base(0, cells, timestamp)
        {
            m_configurationFrame = configurationFrame;
        }

        /// <summary>
        /// Creates a new <see cref="DataFrameBase"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected DataFrameBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize data frame
            m_configurationFrame = (IConfigurationFrame)info.GetValue("configurationFrame", typeof(IConfigurationFrame));
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the <see cref="FundamentalFrameType"/> for this <see cref="DataFrameBase"/>.
        /// </summary>
        public override FundamentalFrameType FrameType
        {
            get
            {
                return FundamentalFrameType.DataFrame;
            }
        }

        /// <summary>
        /// Gets or sets <see cref="IConfigurationFrame"/> associated with this <see cref="DataFrameBase"/>.
        /// </summary>
        public virtual IConfigurationFrame ConfigurationFrame
        {
            get
            {
                return m_configurationFrame;
            }
            set
            {
                m_configurationFrame = value;
            }
        }

        /// <summary>
        /// Gets reference to the <see cref="DataCellCollection"/> for this <see cref="DataFrameBase"/>.
        /// </summary>
        public virtual new DataCellCollection Cells
        {
            get
            {
                return base.Cells as DataCellCollection;
            }
        }

        /// <summary>
        /// Gets or sets the parsing state for the this <see cref="DataFrameBase"/>.
        /// </summary>
        public virtual new IDataFrameParsingState State
        {
            get
            {
                return base.State as IDataFrameParsingState;
            }
            set
            {
                base.State = value;
            }
        }

        /// <summary>
        /// Gets the numeric ID code for this <see cref="DataFrameBase"/>.
        /// </summary>
        /// <remarks>
        /// This value is read-only for <see cref="DataFrameBase"/>; assigning a value will throw an exception. Value returned
        /// is the <see cref="IChannelFrame.IDCode"/> of the associated <see cref="ConfigurationFrame"/>.
        /// </remarks>
        /// <exception cref="NotSupportedException">IDCode of a data frame is read-only, change IDCode is associated configuration frame instead.</exception>
        public override ushort IDCode
        {
            get
            {
                return m_configurationFrame.IDCode;
            }
            set
            {
                throw new NotSupportedException("IDCode of a data frame is read-only, change IDCode is associated configuration frame instead");
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Parses the binary image.
        /// </summary>
        /// <param name="binaryImage">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="binaryImage"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="binaryImage"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        /// <remarks>
        /// This method is overriden to ensure assignment of configuration frame.
        /// </remarks>
        public override int Initialize(byte[] binaryImage, int startIndex, int length)
        {
            // Make sure configuration frame gets assigned before parsing begins...
            IDataFrameParsingState state = State;
            IConfigurationFrame configurationFrame = state.ConfigurationFrame;

            if (configurationFrame != null)
            {
                ConfigurationFrame = configurationFrame;

                // Handle normal parsing
                return base.Initialize(binaryImage, startIndex, length);
            }

            // Otherwise we just skip parsing this frame...
            return state.ParsedBinaryLength;
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            // Serialize data frame
            info.AddValue("configurationFrame", m_configurationFrame, typeof(IConfigurationFrame));
        }

        #endregion
    }
}