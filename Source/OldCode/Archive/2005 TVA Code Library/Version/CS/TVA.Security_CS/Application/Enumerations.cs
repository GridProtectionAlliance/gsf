using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;

//*******************************************************************************************************
//  TVA.Security.Application.Enumerations.vb - Common enumerations used for application security
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  09/22/2006 - Pinal C. Patel
//       Original version of source code generated.
//  04/22/2008 - Pinal C. Patel
//       Added AuthenticationMode enumeration.
//
//*******************************************************************************************************

namespace TVA.Security
{
	namespace Application
	{
		
		/// <summary>
		/// Specifies the server to be used for authentication.
		/// </summary>
		public enum SecurityServer
		{
			/// <summary>
			/// Use the development server.
			/// </summary>
			Development,
			/// <summary>
			/// Use the acceptance server.
			/// </summary>
			Acceptance,
			/// <summary>
			/// Use the production server.
			/// </summary>
			Production
		}
		
		/// <summary>
		/// Specifies the control property to be set if the current user is in a specified role.
		/// </summary>
		public enum ValidRoleAction
		{
			/// <summary>
			/// No control property is be set.
			/// </summary>
			None,
			/// <summary>
			/// Control's Visible property is to be set.
			/// </summary>
			Visible,
			/// <summary>
			/// Control's Enabled property is to be set.
			/// </summary>
			Enabled
		}
		
		/// <summary>
		/// Specifies the mode of authentication.
		/// </summary>
		public enum AuthenticationMode
		{
			/// <summary>
			/// Internal users are authenticated against the Active Directory and external users are
			/// authenticated against credentials stored in the security database.
			/// </summary>
			AD,
			/// <summary>
			/// Users are authenticated against the RSA Security server.
			/// </summary>
			RSA
		}
		
	}
	
}
