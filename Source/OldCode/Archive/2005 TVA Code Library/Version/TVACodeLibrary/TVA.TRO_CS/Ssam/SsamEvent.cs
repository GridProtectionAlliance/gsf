using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;

//*******************************************************************************************************
//  TVA.TRO.Ssam.SsamEvent.vb - SSAM Event
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
//  04/24/2006 - Pinal C. Patel
//       Original version of source code generated
//  08/25/2006 - Pinal C. Patel
//       Moved SsamEntityType and SsamEventType enumerations to Enumerations.vb.
//*******************************************************************************************************

namespace TVA.TRO
{
	namespace Ssam
	{
		
		/// <summary>
		/// Defines an event that can be logged to SSAM.
		/// </summary>
		/// <remarks></remarks>
		[Serializable()]public class SsamEvent
		{
			
			
			private string m_entityID;
			private SsamEntityType m_entityType;
			private SsamEventType m_eventType;
			private string m_errorNumber;
			private string m_message;
			private string m_description;
			
			/// <summary>
			/// Initializes a instance of TVA.TRO.Ssam.SsamEvent with the specified information.
			/// </summary>
			/// <param name="entityID">The mnemonic key or the numeric value of the entity to which the event belongs.</param>
			/// <param name="entityType">One of the TVA.TRO.Ssam.SsamEntityType values.</param>
			/// <param name="eventType">One of the TVA.TRO.Ssam.SsamEvent.SsamEventType values.</param>
			public SsamEvent(string entityID, SsamEntityType entityType, SsamEventType eventType) : this(entityID, entityType, eventType, "", "", "")
			{
				
				
			}
			
			/// <summary>
			/// Initializes a instance of TVA.TRO.Ssam.SsamEvent with the specified information.
			/// </summary>
			/// <param name="entityID">The mnemonic key or the numeric value of the entity to which the event belongs.</param>
			/// <param name="entityType">One of the TVA.TRO.Ssam.SsamEntityType values.</param>
			/// <param name="eventType">One of the TVA.TRO.Ssam.SsamEvent.SsamEventType values.</param>
			/// <param name="message">A brief description of the event (max 120 characters).</param>
			public SsamEvent(string entityID, SsamEntityType entityType, SsamEventType eventType, string message) : this(entityID, entityType, eventType, "", message, "")
			{
				
				
			}
			
			
			/// <summary>
			/// Initializes a instance of TVA.TRO.Ssam.SsamEvent with the specified information.
			/// </summary>
			/// <param name="entityID">The mnemonic key or the numeric value of the entity to which the event belongs.</param>
			/// <param name="entityType">One of the TVA.TRO.Ssam.SsamEntityType values.</param>
			/// <param name="eventType">One of the TVA.TRO.Ssam.SsamEvent.SsamEventType values.</param>
			/// <param name="errorNumber">The error number encountered, if any, for which the event is being logged.</param>
			/// <param name="message">A brief description of the event (max 120 characters).</param>
			/// <param name="description">A detailed description of the event (max 2GB).</param>
			public SsamEvent(string entityID, SsamEntityType entityType, SsamEventType eventType, string errorNumber, string message, string description)
			{
				
				m_entityID = entityID;
				m_entityType = entityType;
				m_eventType = eventType;
				m_errorNumber = errorNumber;
				m_message = message;
				m_description = description;
				
			}
			
			/// <summary>
			/// Gets or sets the mnemonic key or the numeric value of the entity to which this event belongs.
			/// </summary>
			/// <value></value>
			/// <returns>The mnemonic key or the numeric value of the entity to which this event belongs.</returns>
			/// <remarks></remarks>
			public string EntityId
			{
				get
				{
					return m_entityID;
				}
				set
				{
					m_entityID = value;
				}
			}
			
			/// <summary>
			/// Gets or sets type of the entity to which this event belongs.
			/// </summary>
			/// <value></value>
			/// <returns>Type of the entity to which this event belongs.</returns>
			/// <remarks></remarks>
			public SsamEntityType EntityType
			{
				get
				{
					return m_entityType;
				}
				set
				{
					m_entityType = value;
				}
			}
			
			/// <summary>
			/// Gets or sets the type of this event.
			/// </summary>
			/// <value></value>
			/// <returns>The type of this event.</returns>
			/// <remarks></remarks>
			public SsamEventType EventType
			{
				get
				{
					return m_eventType;
				}
				set
				{
					m_eventType = value;
				}
			}
			
			/// <summary>
			/// Gets or sets the error number encountered, if any, for which this event is being logged.
			/// </summary>
			/// <value></value>
			/// <returns>The error number encountered, if any, for which this event is being logged.</returns>
			/// <remarks></remarks>
			public string ErrorNumber
			{
				get
				{
					return m_errorNumber;
				}
				set
				{
					m_errorNumber = value;
				}
			}
			
			/// <summary>
			/// Gets or sets the brief description of this event (max 120 characters).
			/// </summary>
			/// <value></value>
			/// <returns>The brief description of this event (max 120 characters).</returns>
			/// <remarks></remarks>
			public string Message
			{
				get
				{
					return m_message;
				}
				set
				{
					m_message = value;
				}
			}
			
			/// <summary>
			/// Gets or sets the detailed description of this event (max 2GB).
			/// </summary>
			/// <value></value>
			/// <returns>The detailed description of this event (max 2GB).</returns>
			/// <remarks></remarks>
			public string Description
			{
				get
				{
					return m_description;
				}
				set
				{
					m_description = value;
				}
			}
			
		}
		
	}
}
