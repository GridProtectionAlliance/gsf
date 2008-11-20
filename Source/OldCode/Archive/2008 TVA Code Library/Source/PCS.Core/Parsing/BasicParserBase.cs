//*******************************************************************************************************
//  BasicParserBase.cs
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
//  11/20/2008 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using System.Collections.Generic;
using PCS.Configuration;
using PCS.Collections;

namespace PCS.Parsing
{
    /// <summary>
    /// This class defines a basic implementation of parsing functionality suitable for common, simple formatted, binary data streams.
    /// </summary>
    /// <remarks>
    /// This parser is designed as a write-only stream such that data can come from any source.
    /// </remarks>
    /// <typeparam name="TTypeIdentifier">Type of identifier used to distinguish output types.</typeparam>
    /// <typeparam name="TOutputType">Type of the interface or class used to represent outputs.</typeparam>
    [Description("Defines the basic functionality for parsing a binary data stream and returning the parsed data via events."),
    DesignerCategory("Component"),
    DefaultEvent("ParsingException"),
    DefaultProperty("ExecuteParseOnSeparateThread")]
    public abstract class BasicParserBase<TTypeIdentifier, TOutputType> : ParserBase
    {
        #region [ Members ]

        // Nested Types

        // Constants

        // Delegates

        // Events

        /// <summary>
        /// Occurs when a data image is deserialized successfully to one of the output types that the data
        /// image represented.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the object that was deserialized from the binary image.
        /// </remarks>
        public event EventHandler<EventArgs<TOutputType>> DataParsed;

        /// <summary>
        /// Occurs when matching a output type for deserializing the data image cound not be found.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the ID of the output type that could not be found.
        /// </remarks>
        public event EventHandler<EventArgs<TTypeIdentifier>> OutputTypeNotFound;

        // Fields

        #endregion

        #region [ Constructors ]

        #endregion

        #region [ Properties ]

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Raises the <see cref="DataParsed"/> event.
        /// </summary>
        /// <param name="obj">Object deserialized from binary image.</param>
        protected virtual void OnDataParsed(TOutputType obj)
        {
            if (DataParsed != null)
                DataParsed(this, new EventArgs<TOutputType>(obj));
        }

        /// <summary>
        /// Raises the <see cref="OutputTypeNotFound"/> event.
        /// </summary>
        /// <param name="id">ID of the output type that was not found.</param>
        protected virtual void OnOutputTypeNotFound(TTypeIdentifier id)
        {
            if (OutputTypeNotFound != null)
                OutputTypeNotFound(this, new EventArgs<TTypeIdentifier>(id));
        }

        #endregion

        #region [ Operators ]

        #endregion

        #region [ Static ]

        // Static Fields

        // Static Constructor

        // Static Properties

        // Static Methods

        #endregion
        
    }
}
