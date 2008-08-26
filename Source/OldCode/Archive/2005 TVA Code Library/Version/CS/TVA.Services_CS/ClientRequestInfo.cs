using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;

// 02/11/2007

namespace TVA.Services
{
	public class ClientRequestInfo
	{
		
		
		public ClientRequestInfo(ClientInfo sender, ClientRequest request) : this(sender, request, DateTime.Now)
		{
			
			
		}
		
		public ClientRequestInfo(ClientInfo sender, ClientRequest request, DateTime receivedAt)
		{
			
			this.Request = request;
			this.Sender = sender;
			this.ReceivedAt = receivedAt;
			
		}
		
		public ClientRequest Request;
		public ClientInfo Sender;
		public DateTime ReceivedAt;
		
	}
	
}
