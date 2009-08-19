//*******************************************************************************************************
//  IExporter.cs
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
//  06/12/2007 - Pinal C. Patel
//       Original version of source code generated.
//  06/05/2008 - Pinal C. Patel
//       Modified the ExportProcessException event definition.
//  04/17/2009 - Pinal C. Patel
//       Converted to C#.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;

namespace TVA.Historian.Exporters
{
    /// <summary>
    /// Defines an exporter of real-time time series data.
    /// </summary>
    /// <seealso cref="Export"/>
    /// <seealso cref="DataListener"/>
    public interface IExporter : IDisposable
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Occurs when the exporter want to provide a status update.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the status update message.
        /// </remarks>
        event EventHandler<EventArgs<string>> StatusUpdate;

        /// <summary>
        /// Occurs when the exporter finishes processing an <see cref="Export"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="Export"/> that the exporter finished processing.
        /// </remarks>
        event EventHandler<EventArgs<Export>> ExportProcessed;

        /// <summary>
        /// Occurs when the exporter fails to process an <see cref="Export"/> due to an <see cref="Exception"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="Export"/> that the exporter failed to process.
        /// </remarks>
        event EventHandler<EventArgs<Export>> ExportProcessException;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the name of the exporter.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets the <see cref="Export"/>s associated with the exporter.
        /// </summary>
        IList<Export> Exports { get; }

        /// <summary>
        /// Gets the <see cref="DataListener"/>s providing real-time time series data to the exporter.
        /// </summary>
        IList<DataListener> Listeners { get; }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Causes the <see cref="Export"/> with the specified <paramref name="exportName"/> to be processed.
        /// </summary>
        /// <param name="exportName"><see cref="Export.Name"/> of the <see cref="Export"/> to be processed.</param>
        void ProcessExport(string exportName);

        #endregion
    }
}
