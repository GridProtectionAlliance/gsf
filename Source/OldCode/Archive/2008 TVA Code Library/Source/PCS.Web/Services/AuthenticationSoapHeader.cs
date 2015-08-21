using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Xml.Serialization;
using System.Web.Services.Protocols;
using PCS.Security.Application;

//*******************************************************************************************************
//  PCS.Web.Services.AuthenticationSoapHeader.vb - Soap header used for authentication
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [PCS]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/24/2007 - Shyni John
//       Original version of source code generated
//
//*******************************************************************************************************


namespace PCS.Web
{
	namespace Services
	{
		
		[SerializableAttribute(), XmlTypeAttribute(Namespace = "http://troweb/DataServices/"), XmlRootAttribute(Namespace = "http://troweb/DataServices/", IsNullable = false)]public class AuthenticationSoapHeader : SoapHeader
		{
			
			
			
			private string m_userName;
			private string m_password;
			private SecurityServer m_securityServer;
			private bool m_passThroughAuthentication;
			
			public AuthenticationSoapHeader This
			{
				get
				{
					return this;
				}
			}
			
			public string UserName
			{
				get
				{
					return m_userName;
				}
				set
				{
					m_userName = value;
				}
			}
			
			public string Password
			{
				get
				{
					return m_password;
				}
				set
				{
					m_password = value;
				}
			}
			
			public SecurityServer Server
			{
				get
				{
					return m_securityServer;
				}
				set
				{
					m_securityServer = value;
				}
			}
			
			public bool PassThroughAuthentication
			{
				get
				{
					return m_passThroughAuthentication;
				}
				set
				{
					m_passThroughAuthentication = value;
				}
			}
			
		}
		
	}
	
}
