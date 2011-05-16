using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Collections.ObjectModel;

namespace TimeSeriesFramework.UI
{
    /// <summary>
    /// Represents a subscriber authentication request.
    /// </summary>
    public class AuthenticationRequest
    {

        #region [ Members ]

        private Guid m_ID;
        private string m_acronym;
        private string m_name;
        private string m_sharedSecret;
        private string m_authKey;
        private string m_validIpAddresses;
        private bool m_enabled;
        private string m_key;
        private string m_iv;

        #endregion

        #region [ Properties ]

        [XmlAttribute]
        public Guid ID
        {
            get
            {
                return m_ID;
            }
            set
            {
                m_ID = value;
            }
        }

        [XmlAttribute]
        public string Acronym
        {
            get
            {
                return m_acronym;
            }
            set
            {
                m_acronym = value;
            }
        }

        [XmlAttribute]
        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
            }
        }

        [XmlAttribute]
        public string SharedSecret
        {
            get
            {
                return m_sharedSecret;
            }
            set
            {
                m_sharedSecret = value;
            }
        }

        [XmlAttribute]
        public string AuthKey
        {
            get
            {
                return m_authKey;
            }
            set
            {
                m_authKey = value;
            }
        }

        [XmlAttribute]
        public string ValidIPAddresses
        {
            get
            {
                return m_validIpAddresses;
            }
            set
            {
                m_validIpAddresses = value;
            }
        }

        [XmlAttribute]
        public bool Enabled
        {
            get
            {
                return m_enabled;
            }
            set
            {
                m_enabled = value;
            }
        }

        [XmlAttribute]
        public string Key
        {
            get
            {
                return m_key;
            }
            set
            {
                m_key = value;
            }
        }

        [XmlAttribute]
        public string IV
        {
            get
            {
                return m_iv;
            }
            set
            {
                m_iv = value;
            }
        }

        #endregion

    }
}
