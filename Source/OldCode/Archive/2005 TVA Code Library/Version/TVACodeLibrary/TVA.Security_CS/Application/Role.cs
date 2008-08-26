using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;

//*******************************************************************************************************
//  TVA.Security.Application.Role.vb - Application role defined in the security database
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
//  09/26/2006 - Pinal C. Patel
//       Original version of source code generated.
//
//*******************************************************************************************************

namespace TVA.Security
{
	namespace Application
	{
		
		/// <summary>
		/// Represents an application role defined in the security database.
		/// </summary>
		[Serializable()]public class Role
		{
			
			
			#region " Member Declaration "
			
			private string m_name;
			private string m_description;
			private Application m_application;
			
			#endregion
			
			#region " Code Scope: Public "
			
			/// <summary>
			/// Creates an instance of application roles defined in the security database.
			/// </summary>
			/// <param name="name">Name of the role.</param>
			/// <param name="description">Description of the role.</param>
			/// <param name="application">Name of the application to which the role belongs.</param>
			public Role(string name, string description, Application application)
			{
				
				m_name = name;
				m_description = description;
				m_application = application;
				
			}
			
			/// <summary>
			/// Gets the application to which the role belongs.
			/// </summary>
			/// <value></value>
			/// <returns>Application to which the role belongs.</returns>
			public Application Application
			{
				get
				{
					return m_application;
				}
			}
			
			/// <summary>
			/// Gets the role's name.
			/// </summary>
			/// <value></value>
			/// <returns>Name of the role.</returns>
			public string Name
			{
				get
				{
					return m_name;
				}
			}
			
			/// <summary>
			/// Gets the role's description.
			/// </summary>
			/// <value></value>
			/// <returns>Description of the role.</returns>
			public string Description
			{
				get
				{
					return m_description;
				}
			}
			
			/// <summary>
			/// Compares this roles with another role based on the name and description.
			/// </summary>
			/// <param name="obj">Another role to compare against.</param>
			/// <returns>True if roles are the same; otherwise False.</returns>
			public override bool Equals(object obj)
			{
				
				Role other = obj as Role;
				if (other != null)
				{
					return (m_name == other.Name && m_description == other.Description && m_application.Equals(other.Application));
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
