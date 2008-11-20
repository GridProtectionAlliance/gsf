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
    /// This class defines a basic implementation of parsing functionality suitable for common, simple formatted, binary data
    /// streams returning the parsed data via events.
    /// </summary>
    /// <remarks>
    /// This parser is designed as a write-only stream such that data can come from any source.
    /// </remarks>
    /// <typeparam name="TTypeIdentifier">Type of identifier used to distinguish output types.</typeparam>
    /// <typeparam name="TOutputType">Type of the interface or class used to represent outputs.</typeparam>
    [Description("Defines the basic functionality for parsing a binary data stream and returning the parsed data via events."),
    DefaultEvent("DataParsed")]
    public abstract class BasicParserBase<TTypeIdentifier, TOutputType> : ParserBase, IBasicParser<TTypeIdentifier, TOutputType>
    {
        #region [ Members ]

        // Nested Types

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="OptimizeTypeConstruction"/> property.
        /// </summary>
        public const bool DefaultOptimizeTypeConstruction = true;

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
        private bool m_optimizeTypeConstruction;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="BasicParserBase{TTypeIdentifier,TOutputType}"/> class.
        /// </summary>
        protected BasicParserBase()
        {
            m_optimizeTypeConstruction = DefaultOptimizeTypeConstruction;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a boolean value that indicates if data types get constructed in an optimized fashion.
        /// </summary>
        /// <remarks>
        /// This property defaults to true, it only needs to be changed if there are issues with type creation.
        /// </remarks>
        [Category("Settings"),
        DefaultValue(DefaultOptimizeTypeConstruction),
        Description("Indicates whether the data types are constructed in an optimal mode.")]
        public virtual bool OptimizeTypeConstruction
        {
            get
            {
                return m_optimizeTypeConstruction;
            }
            set
            {
                m_optimizeTypeConstruction = value;
            }
        }

        /// <summary>
        /// Gets current status of <see cref="BasicParserBase{TTypeIdentifier,TOutputType}"/>.
        /// </summary>
        [Browsable(false)]
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.Append(base.Status);
                status.Append("   Optimized type creation: ");
                status.Append(m_optimizeTypeConstruction);
                status.AppendLine();

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Saves settings for the data parser object to the config file if the <see cref="ParserBase.PersistSettings"/> 
        /// property is set to true.
        /// </summary>        
        public override void SaveSettings()
        {
            if (PersistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(SettingsCategory))
                    throw new InvalidOperationException("SettingsCategory property has not been set.");

                // Save settings under the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
                settings["OptimizeTypeConstruction", true].Update(m_optimizeTypeConstruction, "True if if data types get constructed in an optimized fashion; otherwise False.");

                // Save base class settings, this will flush any pending changes to config file
                base.SaveSettings();
            }
        }

        /// <summary>
        /// Loads saved settings for the data parser object from the config file if the <see cref="ParserBase.PersistSettings"/> 
        /// property is set to true.
        /// </summary>        
        public override void LoadSettings()
        {
            if (PersistSettings)
            {
                // Load base class settings, this will validate settings category
                base.LoadSettings();
                
                // Save settings under the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
                OptimizeTypeConstruction = settings["OptimizeTypeConstruction", true].ValueAs(m_optimizeTypeConstruction);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        protected override int ParseFrame(byte[] buffer, int offset, int length)
        {
            //{
            //    int parsedBytes;
            //    TTypeIdentifier id;

            //    // Extract the type ID
            //    parsedBytes = ExtractTypeID(buffer, offset, length, out id);
            //    offset += parsedBytes;

            //    if (m_outputTypes.TryGetValue(id, out outputType))
            //    {
            //            instance = outputType.CreateNew();
            //            instance.ParsingState = parsingState;
            //            cursor += instance.Initialize(item[i].Data, cursor);    // Returns the number of bytes used.
            //            output.Add(instance);
            //            m_assemblyAttemptTracker[item[i].Source] = 0;       // <- Necessary overhead :(
            //    }
            //    else
            //    {
            //        // If we come accross data in the image we cannot convert to a type than, we are going
            //        // to have to discard the remainder of the image because we will now know where the
            //        // the next valid block of data is within the image.
            //        cursor = item[i].Data.Length; // Move on to the next data image.
            //        //OnOutputTypeNotFound(new EventArgs<TTypeIdentifier>(parsingState.TypeID));
            //    }

            //    OnDataParsed(output);

            //    return parsedBytes;
            //}

            //protected virtual int ExtractTypeID(byte[] buffer, int offset, int length, out TTypeIdentifier id)
            //{
            //    id = default(TTypeIdentifier);
            //    return 0;
            //}

            return 0;
        }

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
