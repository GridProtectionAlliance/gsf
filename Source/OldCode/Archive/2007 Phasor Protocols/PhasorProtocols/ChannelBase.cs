//*******************************************************************************************************
//  ChannelBase.vb - Channel data base class
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2008
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  3/7/2005 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using TVA;
using TVA.Parsing;

namespace PhasorProtocols
{
    /// <summary>This class represents the common implementation of the protocol independent definition of any kind of data.</summary>
    [CLSCompliant(false), Serializable()]
    public abstract class ChannelBase : IChannel
    {
        // This is the attributes dictionary relevant to all channel properties.  This dictionary
        // will only be instantiated with a call to "Attributes" property which will begin the
        // enumeration of relevant system properties.  This is typically used for display purposes.
        // For example, this information is displayed in a tree view on the the PMU Connection
        // Tester to display attributes of data elements that may be protocol specific
        private Dictionary<string, string> m_attributes;
        private object m_tag;

        // This is expected to be overriden by the final derived class
        public abstract Type DerivedType
        {
            get;
        }

        // This property is not typically overriden
        public virtual ushort BinaryLength
        {
            get
            {
                return (ushort)(HeaderLength + BodyLength + FooterLength);
            }
        }

        int IBinaryDataProvider.BinaryLength
        {
            get
            {
                return (int)BinaryLength;
            }
        }

        // This property is not typically overriden
        public virtual byte[] BinaryImage
        {
            get
            {
                byte[] buffer = new byte[BinaryLength];
                int index = 0;

                // Copy in header, body and footer images
                Common.CopyImage(HeaderImage, buffer, ref index, HeaderLength);
                Common.CopyImage(BodyImage, buffer, ref index, BodyLength);
                Common.CopyImage(FooterImage, buffer, ref index, FooterLength);

                return buffer;
            }
        }

        // This property is not typically overriden
        virtual public void ParseBinaryImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
        {
            // Parse out header, body and footer images
            ParseHeaderImage(state, binaryImage, startIndex);
            startIndex += HeaderLength;

            ParseBodyImage(state, binaryImage, startIndex);
            startIndex += BodyLength;

            ParseFooterImage(state, binaryImage, startIndex);
        }

        protected virtual void ParseHeaderImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
        {
        }

        protected virtual void ParseBodyImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
        {
        }

        protected virtual void ParseFooterImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
        {
        }

        protected virtual ushort HeaderLength
        {
            get
            {
                return 0;
            }
        }

        protected virtual byte[] HeaderImage
        {
            get
            {
                return null;
            }
        }

        protected virtual ushort BodyLength
        {
            get
            {
                return 0;
            }
        }

        protected virtual byte[] BodyImage
        {
            get
            {
                return null;
            }
        }

        protected virtual ushort FooterLength
        {
            get
            {
                return 0;
            }
        }

        protected virtual byte[] FooterImage
        {
            get
            {
                return null;
            }
        }

        public virtual Dictionary<string, string> Attributes
        {
            get
            {
                // Create a new attributes dictionary or clear the contents of any existing one
                if (m_attributes == null)
                {
                    m_attributes = new Dictionary<string, string>();
                }
                else
                {
                    m_attributes.Clear();
                }

                m_attributes.Add("Derived Type", DerivedType.FullName);
                m_attributes.Add("Binary Length", BinaryLength.ToString());

                return m_attributes;
            }
        }

        /// <summary>User definable tag used to hold a reference associated with channel data</summary>
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
    }
}
