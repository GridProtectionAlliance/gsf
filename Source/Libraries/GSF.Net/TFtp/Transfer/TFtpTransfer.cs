//******************************************************************************************************
//  TFtpTransfer.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/12/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

#region [ Contributor License Agreements ]

//*******************************************************************************************************
//
//   Code based on the following project:
//        https://github.com/Callisto82/tftp.net
//  
//   Copyright © 2011, Michael Baer
//
//*******************************************************************************************************

#endregion

using System;
using System.IO;
using System.Net;
using System.Threading;
using GSF.Diagnostics;
using GSF.Net.TFtp.Channel;
using GSF.Net.TFtp.Commands;
using GSF.Net.TFtp.Trace;
using GSF.Net.TFtp.Transfer.States;

// ReSharper disable InconsistentNaming
// ReSharper disable VirtualMemberCallInConstructor
namespace GSF.Net.TFtp.Transfer
{
    internal class TFtpTransfer : ITFtpTransfer
    {
        protected ITransferState State;
        protected readonly ITransferChannel Connection;
        protected Timer Timer;

        public TransferOptionSet ProposedOptions { get; set; }
        public TransferOptionSet NegotiatedOptions { get; private set; }
        public bool WasStarted { get; private set; }
        public Stream InputOutputStream { get; protected set; }

        public TFtpTransfer(ITransferChannel connection, string filename, ITransferState initialState)
        {
            ProposedOptions = TransferOptionSet.NewDefaultSet();
            Filename = filename;
            RetryCount = 5;
            SetState(initialState);
            Connection = connection;
            Connection.OnCommandReceived += Connection_OnCommandReceived;
            Connection.OnError += Connection_OnError;
            Connection.Open();
            Timer = new Timer(Timer_OnTimer, null, 500, 500);
        }

        private void Timer_OnTimer(object context)
        {
            lock (this)
            {
                try
                {
                    State.OnTimer();
                }
                catch (Exception ex)
                {
                    Logger.SwallowException(ex, "Failed to execute TFTP state timer");
                }
            }
        }

        private void Connection_OnCommandReceived(ITFtpCommand command, EndPoint endpoint)
        {
            lock (this)
            {
                State.OnCommand(command, endpoint);
            }
        }

        private void Connection_OnError(TFtpTransferError error)
        {
            lock (this)
            {
                RaiseOnError(error);
            }
        }

        internal virtual void SetState(ITransferState newState)
        {
            lock (this)
            {
                State = DecorateForLogging(newState);
                State.Context = this;
                State.OnStateEnter();
            }
        }

        protected virtual ITransferState DecorateForLogging(ITransferState state)
        {
            return TFtpTrace.Enabled ? new LoggingStateDecorator(state, this) : state;
        }

        internal ITransferChannel GetConnection()
        {
            return Connection;
        }

        internal void RaiseOnProgress(long bytesTransferred)
        {
            OnProgress?.Invoke(this, new TFtpTransferProgress(bytesTransferred, ExpectedSize));
        }

        internal void RaiseOnError(TFtpTransferError error)
        {
            OnError?.Invoke(this, error);
        }

        internal void RaiseOnFinished()
        {
            OnFinished?.Invoke(this);
        }

        internal void FinishOptionNegotiation(TransferOptionSet negotiated)
        {
            NegotiatedOptions = negotiated;

            if (!NegotiatedOptions.IncludesBlockSizeOption)
                NegotiatedOptions.BlockSize = TransferOptionSet.DefaultBlocksize;

            if (!NegotiatedOptions.IncludesTimeoutOption)
                NegotiatedOptions.Timeout = TransferOptionSet.DefaultTimeoutSecs;
        }

        public override string ToString()
        {
            return $"{GetHashCode()} ({Filename})";
        }

        internal void FillOrDisableTransferSizeOption()
        {
            try
            {
                ProposedOptions.TransferSize = (int)InputOutputStream.Length;
            }
            catch (NotSupportedException)
            {
            }
            finally
            {
                if (ProposedOptions.TransferSize <= 0)
                    ProposedOptions.IncludesTransferSizeOption = false;
            }
        }

        #region ITFtpTransfer

        public event TFtpProgressHandler OnProgress;
        public event TFtpEventHandler OnFinished;
        public event TFtpErrorHandler OnError;

        public string Filename { get; }
        public int RetryCount { get; set; }
        public virtual TFtpTransferMode TransferMode { get; set; }
        public object UserContext { get; set; }

        public virtual TimeSpan RetryTimeout 
        {
            get => TimeSpan.FromSeconds(NegotiatedOptions?.Timeout ?? ProposedOptions.Timeout);
            set { ThrowExceptionIfTransferAlreadyStarted(); ProposedOptions.Timeout = value.Seconds; }
        }

        public virtual long ExpectedSize 
        {
            get => NegotiatedOptions?.TransferSize ?? ProposedOptions.TransferSize;
            set { ThrowExceptionIfTransferAlreadyStarted(); ProposedOptions.TransferSize = value; }
        }

        public virtual int BlockSize 
        {
            get => NegotiatedOptions?.BlockSize ?? ProposedOptions.BlockSize;
            set { ThrowExceptionIfTransferAlreadyStarted(); ProposedOptions.BlockSize = value; }
        }

        private BlockCounterWrapAround m_wrapping = BlockCounterWrapAround.ToZero;
        
        public virtual BlockCounterWrapAround BlockCounterWrapping
        {
            get => m_wrapping;
            set { ThrowExceptionIfTransferAlreadyStarted(); m_wrapping = value; }
        }

        private void ThrowExceptionIfTransferAlreadyStarted()
        {
            if (WasStarted)
                throw new InvalidOperationException("You cannot change TFTP transfer options after the transfer has been started.");
        }

        public void Start(Stream data)
        {
            if (WasStarted)
                throw new InvalidOperationException("This transfer has already been started.");

            WasStarted = true;
            InputOutputStream = data ?? throw new ArgumentNullException(nameof(data));

            lock (this)
            {
                State.OnStart();
            }
        }

        public void Cancel(TFtpErrorPacket reason)
        {
            if (reason == null)
                throw new ArgumentNullException(nameof(reason));

            lock (this)
            {
                State.OnCancel(reason);
            }
        }

        public virtual void Dispose()
        {
            lock (this)
            {
                Timer.Dispose();
                Cancel(new TFtpErrorPacket(0, "ITFtpTransfer has been disposed."));

                if (InputOutputStream != null)
                {
                    InputOutputStream.Close();
                    InputOutputStream = null;
                }

                Connection.Dispose();
            }
        }

        #endregion
    }
}
