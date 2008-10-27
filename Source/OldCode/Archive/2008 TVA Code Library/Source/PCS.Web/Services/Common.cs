using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Data;
using System.Data.SqlClient;
//using PCS.Configuration.Common;
using PCS.Security.Cryptography;
//using PCS.Security.Cryptography.Common;
using PCS.Security.Application;
//using PCS.Identity.Common;

//*******************************************************************************************************
//  PCS.Web.Services.Common.vb - Common web service related functions
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [PCS]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/24/2007 - J. Ritchie Carroll
//       Original version of source code generated
//
//*******************************************************************************************************



namespace PCS.Web
{
	namespace Services
	{
		
		/// <summary>Defines common global functions related to Web Services</summary>
		public sealed class Common
		{
			
			
			internal const string WebServiceSecurityKey = "30#TV9B~~E9=%8~l0aV52.S^$j:9F37:9a1308r1A7~!7285:~b465c@5509488r&78V{4%707~34[e]<_352©4)C51,8P2?5M40f©44%j(~F04AB1F420}5~59*~46Cr233d7o+0>7179N`8|.~649CAT9b3~Bc281125Off9%066CDb7C492\\3";
			
			private Common()
			{
				
				// This class contains only global functions and is not meant to be instantiated
				
			}
			
			public static object SetWebServiceCredentials(object webService, SecurityServer server)
			{
				
				// Note "webService" parameter must be "Object", because web services create local proxy implementations
				// of the AuthenticationSoapHeader and do not support interfaces - hence all calls will be made through
				// reflection (i.e., late bound method invocation support), but everything works as expected...
				object with_1 = webService;
				// Remove domain prefix from user ID (if it has one)
				string userName = System.Threading.Thread.CurrentPrincipal.Identity.Name;
				if (userName.Contains("\\"))
				{
					userName = userName.Split('\\')[1].Trim();
				}
				with_1.UserName = PCS.Security.Cryptography.Common.Encrypt(userName, WebServiceSecurityKey, EncryptLevel.Level4);
				with_1.Password = null;
				with_1.Server = server;
				with_1.PassThroughAuthentication = true;
				
				return webService;
				
			}
			
			public static object SetWebServiceCredentials(object webService, string userName, string password, SecurityServer server)
			{
				
				if (string.IsNullOrEmpty(userName))
				{
					throw (new InvalidOperationException("No userName was specified"));
				}
				if (string.IsNullOrEmpty(password))
				{
					throw (new InvalidOperationException("No password was specified"));
				}
				
				// Note "webService" parameter must be "Object", because web services create local proxy implementations
				// of the AuthenticationSoapHeader and do not support interfaces - hence all calls will be made through
				// reflection (i.e., late bound method invocation support), but everything works as expected...
				object with_1 = webService;
				with_1.UserName = PCS.Security.Cryptography.Common.Encrypt(userName, WebServiceSecurityKey, EncryptLevel.Level4);
				with_1.Password = PCS.Security.Cryptography.Common.Encrypt(password, WebServiceSecurityKey, EncryptLevel.Level4);
				with_1.Server = server;
				with_1.PassThroughAuthentication = false;
				
				return webService;
				
			}
			
			public static DataTable ExceptionsArrayListToDataTable(ArrayList exceptions)
			{
				
				DataTable dtResult = new DataTable("Exceptions");
				dtResult.Columns.Add("ExceptionMessage");
				if (exceptions.Count == 0)
				{
					dtResult = HandleNoValuesInFields(dtResult);
				}
				else
				{
					for (int i = 0; i <= exceptions.Count - 1; i++)
					{
						DataRow dr;
						dr = dtResult.NewRow();
						dr[0] = exceptions[i];
						dtResult.Rows.Add(dr);
					}
				}
				
				return dtResult;
				
			}
			
			public static DataTable BuildExportInfoTable(int recCount, string messageName)
			{
				
				DataTable dtExport = new DataTable("ExportInformation");
				dtExport.Columns.Add("RecordCount");
				dtExport.Columns.Add("RunTime");
				dtExport.Columns.Add("RefreshSchedule");
				dtExport.Columns.Add("SourceDataTimeZone");
				
				DataRow dr;
				dr = dtExport.NewRow();
				dr[0] = recCount;
				dr[1] = DateTime.Now;
				dr[2] = PCS.Configuration.Common.CategorizedSettings("WebServices.RefreshSchedule").Item(messageName + ".RefreshSchedule").Value;
				dr[3] = PCS.Configuration.Common.CategorizedSettings("WebServices.SourceDataTimeZone").Item(messageName + ".SourceDataTimeZone").Value;
				dtExport.Rows.Add(dr);
				
				return dtExport;
				
			}
			
			public static DataTable BuildSourceInfoTable(string messageName)
			{
				
				DataTable dtSource = new DataTable("SourceInformation");
				
				string[] sourceTables = PCS.Configuration.Common.CategorizedSettings("WebServices.SourceTables").Item(messageName + ".SourceTables").Value.Split(';');
				//Dim col As DataColumn = dtSource.Columns.Add("SourceTable")
				//col.ColumnMapping = MappingType.Attribute
				dtSource.Columns.Add("SourceTable");
				foreach (string table in sourceTables)
				{
					
					DataRow dr;
					dr = dtSource.NewRow();
					dr[0] = table;
					dtSource.Rows.Add(dr);
				}
				
				return dtSource;
				
			}
			
			
			public static DataTable HandleNoValuesInFields(DataTable dt)
			{
				
				DataRow drNull;
				int i;
				drNull = dt.NewRow();
				for (i = 0; i <= dt.Columns.Count - 1; i++)
				{
					drNull[i] = DBNull.Value;
				}
				
				dt.Rows.Add(drNull);
				
				return dt;
			}
			
		}
		
	}
	
}
