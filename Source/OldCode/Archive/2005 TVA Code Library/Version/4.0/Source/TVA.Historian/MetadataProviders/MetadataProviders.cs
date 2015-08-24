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
    /// A class that loads all <see cref="IMetadataProvider">Metadata Provider</see> adapters.
    /// </summary>
    /// <seealso cref="IMetadataProvider"/>
    public class MetadataProviders : AdapterLoader<IMetadataProvider>
    {
        #region [ Methods ]

        /// <summary>
        /// <see cref="IMetadataProvider.Refresh()"/>es the <see cref="IMetadataProvider.Metadata"/> for all loaded metadata provider <see cref="AdapterLoader{T}.Adapters"/>.
        /// </summary>
        public void RefreshAll()
        {
            OperationQueue.Add(null);
        }

        /// <summary>
        /// <see cref="IMetadataProvider.Refresh()"/>es the <see cref="IMetadataProvider.Metadata"/> for the first of all loaded metadata provider <see cref="AdapterLoader{T}.Adapters"/>.
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
        /// Executes <see cref="IMetadataProvider.Refresh()"/> on the specified metadata provider <paramref name="adapter"/>.
        /// </summary>
        /// <param name="adapter">An <see cref="IMetadataProvider"/> object.</param>
        /// <param name="data"><see cref="System.Reflection.MemberInfo.Name"/> of the <paramref name="adapter"/>.</param>
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
