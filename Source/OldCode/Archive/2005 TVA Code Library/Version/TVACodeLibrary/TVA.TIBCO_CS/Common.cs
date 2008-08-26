using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using Microsoft.Web.Services3.Security.Tokens;

//*******************************************************************************************************
//  TVA.Tibco.Common.vb - Common global Tibco functions
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/30/2007 - Pinal C. Patel
//       Original version of source code generated
//
//*******************************************************************************************************


namespace TVA.TIBCO
{
	public sealed class Common
	{
		
		
		private Common()
		{
			
			// This class contains only global functions and is not meant to be instantiated
			
		}
		
		public static UsernameToken GetUserNameToken()
		{
			
			return new UsernameToken("esocss", "pwd4ctrl", PasswordOption.SendHashed);
			
		}
		
	}
	
}
