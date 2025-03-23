//******************************************************************************************************
//  MetadataProviderBase.cs - Gbtc
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
//  07/07/2009 - Pinal C. Patel
//       Generated original version of source code.
//  08/06/2009 - Pinal C. Patel
//       Made Initialize() virtual so inheriting classes can override the default behavior.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  09/15/2009 - Pinal C. Patel
//       Throwing ArgumentNullException exception in Refresh() if Metadata is null.
//  11/05/2009 - Pinal C. Patel
//       Modified to abort refresh operation during dispose.
//  03/30/2010 - Pinal C. Patel
//       Corrected the usage of Enabled in Refresh().
//  10/11/2010 - Mihir Brahmbhatt
//       Updated header and license agreement.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Configuration;
using System.Threading;
using System.Timers;
using GSF.Adapters;
using GSF.Configuration;
using GSF.Historian.Files;
using Timer = System.Timers.Timer;

// ReSharper disable VirtualMemberCallInConstructor

namespace GSF.Historian.MetadataProviders;

/// <summary>
/// Base class for a provider of updates to the data in a <see cref="MetadataFile"/>.
/// </summary>
public abstract class MetadataProviderBase : Adapter, IMetadataProvider
{
    #region [ Members ]

    // Events

    /// <summary>
    /// Occurs when <see cref="Refresh()"/> of <see cref="Metadata"/> is started.
    /// </summary>
    public event EventHandler MetadataRefreshStart;

    /// <summary>
    /// Occurs when <see cref="Refresh()"/> of <see cref="Metadata"/> is completed.
    /// </summary>
    public event EventHandler MetadataRefreshComplete;

    /// <summary>
    /// Occurs when <see cref="Refresh()"/> of <see cref="Metadata"/> times out.
    /// </summary>
    public event EventHandler MetadataRefreshTimeout;

    /// <summary>
    /// Occurs when an <see cref="Exception"/> is encountered during <see cref="Refresh()"/> of <see cref="Metadata"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="EventArgs{T}.Argument"/> is the <see cref="Exception"/> encountered during <see cref="Refresh()"/>.
    /// </remarks>
    public event EventHandler<EventArgs<Exception>> MetadataRefreshException;

    // Fields
    private int m_refreshInterval;
    private int m_refreshTimeout;
    private Thread m_refreshThread;
    private Timer m_refreshTimer;
    private bool m_disposed;
    private bool m_initialized;

    #endregion

    #region [ Constructors ]

    /// <summary>
    /// Initializes a new instance of the metadata provider.
    /// </summary>
    protected MetadataProviderBase()
    {
        m_refreshInterval = -1;
        m_refreshTimeout = 60;
        PersistSettings = true;
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets or sets the number of seconds to wait for the <see cref="Refresh()"/> to complete.
    /// </summary>
    /// <remarks>
    /// Set <see cref="RefreshTimeout"/> to -1 to wait indefinitely on <see cref="Refresh()"/>.
    /// </remarks>
    public int RefreshTimeout
    {
        get => m_refreshTimeout;
        set => m_refreshTimeout = value < 1 ? -1 : value;
    }

    /// <summary>
    /// Gets or sets the interval in minutes at which the <see cref="Metadata"/> if to be refreshed automatically.
    /// </summary>
    /// <remarks>
    /// Set <see cref="RefreshInterval"/> to -1 to disable auto <see cref="Refresh()"/>.
    /// </remarks>
    public int RefreshInterval
    {
        get => m_refreshInterval;
        set => m_refreshInterval = value < 1 ? -1 : value;
    }

    /// <summary>
    /// Gets or sets the <see cref="MetadataFile"/> to be refreshed by the metadata provider.
    /// </summary>
    public MetadataFile Metadata { get; set; }

    #endregion

    #region [ Methods ]

    /// <summary>
    /// When overridden in a derived class, refreshes the <see cref="Metadata"/> from an external source.
    /// </summary>
    protected abstract void RefreshMetadata();

    /// <summary>
    /// Initializes the metadata provider.
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();

        if (m_initialized)
            return;
        
        // Start refresh timer for auto-refresh.
        if (Enabled && m_refreshInterval > 0)
        {
            m_refreshTimer = new Timer(m_refreshInterval * 60000);
            m_refreshTimer.Elapsed += RefreshTimer_Elapsed;
            m_refreshTimer.Start();
        }

        // Initialize only once.
        m_initialized = true;
    }

    /// <summary>
    /// Saves metadata provider settings to the config file if the <see cref="Adapter.PersistSettings"/> property is set to true.
    /// </summary>
    /// <exception cref="ConfigurationErrorsException"><see cref="Adapter.SettingsCategory"/> has a value of null or empty string.</exception>
    public override void SaveSettings()
    {
        base.SaveSettings();

        if (!PersistSettings)
            return;
        
        // Save settings under the specified category.
        ConfigurationFile config = ConfigurationFile.Current;
        CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
        
        settings[nameof(Enabled), true].Update(Enabled);
        settings[nameof(RefreshTimeout), true].Update(m_refreshTimeout);
        settings[nameof(RefreshInterval), true].Update(m_refreshInterval);
        
        config.Save();
    }

    /// <summary>
    /// Loads saved metadata provider settings from the config file if the <see cref="Adapter.PersistSettings"/> property is set to true.
    /// </summary>
    /// <exception cref="ConfigurationErrorsException"><see cref="Adapter.SettingsCategory"/> has a value of null or empty string.</exception>
    public override void LoadSettings()
    {
        base.LoadSettings();

        if (!PersistSettings)
            return;
        
        // Load settings from the specified category.
        ConfigurationFile config = ConfigurationFile.Current;
        CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
        
        settings.Add(nameof(Enabled), Enabled, "True if this metadata provider is enabled; otherwise False.");
        settings.Add(nameof(RefreshTimeout), m_refreshTimeout, "Number of seconds to wait for metadata refresh to complete.");
        settings.Add(nameof(RefreshInterval), m_refreshInterval, "Interval in minutes at which the metadata is to be refreshed.");
        
        Enabled = settings[nameof(Enabled)].ValueAs(Enabled);
        RefreshTimeout = settings[nameof(RefreshTimeout)].ValueAs(m_refreshTimeout);
        RefreshInterval = settings[nameof(RefreshInterval)].ValueAs(m_refreshInterval);
    }

    /// <summary>
    /// Refreshes the <see cref="Metadata"/> from an external source.
    /// </summary>
    /// <returns>true if the <see cref="Metadata"/> is refreshed; otherwise false.</returns>
    /// <exception cref="ArgumentNullException"><see cref="Metadata"/> is null.</exception>
    public bool Refresh()
    {
        if (!Enabled || (m_refreshThread is not null && m_refreshThread.IsAlive))
            return false;

        if (Metadata is null)
            throw new ArgumentNullException(nameof(Metadata));

        m_refreshThread = new Thread(RefreshInternal);
        m_refreshThread.Start();

        if (m_refreshTimeout < 1)
        {
            // Wait indefinitely on the refresh.
            m_refreshThread.Join(Timeout.Infinite);
        }
        else
        {
            // Wait for the specified time on refresh.
            if (m_refreshThread.Join(m_refreshTimeout * 1000))
                return true;
            
            m_refreshThread.Abort();
            return false;
        }

        return true;
    }

    /// <summary>
    /// Releases the unmanaged resources used by the metadata provider and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        if (m_disposed)
            return;
        
        try
        {
            if (!disposing)
                return;
            
            m_refreshThread?.Abort();

            if (m_refreshTimer is not null)
            {
                m_refreshTimer.Elapsed -= RefreshTimer_Elapsed;
                m_refreshTimer.Dispose();
            }
        }
        finally
        {
            m_disposed = true;  // Prevent duplicate dispose.
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Raises the <see cref="MetadataRefreshStart"/> event.
    /// </summary>
    protected virtual void OnMetadataRefreshStart()
    {
        MetadataRefreshStart?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="MetadataRefreshComplete"/> event.
    /// </summary>
    protected virtual void OnMetadataRefreshComplete()
    {
        MetadataRefreshComplete?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="MetadataRefreshTimeout"/> event.
    /// </summary>
    protected virtual void OnMetadataRefreshTimeout()
    {
        MetadataRefreshTimeout?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="MetadataRefreshException"/> event.
    /// </summary>
    /// <param name="ex"><see cref="Exception"/> to send to <see cref="MetadataRefreshException"/> event.</param>
    protected virtual void OnMetadataRefreshException(Exception ex)
    {
        MetadataRefreshException?.Invoke(this, new EventArgs<Exception>(ex));
    }

    private void RefreshInternal()
    {
        try
        {
            OnMetadataRefreshStart();
            RefreshMetadata();
            OnMetadataRefreshComplete();
        }
        catch (ThreadAbortException)
        {
            OnMetadataRefreshTimeout();
        }
        catch (Exception ex)
        {
            OnMetadataRefreshException(ex);
        }
    }

    private void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
        Refresh();
    }

    #endregion
}