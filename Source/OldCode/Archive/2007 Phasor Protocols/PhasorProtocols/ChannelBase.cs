//*******************************************************************************************************
//  ChannelBase.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  3/7/2005 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using PCS.Parsing;

namespace PCS.PhasorProtocols
{
    /// <summary>
    /// This base class represents the common implementation of the protocol independent definition of any kind of data that can be parsed or generated.
    /// </summary>
    /// <remarks>
    /// This is the root class of the phasor protocol library.<br/>
    /// This base class represents <see cref="IChannel"/> data images for parsing or generation in terms of a header, body and footer (see <see cref="BinaryImageBase"/> for details).
    /// </remarks>
    [Serializable()]
    public abstract class ChannelBase : BinaryImageBase, IChannel
    {
        #region [ Members ]

        // Fields        
        private IChannelParsingState m_state;               // Current parsing state
        private Dictionary<string, string> m_attributes;    // Attributes dictionary
        private object m_tag;                               // User defined tag

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the final derived type of class implementing <see cref="IChannel"/>.
        /// </summary>
        /// <remarks>
        /// This is expected to be overriden by the final derived class.
        /// </remarks>
        public abstract Type DerivedType { get; }

        /// <summary>
        /// Gets or sets the parsing state for the <see cref="IChannel"/> object.
        /// </summary>
        public virtual IChannelParsingState State
        {
            get
            {
                return m_state;
            }
            set
            {
                m_state = value;
            }
        }

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="IChannel"/> object.
        /// </summary>
        /// <remarks>
        /// The attributes dictionary is relevant to all channel properties.  This dictionary will only be instantiated with a call to
        /// the <c>Attributes</c> property which will begin the enumeration of relevant system properties.  This is typically used for
        /// display purposes. For example, this information is displayed in a tree view on the the <b>PMU Connection Tester</b> to display
        /// attributes of data elements that may be protocol specific.
        /// </remarks>
        public virtual Dictionary<string, string> Attributes
        {
            get
            {
                // Create a new attributes dictionary or clear the contents of any existing one
                if (m_attributes == null)
                    m_attributes = new Dictionary<string, string>();
                else
                    m_attributes.Clear();

                m_attributes.Add("Derived Type", DerivedType.FullName);
                m_attributes.Add("Binary Length", BinaryLength.ToString());

                return m_attributes;
            }
        }

        /// <summary>
        /// User definable object used to hold a reference associated with the <see cref="IChannel"/> object.
        /// </summary>
        public virtual object Tag
        {
            get
            {
                return m_tag;
            }
            set
            {
                m_tag = value;
            }
        }

        #endregion
    }
}