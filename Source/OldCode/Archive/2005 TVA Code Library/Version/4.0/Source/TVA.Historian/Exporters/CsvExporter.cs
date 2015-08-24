//*******************************************************************************************************
//  CsvExporter.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: INFO SVCS APP DEV, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  06/22/2007 - Pinal C. Patel
//       Original version of source code generated.
//  04/17/2009 - Pinal C. Patel
//       Converted to C#.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace TVA.Historian.Exporters
{
    /// <summary>
    /// Represents an exporter that can export the current time series data in CSV format to a file.
    /// </summary>
    /// <example>
    /// Definition of a sample <see cref="Export"/> that can be processed by <see cref="CsvExporter"/>:
    /// <code>
    /// <![CDATA[
    /// <?xml version="1.0" encoding="utf-16"?>
    /// <Export xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
    ///   <Name>CsvExport</Name>
    ///   <Type>Intervaled</Type>
    ///   <Interval>60</Interval>
    ///   <Exporter>CsvExporter</Exporter>
    ///   <Settings>
    ///     <ExportSetting>
    ///       <Name>OutputFile</Name>
    ///       <Value>c:\CsvExportOutput.csv</Value>
    ///     </ExportSetting>
    ///   </Settings>
    ///   <Records>
    ///     <ExportRecord>
    ///       <Instance>P2</Instance>
    ///       <Identifier>1885</Identifier>
    ///     </ExportRecord>  
    ///     <ExportRecord>
    ///       <Instance>P2</Instance>
    ///       <Identifier>2711</Identifier>
    ///     </ExportRecord>      
    ///   </Records>
    /// </Export>
    /// ]]>
    /// </code>
    /// <para>
    /// Description of custom settings required by <see cref="CsvExporter"/> in an <see cref="Export"/>:
    /// <list type="table">
    ///     <listheader>
    ///         <term>Setting Name</term>
    ///         <description>Setting Description</description>
    ///     </listheader>
    ///     <item>
    ///         <term>OutputFile</term>
    ///         <description>Name of the CSV file (including path) where export data is to be written.</description>
    ///     </item>
    /// </list>
    /// </para>
    /// </example>
    /// <seealso cref="Export"/>
    public class CsvExporter : ExporterBase
    {
        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvExporter"/> class.
        /// </summary>
        public CsvExporter()
            : this("CsvExporter")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvExporter"/> class.
        /// </summary>
        /// <param name="name"><see cref="ExporterBase.Name"/> of the exporter.</param>
        protected CsvExporter(string name)
            : base(name)
        {
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Processes the <paramref name="export"/> using the current <see cref="DataListener.Data"/>.
        /// </summary>
        /// <param name="export"><see cref="Export"/> to be processed.</param>
        /// <exception cref="ArgumentException"><b>OutputFile</b> setting is missing from the <see cref="Export.Settings"/> of the <paramref name="export"/>.</exception>
        protected override void ProcessExport(Export export)
        {
            // Ensure that required settings are present.
            ExportSetting outputFileSetting = export.FindSetting("OutputFile");
            if (outputFileSetting == null)
                throw new ArgumentException("OutputFile setting is missing.");

            // Write the current data for the export to the specified files in CSV format.
            FileHelper.WriteToFile(outputFileSetting.Value, "CSV", GetExportDataAsDataset(export, null));
        }

        /// <summary>
        /// Processes the <paramref name="export"/> using the real-time <paramref name="data"/>.
        /// </summary>
        /// <param name="export"><see cref="Export"/> to be processed.</param>
        /// <param name="listener"><see cref="DataListener"/> that provided the <paramref name="data"/>.</param>
        /// <param name="data">Real-time time series data received by the <paramref name="listener"/>.</param>
        /// <exception cref="NotSupportedException">Always</exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override void ProcessRealTimeExport(Export export, DataListener listener, IList<IDataPoint> data)
        {
            throw new NotSupportedException();  // Real-Time exports not supported.
        }

        #endregion
    }
}