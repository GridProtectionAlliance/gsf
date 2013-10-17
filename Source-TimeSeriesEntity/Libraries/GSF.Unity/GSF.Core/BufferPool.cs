//******************************************************************************************************
//  BufferPool.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  10/27/2011 - J. Ritchie Carroll
//       Initial version of source generated.
//  10/8/2012 - Danyelle Gilliam
//        Modified Header
//
//******************************************************************************************************




#region [ Contributor License Agreements ]

//******************************************************************************************************
//
//  Copyright © 2011, Grid Protection Alliance.  All Rights Reserved.
//
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//******************************************************************************************************

#endregion

using System;
using System.Collections.Concurrent;
//using System.ServiceModel.Channels;

namespace GSF
{
    /// <summary>
    /// Represents a common buffer pool that can be used by an application.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The buffer pool is statically created at application startup and is available for all components and classes
    /// within an application domain. Every time you need to use a buffer, you take one from the pool, use it, and
    /// return it to the pool when done. This process is much faster than creating and destroying a buffer every
    /// time you need to use one.
    /// </para>
    /// <para>
    /// It is very imporant to return the buffer to the pool when you are finished using it. If you are using a buffer
    /// scoped within a method, make sure to use a try/finally so that you can take the buffer within the try and
    /// return the buffer within the finally. If you are using a buffer as a member scoped class field, make sure
    /// you use the standard dispose pattern and return the buffer in the <see cref="IDisposable.Dispose"/> method.
    /// </para>
    /// </remarks>
    public static class BufferPool
    {
        // Note that the buffer manager will create an queue buffers as needed during run-time, the default maximum
        // pool size and default maximum buffer sizes are set to int max. Even if an application should max the pool
        // size, the buffer manager will still sucessfully provide and manage buffers - they simply won't be cached.
        private static ConcurrentDictionary<int, ConcurrentQueue<byte[]>> m_bufferPools = new ConcurrentDictionary<int, ConcurrentQueue<byte[]>>();
  
        private static int GetStandardBufferSize(int bufferSize)
        {
            int exponent = 10;
            long size = (long)Math.Pow(2, exponent);
            
            while (size <= int.MaxValue)
            {
                if (bufferSize <= size)
                    return (int)size;
                
                exponent++;
                size = (long)Math.Pow(2, exponent);
            }
            
            return int.MaxValue;
        }
        
        private static ConcurrentQueue<byte[]> CreateQueue(int bufferSize)
        {
            return new ConcurrentQueue<byte[]>();        
        }
        
        /// <summary>
        /// Gets a buffer of at least the specified size from the pool.
        /// </summary>
        /// <param name="bufferSize">The size, in bytes, of the requested buffer.</param>
        /// <returns>A byte array that is the requested size of the buffer.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="bufferSize"/> cannot be less than zero.</exception>
        public static byte[] TakeBuffer(int bufferSize)
        {
            bufferSize = GetStandardBufferSize(bufferSize);            
            ConcurrentQueue<byte[]> pool = m_bufferPools.GetOrAdd(bufferSize, CreateQueue);
            byte[] buffer;

            // Attempt to provide user with a queued buffer
            if (!pool.TryDequeue(out buffer))
            {
                // No buffers are available, create a new buffer for the pool
                buffer = new byte[bufferSize];
            }

            return buffer;
        }

        /// <summary>
        /// Returns a buffer to the pool.
        /// </summary>
        /// <param name="buffer">A reference to the buffer being returned.</param>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> reference cannot be null.</exception>
        /// <exception cref="ArgumentException">Length of <paramref name="buffer"/> does not match the pool's buffer length property.</exception>
        public static void ReturnBuffer(byte[] buffer)
        {
            int bufferSize = buffer.Length;
            
            if (bufferSize != GetStandardBufferSize(bufferSize))
                throw new InvalidOperationException("Attempt was amde to return an invalid buffer to buffer pool.");
            
            ConcurrentQueue<byte[]> pool = m_bufferPools[bufferSize];
               
            if ((object)buffer != null)
                pool.Enqueue(buffer);        
        }

        /// <summary>
        /// Releases all the buffers currently cached in the pool.
        /// </summary>
        public static void Clear()
        {
            m_bufferPools.Clear();
        }
    }
}