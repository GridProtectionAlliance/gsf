This project should only contain code that can be loaded inside of SQL Server. 
That means that only the following libraries can be referenced in the list of 
references.

CustomMarshalers
Microsoft.VisualBasic
Microsoft.VisualC
mscorlib
System
System.Configuration
System.Data
System.Data.OracleClient
System.Data.SqlXml
System.Deployment
System.Security
System.Transactions
System.Web.Services
System.Xml
System.Core.dll
System.Xml.Linq.dll

While it might be possible to host other libraries within SQL, it is strongly
discouraged. Any exception will need to be agreed upon by the principal developers
of the GSF code base.