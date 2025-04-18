﻿//******************************************************************************************************
//  ReplicationProviderBase.cs - Gbtc
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
//  11/04/2009 - Pinal C. Patel
//       Generated original version of source code.
//  11/17/2009 - Pinal C. Patel
//       Made Initialize(), SaveSettings() and LoadSettings() overridable in a derived class.
//  03/30/2010 - Pinal C. Patel
//       Corrected the usage of Enabled in Replicate().
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
using Timer = System.Timers.Timer;

// ReSharper disable VirtualMemberCallInConstructor

namespace GSF.Historian.Replication;

/// <summary>
/// Base class for a provider of replication mechanism for the <see cref="IArchive"/>.
/// </summary>
public abstract class ReplicationProviderBase : Adapter, IReplicationProvider
{
    #region [ Members ]

    // Events

    /// <summary>
    /// Occurs when the process of replicating the <see cref="IArchive"/> is started.
    /// </summary>
    public event EventHandler ReplicationStart;

    /// <summary>
    /// Occurs when the process of replicating the <see cref="IArchive"/> is complete.
    /// </summary>
    public event EventHandler ReplicationComplete;

    /// <summary>
    /// Occurs when an <see cref="Exception"/> is encountered during the replication process of <see cref="IArchive"/>.
    /// </summary>
    public event EventHandler<EventArgs<Exception>> ReplicationException;

    /// <summary>
    /// Occurs when the <see cref="IArchive"/> is being replicated.
    /// </summary>
    public event EventHandler<EventArgs<ProcessProgress<int>>> ReplicationProgress;

    // Fields
    private int m_replicationInterval;
    private Thread m_replicationThread;
    private Timer m_replicationTimer;
    private bool m_initialized;
    private bool m_disposed;

    #endregion

    #region [ Constructors ]

    /// <summary>
    /// Initializes a new instance of the replication provider.
    /// </summary>
    protected ReplicationProviderBase()
    {
        m_replicationInterval = -1;
        PersistSettings = true;
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets or sets the primary location of the <see cref="IArchive"/>.
    /// </summary>
    public string ArchiveLocation { get; set; }

    /// <summary>
    /// Gets or sets the mirrored location of the <see cref="IArchive"/>.
    /// </summary>
    public string ReplicaLocation { get; set; }

    /// <summary>
    /// Gets or sets the interval in minutes at which the <see cref="IArchive"/> is to be replicated.
    /// </summary>
    public int ReplicationInterval
    {
        get => m_replicationInterval;
        set => m_replicationInterval = value < 0 ? -1 : value;
    }

    #endregion

    #region [ Methods ]

    /// <summary>
    /// When overridden in a derived class, replicates the <see cref="IArchive"/>.
    /// </summary>
    protected abstract void ReplicateArchive();

    /// <summary>
    /// Initializes the replication provider.
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();

        if (m_initialized)
            return;
        
        // Start timer for periodic replication.
        if (Enabled && m_replicationInterval > 0)
        {
            m_replicationTimer = new Timer(m_replicationInterval * 60000);
            m_replicationTimer.Elapsed += ReplicationTimer_Elapsed;
            m_replicationTimer.Start();
        }

        // Initialize only once.
        m_initialized = true;
    }

    /// <summary>
    /// Saves replication provider settings to the config file if the <see cref="Adapter.PersistSettings"/> property is set to true.
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
        settings[nameof(ArchiveLocation), true].Update(ArchiveLocation);
        settings[nameof(ReplicaLocation), true].Update(ReplicaLocation);
        settings[nameof(ReplicationInterval), true].Update(m_replicationInterval);
        
        config.Save();
    }

    /// <summary>
    /// Loads saved replication provider settings from the config file if the <see cref="Adapter.PersistSettings"/> property is set to true.
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
        
        settings.Add(nameof(Enabled), Enabled, "True if this replication provider is enabled; otherwise False.");
        settings.Add(nameof(ArchiveLocation), ArchiveLocation, "Path to the primary location of time-series data archive.");
        settings.Add(nameof(ReplicaLocation), ReplicaLocation, "Path to the mirrored location of time-series data archive.");
        settings.Add(nameof(ReplicationInterval), m_replicationInterval, "Interval in minutes at which the time-series data archive is to be replicated.");
        
        Enabled = settings[nameof(Enabled)].ValueAs(Enabled);
        ArchiveLocation = settings[nameof(ArchiveLocation)].ValueAs(ArchiveLocation);
        ReplicaLocation = settings[nameof(ReplicaLocation)].ValueAs(ReplicaLocation);
        ReplicationInterval = settings[nameof(ReplicationInterval)].ValueAs(m_replicationInterval);
    }

    /// <summary>
    /// Replicates the <see cref="IArchive"/>.
    /// </summary>
    /// <returns>true if the replication is successful; otherwise false.</returns>
    /// <exception cref="ArgumentNullException"><see cref="ArchiveLocation"/> or <see cref="ReplicaLocation"/> is null or empty string.</exception>
    public bool Replicate()
    {
        if (!Enabled || (m_replicationThread is not null && m_replicationThread.IsAlive))
            return false;

        if (string.IsNullOrEmpty(ArchiveLocation))
            throw new ArgumentNullException(nameof(ArchiveLocation));

        if (string.IsNullOrEmpty(ReplicaLocation))
            throw new ArgumentNullException(nameof(ReplicaLocation));

        m_replicationThread = new Thread(ReplicateInternal);
        m_replicationThread.Start();
        m_replicationThread.Join();

        return true;
    }

    /// <summary>
    /// Releases the unmanaged resources used by the replication provider and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        if (m_disposed)
            return;
        
        try
        {
            // This will be done regardless of whether the object is finalized or disposed.				
            if (!disposing)
                return;
            
            m_replicationThread?.Abort();

            if (m_replicationTimer is not null)
            {
                m_replicationTimer.Elapsed -= ReplicationTimer_Elapsed;
                m_replicationTimer.Dispose();
            }
        }
        finally
        {
            m_disposed = true;  // Prevent duplicate dispose.
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Raises the <see cref="ReplicationStart"/> event.
    /// </summary>
    protected virtual void OnReplicationStart()
    {
        ReplicationStart?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="ReplicationComplete"/> event.
    /// </summary>
    protected virtual void OnReplicationComplete()
    {
        ReplicationComplete?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="ReplicationException"/> event.
    /// </summary>
    /// <param name="ex"><see cref="Exception"/> to send to <see cref="ReplicationException"/> event.</param>
    protected virtual void OnReplicationException(Exception ex)
    {
        ReplicationException?.Invoke(this, new EventArgs<Exception>(ex));
    }

    /// <summary>
    /// Raises the <see cref="ReplicationProgress"/> event.
    /// </summary>
    /// <param name="replicationProgress"><see cref="ProcessProgress{T}"/> to send to <see cref="ReplicationProgress"/> event.</param>
    protected virtual void OnReplicationProgress(ProcessProgress<int> replicationProgress)
    {
        ReplicationProgress?.Invoke(this, new EventArgs<ProcessProgress<int>>(replicationProgress));
    }

    private void ReplicateInternal()
    {
        try
        {
            OnReplicationStart();
            ReplicateArchive();
            OnReplicationComplete();
        }
        catch (Exception ex)
        {
            OnReplicationException(ex);
        }
    }

    private void ReplicationTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
        Replicate();
    }

    #endregion
}