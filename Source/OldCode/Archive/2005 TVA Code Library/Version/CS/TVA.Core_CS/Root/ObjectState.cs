//*******************************************************************************************************
//  ObjectState.vb - Generic object state class
//  Copyright © 2005 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  05/23/2007 - Pinal C. Patel
//      Generated original version of source code.
//  09/09/2008 - J. Ritchie Carroll
//      Converted to C#.
//
//*******************************************************************************************************

using System;

namespace TVA
{
    /// <summary>
    /// Generic object state class.
    /// </summary>
    /// <remarks>
    /// Tracks current and previous states of a labeled object.
    /// </remarks>
    /// <typeparam name="TState">Object state to track.</typeparam>
    [Serializable()]
    public class ObjectState<TState>
    {
        #region " Member Declaration "

        private string m_objectName;
        private TState m_currentState;
        private TState m_previousState;

        #endregion

        #region " Code Scope: Public "

        public ObjectState(string objectName) : this(objectName, default(TState)) {}

        public ObjectState(string objectName, TState currentState) : this(objectName, default(TState), currentState) {}

        public ObjectState(string objectName, TState previousState, TState currentState)
        {
            m_objectName = objectName;
            m_currentState = currentState;
            m_previousState = previousState;
        }

        public string ObjectName
        {
            get
            {
                return m_objectName;
            }
            set
            {
                m_objectName = value;
            }
        }

        public TState CurrentState
        {
            get
            {
                return m_currentState;
            }
            set
            {
                m_currentState = value;
            }
        }

        public TState PreviousState
        {
            get
            {
                return m_previousState;
            }
            set
            {
                m_previousState = value;
            }
        }

        #endregion
    }
}