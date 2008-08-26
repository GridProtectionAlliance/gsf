using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;

//*******************************************************************************************************
//  TVA.Security.Application.Group.vb - User group defined in the security database
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
//  10/19/2006 - Pinal C. Patel
//       Original version of source code generated.
//
//*******************************************************************************************************

namespace TVA.Security
{
	namespace Application
	{
		
		/// <summary>
		/// Represents a user group defined in the security database.
		/// </summary>
		[Serializable()]public class Group
		{
			
			
			#region " Member Declaration "
			
			private string m_name;
			private string m_description;
			
			#endregion
			
			#region " Code Scope: Public "
			
			/// <summary>
			/// Creates an instance of user group defined in the security database.
			/// </summary>
			/// <param name="name">Name of the user group.</param>
			/// <param name="description">Description of the user group.</param>
			public Group(string name, string description)
			{
				
				m_name = name;
				m_description = description;
				
			}
			
			/// <summary>
			/// Gets the user group's name.
			/// </summary>
			/// <value></value>
			/// <returns>Name of the user group.</returns>
			public string Name
			{
				get
				{
					return m_name;
				}
			}
			
			/// <summary>
			/// Gets the user group's description.
			/// </summary>
			/// <value></value>
			/// <returns>Description of the user group.</returns>
			public string Description
			{
				get
				{
					return m_description;
				}
			}
			
			/// <summary>
			/// Compares this user group with another user group based on the name and description.
			/// </summary>
			/// <param name="obj">Another user group to compare against.</param>
			/// <returns>True if user groups are the same; otherwise False.</returns>
			public override bool Equals(object obj)
			{
				
				Group other = obj as Group;
				if (other != null)
				{
					return (m_name == other.Name && m_description == other.Description);
				}
				else
				{
					throw (new ArgumentException(string.Format("Cannot compare {0} with {1}.", this.GetType().Name, other.GetType().Name)));
				}
				
			}
			
			#endregion
			
		}
		
	}
}
