﻿//******************************************************************************************************
//  DataService.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  08/27/2009 - Pinal C. Patel
//       Generated original version of source code.
//  09/02/2009 - Pinal C. Patel
//       Modified configuration of the default WebHttpBinding to enable receiving of large payloads.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/01/2009 - Pinal C. Patel
//       Added a default protected constructor.
//  06/22/2010 - Pinal C. Patel
//       Modified the default constructor to set the base class Singleton property to true.
//  10/11/2010 - Mihir Brahmbhatt
//       Updated header and license agreement.
//  11/07/2010 - Pinal C. Patel
//       Modified to fix breaking changes made to SelfHostingService.
//  03/31/2011 - J. Ritchie Carroll
//       Updated to allow customizable close timeout and maximum possible number of items to be
//       returned from data service.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Diagnostics.CodeAnalysis;
using System.ServiceModel.Description;
using GSF.Configuration;
using GSF.ServiceModel;

// ReSharper disable VirtualMemberCallInConstructor

namespace GSF.Historian.DataServices;

/// <summary>
/// A base class for web service that can send and receive historian data over REST (Representational State Transfer) interface.
/// </summary>
[SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
public class DataService : SelfHostingService, IDataService
{
    #region [ Constructors ]

    /// <summary>
    /// Initializes a new instance of historian data web service.
    /// </summary>
    protected DataService()
    {
        Singleton = true;
        PublishMetadata = true;
        PersistSettings = true;

        // We set the default close timeout to 2 minutes
        CloseTimeout = TimeSpan.Parse("00:02:00");
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets or sets the <see cref="IArchive"/> used by the web service for its data.
    /// </summary>
    public virtual IArchive Archive { get; set; }

    /// <summary>
    /// Gets or sets the desired <see cref="System.ServiceModel.Channels.Binding.CloseTimeout"/> used as
    /// the interval of time provided for a connection to close before the transport raises an exception.
    /// </summary>
    public TimeSpan CloseTimeout { get; set; }

    #endregion

    #region [ Methods ]

    /// <summary>
    /// Loads saved web service settings from the config file if the <see cref="GSF.Adapters.Adapter.PersistSettings"/> property is set to true.
    /// </summary>
    public override void LoadSettings()
    {
        base.LoadSettings();

        if (!PersistSettings)
            return;
        
        // Load settings from the specified category.
        ConfigurationFile config = ConfigurationFile.Current;
        CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
        
        settings.Add(nameof(CloseTimeout), CloseTimeout, "Maximum time allowed for a connection to close before raising a timeout exception.");
        
        CloseTimeout = settings[nameof(CloseTimeout)].ValueAs(CloseTimeout);
    }

    /// <summary>
    /// Saves web service settings to the config file if the <see cref="GSF.Adapters.Adapter.PersistSettings"/> property is set to true.
    /// </summary>
    public override void SaveSettings()
    {
        base.SaveSettings();

        if (!PersistSettings)
            return;
        
        // Save settings under the specified category.
        ConfigurationFile config = ConfigurationFile.Current;
        CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
        
        settings[nameof(CloseTimeout), true].Update(CloseTimeout);
        
        config.Save();
    }

    /// <summary>
    /// Raises the <see cref="SelfHostingService.ServiceHostCreated"/> event.
    /// </summary>
    /// <remarks>
    /// We override default behavior to update the default close timeout and to make sure the
    /// maximum number of items are allowed in the returned graph.
    /// </remarks>
    protected override void OnServiceHostCreated()
    {
        base.OnServiceHostCreated();

        foreach (ServiceEndpoint endpoint in ServiceHost.Description.Endpoints)
        {
            // Update the close timeout based on customizable setting
            if (endpoint.Binding is not null)
                endpoint.Binding.CloseTimeout = CloseTimeout;

            // Verify that max items allowed in object graphs are set to maximum possible
            foreach (OperationDescription description in endpoint.Contract.Operations)
            {
                DataContractSerializerOperationBehavior behavior = description.Behaviors.Find<DataContractSerializerOperationBehavior>();
                    
                if (behavior is not null)
                    behavior.MaxItemsInObjectGraph = int.MaxValue;
            }
        }
    }

    #endregion
}