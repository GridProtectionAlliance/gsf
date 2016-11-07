//******************************************************************************************************
//  CachedFileStream.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
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
//  12/04/2014 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;

namespace GSF.IO
{
    /// <summary>
    /// Represents a file stream that caches recently used blocks in memory.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class was developed as a wrapper around <see cref="System.IO.FileStream"/> with the same
    /// basic functionality. This stream treats the file as a collection of contiguous blocks which can
    /// be loaded into an in-memory lookup table to greatly improve seek times. This class generally
    /// performs significantly better than the standard FileStream class when maintaining multiple
    /// file pointers and frequently seeking back and forth between them. For an example of a good
    /// use-case, consider copying a list of serialized objects from one location in the file to another
    /// without loading the whole list into memory.
    /// </para>
    /// 
    /// <para>
    /// This class likely will not perform significantly better or worse than the standard FileStream
    /// when performing strictly sequential reads or writes. For an example of a bad use-case, consider
    /// reading a list of objects into memory or simply appending new data to the end of the file. In
    /// both of these cases, it may be better to use the standard FileStream for less memory overhead
    /// and more predictable flush behavior.
    /// </para>
    /// 
    /// <para>
    /// Blocks are loaded into memory as they are used (for read/write operations) and will be kept in
    /// memory for as long as possible. Blocks can only be flushed from the cache by decreasing the
    /// cache size or by accessing a non-cached block when the cache is already full. In these cases,
    /// a least recently used algorithm is executed to determine which blocks should be removed from
    /// the cache either to decrease the size of the cache or to make room for a new block.
    /// </para>
    /// 
    /// <para>
    /// Write operations are also cached such that they will not be committed to the file until the
    /// block is removed from the cache or a call to <see cref="Flush()"/> has been made. This can
    /// have some additional implications as compared to the standard FileStream. For instance,
    /// write operations may not be committed to the file in the order in which they were executed.
    /// Users of this class will need to be judicious about when and how often they call Flush.
    /// </para>
    /// </remarks>
    public class CachedFileStream : Stream
    {
        #region [ Members ]

        // Nested Types

        private class Block
        {
            #region [ Members ]

            // Fields
            private long m_blockIndex;
            private byte[] m_buffer;
            private int m_refCount;
            private bool m_isDirty;

            #endregion

            #region [ Constructors ]

            public Block(long blockIndex, byte[] buffer)
            {
                m_blockIndex = blockIndex;
                m_buffer = buffer;
                m_refCount = 0;
            }

            #endregion

            #region [ Properties ]

            public long BlockIndex
            {
                get
                {
                    return m_blockIndex;
                }
            }

            public byte[] Buffer
            {
                get
                {
                    return m_buffer;
                }
            }

            public int RefCount
            {
                get
                {
                    return m_refCount;
                }
                set
                {
                    m_refCount = value;
                }
            }

            public bool IsDirty
            {
                get
                {
                    return m_isDirty;
                }
                set
                {
                    m_isDirty = value;
                }
            }

            #endregion
        }

        // Constants

        /// <summary>
        /// Default value for the <see cref="BlockSize"/> property.
        /// </summary>
        public const int DefaultBlockSize = 4096;

        /// <summary>
        /// Default value for the <see cref="CacheSize"/> property.
        /// </summary>
        public const long DefaultCacheSize = 262144L;

        // Fields
        private int m_blockSize;
        private long m_cacheSize;
        private long m_position;
        private long m_length;

        private FileStream m_fileStream;
        private Dictionary<long, Block> m_blockLookup;
        private Dictionary<long, Block> m_dirtyBlockLookup;
        private List<Block> m_queue;

        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.IO.FileStream"/> class with the specified path, creation mode, read/write and sharing permission, the access other FileStreams can have to the same file, the buffer size, and additional file options.
        /// </summary>
        /// <param name="path">A relative or absolute path for the file that the current FileStream object will encapsulate.</param>
        /// <param name="mode">A constant that determines how to open or create the file.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="path"/> is null.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="path"/> is an empty string (""), contains only white space, or contains one or more invalid characters. -or-<paramref name="path"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an NTFS environment.</exception>
        /// <exception cref="T:System.NotSupportedException"><paramref name="path"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in a non-NTFS environment.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="mode"/> contains an invalid value.</exception>
        /// <exception cref="T:System.IO.FileNotFoundException">The file cannot be found, such as when <paramref name="mode"/> is FileMode.Truncate or FileMode.Open, and the file specified by <paramref name="path"/> does not exist. The file must already exist in these modes.</exception>
        /// <exception cref="T:System.IO.IOException">An I/O error, such as specifying FileMode.CreateNew when the file specified by <paramref name="path"/> already exists, occurred.-or-The stream has been closed.</exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive.</exception>
        /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.</exception>
        public CachedFileStream(string path, FileMode mode)
            : this(path, mode, (mode == FileMode.Append) ? FileAccess.Write : FileAccess.ReadWrite)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.IO.FileStream"/> class with the specified path, creation mode, read/write and sharing permission, the access other FileStreams can have to the same file, the buffer size, and additional file options.
        /// </summary>
        /// <param name="path">A relative or absolute path for the file that the current FileStream object will encapsulate.</param>
        /// <param name="mode">A constant that determines how to open or create the file.</param>
        /// <param name="access">A constant that determines how the file can be accessed by the FileStream object. This gets the <see cref="P:System.IO.FileStream.CanRead"/> and <see cref="P:System.IO.FileStream.CanWrite"/> properties of the FileStream object. <see cref="P:System.IO.FileStream.CanSeek"/> is true if <paramref name="path"/> specifies a disk file.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="path"/> is null.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="path"/> is an empty string (""), contains only white space, or contains one or more invalid characters. -or-<paramref name="path"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an NTFS environment.</exception>
        /// <exception cref="T:System.NotSupportedException"><paramref name="path"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in a non-NTFS environment.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="mode"/> or <paramref name="access"/> contain an invalid value.</exception>
        /// <exception cref="T:System.IO.FileNotFoundException">The file cannot be found, such as when <paramref name="mode"/> is FileMode.Truncate or FileMode.Open, and the file specified by <paramref name="path"/> does not exist. The file must already exist in these modes.</exception>
        /// <exception cref="T:System.IO.IOException">An I/O error, such as specifying FileMode.CreateNew when the file specified by <paramref name="path"/> already exists, occurred.-or-The stream has been closed.</exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive.</exception>
        /// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="access"/> requested is not permitted by the operating system for the specified <paramref name="path"/>, such as when <paramref name="access"/> is Write or ReadWrite and the file or directory is set for read-only access.</exception>
        /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.</exception>
        public CachedFileStream(string path, FileMode mode, FileAccess access)
            : this(path, mode, access, FileShare.Read)
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.IO.FileStream"/> class with the specified path, creation mode, read/write and sharing permission, the access other FileStreams can have to the same file, the buffer size, and additional file options.
        /// </summary>
        /// <param name="path">A relative or absolute path for the file that the current FileStream object will encapsulate.</param>
        /// <param name="mode">A constant that determines how to open or create the file.</param>
        /// <param name="access">A constant that determines how the file can be accessed by the FileStream object. This gets the <see cref="P:System.IO.FileStream.CanRead"/> and <see cref="P:System.IO.FileStream.CanWrite"/> properties of the FileStream object. <see cref="P:System.IO.FileStream.CanSeek"/> is true if <paramref name="path"/> specifies a disk file.</param>
        /// <param name="share">A constant that determines how the file will be shared by processes.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="path"/> is null.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="path"/> is an empty string (""), contains only white space, or contains one or more invalid characters. -or-<paramref name="path"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an NTFS environment.</exception>
        /// <exception cref="T:System.NotSupportedException"><paramref name="path"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in a non-NTFS environment.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="mode"/>, <paramref name="access"/>, or <paramref name="share"/> contain an invalid value.</exception>
        /// <exception cref="T:System.IO.FileNotFoundException">The file cannot be found, such as when <paramref name="mode"/> is FileMode.Truncate or FileMode.Open, and the file specified by <paramref name="path"/> does not exist. The file must already exist in these modes.</exception>
        /// <exception cref="T:System.IO.IOException">An I/O error, such as specifying FileMode.CreateNew when the file specified by <paramref name="path"/> already exists, occurred.-or-The stream has been closed.</exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive.</exception>
        /// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="access"/> requested is not permitted by the operating system for the specified <paramref name="path"/>, such as when <paramref name="access"/> is Write or ReadWrite and the file or directory is set for read-only access.</exception>
        /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.</exception>
        public CachedFileStream(string path, FileMode mode, FileAccess access, FileShare share)
            : this(path, mode, access, share, DefaultBlockSize)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.IO.FileStream"/> class with the specified path, creation mode, read/write and sharing permission, the access other FileStreams can have to the same file, the buffer size, and additional file options.
        /// </summary>
        /// <param name="path">A relative or absolute path for the file that the current FileStream object will encapsulate.</param>
        /// <param name="mode">A constant that determines how to open or create the file.</param>
        /// <param name="access">A constant that determines how the file can be accessed by the FileStream object. This gets the <see cref="P:System.IO.FileStream.CanRead"/> and <see cref="P:System.IO.FileStream.CanWrite"/> properties of the FileStream object. <see cref="P:System.IO.FileStream.CanSeek"/> is true if <paramref name="path"/> specifies a disk file.</param>
        /// <param name="share">A constant that determines how the file will be shared by processes.</param>
        /// <param name="blockSize">A positive <see cref="T:System.Int32"/> value greater than 0 indicating the block size. For <paramref name="blockSize"/> values between one and eight, the actual block size is set to eight bytes.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="path"/> is null.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="path"/> is an empty string (""), contains only white space, or contains one or more invalid characters. -or-<paramref name="path"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an NTFS environment.</exception>
        /// <exception cref="T:System.NotSupportedException"><paramref name="path"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in a non-NTFS environment.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="blockSize"/> is negative or zero.-or- <paramref name="mode"/>, <paramref name="access"/>, or <paramref name="share"/> contain an invalid value.</exception>
        /// <exception cref="T:System.IO.FileNotFoundException">The file cannot be found, such as when <paramref name="mode"/> is FileMode.Truncate or FileMode.Open, and the file specified by <paramref name="path"/> does not exist. The file must already exist in these modes.</exception>
        /// <exception cref="T:System.IO.IOException">An I/O error, such as specifying FileMode.CreateNew when the file specified by <paramref name="path"/> already exists, occurred.-or-The stream has been closed.</exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive.</exception>
        /// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="access"/> requested is not permitted by the operating system for the specified <paramref name="path"/>, such as when <paramref name="access"/> is Write or ReadWrite and the file or directory is set for read-only access.</exception>
        /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.</exception>
        public CachedFileStream(string path, FileMode mode, FileAccess access, FileShare share, int blockSize)
            : this(path, mode, access, share, blockSize, FileOptions.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.IO.FileStream"/> class with the specified path, creation mode, read/write and sharing permission, the access other FileStreams can have to the same file, the buffer size, and additional file options.
        /// </summary>
        /// <param name="path">A relative or absolute path for the file that the current FileStream object will encapsulate.</param>
        /// <param name="mode">A constant that determines how to open or create the file.</param>
        /// <param name="access">A constant that determines how the file can be accessed by the FileStream object. This gets the <see cref="P:System.IO.FileStream.CanRead"/> and <see cref="P:System.IO.FileStream.CanWrite"/> properties of the FileStream object. <see cref="P:System.IO.FileStream.CanSeek"/> is true if <paramref name="path"/> specifies a disk file.</param>
        /// <param name="share">A constant that determines how the file will be shared by processes.</param>
        /// <param name="blockSize">A positive <see cref="T:System.Int32"/> value greater than 0 indicating the block size. For <paramref name="blockSize"/> values between one and eight, the actual block size is set to eight bytes.</param>
        /// <param name="useAsync">Specifies whether to use asynchronous I/O or synchronous I/O. However, note that the underlying operating system might not support asynchronous I/O, so when specifying true, the handle might be opened synchronously depending on the platform. When opened asynchronously, the <see cref="M:System.IO.FileStream.BeginRead(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)"/> and <see cref="M:System.IO.FileStream.BeginWrite(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)"/> methods perform better on large reads or writes, but they might be much slower for small reads or writes. If the application is designed to take advantage of asynchronous I/O, set the <paramref name="useAsync"/> parameter to true. Using asynchronous I/O correctly can speed up applications by as much as a factor of 10, but using it without redesigning the application for asynchronous I/O can decrease performance by as much as a factor of 10.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="path"/> is null.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="path"/> is an empty string (""), contains only white space, or contains one or more invalid characters. -or-<paramref name="path"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an NTFS environment.</exception>
        /// <exception cref="T:System.NotSupportedException"><paramref name="path"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in a non-NTFS environment.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="blockSize"/> is negative or zero.-or- <paramref name="mode"/>, <paramref name="access"/>, or <paramref name="share"/> contain an invalid value.</exception>
        /// <exception cref="T:System.IO.FileNotFoundException">The file cannot be found, such as when <paramref name="mode"/> is FileMode.Truncate or FileMode.Open, and the file specified by <paramref name="path"/> does not exist. The file must already exist in these modes.</exception>
        /// <exception cref="T:System.IO.IOException">An I/O error, such as specifying FileMode.CreateNew when the file specified by <paramref name="path"/> already exists, occurred.-or-The stream has been closed.</exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive.</exception>
        /// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="access"/> requested is not permitted by the operating system for the specified <paramref name="path"/>, such as when <paramref name="access"/> is Write or ReadWrite and the file or directory is set for read-only access.</exception>
        /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.</exception>
        public CachedFileStream(string path, FileMode mode, FileAccess access, FileShare share, int blockSize, bool useAsync)
            : this(path, mode, access, share, blockSize, useAsync ? FileOptions.Asynchronous : FileOptions.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.IO.FileStream"/> class with the specified path, creation mode, read/write and sharing permission, the access other FileStreams can have to the same file, the buffer size, and additional file options.
        /// </summary>
        /// <param name="path">A relative or absolute path for the file that the current FileStream object will encapsulate.</param>
        /// <param name="mode">A constant that determines how to open or create the file.</param>
        /// <param name="access">A constant that determines how the file can be accessed by the FileStream object. This gets the <see cref="P:System.IO.FileStream.CanRead"/> and <see cref="P:System.IO.FileStream.CanWrite"/> properties of the FileStream object. <see cref="P:System.IO.FileStream.CanSeek"/> is true if <paramref name="path"/> specifies a disk file.</param>
        /// <param name="share">A constant that determines how the file will be shared by processes.</param>
        /// <param name="blockSize">A positive <see cref="T:System.Int32"/> value greater than 0 indicating the block size. For <paramref name="blockSize"/> values between one and eight, the actual block size is set to eight bytes.</param>
        /// <param name="options">A value that specifies additional file options.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="path"/> is null.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="path"/> is an empty string (""), contains only white space, or contains one or more invalid characters. -or-<paramref name="path"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an NTFS environment.</exception>
        /// <exception cref="T:System.NotSupportedException"><paramref name="path"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in a non-NTFS environment.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="blockSize"/> is negative or zero.-or- <paramref name="mode"/>, <paramref name="access"/>, or <paramref name="share"/> contain an invalid value.</exception>
        /// <exception cref="T:System.IO.FileNotFoundException">The file cannot be found, such as when <paramref name="mode"/> is FileMode.Truncate or FileMode.Open, and the file specified by <paramref name="path"/> does not exist. The file must already exist in these modes.</exception>
        /// <exception cref="T:System.IO.IOException">An I/O error, such as specifying FileMode.CreateNew when the file specified by <paramref name="path"/> already exists, occurred.-or-The stream has been closed.</exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive.</exception>
        /// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="access"/> requested is not permitted by the operating system for the specified <paramref name="path"/>, such as when <paramref name="access"/> is Write or ReadWrite and the file or directory is set for read-only access. -or-<see cref="F:System.IO.FileOptions.Encrypted"/> is specified for <paramref name="options"/>, but file encryption is not supported on the current platform.</exception>
        /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.</exception>
        public CachedFileStream(string path, FileMode mode, FileAccess access, FileShare share, int blockSize, FileOptions options)
            : this(blockSize)
        {
            m_fileStream = new FileStream(path, mode, access, share, blockSize, options);
            m_length = m_fileStream.Length;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.IO.FileStream"/> class with the specified path, creation mode, access rights and sharing permission, the buffer size, and additional file options.
        /// </summary>
        /// <param name="path">A relative or absolute path for the file that the current <see cref="T:System.IO.FileStream"/> object will encapsulate.</param>
        /// <param name="mode">A constant that determines how to open or create the file.</param>
        /// <param name="rights">A constant that determines the access rights to use when creating access and audit rules for the file.</param>
        /// <param name="share">A constant that determines how the file will be shared by processes.</param>
        /// <param name="blockSize">A positive <see cref="T:System.Int32"/> value greater than 0 indicating the block size. For <paramref name="blockSize"/> values between one and eight, the actual block size is set to eight bytes.</param>
        /// <param name="options">A constant that specifies additional file options.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="path"/> is null.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="path"/> is an empty string (""), contains only white space, or contains one or more invalid characters. -or-<paramref name="path"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an NTFS environment.</exception>
        /// <exception cref="T:System.NotSupportedException"><paramref name="path"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in a non-NTFS environment.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="blockSize"/> is negative or zero.-or- <paramref name="mode"/>, access, or <paramref name="share"/> contain an invalid value.</exception>
        /// <exception cref="T:System.IO.FileNotFoundException">The file cannot be found, such as when <paramref name="mode"/> is FileMode.Truncate or FileMode.Open, and the file specified by <paramref name="path"/> does not exist. The file must already exist in these modes.</exception>
        /// <exception cref="T:System.PlatformNotSupportedException">The current operating system is not Windows NT or later.</exception>
        /// <exception cref="T:System.IO.IOException">An I/O error, such as specifying FileMode.CreateNew when the file specified by <paramref name="path"/> already exists, occurred. -or-The stream has been closed.</exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive.</exception>
        /// <exception cref="T:System.UnauthorizedAccessException">The access requested is not permitted by the operating system for the specified <paramref name="path"/>, such as when access is Write or ReadWrite and the file or directory is set for read-only access. -or-<see cref="F:System.IO.FileOptions.Encrypted"/> is specified for <paramref name="options"/>, but file encryption is not supported on the current platform.</exception>
        /// <exception cref="T:System.IO.PathTooLongException">The specified <paramref name="path"/>, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.</exception>
        public CachedFileStream(string path, FileMode mode, FileSystemRights rights, FileShare share, int blockSize, FileOptions options)
            : this(blockSize)
        {
            m_fileStream = new FileStream(path, mode, rights, share, blockSize, options);
            m_length = m_fileStream.Length;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.IO.FileStream"/> class with the specified path, creation mode, access rights and sharing permission, the buffer size, additional file options, access control and audit security.
        /// </summary>
        /// <param name="path">A relative or absolute path for the file that the current <see cref="T:System.IO.FileStream"/> object will encapsulate.</param>
        /// <param name="mode">A constant that determines how to open or create the file.</param>
        /// <param name="rights">A constant that determines the access rights to use when creating access and audit rules for the file.</param>
        /// <param name="share">A constant that determines how the file will be shared by processes.</param>
        /// <param name="blockSize">A positive <see cref="T:System.Int32"/> value greater than 0 indicating the block size. For <paramref name="blockSize"/> values between one and eight, the actual block size is set to eight bytes.</param>
        /// <param name="options">A constant that specifies additional file options.</param>
        /// <param name="fileSecurity">A constant that determines the access control and audit security for the file.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="path"/> is null.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="path"/> is an empty string (""), contains only white space, or contains one or more invalid characters. -or-<paramref name="path"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an NTFS environment.</exception><exception cref="T:System.NotSupportedException"><paramref name="path"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in a non-NTFS environment.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="blockSize"/> is negative or zero.-or- <paramref name="mode"/>, access, or <paramref name="share"/> contain an invalid value.</exception>
        /// <exception cref="T:System.IO.FileNotFoundException">The file cannot be found, such as when <paramref name="mode"/> is FileMode.Truncate or FileMode.Open, and the file specified by <paramref name="path"/> does not exist. The file must already exist in these modes.</exception>
        /// <exception cref="T:System.IO.IOException">An I/O error, such as specifying FileMode.CreateNew when the file specified by <paramref name="path"/> already exists, occurred. -or-The stream has been closed.</exception><exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception><exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive.</exception>
        /// <exception cref="T:System.UnauthorizedAccessException">The access requested is not permitted by the operating system for the specified <paramref name="path"/>, such as when access is Write or ReadWrite and the file or directory is set for read-only access. -or-<see cref="F:System.IO.FileOptions.Encrypted"/> is specified for <paramref name="options"/>, but file encryption is not supported on the current platform.</exception><exception cref="T:System.IO.PathTooLongException">The specified <paramref name="path"/>, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.</exception>
        /// <exception cref="T:System.PlatformNotSupportedException">The current operating system is not Windows NT or later.</exception>
        public CachedFileStream(string path, FileMode mode, FileSystemRights rights, FileShare share, int blockSize, FileOptions options, FileSecurity fileSecurity)
            : this(blockSize)
        {
            m_fileStream = new FileStream(path, mode, rights, share, blockSize, options, fileSecurity);
            m_length = m_fileStream.Length;
        }

        private CachedFileStream(int blockSize)
        {
            if (blockSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(blockSize));

            if (blockSize < 8)
                blockSize = 8;

            m_blockSize = blockSize;
            m_cacheSize = DefaultCacheSize;

            m_blockLookup = new Dictionary<long, Block>();
            m_dirtyBlockLookup = new Dictionary<long, Block>();
            m_queue = new List<Block>();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the size of the cached blocks.
        /// </summary>
        public int BlockSize
        {
            get
            {
                return m_blockSize;
            }
        }

        /// <summary>
        /// Gets or sets the maximum size of the cache.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Cache size is set to a value less than zero.</exception>
        public long CacheSize
        {
            get
            {
                return m_cacheSize;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                m_cacheSize = value;
                PurgeCache(m_cacheSize);
            }
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <returns>
        /// true if the stream supports reading; otherwise, false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override bool CanRead
        {
            get
            {
                return m_fileStream.CanRead;
            }
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <returns>
        /// true if the stream supports writing; otherwise, false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override bool CanWrite
        {
            get
            {
                return m_fileStream.CanWrite;
            }
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <returns>
        /// true if the stream supports seeking; otherwise, false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override bool CanSeek
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// When overridden in a derived class, gets or sets the position within the current stream.
        /// </summary>
        /// <returns>
        /// The current position within the stream.
        /// </returns>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed.</exception>
        /// <filterpriority>1</filterpriority>
        public override long Position
        {
            get
            {
                if (m_disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return m_position;
            }
            set
            {
                if (m_disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                m_position = value;
            }
        }

        /// <summary>
        /// When overridden in a derived class, gets the length in bytes of the stream.
        /// </summary>
        /// <returns>
        /// A long value representing the length of the stream in bytes.
        /// </returns>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed.</exception>
        /// <filterpriority>1</filterpriority>
        public override long Length
        {
            get
            {
                if (m_disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return m_length;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// When overridden in a derived class, sets the position within the current stream.
        /// </summary>
        /// <returns>
        /// The new position within the current stream.
        /// </returns>
        /// <param name="offset">A byte offset relative to the <paramref name="origin"/> parameter.</param>
        /// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin"/> indicating the reference point used to obtain the new position.</param>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed.</exception>
        /// <filterpriority>1</filterpriority>
        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;

                case SeekOrigin.Current:
                    Position += offset;
                    break;

                case SeekOrigin.End:
                    Position = m_fileStream.Length + offset;
                    break;
            }

            return m_position;
        }

        /// <summary>
        /// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset"/> and (<paramref name="offset"/> + <paramref name="count"/> - 1) replaced by the bytes read from the current source. </param><param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin storing the data read from the current stream. </param><param name="count">The maximum number of bytes to be read from the current stream. </param>
        /// <returns>
        /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
        /// </returns>
        /// <exception cref="T:System.ArgumentException"><paramref name="offset"/> and <paramref name="count"/> describe an invalid range in <paramref name="buffer"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative.</exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support reading.</exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed.</exception>
        /// <filterpriority>1</filterpriority>
        public override int Read(byte[] buffer, int offset, int count)
        {
            Block block;
            int blockOffset;
            int blockCount;

            int bufferOffset;
            int totalCount;
            long end;

            if (m_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (!CanRead)
                throw new NotSupportedException("Stream does not support reading.");

            if ((object)buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            if (offset + count > buffer.Length)
                throw new ArgumentException("Amount to be read exceeds the size of the buffer.");

            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (count == 0 || m_position >= Length)
                return 0;

            bufferOffset = offset;
            end = Math.Min(m_position + count, Length);
            totalCount = (int)(end - m_position);

            while (m_position < end)
            {
                block = GetBlock(m_position / m_blockSize);
                blockOffset = (int)(m_position % m_blockSize);
                blockCount = Math.Min((int)(end - m_position), m_blockSize - blockOffset);

                Buffer.BlockCopy(block.Buffer, blockOffset, buffer, bufferOffset, blockCount);

                m_position += blockCount;
                bufferOffset += blockCount;
            }

            return totalCount;
        }

        /// <summary>
        /// When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count"/> bytes from <paramref name="buffer"/> to the current stream. </param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin copying bytes to the current stream. </param>
        /// <param name="count">The number of bytes to be written to the current stream. </param>
        /// <exception cref="T:System.ArgumentException"><paramref name="offset"/> and <paramref name="count"/> describe an invalid range in <paramref name="buffer"/>.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative.</exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support writing.</exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed.</exception>
        /// <filterpriority>1</filterpriority>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (m_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (!CanWrite)
                throw new NotSupportedException("Stream does not support writing.");

            if ((object)buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            if (offset + count > buffer.Length)
                throw new ArgumentException("Amount to be read exceeds the size of the buffer.");

            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            int bufferOffset = offset;
            long end = m_position + count;

            if (end > m_length)
                m_length = end;

            while (m_position < end)
            {
                Block block = GetBlock(m_position / m_blockSize);
                int blockOffset = (int)(m_position % m_blockSize);
                int blockCount = Math.Min((int)(end - m_position), m_blockSize - blockOffset);

                Buffer.BlockCopy(buffer, bufferOffset, block.Buffer, blockOffset, blockCount);
                m_dirtyBlockLookup[block.BlockIndex] = block;
                block.IsDirty = true;

                m_position += blockCount;
                bufferOffset += blockCount;
            }
        }

        /// <summary>
        /// Clears buffers for this stream and causes any buffered data to be written to the file.
        /// </summary>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
        /// <filterpriority>2</filterpriority>
        public override void Flush()
        {
            Flush(false);
        }

        /// <summary>
        /// Clears buffers for this stream and causes any buffered data to be written to the file,
        /// and also clears all intermediate file buffers.
        /// </summary>
        /// <param name="flushToDisk">true to flush all intermediate file buffers; otherwise, false.</param>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
        /// <filterpriority>2</filterpriority>
        public void Flush(bool flushToDisk)
        {
            foreach (Block block in m_dirtyBlockLookup.Values)
            {
                WriteToFileStream(block);
                block.IsDirty = false;
            }

            m_dirtyBlockLookup.Clear();
            m_fileStream.Flush(flushToDisk);
        }

        /// <summary>
        /// When overridden in a derived class, sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">Attempted to set the <paramref name="value"/> parameter to less than 0.</exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support both writing and seeking.</exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed.</exception>
        /// <filterpriority>2</filterpriority>
        public override void SetLength(long value)
        {
            if (m_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (!CanWrite)
                throw new NotSupportedException("The stream does not support writing.");

            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));

            // When decreasing the file size, purge the cache
            // of any blocks beyond the new length of the file
            if (value < m_length)
            {
                m_length = value;

                // Decreasing the file size may cause some cached
                // blocks to fall outside the bounds of the file
                // so clean the queue to remove those blocks
                CleanQueue();
            }
            else
            {
                m_length = value;
            }

            m_fileStream.SetLength(value);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.IO.Stream"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!m_disposed)
                {
                    if ((object)m_fileStream != null)
                    {
                        Flush();
                        m_fileStream.Dispose();
                        m_fileStream = null;
                    }
                }
            }
            finally
            {
                m_disposed = true;
                base.Dispose(disposing);
            }
        }

        private Block GetBlock(long blockIndex)
        {
            Block block;
            byte[] buffer;

            // If the block is cached, add a reference to
            // the block, check to see if the queue should
            // be cleaned, then return the block
            if (m_blockLookup.TryGetValue(blockIndex, out block))
            {
                AddRef(block);

                if (m_queue.Count > 2L * m_cacheSize / m_blockSize)
                    CleanQueue();

                return block;
            }

            // Make room in the cache for the new block
            PurgeCache(m_cacheSize - m_blockSize);

            // Allocate a new buffer for the block
            buffer = new byte[m_blockSize];

            if (blockIndex * m_blockSize < m_fileStream.Length)
            {
                // Read the block from the file
                m_fileStream.Seek(blockIndex * m_blockSize, SeekOrigin.Begin);
                m_fileStream.Read(buffer, 0, m_blockSize);
            }

            // Add the block to the cache
            block = new Block(blockIndex, buffer);
            m_blockLookup.Add(blockIndex, block);
            AddRef(block);

            // Return the block that was cached
            return block;
        }

        private void PurgeCache(long cacheSize)
        {
            Block block;
            int i = 0;

            // If the cache is full, remove the least recently used block
            while (m_blockLookup.Count * m_blockSize > cacheSize)
            {
                while (i < m_queue.Count)
                {
                    // Go through the queue of referenced blocks
                    // and remove references until one of the
                    // blocks has reached zero references
                    block = m_queue[i];
                    RemoveRef(block);
                    i++;

                    if (block.RefCount == 0)
                    {
                        // If block is dirty, write it to the file
                        // and remove it from the dirty block lookup
                        if (block.IsDirty)
                        {
                            WriteToFileStream(block);
                            m_dirtyBlockLookup.Remove(block.BlockIndex);
                            block.IsDirty = false;
                        }

                        // Remove the fully dereferenced block from the cache
                        m_blockLookup.Remove(block.BlockIndex);

                        // Break out of the inner loop to check
                        // the outer loop's condition again
                        break;
                    }
                }
            }

            // Remove dereferenced blocks from the queue
            m_queue.RemoveRange(0, i);
        }

        private void CleanQueue()
        {
            int count;

            Block block;
            long bytesToEOF;

            // Get the size of the
            // queue before cleaning
            count = m_queue.Count;

            // Go through all existing blocks in the queue
            for (int i = 0; i < count; i++)
            {
                // Remove a reference from
                // the current block
                block = m_queue[i];
                RemoveRef(block);

                // If the reference count is zero take some
                // action to either dump or preserve the block
                if (block.RefCount == 0)
                {
                    // Get the number of bytes from the start of the block to the end of the file
                    bytesToEOF = m_length - (block.BlockIndex * m_blockSize);

                    if (bytesToEOF > 0)
                    {
                        // This block is still at least partially contained
                        // in the file, so add it back to the end of the queue
                        AddRef(block);
                        
                        // If any of the bytes in the block exceed the
                        // end of the file, make sure to clear them
                        if (bytesToEOF < m_blockSize)
                            Array.Clear(block.Buffer, (int)bytesToEOF, m_blockSize - (int)bytesToEOF);
                    }
                    else
                    {
                        // This block entirely exceeds the
                        // length of the file so remove it
                        m_blockLookup.Remove(block.BlockIndex);
                        m_dirtyBlockLookup.Remove(block.BlockIndex);
                    }
                }
            }

            // Remove dereferenced blocks from the queue
            m_queue.RemoveRange(0, count);
        }

        private void WriteToFileStream(Block block)
        {
            long blockPointer = block.BlockIndex * m_blockSize;
            m_fileStream.Seek(blockPointer, SeekOrigin.Begin);
            m_fileStream.Write(block.Buffer, 0, Math.Min((int)(m_length - blockPointer), m_blockSize));
        }

        private void AddRef(Block block)
        {
            if (m_queue.LastOrDefault() != block)
            {
                m_queue.Add(block);
                block.RefCount++;
            }
        }

        private void RemoveRef(Block block)
        {
            block.RefCount--;
        }

        #endregion
    }
}
