using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;

// 05/02/2007

namespace TVA.Services
{
	public class ClientRequestHandlerInfo
	{
		
		
		public delegate void HandlerMethodSignature(ClientRequestInfo requestInfo);
		
		public ClientRequestHandlerInfo(string requestCommand, string requestDescription, HandlerMethodSignature handlerMethod) : this(requestCommand, requestDescription, handlerMethod, true)
		{
			
			
		}
		
		public ClientRequestHandlerInfo(string requestCommand, string requestDescription, HandlerMethodSignature handlerMethod, bool isAdvertised)
		{
			
			this.Command = requestCommand;
			this.CommandDescription = requestDescription;
			this.HandlerMethod = handlerMethod;
			this.IsAdvertised = isAdvertised;
			
		}
		
		public string Command;
		public string CommandDescription;
		public HandlerMethodSignature HandlerMethod;
		public bool IsAdvertised;
		
	}
	
}
