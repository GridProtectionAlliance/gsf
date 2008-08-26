using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;

//*******************************************************************************************************
//  TVA.Threading.Enumerations.vb - Threading Enumerations
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/25/2008 - J. Ritchie Carroll
//       Initial version of source generated.
//
//*******************************************************************************************************

namespace TVA
{
	namespace Threading
	{
		
		/// <summary>
		/// Managed Thread Types
		/// </summary>
		public enum ThreadType
		{
			/// <summary>Standard thread created with public constructor</summary>
			StandardThread,
			/// <summary>Queued thread added into managed thread pool</summary>
			QueuedThread
		}
		
		/// <summary>
		/// Managed Thread States
		/// </summary>
		public enum ThreadStatus
		{
			/// <summary>Thread created, not started</summary>
			Unstarted,
			/// <summary>Thread queued for execution</summary>
			Queued,
			/// <summary>Thread start requested, execution pending</summary>
			Started,
			/// <summary>Thread executing</summary>
			Executing,
			/// <summary>Thread completed</summary>
			Completed,
			/// <summary>Thread aborted</summary>
			Aborted
		}
		
	}
	
}
