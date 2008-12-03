//*******************************************************************************************************
//  MultiSourceFrameParserBase.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  12/03/2008 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.ComponentModel;
using System.Collections.Generic;

namespace PCS.Parsing
{
    /// <summary>
    /// This class defines a basic implementation of parsing functionality suitable for automating the parsing of multiple
    /// binary data streams, each represented as frames with common headers and returning the parsed data via an event.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This parser is designed as a write-only stream such that data can come from any source.
    /// </para>
    /// <para>
    /// This class is more specific than the <see cref="StreamParserBase"/> in that it can automate the parsing of a
    /// particular protocol that is formatted as a series of frames that have a common method of identification.
    /// Automation of type creation occurs by loading implementations of common types that implement the
    /// <see cref="IBinaryImageConsumer{TTypeIdentifier}"/> interface. The common method of identification is
    /// handled by creating a class derived from the <see cref="ICommonHeader{TTypeIdentifier}"/> which primarily
    /// includes a TypeID property, but also should include any state information needed to parse a particular frame if
    /// necessary. Derived classes override the <see cref="FrameParserBase{TTypeIdentifier, TOutputType}.ParseCommonHeader"/>
    /// function in order to parse the <see cref="ICommonHeader{TTypeIdentifier}"/> from a provided binary image. Also, 
    /// since a data source identifier is being specified to track the incoming data source - user will need to override
    /// the <see cref="SerializeSourceID"/> and <see cref="DeserializeSourceID"/> as well.
    /// </para>
    /// </remarks>
    /// <typeparam name="TSourceIdentifier">Type of identifier for the data source.</typeparam>
    /// <typeparam name="TTypeIdentifier">Type of identifier used to distinguish output types.</typeparam>
    /// <typeparam name="TOutputType">Type of the interface or class used to represent outputs.</typeparam>
    [Description("Defines the basic functionality for parsing multiple binary data streams, each represented as frames with common headers and returning the parsed data via an event."),
    DefaultEvent("DataParsed")]
    public abstract class MultiSourceFrameParserBase<TSourceIdentifier, TTypeIdentifier, TOutputType> : FrameParserBase<TTypeIdentifier, TOutputType> where TOutputType : IBinaryImageConsumer<TTypeIdentifier>
    {
        #region [ Members ]

        // Nested Types

        // Constants

        // Delegates

        // Events

        /// <summary>
        /// Occurs when a data image is deserialized successfully to one or more object of the <see cref="Type"/> 
        /// that the data image was for.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="EventArgs{T1, T2}.Argument1"/> is the ID of the source for the data image.
        /// </para>
        /// <para>
        /// <see cref="EventArgs{T1, T2}.Argument2"/> is a list of objects deserialized from the data image.
        /// </para>
        /// </remarks>
        [Description("Occurs when a data image is deserialized successfully to one or more object of the Type that the data image was for.")]
        public new event EventHandler<EventArgs<TSourceIdentifier, List<TOutputType>>> DataParsed;

        // Fields
        private List<TOutputType> m_parsedOutputs;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="MultiSourceFrameParserBase{TSourceIdentifier,TTypeIdentifier,TOutputType}"/> class.
        /// </summary>
        public MultiSourceFrameParserBase()
        {
            // We attach to base class event to get individual parsed data elements
            base.DataParsed += CumulateParsedData;
            
            m_parsedOutputs = new List<TOutputType>();
        }

        #endregion

        #region [ Properties ]

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Writes a sequence of bytes, from the specified data source, onto the stream for parsing.
        /// </summary>
        /// <param name="source">ID of the data source.</param>
        /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        public void Write(TSourceIdentifier source, byte[] buffer, int offset, int count)
        {
            // Serialize source ID
            byte[] sourceID = SerializeSourceID(source);

            // Prepend serialized source ID before actual buffer so data can be tracked with data source
            buffer = sourceID.Combine(0, sourceID.Length, buffer, offset, count);

            // Send combined buffer to parsing engine
            base.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Consumers should call the <see cref="Write(TSourceIdentifier,byte[],int,int)"/> overload instead to make sure data source ID gets tracked with data buffer.
        /// </summary>
        /// <exception cref="NotImplementedException">This function should not be called directly.</exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException("This function should not be called directly, call overload that takes data source ID as parameter instead.");
        }

        /// <summary>
        /// Output type specific frame parsing algorithm.
        /// </summary>
        /// <param name="buffer">Buffer containing data to parse.</param>
        /// <param name="offset">Offset index into buffer that represents where to start parsing.</param>
        /// <param name="length">Maximum length of valid data from offset.</param>
        /// <returns>The length of the data that was parsed.</returns>
        protected override int ParseFrame(byte[] buffer, int offset, int length)
        {
            int parsedFrameLength;

            parsedFrameLength =  base.ParseFrame(buffer, offset, length);

            return parsedFrameLength;
        }

        /// <summary>
        /// Serializes a data source identifier into a binary image.
        /// </summary>
        /// <param name="sourceID">Data source ID to serialize.</param>
        /// <returns>Binary serialized representation of data source identifier.</returns>
        /// <remarks>
        /// This example shows how to serialize a TSourceIdentifier defined as a Guid:
        /// <code>
        /// protected override byte[] SerializeSourceID(TSourceIdentifier sourceID)
        /// {
        ///     return sourceID.ToByteArray();
        /// }
        /// </code>
        /// </remarks>
        protected abstract byte[] SerializeSourceID(TSourceIdentifier sourceID);

        /// <summary>
        /// Deserializes data source identifier from a binary image.
        /// </summary>
        /// <param name="buffer">Buffer containing data source identifier to deserialize.</param>
        /// <param name="offset">Offset index into buffer that represents where to start deserializing.</param>
        /// <param name="length">Maximum length of valid data from offset.</param>
        /// <param name="sourceID">Deserialized data source identifier.</param>
        /// <returns>The length of the data that was parsed.</returns>
        /// <remarks>
        /// <para>
        /// Implementors should return 0 if there is not enough data available to represent the image, do this
        /// instead of throwing an exception.
        /// </para>
        /// <para>
        /// This example shows how to deserialize a TSourceIdentifier defined as a Guid:
        /// <code>
        /// protected override int DeserializeSourceID(byte[] buffer, int offset, int length, out TSourceIdentifier sourceID);
        /// {
        ///     if (length &lt; 16)
        ///     {
        ///         // Not enough data to represent data source Guid
        ///         sourceID = Guid.Empty;
        ///         return 0;
        ///     }
        ///     else
        ///     {
        ///         sourceID = new Guid(buffer.BlockCopy(offset, 16));
        ///         return 16;
        ///     }
        /// }
        /// </code>
        /// </para>
        /// </remarks>
        protected abstract int DeserializeSourceID(byte[] buffer, int offset, int length, out TSourceIdentifier sourceID);

        /// <summary>
        /// Raises the <see cref="DataParsed"/> event.
        /// </summary>
        /// <param name="sourceID">Data source ID.</param>
        /// <param name="parsedData">List of parsed events.</param>
        protected void OnDataParsed(TSourceIdentifier sourceID, List<TOutputType> parsedData)
        {            
            if (DataParsed != null)
                DataParsed(this, new EventArgs<TSourceIdentifier,List<TOutputType>>(sourceID, parsedData));
        }

        // Cumulate individual data elements for all buffers received from a single data source
        private void CumulateParsedData(object source, EventArgs<TOutputType> data)
        {
            m_parsedOutputs.Add(data.Argument);
        }

        #endregion
    }
}
