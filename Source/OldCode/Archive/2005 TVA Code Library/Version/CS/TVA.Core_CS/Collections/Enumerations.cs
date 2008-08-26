using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;

//*******************************************************************************************************
//  Enumerations.vb - Global enumerations for this namespace
//  Copyright © 2005 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  07/11/2006 - J. Ritchie Carroll
//       Moved all namespace level enumerations into "Enumerations.vb" file.
//  04/05/2007 - J. Ritchie Carroll
//       Added "RequeueMode" enumeration.
//  08/17/2007 - Darrell Zuercher
//       Edited code comments.
//
//*******************************************************************************************************

namespace TVA
{
	namespace Collections
	{
		
		/// <summary>Enumeration of possible queue threading modes.</summary>
		public enum QueueThreadingMode
		{
			/// <summary>Processes several items in the queue at once on different threads, where processing order is not
			/// important.</summary>
			Asynchronous,
			/// <summary>Processes items in the queue one at a time on a single thread, where processing order is important.</summary>
			Synchronous
		}
		
		/// <summary>Enumeration of possible queue processing styles.</summary>
		public enum QueueProcessingStyle
		{
			/// <summary>Defines queue processing delegate to process only one item at a time.</summary>
			/// <remarks>This is the typical processing style when the threading mode is asynchronous.</remarks>
			OneAtATime,
			/// <summary>Defines queue processing delegate to process all currently available items in the queue. Items are
			/// passed into delegate as an array.</summary>
			/// <remarks>This is the optimal processing style when the threading mode is synchronous.</remarks>
			ManyAtOnce
		}
		
		/// <summary>Enumeration of possible requeue modes.</summary>
		public enum RequeueMode
		{
			/// <summary>Requeues item at the beginning of the list.</summary>
			Prefix,
			/// <summary>Requeues item at the end of the list.</summary>
			Suffix
		}
		
	}
}
