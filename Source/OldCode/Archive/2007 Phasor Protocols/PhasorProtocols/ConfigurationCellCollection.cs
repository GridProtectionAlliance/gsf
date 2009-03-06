//*******************************************************************************************************
//  ConfigurationCellCollection.cs
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
using System.Linq;
using System.Runtime.Serialization;
using PCS.Collections;

namespace PCS.PhasorProtocols
{
    /// <summary>
    /// Represents a protocol independent collection of <see cref="IConfigurationCell"/> objects.
    /// </summary>
    [Serializable()]
    public class ConfigurationCellCollection : ChannelCellCollectionBase<IConfigurationCell>
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ConfigurationCellCollection"/> from specified parameters.
        /// </summary>
        /// <param name="lastValidIndex">Last valid index for the collection (i.e., maximum count - 1).</param>
        /// <param name="constantCellLength">Sets flag that determines if the lengths of <see cref="IConfigurationCell"/> elements in this <see cref="ConfigurationCellCollection"/> are constant.</param>
        /// <remarks>
        /// <paramref name="lastValidIndex"/> is used instead of maximum count so that maximum type values may
        /// be specified as needed. For example, if the protocol specifies a collection with a signed 16-bit
        /// maximum length you can specify <see cref="Int16.MaxValue"/> (i.e., 32,767) as the last valid index
        /// for the collection since total number of items supported would be 32,768.
        /// </remarks>
        public ConfigurationCellCollection(int lastValidIndex, bool constantCellLength)
            : base(lastValidIndex, constantCellLength)
        {
        }

        /// <summary>
        /// Creates a new <see cref="ConfigurationCellCollection"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected ConfigurationCellCollection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Attempts to get <see cref="IConfigurationCell"/> with the specified label.
        /// </summary>
        /// <param name="label">The <see cref="IConfigurationCell.IDLabel"/> to find.</param>
        /// <param name="configurationCell">
        /// When this method returns, contians the <see cref="IConfigurationCell"/> with the specified <paramref name="label"/>, if found;
        /// otherwise, null is returned.
        /// </param>
        /// <returns><c>true</c> if the <see cref="ConfigurationCellCollection"/> contains an element with the specified <paramref name="label"/>; otherwise, <c>false</c>.</returns>
        public virtual bool TryGetByIDLabel(string label, ref IConfigurationCell configurationCell)
        {
            configurationCell = this.FirstOrDefault(cell => string.Compare(cell.IDLabel, label, true) == 0);
            return (configurationCell != null);
        }

        /// <summary>
        /// Attempts to get <see cref="IConfigurationCell"/> with the specified ID code.
        /// </summary>
        /// <param name="idCode">The <see cref="IChannelCell.IDCode"/> to find.</param>
        /// <param name="configurationCell">
        /// When this method returns, contians the <see cref="IConfigurationCell"/> with the specified <paramref name="idCode"/>, if found;
        /// otherwise, null is returned.
        /// </param>
        /// <returns><c>true</c> if the <see cref="ConfigurationCellCollection"/> contains an element with the specified <paramref name="idCode"/>; otherwise, <c>false</c>.</returns>
        public virtual bool TryGetByIDCode(ushort idCode, ref IConfigurationCell configurationCell)
        {
            configurationCell = this.FirstOrDefault(cell => cell.IDCode == idCode);
            return (configurationCell != null);
        }

        /// <summary>
        /// Attempts to find the index of the <see cref="IConfigurationCell"/> with the specified label.
        /// </summary>
        /// <param name="label">The <see cref="IConfigurationCell.IDLabel"/> to find.</param>
        /// <returns>Index of the <see cref="ConfigurationCellCollection"/> that contains the specified <paramref name="label"/>; otherwise, <c>-1</c>.</returns>
        public virtual int IndexOfIDLabel(string label)
        {
            return this.IndexOf(cell => string.Compare(cell.IDLabel, label, true) == 0);
        }

        /// <summary>
        /// Attempts to find the index of the <see cref="IConfigurationCell"/> with the specified ID code.
        /// </summary>
        /// <param name="idCode">The <see cref="IChannelCell.IDCode"/> to find.</param>
        /// <returns>Index of the <see cref="ConfigurationCellCollection"/> that contains the specified <paramref name="idCode"/>; otherwise, <c>-1</c>.</returns>
        public virtual int IndexOfIDCode(ushort idCode)
        {
            return this.IndexOf(cell => cell.IDCode == idCode);
        }

        #endregion
    }
}