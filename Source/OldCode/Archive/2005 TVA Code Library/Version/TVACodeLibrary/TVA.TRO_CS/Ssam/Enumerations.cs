using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.ComponentModel;

// 08-24-06


namespace TVA.TRO
{
	namespace Ssam
	{
		
		/// <summary>
		/// The SSAM server with which we want to connect.
		/// </summary>
		/// <remarks></remarks>
		public enum SsamServer
		{
			/// <summary>
			/// The development SSAM server.
			/// </summary>
			/// <remarks></remarks>
			Development,
			[EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]Acceptance,
			/// <summary>
			/// The production SSAM server.
			/// </summary>
			/// <remarks></remarks>
			Production
		}
		
		/// <summary>
		/// The current state of the connection with the SSAM server.
		/// </summary>
		/// <remarks></remarks>
		public enum SsamConnectionState
		{
			/// <summary>
			/// Connection with the SSAM server is closed.
			/// </summary>
			/// <remarks></remarks>
			Closed,
			/// <summary>
			/// Connection with the SSAM server is open and some activity is in progress.
			/// </summary>
			/// <remarks></remarks>
			OpenAndActive,
			/// <summary>
			/// Connection with the SSAM server is open, but no activity is in progress.
			/// </summary>
			/// <remarks></remarks>
			OpenAndInactive
		}
		
		/// <summary>
		/// Specifies the type of entity to which the event belongs.
		/// </summary>
		/// <remarks></remarks>
		public enum SsamEntityType
		{
			/// <summary>
			/// This entity type represents a data-flow.
			/// </summary>
			/// <remarks></remarks>
			Flow = 1,
			/// <summary>
			/// This entity type represents a piece of equipment.
			/// </summary>
			/// <remarks></remarks>
			Equipment = 2,
			/// <summary>
			/// This entity type represents a Process.
			/// </summary>
			/// <remarks></remarks>
			Process = 3,
			/// <summary>
			/// This entity type represents a System.
			/// </summary>
			/// <remarks></remarks>
			System = 4,
			/// <summary>
			/// This entity type represents a data item like a file or table.
			/// </summary>
			/// <remarks></remarks>
			Data = 5
		}
		
		/// <summary>
		/// Specifies the type of SSAM event.
		/// </summary>
		/// <remarks></remarks>
		public enum SsamEventType
		{
			/// <summary>
			/// This event reports a successful action on some entity.
			/// </summary>
			/// <remarks></remarks>
			Success = 1,
			/// <summary>
			/// This event is a warning that something may be going wrong soon.
			/// </summary>
			/// <remarks></remarks>
			Warning = 2,
			/// <summary>
			/// This event is an alarm that something has already gone wrong.
			/// </summary>
			/// <remarks></remarks>
			Alarm = 3,
			/// <summary>
			/// This event reports an unexpected error in an application that may or may not matter.
			/// </summary>
			/// <remarks></remarks>
			@Error = 4,
			/// <summary>
			/// This event reports information that may be of interest to someone.
			/// </summary>
			/// <remarks></remarks>
			Information = 5,
			/// <summary>
			/// This event reports an alarm notification that was sent that has not been acknowledged.
			/// </summary>
			/// <remarks></remarks>
			Escalation = 6,
			/// <summary>
			/// This event reports a cluster failover on some process (informational).
			/// </summary>
			/// <remarks></remarks>
			Failover = 7,
			/// <summary>
			/// This event halts the Ssam monitoring/dispatching process - remove later? [fixme]
			/// </summary>
			/// <remarks></remarks>
			[Obsolete("This enumeration will be deprecated in future release.")]Quit = 8,
			/// <summary>
			/// This action handles a "Synchronize-SSAM" notification by synchronizing the monitor database with
			/// the system-configuration database.
			/// </summary>
			/// <remarks></remarks>
			Synchronize = 9,
			/// <summary>
			/// This action handles a "Terminate-SSAM" notification by rescheduling all events.
			/// </summary>
			/// <remarks></remarks>
			Reschedule = 10,
			/// <summary>
			/// This action makes the monitor skip old events, reschedule, and return to real-time processing.
			/// </summary>
			/// <remarks></remarks>
			CatchUp = 11
		}
		
	}
}
