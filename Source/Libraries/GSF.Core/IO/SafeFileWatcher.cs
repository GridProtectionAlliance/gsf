//******************************************************************************************************
//  SafeFileWatcher.cs - Gbtc
//
//  Copyright © 2015, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  07/28/2015 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security;

namespace GSF.IO
{
    /// <summary>
    /// Represents a wrapper around the native .NET <see cref="FileSystemWatcher"/> that avoids problems with
    /// dangling references when using a file watcher instance as a class member that never gets disposed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The design goal of the SafeFileWatcher is to avoid accidental memory leaks caused by use of .NET's native
    /// file system watcher when used as a member of a class that consumers fail to properly dispose. If a class
    /// has a reference to a file watcher as a member variable and attaches to the file watcher's events, the
    /// file watcher will maintain a reference the parent class so it can call its event handler. If the parent
    /// class is not disposed properly, the file watcher will thusly not be disposed and will maintain the
    /// reference to the parent class - the garbage collector will never collect the parent because it has a
    /// valid reference and no collection means the parent finalizer will never get called and the file system
    /// watcher will never get disposed. Creating multiple instances of parent class and not disposing of them
    /// will cause a memory leak even with a properly designed disposable pattern. Using the SafeFileWatcher
    /// instead of directly using the FileSystemWatcher will resolve this potential issue.
    /// </para>
    /// <para>
    /// Note that component model implementation is not fully replicated - if you are using a file system watcher
    /// on a design surface, this safety wrapper will usually not be needed. This class has benefit when a class
    /// will dynamically use a file watcher and needs to make sure any unmanaged resources get properly released
    /// even if a consumer neglects to call the dispose function.
    /// </para>
    /// </remarks>
    [SecurityCritical]
    public class SafeFileWatcher : IDisposable
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Occurs when a file or directory in the specified <see cref="SafeFileWatcher.Path"/> is changed.
        /// </summary>
        public event FileSystemEventHandler Changed;

        /// <summary>
        /// Occurs when a file or directory in the specified <see cref="SafeFileWatcher.Path"/> is created.
        /// </summary>
        public event FileSystemEventHandler Created;

        /// <summary>
        /// Occurs when a file or directory in the specified <see cref="SafeFileWatcher.Path"/> is deleted.
        /// </summary>
        public event FileSystemEventHandler Deleted;

        /// <summary>
        /// Occurs when a file or directory in the specified <see cref="SafeFileWatcher.Path"/> is renamed.
        /// </summary>
        public event RenamedEventHandler Renamed;

        /// <summary>
        /// Occurs when the internal buffer overflows.
        /// </summary>
        public event ErrorEventHandler Error;

        // Fields
        private readonly FileSystemWatcher m_fileSystemWatcher;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeFileWatcher"/> class.
        /// </summary>
        public SafeFileWatcher()
        {
            m_fileSystemWatcher = new FileSystemWatcher();
            InitializeFileSystemWatcher();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeFileWatcher"/> class, given the specified directory to monitor.
        /// </summary>
        /// <param name="path">The directory to monitor, in standard or Universal Naming Convention (UNC) notation.
        /// </param>
        /// <exception cref="ArgumentNullException">The <paramref name="path"/> parameter is null.</exception>
        /// <exception cref="ArgumentException">
        /// The <paramref name="path"/> parameter is an empty string (""). -or-
        /// The path specified through the <paramref name="path"/> parameter does not exist.
        /// </exception>
        public SafeFileWatcher(string path)
        {
            m_fileSystemWatcher = new FileSystemWatcher(path);
            InitializeFileSystemWatcher();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeFileWatcher"/> class, given the specified directory and type of files to monitor.
        /// </summary>
        /// <param name="path">The directory to monitor, in standard or Universal Naming Convention (UNC) notation.</param>
        /// <param name="filter">The type of files to watch. For example, "*.txt" watches for changes to all text files.</param>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="path"/> parameter is null. -or-
        /// The <paramref name="filter"/> parameter is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The <paramref name="path"/> parameter is an empty string (""). -or-
        /// The path specified through the <paramref name="path"/> parameter does not exist.
        /// </exception>
        public SafeFileWatcher(string path, string filter)
        {
            m_fileSystemWatcher = new FileSystemWatcher(path, filter);
            InitializeFileSystemWatcher();
        }

        // Attach to file system watcher events via lambda function using a weak reference to this class instance
        // connected through static method so that file watcher cannot hold onto a reference to this class - this
        // way even if consumer neglects to dispose this class, it will get properly garbage collected and finalized
        // because there will be no remaining references to this class instance. Also, even though the following
        // intermediate lambda classes that get created will be attached to the file system watcher event handlers,
        // they will also be freed because this class will make sure the file system watcher instance is handled
        // like an unmanaged resource, i.e., it always gets disposed, via the finalizer if need be.
        private void InitializeFileSystemWatcher()
        {
            WeakReference<SafeFileWatcher> reference = new WeakReference<SafeFileWatcher>(this);

            m_fileSystemWatcher.Changed += (sender, e) => OnChanged(reference, e);
            m_fileSystemWatcher.Created += (sender, e) => OnCreated(reference, e);
            m_fileSystemWatcher.Deleted += (sender, e) => OnDeleted(reference, e);
            m_fileSystemWatcher.Renamed += (sender, e) => OnRenamed(reference, e);
            m_fileSystemWatcher.Error += (sender, e) => OnError(reference, e);
        }

        /// <summary>
        /// Terminates <see cref="SafeFileWatcher"/> instance making sure to release unmanaged resources.
        /// </summary>
        ~SafeFileWatcher()
        {
            // Finalizer desired because we want to dispose FileSystemWatcher to force release of pinned buffers
            // and thus release reference to any associated event delegates allowing garbage collection
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the path of the directory to watch.
        /// </summary>
        /// <returns>
        /// The path to monitor. The default is an empty string ("").
        /// </returns>
        /// <exception cref="ArgumentException">
        /// The specified path does not exist or could not be found. -or-
        /// The specified path contains wildcard characters. -or-
        /// The specified path contains invalid path characters.
        /// </exception>
        public string Path
        {
            get
            {
                return m_fileSystemWatcher.Path;
            }
            set
            {
                m_fileSystemWatcher.Path = value;
            }
        }

        /// <summary>
        /// Gets or sets the filter string used to determine what files are monitored in a directory.
        /// </summary>
        /// <returns>
        /// The filter string. The default is "*.*" (Watches all files.) 
        /// </returns>
        public string Filter
        {
            get
            {
                return m_fileSystemWatcher.Filter;
            }
            set
            {
                m_fileSystemWatcher.Filter = value;
            }
        }

        /// <summary>
        /// Gets or sets the type of changes to watch for.
        /// </summary>
        /// <returns>
        /// One of the <see cref="NotifyFilters"/> values. The default is the bitwise OR combination of LastWrite, FileName, and DirectoryName.</returns>
        /// <exception cref="ArgumentException">The value is not a valid bitwise OR combination of the <see cref="NotifyFilters"/> values.</exception>
        /// <exception cref="InvalidEnumArgumentException">The value that is being set is not valid.</exception>
        public NotifyFilters NotifyFilter
        {
            get
            {
                return m_fileSystemWatcher.NotifyFilter;
            }
            set
            {
                m_fileSystemWatcher.NotifyFilter = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the component is enabled.
        /// </summary>
        /// <returns>
        /// true if the component is enabled; otherwise, false. The default is false. If you are using the component on a designer in Visual Studio 2005, the default is true.</returns>
        /// <exception cref="ObjectDisposedException">The <see cref="FileSystemWatcher"/> object has been disposed.</exception>
        /// <exception cref="PlatformNotSupportedException">The current operating system is not Microsoft Windows NT or later.</exception>
        /// <exception cref="FileNotFoundException">The directory specified in <see cref="SafeFileWatcher.Path"/> could not be found.</exception>
        /// <exception cref="ArgumentException"><see cref="SafeFileWatcher.Path"/> has not been set or is invalid.</exception>
        public bool EnableRaisingEvents
        {
            get
            {
                return m_fileSystemWatcher.EnableRaisingEvents;
            }
            set
            {
                m_fileSystemWatcher.EnableRaisingEvents = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether subdirectories within the specified path should be monitored.
        /// </summary>
        /// <returns>
        /// true if you want to monitor subdirectories; otherwise, false. The default is false.
        /// </returns>
        public bool IncludeSubdirectories
        {
            get
            {
                return m_fileSystemWatcher.IncludeSubdirectories;
            }
            set
            {
                m_fileSystemWatcher.IncludeSubdirectories = value;
            }
        }

        /// <summary>
        /// Gets or sets the size of the internal buffer.
        /// </summary>
        /// <returns>
        /// The internal buffer size. The default is 8192 (8K).
        /// </returns>
        public int InternalBufferSize
        {
            get
            {
                return m_fileSystemWatcher.InternalBufferSize;
            }
            set
            {
                m_fileSystemWatcher.InternalBufferSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the object used to marshal the event handler calls issued as a result of a directory change.
        /// </summary>
        /// <returns>
        /// The <see cref="ISynchronizeInvoke"/> that represents the object used to marshal the event handler calls issued as a result of a directory change. The default is null.
        /// </returns>
        public ISynchronizeInvoke SynchronizingObject
        {
            get
            {
                return m_fileSystemWatcher.SynchronizingObject;
            }
            set
            {
                m_fileSystemWatcher.SynchronizingObject = value;
            }
        }

        /// <summary>
        /// Gets or sets an <see cref="ISite"/> for the <see cref="SafeFileWatcher"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="ISite"/> for the <see cref="SafeFileWatcher"/>.
        /// </returns>
        public ISite Site
        {
            get
            {
                return m_fileSystemWatcher.Site;
            }
            set
            {
                m_fileSystemWatcher.Site = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="SafeFileWatcher"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="SafeFileWatcher"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "m_fileSystemWatcher")]
        protected virtual void Dispose(bool disposing)
        {
            if (m_disposed)
                return;

            try
            {
                // Treating file system watcher like an unmanaged resource
                m_fileSystemWatcher?.Dispose();
            }
            finally
            {
                m_disposed = true;
            }
        }

        /// <summary>
        /// A synchronous method that returns a structure that contains specific information on the change that occurred, given the type of change you want to monitor.
        /// </summary>
        /// <returns>A <see cref="WaitForChangedResult"/> that contains specific information on the change that occurred.</returns>
        /// <param name="changeType">The <see cref="WatcherChangeTypes"/> to watch for.</param>
        public WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType)
        {
            return m_fileSystemWatcher.WaitForChanged(changeType);
        }

        /// <summary>
        /// A synchronous method that returns a structure that contains specific information on the change that occurred, given the type of change you want to monitor and the time (in milliseconds) to wait before timing out.
        /// </summary>
        /// <returns>A <see cref="WaitForChangedResult"/> that contains specific information on the change that occurred.</returns>
        /// <param name="changeType">The <see cref="WatcherChangeTypes"/> to watch for.</param>
        /// <param name="timeout">The time (in milliseconds) to wait before timing out.</param>
        public WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType, int timeout)
        {
            return m_fileSystemWatcher.WaitForChanged(changeType, timeout);
        }

        /// <summary>
        /// Begins the initialization of a <see cref="SafeFileWatcher"/> used on a form or used by another component. The initialization occurs at run time.
        /// </summary>
        public void BeginInit()
        {
            m_fileSystemWatcher.BeginInit();
        }

        /// <summary>
        /// Ends the initialization of a <see cref="SafeFileWatcher"/> used on a form or used by another component. The initialization occurs at run time.
        /// </summary>
        public void EndInit()
        {
            m_fileSystemWatcher.EndInit();
        }

        private void OnChanged(FileSystemEventArgs e)
        {
            if ((object)Changed != null)
                Changed(this, e);
        }

        private void OnCreated(FileSystemEventArgs e)
        {
            if ((object)Created != null)
                Created(this, e);
        }

        private void OnDeleted(FileSystemEventArgs e)
        {
            if ((object)Deleted != null)
                Deleted(this, e);
        }

        private void OnRenamed(RenamedEventArgs e)
        {
            if ((object)Renamed != null)
                Renamed(this, e);
        }

        private void OnError(ErrorEventArgs e)
        {
            if ((object)Error != null)
                Error(this, e);
        }

        #endregion

        #region [ Static ]

        // Static Methods
        private static void OnChanged(WeakReference<SafeFileWatcher> reference, FileSystemEventArgs e)
        {
            SafeFileWatcher instance;

            if (reference.TryGetTarget(out instance))
                instance.OnChanged(e);
        }

        private static void OnCreated(WeakReference<SafeFileWatcher> reference, FileSystemEventArgs e)
        {
            SafeFileWatcher instance;

            if (reference.TryGetTarget(out instance))
                instance.OnCreated(e);
        }

        private static void OnDeleted(WeakReference<SafeFileWatcher> reference, FileSystemEventArgs e)
        {
            SafeFileWatcher instance;

            if (reference.TryGetTarget(out instance))
                instance.OnDeleted(e);
        }

        private static void OnRenamed(WeakReference<SafeFileWatcher> reference, RenamedEventArgs e)
        {
            SafeFileWatcher instance;

            if (reference.TryGetTarget(out instance))
                instance.OnRenamed(e);
        }

        private static void OnError(WeakReference<SafeFileWatcher> reference, ErrorEventArgs e)
        {
            SafeFileWatcher instance;

            if (reference.TryGetTarget(out instance))
                instance.OnError(e);
        }

        #endregion
    }
}
