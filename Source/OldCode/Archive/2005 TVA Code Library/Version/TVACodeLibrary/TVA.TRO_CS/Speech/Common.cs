using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Data;
using System.Data.SqlClient;
//using TVA.Data.Common;
using System.Net;
using System.Net.Sockets;


namespace TVA.TRO
{
	namespace Speech
	{
		
		public sealed class Common
		{
			
			
			private Common()
			{
				
				// This class is not to be instantiated.
				
			}
			
			/// <summary>
			/// Marks an event as complete so that looping service will not make any more calls for the event.
			/// </summary>
			/// <param name="EventId">Id of an event to be marked as complete.</param>
			/// <param name="EnvironmentType">Environment: Development/Acceptance/Production.</param>
			/// <remarks></remarks>
			public static void StopCalling(int EventId, Environment EnvironmentType)
			{
				SqlConnection connection = new SqlConnection(GetConnectionString(EnvironmentType));
				try
				{
					if (connection.State != ConnectionState.Open)
					{
						connection.Open();
					}
					TVA.Data.Common.ExecuteScalar("Update Events Set Active = \'0\' Where ID = " + EventId, connection);
					TVA.Data.Common.ExecuteScalar("Delete From LoopingTable Where EventId = " + EventId, connection);
				}
				catch
				{
					throw;
				}
				finally
				{
					if (connection.State != ConnectionState.Closed)
					{
						connection.Close();
					}
					if (connection != null)
					{
						connection.Dispose();
					}
				}
			}
			
			/// <summary>
			/// Return call log information from the database for an event.
			/// </summary>
			/// <param name="EventId">Id of an event for which call log needs to be retrieved.</param>
			/// <param name="EnvironmentType">Environment: Development/Acceptance/Production.</param>
			/// <returns></returns>
			/// <remarks></remarks>
			public static DataTable GetCallLog(int EventId, Environment EnvironmentType)
			{
				DataTable callLogData = new DataTable();
				SqlConnection connection = new SqlConnection(GetConnectionString(EnvironmentType));
				try
				{
					if (connection.State != ConnectionState.Open)
					{
						connection.Open();
					}
					callLogData = TVA.Data.Common.RetrieveData("Select * From CallLogDetail Where EventId = " + EventId, connection);
				}
				catch
				{
					throw;
				}
				finally
				{
					if (connection.State != ConnectionState.Closed)
					{
						connection.Close();
					}
					if (connection != null)
					{
						connection.Dispose();
					}
				}
				
				return callLogData;
			}
			
			/// <summary>
			/// Return database connection string based on the environment type.
			/// </summary>
			/// <param name="EnvironmentType">Environment type: Development, acceptance, production</param>
			/// <returns>Database connection string</returns>
			/// <remarks></remarks>
			public static string GetConnectionString(Environment EnvironmentType)
			{
				if (EnvironmentType == Environment.Development)
				{
					return "Server=RGOCDSQL; Database=Speech; UID=trospeech; PWD=tr0speech";
				}
				else if (EnvironmentType == Environment.Acceptance)
				{
					return "Server=ESOASQLGENDAT\\GENDAT; Database=Speech; UID=trospeech; PWD=tr0speech";
				}
				else if (EnvironmentType == Environment.Production)
				{
					return "Server=ESOOPSQL1; Database=Speech; UID=trospeech; PWD=tr0speech";
				}
				else
				{
					return "Server=RGOCDSQL; Database=Speech; UID=trospeech; PWD=tr0speech";
				}
			}
			
			public static bool IsLoopingServiceRunning(Environment EnvironmentType)
			{
				if (EnvironmentType == Environment.Development)
				{
					return PingServer("RGOCMSSPEECH3", 6999);
				}
				else if (EnvironmentType == Environment.Acceptance)
				{
					return PingServer("RGOCMSSPEECH3", 6999);
				}
				else if (EnvironmentType == Environment.Production)
				{
					return PingServer("RGOCMSSPEECH3", 6999);
				}
				else
				{
					return PingServer("RGOCMSSPEECH3", 6999);
				}
			}
			
			/// <summary>
			/// This function pings the server based on supplied name on port and returns boolean result.
			/// </summary>
			/// <param name="ServerName"></param>
			/// <param name="port"></param>
			/// <returns></returns>
			/// <remarks></remarks>
			private static bool PingServer(string ServerName, int port)
			{
				bool bUP = false;
				IPHostEntry hostEntry = null;
				// Get host related information.
				try
				{
					hostEntry = Dns.GetHostEntry(ServerName.Trim());
					IPAddress address;
					foreach (IPAddress tempLoopVar_address in hostEntry.AddressList)
					{
						address = tempLoopVar_address;
						IPEndPoint endPoint = new IPEndPoint(address, port);
						Socket tempSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
						tempSocket.Connect(endPoint);
						if (tempSocket.Connected)
						{
							bUP = true;
							tempSocket.Close();
							tempSocket = null;
							break;
						}
					}
					
				}
				catch (Exception)
				{
					bUP = false;
					
				}
				
				return bUP;
			}
		}
		
	}
}
