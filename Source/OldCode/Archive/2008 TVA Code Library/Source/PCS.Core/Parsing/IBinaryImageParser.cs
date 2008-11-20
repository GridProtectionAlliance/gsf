//*******************************************************************************************************
//  IFrameParser.vb - Frame parsing interface
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2008
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/14/2005 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************

using System;
using PCS;

namespace PCS.Parsing
{
    /// <summary>
    /// This interface represents the protocol independent representation of a binary image parser.
    /// </summary>
    public interface IBinaryImageParser<TTypeIdentifier, TOutputType> : ISupportLifecycle, IStatusProvider
    {
        //event EventHandler<EventArgs<IBinaryImageConsumer>> ReceivedImage;
        //event EventHandler<EventArgs<IBinaryImageConsumer>> ReceivedUndeterminedImage;

        /// <summary>
        /// Occurs when a data image is deserialized successfully to one of the output types that the data
        /// image represented.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the object that was deserialized from the binary image.
        /// </remarks>
        event EventHandler<EventArgs<TOutputType>> DataParsed;

        /// <summary>
        /// Occurs when matching a output type for deserializing the data image cound not be found.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the ID of the output type that could not be found.
        /// </remarks>
        event EventHandler<EventArgs<TTypeIdentifier>> OutputTypeNotFound;

        /// <summary>
        /// Occurs when data image cannot be deserialized to the output type that the data image represented.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the binary image that failed to parse.
        /// </remarks>
        event EventHandler<EventArgs<byte[]>> DataDiscarded;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered while attempting to parse data.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="Exception"/> encountered while parsing data.
        /// </remarks>
        event EventHandler<EventArgs<Exception>> ParsingException;

        /// <summary>
        /// Start the data parser.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the data parser.
        /// </summary>
        void Stop();

        /// <summary>
        /// Gets or sets flag that detemines if the internal buffer queue is enabled.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When interface implementations set this property to true, an internal buffer queue should be enabled causing
        /// any data to added to <see cref="Write"/> method to be queued for parsing on a separate thread. If the property
        /// is set to false, the parse should occur immediately on the thread that invoked the <see cref="Write"/> method.
        /// </para>
        /// </remarks>
        bool ExecuteParseOnSeparateThread
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the total number of buffers that are currently queued for processing, if any.
        /// </summary>
        int QueuedBuffers
        {
            get;
        }

        /// <summary>
        /// Writes a sequence of bytes onto the <see cref="IBinaryImageParser"/> stream for parsing.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        void Write(byte[] buffer, int offset, int count);
    }
}