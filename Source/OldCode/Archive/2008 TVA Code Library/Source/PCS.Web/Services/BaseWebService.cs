using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Xml;
using System.Data.SqlClient;
using PCS.Security.Cryptography;
//using PCS.Security.Cryptography.Common;
using PCS.Security.Application;
//using PCS.Identity.Common;
using PCS.Web.Services.Common;
using System.Security.Principal;

// 02/14/2007



namespace PCS.Web
{
	namespace Services
	{
		
		//<WebService(Namespace:="http://troweb/DataServices/")> _
		//<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
		//<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
		public abstract class BaseWebService : System.Web.Services.WebService, IBusinessObjectsAdapter
		{
			
			
			private IBusinessObjectsAdapter m_businessObjectAdapter;
			
			protected string DllName;
			protected string FullyQualifiedClassName;
			protected AuthenticationSoapHeader PCSWebServiceCredentials;
			
			public BaseWebService()
			{
				
				
			}
			
			public bool UserHasAccessToData(string roleName)
			{
				
				return AuthenticateUser(PCS.Security.Cryptography.Common.Decrypt(PCSWebServiceCredentials.UserName, WebServiceSecurityKey, EncryptLevel.Level4), PCS.Security.Cryptography.Common.Decrypt(PCSWebServiceCredentials.Password, WebServiceSecurityKey, EncryptLevel.Level4), roleName, PCSWebServiceCredentials.Server, PCSWebServiceCredentials.PassThroughAuthentication);
				
			}
			
			public string BuildMessage()
			{
				
				return m_businessObjectAdapter.BuildMessage();
				
			}
			
			public void Initialize(params object[] itemList)
			{
				
				System.Reflection.Assembly a = System.Reflection.Assembly.LoadFrom(Server.MapPath(DllName)); //"C:\Documents and Settings\sjohn\My Documents\Visual Studio 2005\Projects\abc\abc\bin\Release\abc.dll")
				System.Type t = a.GetType(FullyQualifiedClassName, true);
				m_businessObjectAdapter = Activator.CreateInstance(t);
				m_businessObjectAdapter.Initialize(itemList);
				
			}
			
			public XmlDocument ConvertToXMLDoc(string xmlData)
			{
				
				XmlDocument xmlDoc = new XmlDocument();
				System.Xml.XmlTextReader xmlReader;
				xmlReader = new System.Xml.XmlTextReader(xmlData, System.Xml.XmlNodeType.Document, null);
				xmlReader.ReadOuterXml();
				xmlDoc.Load(xmlReader);
				
				return xmlDoc;
				
			}
			
			public static bool AuthenticateUser(string userID, string password, string roleName, SecurityServer server, bool passThroughAuthentication)
			{
				
				//' Don't allow users to spoof authentication :)
				//PCS.Configuration.Common.CategorizedSettings("WebServicesDetails").Add("TestUser", My.User.CurrentPrincipal.Identity.Name, "test", False)
				//PCS.Configuration.Common.SaveSettings()
				//If passThroughAuthentication Then
				// Dim userName As String = System.Threading.Thread.CurrentPrincipal.Identity.Name
				// If userName.Contains("\") Then userName = userName.Split("\"c)(1).Trim()
				// If String.Compare(userID, userName, True) <> 0 Then Return False
				//End If
				
				try
				{
					// 04/25/2008 - PCP: Modified to use new User class contructor.
					PCS.Security.Application.User with_1 = new User(userID, password, server);
					// When not using pass through authentication, web service validates user name and password
					// otherwise only user name is used to verify user is in role and it becomes the responsibility
					// of the owning application to handle user authentication...
					if (! passThroughAuthentication && ! with_1.IsAuthenticated)
					{
						return false;
					}
					
					if (with_1.FindRole(roleName) != null)
					{
						return true;
					}
				}
				catch
				{
					
				}
				
				return false;
				
			}
			
		}
		
	}
}
