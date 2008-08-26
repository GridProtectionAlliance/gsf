using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
//using TVA.Text.Common;

//*******************************************************************************************************
//  TVA.Services.ServiceResponse.vb - Service Response to Clients
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
//  08/29/2006 - Pinal C. Patel
//       Original version of source code generated
//
//*******************************************************************************************************


namespace TVA.Services
{
	[Serializable()]public class ServiceResponse
	{
		
		
		private string m_type;
		private string m_message;
		private List<object> m_attachments;
		
		/// <summary>
		/// Initializes a default instance of service response.
		/// </summary>
		public ServiceResponse() : this("UNDETERMINED")
		{
			
			
		}
		
		/// <summary>
		/// Initializes a instance of service response with the specified type.
		/// </summary>
		/// <param name="type">The type of service response.</param>
		public ServiceResponse(string type) : this(type, "")
		{
			
			
		}
		
		/// <summary>
		/// Initializes a instance of service response with the specified type and message.
		/// </summary>
		/// <param name="type">The type of service response.</param>
		/// <param name="message">The message of the service response.</param>
		public ServiceResponse(string type, string message)
		{
			
			m_type = type.ToUpper();
			m_message = message;
			m_attachments = new List<object>;
			
		}
		
		/// <summary>
		/// Gets or sets the type of response being sent to the client.
		/// </summary>
		/// <value></value>
		/// <returns>The type of response being sent to the client.</returns>
		public string Type
		{
			get
			{
				return m_type;
			}
			set
			{
				m_type = value.ToUpper();
			}
		}
		
		/// <summary>
		/// Gets or sets the message being sent to the client.
		/// </summary>
		/// <value></value>
		/// <returns>The message being sent to the client.</returns>
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
		/// Gets a list of attachments being sent to the client.
		/// </summary>
		/// <value></value>
		/// <returns>A list of attachments being sent to the client.</returns>
		public List<object> Attachments
		{
			get
			{
				return m_attachments;
			}
		}
		
	}
}
