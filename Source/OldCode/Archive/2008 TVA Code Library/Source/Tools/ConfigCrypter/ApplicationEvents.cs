using System.Diagnostics;
using System;
using System.Windows.Forms;
using System.Collections;
using System.Drawing;
using Microsoft.VisualBasic;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;


namespace ConfigCrypter
{
	namespace My
	{
		
		// The following events are availble for MyApplication:
		//
		// Startup: Raised when the application starts, before the startup form is created.
		// Shutdown: Raised after all application forms are closed.  This event is not raised if the application terminates abnormally.
		// UnhandledException: Raised if the application encounters an unhandled exception.
		// StartupNextInstance: Raised when launching a single-instance application and the application is already active.
		// NetworkAvailabilityChanged: Raised when the network connection is connected or disconnected.
		partial class MyApplication
		{
			
			
			private void MyApplication_Startup(object sender, Microsoft.VisualBasic.ApplicationServices.StartupEventArgs e)
			{
				
				m_assemblyCache = new Dictionary<string, Assembly>();
				LoadAssemblyFromResource("PCS.Core");
				
			}
			
			#region " Load Embedded Assembly "
			
			private bool m_addedResolver;
			private Dictionary<string, Assembly> m_assemblyCache;
			
			public void LoadAssemblyFromResource(string assemblyName)
			{
				
				// Hook into assembly resolve event for current domain so we can load assembly from embedded resource
				if (! m_addedResolver)
				{
					AppDomain.CurrentDomain.AssemblyResolve += new System.ResolveEventHandler(ResolveAssemblyFromResource);
					m_addedResolver = true;
				}
				
				// Load the assembly (this will invoke event that will resolve assembly from resource)
				AppDomain.CurrentDomain.Load(assemblyName);
				
			}
			
			private System.Reflection.Assembly ResolveAssemblyFromResource(object sender, ResolveEventArgs e)
			{
				
				//LogMessage("Resolving assembly...")
				
				System.Reflection.Assembly resourceAssembly = null;
				string shortName = e.Name.Split(',')[0];
				
				object temp_object = resourceAssembly;
				System.Reflection.Assembly temp_SystemReflectionAssembly =  (System.Reflection.Assembly) (temp_object);
				if (! m_assemblyCache.TryGetValue(shortName, out temp_SystemReflectionAssembly))
				{
					// Loop through all of the resources in the executing assembly
					foreach (string name in System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceNames())
					{
						// See if the embedded resource name matches assembly we are trying to load
						if (string.Compare(Path.GetFileNameWithoutExtension(name), this.GetType().Assembly.GetExportedTypes()[0].Namespace + "." + shortName, true) == 0)
						{
							// If so, load embedded resource assembly into a binary buffer
							System.IO.Stream with_1 = Assembly.GetEntryAssembly().GetManifestResourceStream(name);
							int length = (int) with_1.Length;
							byte[] buffer = new byte[length];
							with_1.Read(buffer, 0, length);
							with_1.Close();
							
							// Load assembly from binary buffer
							resourceAssembly = Assembly.Load(buffer);
							m_assemblyCache.Add(shortName, resourceAssembly);
							break;
						}
					}
				}
				
				return resourceAssembly;
				
			}
			
			#endregion
			
		}
		
	}
	
	
}
