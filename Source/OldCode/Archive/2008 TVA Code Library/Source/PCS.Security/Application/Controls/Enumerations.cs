using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;

//*******************************************************************************************************
//  PCS.Security.Application.Controls.Enumerations.vb - Common enumerations used by security controls
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [PCS]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  04/30/2008 - Pinal C. Patel
//       Original version of source code generated.
//
//*******************************************************************************************************

namespace PCS.Security
{
	namespace Application
	{
		namespace Controls
		{
			
			
			/// <summary>
			/// Specifies the type of message that is to be displayed inside of the container control.
			/// </summary>
			public enum MessageType
			{
				/// <summary>
				/// Information message.
				/// </summary>
				Information,
				/// <summary>
				/// Warning message.
				/// </summary>
				Warning,
				/// <summary>
				/// Error message.
				/// </summary>
				@Error
			}
			
		}
	}
}
