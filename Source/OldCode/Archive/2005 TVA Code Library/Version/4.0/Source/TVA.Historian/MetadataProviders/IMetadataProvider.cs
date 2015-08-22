//*******************************************************************************************************
//  IMetadataProvider.cs
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
//  07/06/2009 - Pinal C. Patel
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using TVA.Configuration;
using TVA.Historian.Files;

namespace TVA.Historian.MetadataProviders
{
    /// <summary>
    /// Defines a provider of updates to the data in a <see cref="MetadataFile"/>.
    /// </summary>
    /// <seealso cref="MetadataFile"/>
    public interface IMetadataProvider : ISupportLifecycle, IPersistSettings
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Occurs when <see cref="Refresh()"/> of <see cref="Metadata"/> is started.
        /// </summary>
        event EventHandler MetadataRefreshStart;

        /// <summary>
        /// Occurs when <see cref="Refresh()"/> of <see cref="Metadata"/> is completed.
        /// </summary>
        event EventHandler MetadataRefreshComplete;

        /// <summary>
        /// Occurs when <see cref="Refresh()"/> of <see cref="Metadata"/> times out.
        /// </summary>
        event EventHandler MetadataRefreshTimeout;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered during <see cref="Refresh()"/> of <see cref="Metadata"/>.
        /// </summary>
        event EventHandler<EventArgs<Exception>> MetadataRefreshException;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the number of seconds to wait for the <see cref="Refresh()"/> to complete.
        /// </summary>
        int RefreshTimeout { get; set; }

        /// <summary>
        /// Gets or sets the number of minutes at which the <see cref="Metadata"/> if to be refreshed automatically.
        /// </summary>
        int RefreshInterval { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MetadataFile"/> to be refreshed by the metadata provider.
        /// </summary>
        MetadataFile Metadata { get; set; }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Refreshes the <see cref="Metadata"/> from an external source.
        /// </summary>
        /// <returns>true if the <see cref="Metadata"/> is refreshed; otherwise false.</returns>
        bool Refresh();

        #endregion
    }
}
