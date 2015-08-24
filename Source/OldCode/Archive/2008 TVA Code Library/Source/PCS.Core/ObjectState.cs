//*******************************************************************************************************
//  ObjectState.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  05/23/2007 - Pinal C. Patel
//      Generated original version of source code.
//  09/09/2008 - J. Ritchie Carroll
//      Converted to C#.
//  11/05/2008 - Pinal C. Patel
//      Edited code comments.
//
//*******************************************************************************************************

using System;

namespace PCS
{
    /// <summary>
    /// A serializable class that can be used to track the current and previous state of an object.
    /// </summary>
    /// <typeparam name="TState">Type of the state to track.</typeparam>
    [Serializable()]
    public class ObjectState<TState>
    {
        #region [ Members ]

        // Fields
        private string m_objectName;
        private TState m_currentState;
        private TState m_previousState;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectState{TState}"/> class.
        /// </summary>
        /// <param name="objectName">The text label for the object whose state is being tracked.</param>
        public ObjectState(string objectName)
            : this(objectName, default(TState))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectState{TState}"/> class.
        /// </summary>
        /// <param name="objectName">The text label for the object whose state is being tracked.</param>
        /// <param name="currentState">The current state of the object.</param>
        public ObjectState(string objectName, TState currentState)
            : this(objectName, currentState, default(TState))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectState{TState}"/> class.
        /// </summary>
        /// <param name="objectName">The text label for the object whose state is being tracked.</param>
        /// <param name="currentState">The current state of the object.</param>
        /// <param name="previousState">The previous state of the object.</param>
        public ObjectState(string objectName, TState currentState, TState previousState)
        {
            this.ObjectName = objectName;
            this.CurrentState = currentState;
            this.PreviousState = previousState;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a text label for the object whose state is being tracked.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being set is a null or empty string.</exception>
        public string ObjectName
        {
            get
            {
                return m_objectName;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException();

                m_objectName = value;
            }
        }

        /// <summary>
        /// Gets or sets the current state of the object.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the previous state of the object.
        /// </summary>
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