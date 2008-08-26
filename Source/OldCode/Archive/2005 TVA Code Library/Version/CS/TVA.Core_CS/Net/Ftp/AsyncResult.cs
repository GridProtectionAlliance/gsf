using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;

// James Ritchie Carroll - 2003

namespace TVA
{
	namespace Net
	{
		namespace Ftp
		{
			
			
			public class AsyncResult
			{
				
				
				public const int Complete = 0;
				public const int Fail = 1;
				public const int Abort = 2;
				
				private BitArray m_result;
				private string m_message;
				private int m_ftpResponse;
				
				internal AsyncResult() : this("Success.", System.Convert.ToInt32(Response.InvalidCode), Complete)
				{
					
					
				}
				
				internal AsyncResult(string message, int result) : this(message, System.Convert.ToInt32(Response.InvalidCode), result)
				{
					
					
				}
				
				internal AsyncResult(string message, int ftpCode, int result)
				{
					
					m_result = new BitArray(3);
					m_message = message;
					m_ftpResponse = ftpCode;
					m_result[result] = true;
					
				}
				
				public bool IsSuccess
				{
					get
					{
						return m_result[Complete];
					}
				}
				
				public bool IsFailed
				{
					get
					{
						return m_result[Fail];
					}
				}
				
				public bool IsAborted
				{
					get
					{
						return m_result[Abort];
					}
				}
				
				public int ResponseCode
				{
					get
					{
						return m_ftpResponse;
					}
				}
				
				public string Message
				{
					get
					{
						return m_message;
					}
				}
				
			}
			
		}
	}
}
