//*******************************************************************************************************
//  ProcessProgressHandler.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/04/2008 - James R. Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;

namespace TVA
{
    /// <summary>
    /// Defines a delegate handler for a <see cref="TVA.ProcessProgress{TUnit}"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This handler is used by methods with an <see cref="Action"/> delegate parameter (e.g., Action&lt;ProcessProgress&lt;long&gt;&gt;)
    /// providing a simple callback mechanism for reporting progress on a long operation.
    /// </para>
    /// <para>
    /// Examples include:
    /// <see cref="TVA.IO.Compression.CompressionExtensions.Compress(System.IO.Stream,System.IO.Stream,TVA.IO.Compression.CompressionStrength,Action{TVA.ProcessProgress{long}})"/>, 
    /// <see cref="TVA.Security.Cryptography.Cipher.Encrypt(System.IO.Stream,System.IO.Stream,byte[],byte[],TVA.Security.Cryptography.CipherStrength,Action{TVA.ProcessProgress{long}})"/> and
    /// <see cref="TVA.Security.Cryptography.Cipher.Decrypt(System.IO.Stream,System.IO.Stream,byte[],byte[],TVA.Security.Cryptography.CipherStrength,Action{TVA.ProcessProgress{long}})"/>
    /// </para>
    /// </remarks>
    /// <typeparam name="TUnit">Unit of progress used (long, double, int, etc.)</typeparam>
    public class ProcessProgressHandler<TUnit> where TUnit : struct
    {
        #region [ Members ]

        // Fields
        private Action<ProcessProgress<TUnit>> m_progressHandler;
        private ProcessProgress<TUnit> m_progressInstance;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new process progress handler for the specified parameters.
        /// </summary>
        /// <param name="progressHandler">Delegate callback to invoke as process progresses.</param>
        /// <param name="processName">Descriptive name of process, if useful.</param>
        public ProcessProgressHandler(Action<ProcessProgress<TUnit>> progressHandler, string processName)
        {
            m_progressHandler = progressHandler;
            m_progressInstance = new ProcessProgress<TUnit>(processName);
        }

        /// <summary>
        /// Constructs a new process progress handler for the specified parameters.
        /// </summary>
        /// <param name="progressHandler">Delegate callback to invoke as process progresses.</param>
        /// <param name="processName">Descriptive name of process, if useful.</param>
        /// <param name="total">Total number of units to be processed.</param>
        public ProcessProgressHandler(Action<ProcessProgress<TUnit>> progressHandler, string processName, TUnit total)
            : this(progressHandler, processName)
        {
            m_progressInstance.Total = total;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets instance of <see cref="TVA.ProcessProgress{TUnit}"/> used to track progress for this handler.
        /// </summary>
        public ProcessProgress<TUnit> ProcessProgress
        {
            get
            {
                return m_progressInstance;
            }
        }

        /// <summary>
        /// Gets or sets reference to delegate handler used as a callback to report process progress.
        /// </summary>
        public Action<ProcessProgress<TUnit>> ProgressHandler
        {
            get
            {
                return m_progressHandler;
            }
            set
            {
                m_progressHandler = value;
            }
        }

        /// <summary>
        /// Gets or sets current process progress (i.e., number of units completed processing so far) - note that when this
        /// property value is assigned, the callback function is automatically called with updated <see cref="TVA.ProcessProgress{TUnit}"/>
        /// instance so consumer can track progress.
        /// </summary>
        /// <value>Number of units completed processing so far.</value>
        public TUnit Complete
        {
            get
            {
                return m_progressInstance.Complete;
            }
            set
            {
                UpdateProgress(value);
            }
        }

        /// <summary>
        /// Gets or sets total number of units to be processed.
        /// </summary>
        public TUnit Total
        {
            get
            {
                return m_progressInstance.Total;
            }
            set
            {
                m_progressInstance.Total = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Calls callback function with updated <see cref="TVA.ProcessProgress{TUnit}"/> instance so consumer can track progress.
        /// </summary>
        /// <param name="completed">Number of units completed processing so far.</param>
        /// <remarks>
        /// Note that assigning a value to the <see cref="Complete"/> property will have the same effect as calling this method.
        /// </remarks>
        public void UpdateProgress(TUnit completed)
        {
            // Update bytes completed
            m_progressInstance.Complete = completed;

            // Call user function
            m_progressHandler(m_progressInstance);
        }

        #endregion
    }
}
