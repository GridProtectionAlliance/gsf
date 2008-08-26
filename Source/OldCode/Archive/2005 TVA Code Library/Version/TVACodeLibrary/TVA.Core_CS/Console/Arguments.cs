using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;

//*******************************************************************************************************
//  TVA.Console.Arguments.vb - Command Line Parameter Parsing Class
//  Copyright © 2007 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  03/28/2006 - Pinal C. Patel
//       Migrated 2.0 version of source code from 1.1 source (TVA.Console)
//  10/09/2007 - J. Ritchie Carroll / Pinal C. Patel
//       Fixed stand-alone argument bug at end of line and changed class to use generic Dictionary class
//
//*******************************************************************************************************


namespace TVA
{
	namespace Console
	{
		
		[Serializable()]public class Arguments : IEnumerable
		{
			
			
			private string m_commandLine;
			private string m_orderedArgID;
			private int m_orderedArgCount;
			private Dictionary<string, string> m_parameters;
			
			public Arguments(string commandLine) : this(commandLine, "OrderedArg")
			{
				
				
			}
			
			public Arguments(string commandLine, string orderedArgID)
			{
				
				Regex spliter = new Regex("^-{1,2}|^/|=|:", RegexOptions.IgnoreCase || RegexOptions.Compiled);
				Regex remover = new Regex("^[\'\"]?(.*?)[\'\"]?$", RegexOptions.IgnoreCase || RegexOptions.Compiled);
				string parameter;
				string[] parts;
				
				m_parameters = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
				m_orderedArgCount = 0;
				m_commandLine = commandLine;
				m_orderedArgID = orderedArgID;
				
				// Valid parameters forms:
				//   {-,/,--}param{=,:}((",')value(",'))
				// Examples:
				//   -param1=value1 --param2 /param3:"Test-:-work"
				//   /param4=happy -param5 '--=nice=--'
				foreach (string arg in Common.ParseCommand(m_commandLine))
				{
					// Found just a parameter in last pass...
					// The last parameter is still waiting, with no value, set it to nothing.
					if (parameter != null)
					{
						if (! m_parameters.ContainsKey(parameter))
						{
							m_parameters.Add(parameter, null);
						}
					}
					parameter = null;
					
					if (! string.IsNullOrEmpty(arg))
					{
						// If this argument begins with a quote, we treat it as a stand-alone argument
						if (arg[0] == '\"' || arg[0] == '\'')
						{
							// Handle stand alone ordered arguments
							m_orderedArgCount++;
							parameter = orderedArgID + m_orderedArgCount;
							
							// Remove possible enclosing characters (",')
							if (! m_parameters.ContainsKey(parameter))
							{
								arg = remover.Replace(arg, "$1");
								m_parameters.Add(parameter, arg);
							}
							
							parameter = null;
						}
						else
						{
							// Look for new parameters (-,/ or --) and a
							// possible enclosed value (=,:)
							parts = spliter.Split(arg, 3);
							
							switch (parts.Length)
							{
								case 1:
									// Handle stand alone ordered arguments
									m_orderedArgCount++;
									parameter = orderedArgID + m_orderedArgCount;
									
									// Remove possible enclosing characters (",')
									if (! m_parameters.ContainsKey(parameter))
									{
										arg = remover.Replace(arg, "$1");
										m_parameters.Add(parameter, arg);
									}
									
									parameter = null;
									break;
								case 2:
									// Found just a parameter
									parameter = parts[1];
									break;
								case 3:
									// Parameter with enclosed value
									parameter = parts[1];
									
									// Remove possible enclosing characters (",')
									if (! m_parameters.ContainsKey(parameter))
									{
										parts[2] = remover.Replace(parts[2], "$1");
										m_parameters.Add(parameter, parts[2]);
									}
									
									parameter = null;
									break;
							}
						}
					}
				}
				
				// In case a parameter is still waiting
				if (parameter != null)
				{
					if (! m_parameters.ContainsKey(parameter))
					{
						m_parameters.Add(parameter, null);
					}
				}
				
			}
			
			// Retrieve a parameter value if it exists
			public virtual string this[string param]
			{
				get
				{
					return m_parameters(param);
				}
			}
			
			public virtual bool Exists(string param)
			{
				return m_parameters.ContainsKey(param);
			}
			
			public virtual int Count
			{
				get
				{
					return m_parameters.Count;
				}
			}
			
			public virtual string OrderedArgID
			{
				get
				{
					return m_orderedArgID;
				}
			}
			
			public virtual int OrderedArgCount
			{
				get
				{
					return m_orderedArgCount;
				}
			}
			
			public virtual bool ContainsHelpRequest
			{
				get
				{
					return (m_parameters.ContainsKey("") || m_parameters.ContainsKey("Help"));
				}
			}
			
			public IEnumerator GetEnumerator()
			{
				
				return m_parameters.GetEnumerator();
				
			}
			
			public override string ToString()
			{
				
				return m_commandLine;
				
			}
			
			protected Dictionary<string, string> InternalDictionary
			{
				get
				{
					return m_parameters;
				}
			}
			
		}
		
	}
}
