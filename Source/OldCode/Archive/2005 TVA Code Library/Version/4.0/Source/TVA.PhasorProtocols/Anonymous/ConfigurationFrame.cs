//*******************************************************************************************************
//  ConfigurationFrame.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  05/05/2009 - James R. Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Soap;
using TVA.IO;
using TVA.IO.Checksums;

namespace TVA.PhasorProtocols.Anonymous
{
    /// <summary>
    /// Represents a protocol independent implementation of a <see cref="IConfigurationFrame"/> that can be sent or received.
    /// </summary>
    [Serializable()]
    public class ConfigurationFrame : ConfigurationFrameBase
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ConfigurationFrame"/> from specified parameters.
        /// </summary>
        /// <param name="idCode">The ID code of this <see cref="ConfigurationFrame"/>.</param>
        /// <param name="timestamp">The exact timestamp, in <see cref="Ticks"/>, of the data represented by this <see cref="ConfigurationFrame"/>.</param>
        /// <param name="frameRate">The defined frame rate of this <see cref="ConfigurationFrame"/>.</param>
        public ConfigurationFrame(ushort idCode, Ticks timestamp, ushort frameRate)
            : base(idCode, new ConfigurationCellCollection(), timestamp, frameRate)
        {
        }

        /// <summary>
        /// Creates a new <see cref="ConfigurationFrame"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected ConfigurationFrame(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets reference to the <see cref="ConfigurationCellCollection"/> for this <see cref="ConfigurationFrame"/>.
        /// </summary>
        public new ConfigurationCellCollection Cells
        {
            get
            {
                return base.Cells as ConfigurationCellCollection;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Calculates checksum of given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">Buffer image over which to calculate checksum.</param>
        /// <param name="offset">Start index into <paramref name="buffer"/> to calculate checksum.</param>
        /// <param name="length">Length of data within <paramref name="buffer"/> to calculate checksum.</param>
        /// <returns>Checksum over specified portion of <paramref name="buffer"/>.</returns>
        protected override ushort CalculateChecksum(byte[] buffer, int offset, int length)
        {
            // Just returning calculated CRC-CCITT over given buffer as a default CRC
            return buffer.CrcCCITTChecksum(offset, length);
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Serialize configuration frame to cache folder for later use (if needed).
        /// </summary>
        /// <param name="state">
        /// Reference to a <see cref="EventArgs{T1, T2, T3}"/> instance containing the following:<br/>
        /// a reference to <see cref="IConfigurationFrame"/> (as T1),<br/>
        /// a <see cref="Action{T}"/> delegate to handle process exceptions (as T2),<br/>
        /// and a <see cref="string"/> representing the file name (as T3).
        /// </param>
        /// <remarks>
        /// It is expected that this function will be called from the <see cref="System.Threading.ThreadPool"/>.
        /// </remarks>
        public static void Cache(object state)
        {
            EventArgs<IConfigurationFrame, Action<Exception>, string> e = state as EventArgs<IConfigurationFrame, Action<Exception>, string>;

            if (e != null)
            {
                IConfigurationFrame configurationFrame = e.Argument1;
                Action<Exception> exceptionHandler = e.Argument2;
                string name = e.Argument3;

                try
                {
                    // Define configuration cache sub-directory
                    string cachePath = string.Format("{0}\\ConfigurationCache\\", FilePath.GetAbsolutePath(""));

                    // Make sure configuration cache directory exists
                    if (!Directory.Exists(cachePath))
                        Directory.CreateDirectory(cachePath);

                    // Serialize configuration frame to a file
                    FileStream configFile = File.Create(string.Format("{0}{1}.configuration.xml", cachePath, name));
                    SoapFormatter xmlSerializer = new SoapFormatter();

                    xmlSerializer.AssemblyFormat = FormatterAssemblyStyle.Simple;
                    xmlSerializer.TypeFormat = FormatterTypeStyle.TypesWhenNeeded;
                    xmlSerializer.Serialize(configFile, configurationFrame);

                    configFile.Close();
                }
                catch (Exception ex)
                {
                    exceptionHandler(new InvalidOperationException(string.Format("Failed to serialize configuration frame: {0}", ex.Message), ex));
                }
            }
        }

        #endregion        
    }
}