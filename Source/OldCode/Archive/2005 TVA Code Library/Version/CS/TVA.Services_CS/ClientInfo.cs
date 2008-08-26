using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
//using TVA.Assembly;

// 09-12-06


namespace TVA.Services
{
	[Serializable()]public class ClientInfo
	{
		
		
		public ClientInfo(Guid clientID)
		{
			
			this.ClientID = clientID;
			ClientType = TVA.Common.GetApplicationType();
			UserName = System.Threading.Thread.CurrentPrincipal.Identity.Name;
			if (string.IsNullOrEmpty(UserName))
			{
				UserName = Environment.UserDomainName + "\\" + Environment.UserName;
			}
			MachineName = Environment.MachineName;
			if ((ClientType == ApplicationType.WindowsCui) || (ClientType == ApplicationType.WindowsGui))
			{
				ClientName = AppDomain.CurrentDomain.FriendlyName;
			}
			else if (ClientType == ApplicationType.Web)
			{
				ClientName = System.Web.HttpContext.Current.Request.ApplicationPath;
			}
			
		}
		
		public Guid ClientID;
		public ApplicationType ClientType;
		public string ClientName;
		public string UserName;
		public string MachineName;
		public DateTime ConnectedAt;
		
	}
	
}
