//*******************************************************************************************************
//  TransportProvider.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C Patel
//      Office: INFO SVCS APP DEV, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/12/2008 - Pinal C Patel
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Threading;

namespace PCS.Communication
{
    /// <summary>
    /// A class for managing the communication between server and client.
    /// </summary>
    /// <typeparam name="T"><see cref="Type"/> of the object used for server-client communication.</typeparam>
    public class TransportProvider<T>
    {
        #region [ Members ]

        // Fields

        /// <summary>
        /// ID of the <see cref="TransportProvider{T}"/> object.
        /// </summary>
        public Guid ID;

        /// <summary>
        /// Provider for the transportation of data.
        /// </summary>
        public T Provider;

        /// <summary>
        /// Passphrase used for encryption of data.
        /// </summary>
        public string Passphrase;

        /// <summary>
        /// Buffer used for sending data.
        /// </summary>
        public byte[] SendBuffer;

        /// <summary>
        /// Zero-based index of <see cref="SendBuffer"/> from which data is to be sent.
        /// </summary>
        public int SendBufferOffset;

        /// <summary>
        /// Number of bytes to be sent from <see cref="SendBuffer"/> starting at <see cref="SendBufferOffset"/>.
        /// </summary>
        public int SendBufferLength;

        /// <summary>
        /// Buffer used for receiving data.
        /// </summary>
        public byte[] ReceiveBuffer;

        /// <summary>
        /// Zero-based index of <see cref="ReceiveBuffer"/> at which data is to be received.
        /// </summary>
        public int ReceiveBufferOffset;

        /// <summary>
        /// Number of bytes received in <see cref="ReceiveBuffer"/> starting at <see cref="ReceiveBufferOffset"/>.
        /// </summary>
        public int ReceiveBufferLength;

        /// <summary>
        /// <see cref="TransportStatistics"/> for the <see cref="TransportProvider{T}"/> object.
        /// </summary>
        public TransportStatistics Statistics;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="TransportProvider{T}"/> class.
        /// </summary>
        public TransportProvider()
        {
            ID = Guid.NewGuid();
            Statistics = new TransportStatistics();
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Resets <see cref="TransportProvider{T}"/>.
        /// </summary>
        public void Reset()
        {
            Passphrase = string.Empty;
            SendBuffer = null;
            SendBufferOffset = 0;
            SendBufferLength = -1;
            ReceiveBuffer = null;
            ReceiveBufferOffset = 0;
            ReceiveBufferLength = -1;

            // Reset the statistics.
            Statistics.Reset();

            // Cleanup the provider.
            try
            {
                ((IDisposable)Provider).Dispose();
            }
            catch
            {
                // Ignore encountered exception during dispose.
            }
            finally
            {
                Provider = default(T);
            }    
        }

        /// <summary>
        /// Asynchronously waits for an asynchronous operation to complete within the <paramref name="timeout"/> period and 
        /// invokes the <paramref name="asyncCallback"/> with the <paramref name="asyncResult"/> if the operation times out.
        /// </summary>
        /// <param name="timeout">The number of milliseconds to wait for the asynchronous operation to complete.</param>
        /// <param name="asyncCallback">The <see cref="AsyncCallback"/> of the asynchronous operation being monitored.</param>
        /// <param name="asyncResult">The <see cref="IAsyncResult"/> of the asynchronous operation being monitored.</param>
        public void WaitAsync(int timeout, AsyncCallback asyncCallback, IAsyncResult asyncResult)
        {
            ThreadPool.RegisterWaitForSingleObject(asyncResult.AsyncWaitHandle, 
                                                   WaitAsyncCallback, 
                                                   new object[] { asyncCallback, asyncResult }, 
                                                   timeout, 
                                                   true);
        }

        private void WaitAsyncCallback(object state, bool timedout)
        {
            if (timedout)
            {
                // The async operation timed-out, so we invoke the specified callback.
                object[] data = (object[])state;
                ((AsyncCallback)data[0])((IAsyncResult)data[1]);
            }
        }

        #endregion
    }
}
