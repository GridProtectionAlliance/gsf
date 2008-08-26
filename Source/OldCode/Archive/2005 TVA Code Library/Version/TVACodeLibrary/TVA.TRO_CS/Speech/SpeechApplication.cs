using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Data;
using System.ComponentModel;
using System.Data.SqlClient;
//using TVA.Data.Common;
//using TVA.Text.Common;
//using TVA.TRO.Speech.Common;


namespace TVA.TRO
{
	namespace Speech
	{
		
		public class SpeechApplication
		{
			
			
			private string m_applicationID;
			private string m_applicationName;
			private string m_applicationOwner;
			private string m_phoneNumber;
			private string m_extension;
			private string m_message;
			private int m_loopCount;
			private bool m_loopCalls;
			private bool m_logResponses;
			private bool m_isLongDistance;
			private SqlConnection m_connection;
			
			#region " Properties"
			
			public string ApplicationID
			{
				get
				{
					return m_applicationID;
				}
			}
			
			public string ApplicationName
			{
				get
				{
					return m_applicationName;
				}
			}
			
			[Browsable(false)]public string ApplicationOwner
			{
				get
				{
					return m_applicationOwner;
				}
			}
			
			[Browsable(false)]public int LoopCount
			{
				get
				{
					return m_loopCount;
				}
			}
			
			[Browsable(false)]public bool LoopCalls
			{
				get
				{
					return m_loopCalls;
				}
			}
			
			[Browsable(false)]public bool LogResponses
			{
				get
				{
					return m_logResponses;
				}
			}
			
			/// <summary>
			/// 10 digit phone number to call.
			/// </summary>
			/// <value></value>
			/// <returns></returns>
			/// <remarks></remarks>
			[Description("Provide 10 digit phone number.")]public string PhoneNumber
			{
				get
				{
					return m_phoneNumber;
				}
				set
				{
					m_phoneNumber = TVA.Text.Common.RemoveCharacters(value, new TVA.Text.Common.CharacterTestFunctionSignature(IsCharacterInvalid));
				}
			}
			
			[Description("Provide extension number if any for this phone call."), DefaultValue("")]public string Extension
			{
				get
				{
					return m_extension;
				}
				set
				{
					m_extension = value;
				}
			}
			
			/// <summary>
			/// Message to be played.
			/// </summary>
			/// <value></value>
			/// <returns></returns>
			/// <remarks></remarks>
			[Description("Message to be played to the recipient.")]public string Message
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
			
			#endregion
			
			#region " Constructors"
			/// <summary>
			/// Initialize SpeechApplication Object
			/// </summary>
			/// <param name="ApplicationID">Four Characters Application ID</param>
			public SpeechApplication(string ApplicationID) : this(ApplicationID, Environment.Development)
			{
				
				
			}
			
			public SpeechApplication(string ApplicationID, Environment EnvironmentType)
			{
				m_connection = new SqlConnection(TVA.TRO.Speech.Common.GetConnectionString(EnvironmentType));
				try
				{
					m_extension = string.Empty;
					m_message = string.Empty;
					m_phoneNumber = string.Empty;
					if ((m_connection != null)&& m_connection.State != ConnectionState.Open)
					{
						m_connection.Open();
					}
					DataRow appInfoRow = TVA.Data.Common.RetrieveRow("Select * From Applications Where ID = \'" + ApplicationID + "\'", m_connection);
					if (appInfoRow != null)
					{
						m_applicationID = ApplicationID;
						m_applicationName = appInfoRow["ApplicationName"].ToString();
						m_applicationOwner = appInfoRow["ApplicationOwner"].ToString();
						m_loopCalls = Convert.ToBoolean(appInfoRow["LoopCalls"]);
						m_loopCount = Convert.ToInt32(appInfoRow["LoopCount"]);
						m_logResponses = Convert.ToBoolean(appInfoRow["LogResponses"]);
					}
					else
					{
						throw (new ArgumentException("Application does not exist in the database."));
					}
				}
				catch
				{
					throw;
				}
				finally
				{
					if (m_connection.State != ConnectionState.Closed)
					{
						m_connection.Close();
					}
				}
			}
			
			#endregion
			
			#region " Private Methods"
			
			private bool IsLongDistance(string PhoneNumber)
			{
				
				if ((m_connection != null)&& m_connection.State != ConnectionState.Open)
				{
					m_connection.Open();
				}
				string areaCode = PhoneNumber.Substring(0, 3);
				string exchange = PhoneNumber.Substring(3, 3);
				
				return ! Convert.ToBoolean(TVA.Data.Common.ExecuteScalar("Select Count(*) From ChattanoogaLocalPhones Where AreaCode = \'" + areaCode + "\' AND Exchange = \'" + exchange + "\'", m_connection));
				
			}
			
			/// <summary>
			/// Test the specified character to see if it is one of the invalid (non-digit) characters found
			/// in phone numbers.
			/// </summary>
			private bool IsCharacterInvalid(char character)
			{
				
				return (character == ' ' || character == '(' || character == ')' || character == '-');
				
			}
			
			#endregion
			
			#region " Public Methods"
			
			public int MakeCall()
			{
				int eventId = 0;
				try
				{
					if ((m_connection != null)&& m_connection.State != ConnectionState.Open)
					{
						m_connection.Open();
					}
					if (string.IsNullOrEmpty(m_message))
					{
						throw (new ArgumentException("Message property cannot be null or empty."));
					}
					if (string.IsNullOrEmpty(m_phoneNumber))
					{
						throw (new ArgumentException("PhoneNumber property cannot be null or empty."));
					}
					if (m_phoneNumber.Length != 10)
					{
						throw (new ArgumentException("PhoneNumber must be 10 digits."));
					}
					if (IsLongDistance(m_phoneNumber))
					{
						m_phoneNumber = "1" + m_phoneNumber;
					}
					
					eventId = Convert.ToInt32(TVA.Data.Common.ExecuteScalar("CreateEvent", m_connection, m_applicationID, m_phoneNumber, m_extension, m_message));
				}
				catch
				{
					throw;
				}
				finally
				{
					if (m_connection.State != ConnectionState.Closed)
					{
						m_connection.Close();
					}
				}
				return eventId;
			}
			
			#endregion
			
		}
		
	}
	
}
