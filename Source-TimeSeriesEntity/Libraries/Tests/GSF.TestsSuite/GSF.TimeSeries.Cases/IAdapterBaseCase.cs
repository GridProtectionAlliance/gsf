#region [ Using ]
using System;
using GSF.TestsSuite.TimeSeries.Cases;
using GSF.TestsSuite.TimeSeries.Wrappers;
#endregion

namespace GSF.TestsSuite.TimeSeries.Cases
{
    public class IAdapterBaseCase : AdapterBaseWrapper
    {
        #region [ Members ]
        private IAllAdaptersCase m_IAdapterCase;
        private IWaitHandlesCase m_IWaitHandlesCase;
        #endregion

        public IAdapterBaseCase()
            : base()
        {
            // init
            m_IWaitHandlesCase = new IWaitHandlesCase();
            m_IAdapterCase = new IAllAdaptersCase();
     
            //base.pExternalEventHandles = m_IWaitHandlesCase.AutoResetEvents;
            //base.AssignParentCollection(m_IAdapterCase.OutputAdapterCollection);
            base.InitializationTimeout = 1;
            base.Initialize();
        }

        /*
        #region [ Dispose ]
        /// <summary>
        /// Disposed function
        /// </summary>
        private bool isDisposed = false;

        /// <summary>
        /// Wait Handles Case
        /// </summary>
        ~IAdapterBaseCase()
        {
            Dispose(false);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="isDisposing">Dispose</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (!isDisposed)
            {
                if (isDisposing)
                {
                    m_IAdapterCase.Dispose();
                    m_IWaitHandlesCase.Dispose();
                }
                isDisposed = false;
            }
        }

        #endregion
         */
    }
}