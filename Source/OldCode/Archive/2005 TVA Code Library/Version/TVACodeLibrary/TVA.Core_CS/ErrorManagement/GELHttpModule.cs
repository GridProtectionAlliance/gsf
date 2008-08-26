using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Web;
//using TVA.Configuration.Common;

//*******************************************************************************************************
//  TVA.ErrorManagement.GELHttpModule.vb - Common Configuration Functions
//  Copyright © 2007 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  04/17/2007 - Pinal C. Patel
//       Generated original version of source code.
//  09/13/2007 - Darrell Zuercher
//       Edited code comments.
//
//*******************************************************************************************************



namespace TVA
{
	namespace ErrorManagement
	{
		
		public class GELHttpModule : IHttpModule
		{
			
			
			// Usage sample in web.config:
			//<configuration>
			//  <configSections>
			//    <section name="categorizedSettings" type="TVA.Configuration.CategorizedSettingsSection, TVA.Core" />
			//  </configSections>
			//  <categorizedSettings>
			//    <globalExceptionLogger>
			//      <clear />
			//      <add name="AutoStart" value="False" description="True if the logger is to be started automatically after initialization is complete; otherwise False." encrypted="false" />
			//      <add name="LogToUI" value="True" description="True if an encountered exception is to be logged to the User Interface; otherwise False." encrypted="false" />
			//      <add name="LogToFile" value="False" description="True if an encountered exception is to be logged to a file; otherwise False." encrypted="false" />
			//      <add name="LogToEmail" value="True" description="True if an email is to be sent with the details of an encountered exception; otherwise False." encrypted="false" />
			//      <add name="LogToEventLog" value="False" description="True if an encountered exception is to be logged to the Event Log; otherwise False." encrypted="false" />
			//      <add name="LogToScreenshot" value="False" description="True if a screenshot is to be taken when an exception is encountered; otherwise False." encrypted="false" />
			//      <add name="EmailServer" value="mailhost.cha.tva.gov" description="Name of the email server to be used for sending the email message." encrypted="false" />
			//      <add name="EmailRecipients" value="pcpatel@tva.gov" description="Comma-seperated list of recipients email addresses for the email message." encrypted="false" />
			//      <add name="ContactPersonName" value="" description="Name of the person that the end-user can contact when an exception is encountered." encrypted="false" />
			//      <add name="ContactPersonPhone" value="" description="Phone number of the person that the end-user can contact when an exception is encountered." encrypted="false" />
			//    </globalExceptionLogger>
			//  </categorizedSettings>
			//  <system.web>
			//    <httpModules>
			//      <add name="GELHttpModule" type="TVA.ErrorManagement.GELHttpModule, TVA.Core" />
			//    </httpModules>
			//  </system.web>
			//</configuration>
			
			public void Dispose()
			{
				
				// We do not have to dispose of anything.
				
			}
			
			public void Init(System.Web.HttpApplication context)
			{
				
				try
				{
					if (TVA.Configuration.Common.CategorizedSettings(typeof(GlobalExceptionLogger).Name).Count == 0)
					{
						object with_1 = new GlobalExceptionLogger();
						with_1.PersistSettings = true;
						with_1.SaveSettings();
					}
				}
				catch (Exception)
				{
					
				}
				
				context.Error += new System.EventHandler(OnError);
				
			}
			
			private void OnError(object sender, System.EventArgs e)
			{
				
				object with_1 = new GlobalExceptionLogger();
				// Logs the encountered exception.
				with_1.BeginInit();
				with_1.EndInit();
				with_1.Log(HttpContext.Current.Server.GetLastError());
				
			}
			
		}
		
	}
}
