//*******************************************************************************************************
//  MetadataProviders.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C Patel
//      Office: INFO SVCS APP DEV, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  08/04/2009 - Pinal C Patel
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;

namespace TVA.Historian.MetadataProviders
{
    /// <summary>
    /// A class that loads all providers of data for <see cref="TVA.Historian.Files.MetadataFile"/> and enables the execution of a manual refresh using the loaded providers.
    /// </summary>
    public class MetadataProviders : AdapterLoader<IMetadataProvider>
    {
        #region [ Methods ]

        /// <summary>
        /// <see cref="IMetadataProvider.Refresh()"/>es <see cref="IMetadataProvider.Metadata"/> using all loaded metadata provider <see cref="AdapterProvider{T}.Adapters"/>.
        /// </summary>
        public void RefreshAll()
        {
            OperationQueue.Add(null);
        }

        /// <summary>
        /// <see cref="IMetadataProvider.Refresh()"/>es <see cref="IMetadataProvider.Metadata"/> using the first of all loaded metadata provider <see cref="AdapterProvider{T}.Adapters"/>.
        /// </summary>
        public void RefreshOne()
        {
            lock (Adapters)
            {
                foreach (IMetadataProvider adapter in Adapters)
                {
                    if (adapter.Enabled)
                    {
                        OperationQueue.Add(adapter.GetType().Name);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Executes <see cref="IMetadataProvider.Refresh()"/> on the specified metadata provider.
        /// </summary>
        /// <param name="adapter">Metadata provider on which the refresh is to be executed.</param>
        /// <param name="data">Data to be used when executing the refresh.</param>
        protected override void ExecuteAdapterOperation(IMetadataProvider adapter, object data)
        {
            if (string.IsNullOrEmpty(Convert.ToString(data)) ||
                string.Compare(data.ToString(), adapter.GetType().Name, true) == 0)
            {
                adapter.Refresh();
            }
        }

        #endregion        
    }
}
