import System;
import System.IO;
import System.Web;
import System.Data.OleDb;
import System.Web.SessionState;
import System.Reflection;
import System.Runtime.InteropServices;

// James Ritchie Carroll - 2003
[assembly: AssemblyTitle("TVA .NET Code Library: VariableEvaluator")]
[assembly: AssemblyDescription("Shared .NET Variable Evaluation Code Library ")]
[assembly: AssemblyCompany("TVA")]
[assembly: AssemblyProduct("Shared .NET Code Library for TVA")]
[assembly: AssemblyCopyright("Copyright © 2003, TVA - All rights reserved")]
[assembly: AssemblyTrademark("Author: James Ritchie Carroll")]
[assembly: CLSCompliant(true)]
[assembly: Guid("1ABB1D87-010F-43B3-9A54-E74B691C008A")]
[assembly: AssemblyVersion("7.5.1.64432")]

// NOTE!  In a VB.NET project, Visual Studio doesn't support custom build steps,
// so if you make changes to this code you must "manually" compile them by running
// the "RebuildVarEval.bat" in the project directory.  Also note that running the
// project level rebuild script "RebuildAll.bat" automatically does this...

// This utility function is written in JScript so it can dynamically evaluate
// variable values when needed using the "eval" function.  This allows any
// defined variables to reference other variables and code by using JScript 
// expressions, for example: Application('RemotePath') + 'Header.aspx'
package TVA.Config
{	
	public class VariableEvaluator
	{
		// Define some properties that are useful for eval scripts
		public var Global : HttpApplication = null;
		public var Application : HttpApplicationState = null;
		public var Session : HttpSessionState = null;
		public var Variables : Object = null;
		public var Connection : OleDbConnection = null;
		private var LogFile : StreamWriter = null;
		
		public function Evaluate(VarName : String, VarType : String, VarValue : Object, LogFile : StreamWriter) : Object
		{
			var objVal : Object = null;
			
			this.LogFile = LogFile;
		
			if (String.Compare(VarType, "EVAL", true) == 0)
			{				
				try
				{
					if (VarValue == null) throw new ArgumentNullException();
					
					// Use JScript "eval()" function to execute expression based variables
					objVal = eval(VarValue, "unsafe");
					
					LogMessage(VarName + " [EVAL] = " + objVal);
				}
				catch (ex : Exception)
				{
					LogMessage("\r\n!ERROR: Failed to load value for \"" + VarName + " [EVAL]\".  JScript eval(\"" + VarValue + "\") failed - " + ex.Message + "\r\n");
				}
				
				return objVal;
			}
			else if (String.Compare(VarType, "BOOL", true) == 0)
			{
				var blnVal : Boolean = false;
					
				try
				{
					if (VarValue == null) throw new ArgumentNullException();
					
					// Cast variable value as a boolean
					if (String.Compare(VarValue, "TRUE", true) == 0)
						blnVal = true;
					else
						blnVal = Boolean(parseInt(VarValue));
					
					LogMessage(VarName + " [BOOL] = " + blnVal);
				}
				catch (ex : Exception)
				{
					LogMessage("\r\n!ERROR: Failed to load value for \"" + VarName + " [BOOL]\".  Could not cast " + VarValue + " to Boolean - " + ex.Message + "\r\n");
				}

				return blnVal;
			}
			else if (String.Compare(VarType, "INT", true) == 0)
			{
				var intVal : Int32 = 0;
					
				try
				{
					if (VarValue == null) throw new ArgumentNullException();
					
					// Cast variable value as an integer
					intVal = Int32(parseInt(VarValue));
					
					LogMessage(VarName + " [INT] = " + intVal);
				}
				catch (ex : Exception)
				{
					LogMessage("\r\n!ERROR: Failed to load value for \"" + VarName + " [INT]\".  Could not cast " + VarValue + " to Int32 - " + ex.Message + "\r\n");
				}
				
				return intVal;
			}
			else if (String.Compare(VarType, "FLOAT", true) == 0)
			{
				var sngVal : Single = 0.0;
					
				try
				{
					if (VarValue == null) throw new ArgumentNullException();
					
					// Cast variable value as a float (single)
					sngVal = Single(parseFloat(VarValue));
					
					LogMessage(VarName + " [FLOAT] = " + sngVal);
				}
				catch (ex : Exception)
				{
					LogMessage("\r\n!ERROR: Failed to load value for \"" + VarName + " [FLOAT]\".  Could not cast " + VarValue + " to Single - " + ex.Message + "\r\n");
				}
				
				return sngVal;
			}
			else if (String.Compare(VarType, "DATE", true) == 0 || String.Compare(VarType, "DATETIME", true) == 0)
			{
				var dtmVal : DateTime = null;
					
				try
				{
					if (VarValue == null) throw new ArgumentNullException();
					
					// Cast variable value as a date/time
					dtmVal = DateTime(new Date(VarValue));
					
					LogMessage(VarName + " [DATE] = " + dtmVal);
				}
				catch (ex : Exception)
				{
					LogMessage("\r\n!ERROR: Failed to load value for \"" + VarName + " [" + VarType + "]\".  Could not cast " + VarValue + " to DateTime - " + ex.Message + "\r\n");
				}

				return dtmVal;
			}
			else if (String.Compare(VarType, "DATABASE", true) == 0 || String.Compare(VarType, "OLEDB", true) == 0)
			{
				try
				{
					if (VarValue == null) throw new ArgumentNullException();
					if (Connection == null) throw new ArgumentNullException("Encountered database variable type when OleDb Connection was not initialized");
					
					// Get value from database using given scalar expression
					var cmd : OleDbCommand = new OleDbCommand(VarValue, Connection);
					objVal = cmd.ExecuteScalar();
					
					LogMessage(VarName + " [DATABASE] = " + objVal);
				}
				catch (ex : Exception)
				{
					LogMessage("\r\n!ERROR: Failed to load value for \"" + VarName + " [" + VarType + "]\".  Failed to evaluate database scalar expression \"" + VarValue + "\" - " + ex.Message + "\r\n");
				}
				
				return objVal;
			}
			else
			{
				var strVal : String = "";
				
				// Load all other variable types directly as a String
				try
				{
					// Cast variable value as a String (null values become empty strings)
					if (VarValue == null)
						strVal = "";
					else
						strVal = String(VarValue);
					
					LogMessage(VarName + " [" + VarType + "] = " + strVal);
				}
				catch (ex : Exception)
				{
					LogMessage("\r\n!ERROR: Failed to load value for \"" + VarName + " [" + VarType + "]\".  Could not cast " + VarValue + " to String - " + ex.Message + "\r\n");
				}
				
				return strVal;
			}
			
			return null;
		}
		
		private function LogMessage(Message : String)
		{
			try
			{
				if (LogFile) LogFile.WriteLine(Message);
			}
			catch (ex : Exception)
			{
				// We just keep going if we couldn't write message to log file...
			}
		}
	}
}