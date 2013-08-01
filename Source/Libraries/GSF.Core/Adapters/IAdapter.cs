//******************************************************************************************************
//  IAdapter.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  09/23/2010 - Pinal C. Patel
//       Generated original version of source code.
//  03/08/2011 - Pinal C. Patel
//       Added StatusUpdate and Disposed events.
//       Added Type and File properties to support serialized adapter instances.
//  04/05/2011 - Pinal C. Patel
//       Changed properties Type to TypeName and File to HostFile to avoid naming conflict.
//  04/08/2011 - Pinal C. Patel
//       Added ExecutionException event.
//       Renamed StatusUpdate event to StatusMessage.
//  11/23/2011 - J. Ritchie Carroll
//       Modified to support buffer optimized ISupportBinaryImage.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using GSF.Configuration;

namespace GSF.Adapters
{
    /// <summary>
    /// Defines an adapter that could execute in isolation in a separate <see cref="AppDomain"/>.
    /// </summary>
    public interface IAdapter : ISupportLifecycle, IProvideStatus, IPersistSettings
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Occurs when the <see cref="IAdapter"/> wants to provide a status update.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1,T2}.Argument1"/> is the <see cref="UpdateType"/>.<br/>
        /// <see cref="EventArgs{T1,T2}.Argument2"/> is the update message.
        /// </remarks>
        event EventHandler<EventArgs<UpdateType, string>> StatusUpdate;

        /// <summary>
        /// Occurs when the <see cref="IAdapter"/> encounters an <see cref="Exception"/> during execution.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1,T2}.Argument1"/> is the text that describes the activity that was being performed by the <see cref="IAdapter"/>.<br/>
        /// <see cref="EventArgs{T1,T2}.Argument2"/> is the encountered <see cref="Exception"/>.
        /// </remarks>
        event EventHandler<EventArgs<string, Exception>> ExecutionException;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the text representation of the <see cref="IAdapter"/>'s <see cref="TypeName"/>.
        /// </summary>
        string TypeName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the path to the file where the <see cref="IAdapter"/> is housed.
        /// </summary>
        string HostFile
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the <see cref="AppDomain"/> in which the <see cref="IAdapter"/> is executing.
        /// </summary>
        AppDomain Domain
        {
            get;
        }

        /// <summary>
        /// Gets the memory utilization of the <see cref="IAdapter"/> in bytes if executing in a separate <see cref="AppDomain"/>, otherwise <see cref="Double.NaN"/>.
        /// </summary>
        double MemoryUsage
        {
            get;
        }

        /// <summary>
        /// Gets the % processor utilization of the <see cref="IAdapter"/> if executing in a separate <see cref="AppDomain"/> otherwise <see cref="Double.NaN"/>.
        /// </summary>
        double ProcessorUsage
        {
            get;
        }

        #endregion
    }
}
