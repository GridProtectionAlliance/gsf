// VsPkg.cs : Implementation of TVACodeLibraryComponentInstaller
//

using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Design;
using System.Reflection;
using System.Security.Permissions;
using System.Security;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;

namespace TVA
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the registration utility (regpkg.exe) that this class needs
    // to be registered as package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    
    // A Visual Studio component can be registered under different regitry roots; for instance
    // when you debug your package you want to register it in the experimental hive. This
    // attribute specifies the registry root to use if no one is provided to regpkg.exe with
    // the /root switch.
    [DefaultRegistryRoot("Software\\Microsoft\\VisualStudio\\9.0")]
    
    // This attribute is used to register the informations needed to show the this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration(false, "#110", "#112", "2.0", IconResourceID = 400)]
    
    // In order be loaded inside Visual Studio in a machine that has not the VS SDK installed, 
    // package needs to have a valid load key (it can be requested at 
    // http://msdn.microsoft.com/vstudio/extend/). This attributes tells the shell that this 
    // package has a load key embedded in its resources.
    [ProvideLoadKey("Standard", "2.0", "TVA Code Library Component Installer", "TVA", 120)]

    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource(1000, 1)]

    // This package provides toolbox items, increment version number when adding new components so toolbox cache will be reloaded
    [ProvideToolboxItemsAttribute(1)]

    [Guid(GuidList.guidTVAComponentInstallerPkgString), DisplayName("TVA Code Library Component Installer")]
    public sealed class TVACodeLibraryComponentInstaller : Package
    {
        const String ReleaseTabName = "TVA Code Library Components";
        const String BetaTabName = "TVA Code Library Beta Components";

        //const String ReleaseAssemblyPath = "C:\\Projects\\Applications\\TVA Code Library\\Source\\bin";
        const String ReleaseAssemblyPath = "C:\\Projects\\Applications\\TVA Code Library\\Version\\4.0\\Build\\Output\\Release";
        const String BetaAssemblyPath = "C:\\Projects\\Applications\\TVA Code Library\\Version\\4.0\\Build\\Output\\Debug";

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public TVACodeLibraryComponentInstaller()
        {
            //Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

        /////////////////////////////////////////////////////////////////////////////
        // Overriden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            //Trace.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .ctc file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                // Create commands for the menu items
                CommandID menuCommandID = new CommandID(GuidList.guidTVAComponentInstallerCmdSet, (int)PkgCmdIDList.cmdidReloadReleaseComponents);
                MenuCommand menuItem = new MenuCommand(new EventHandler(ReloadReleaseComponentsToolboxTab), menuCommandID);
                mcs.AddCommand(menuItem);
                
                menuCommandID = new CommandID(GuidList.guidTVAComponentInstallerCmdSet, (int)PkgCmdIDList.cmdidReloadBetaComponents);
                menuItem = new MenuCommand(new EventHandler(ReloadBetaComponentsToolboxTab), menuCommandID);
                mcs.AddCommand(menuItem);
            }

            //Add the command handlers for menu (commands must exist in the .ctc file)
            //Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Subscribing to ToolboxInitialized and ToolboxUpgraded in Initialize() of : {0}", this.ToString()));
            ToolboxInitialized += new EventHandler(OnToolboxInitialized);
            //ToolboxUpgraded += new EventHandler(OnToolboxUpgraded);
        }
        #endregion

        private void ReloadReleaseComponentsToolboxTab(object sender, EventArgs e)
        {
            InitializeToolBoxTab(ReleaseTabName, ReleaseAssemblyPath);
        }

        private void ReloadBetaComponentsToolboxTab(object sender, EventArgs e)
        {
            InitializeToolBoxTab(BetaTabName, BetaAssemblyPath);
        }

        private void OnToolboxInitialized(object sender, EventArgs e)
        {
            // We intially load release build components from default locations - user can
            // manually override and load beta build components from menu
            CreateToolBoxTab(ReleaseTabName, ReleaseAssemblyPath);
        }

        private void ShowMessageBox(string message)
        {
            IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
            Guid clsid = Guid.Empty;
            int result;
            uiShell.ShowMessageBox(
                       0,
                       ref clsid,
                       "TVA Code Library Component Installer",
                       message,
                       string.Empty,
                       0,
                       OLEMSGBUTTON.OLEMSGBUTTON_OK,
                       OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                       OLEMSGICON.OLEMSGICON_INFO,
                       0,        // false
                       out result);
        }

        private void InitializeToolBoxTab(string toolBoxTabName, string assemblyPath)
        {
            VerifyParameters verifyParametersDialog = new VerifyParameters();

            verifyParametersDialog.Text = string.Format(verifyParametersDialog.Text, toolBoxTabName);
            verifyParametersDialog.TextBoxTabName.Text = toolBoxTabName;
            verifyParametersDialog.TextBoxAssemblyPath.Text = assemblyPath;

            if (verifyParametersDialog.ShowDialog() == DialogResult.OK)
            {
                // Get any updated parameters
                toolBoxTabName = verifyParametersDialog.TextBoxTabName.Text;
                assemblyPath = verifyParametersDialog.TextBoxAssemblyPath.Text;

                // Create toolbox
                CreateToolBoxTab(toolBoxTabName, assemblyPath);
            }
        }

        // Use reflection to fill a list of ToolboxItems from assemblies in the specified
        // path to be used in OnToolboxInitialized and OnToolboxUpgraded
        private void CreateToolBoxTab(string toolBoxTabName, string assemblyPath)
        {
            ProgressDialog progress = null;

            try
            {
                if (Directory.Exists(assemblyPath))
                {
                    // Scan each type in the assembly, determine if it is a valid toolbox item implementation
                    // If so, create the item and add it to our ToolboxItemList
                    List<ToolboxItem> toolboxItems = new List<ToolboxItem>();
                    string[] assemblyNames = Directory.GetFiles(assemblyPath, "*.dll");

                    if (assemblyNames.Length > 0)
                    {
                        //// Create application domain setup information.
                        //AppDomainSetup domainInfo = new AppDomainSetup();

                        //domainInfo.ApplicationBase = assemblyPath;
                        //domainInfo.PrivateBinPath = assemblyPath;

                        //// We create a seperate application domain for each toolbox tab creation because the
                        //// "Assembly.LoadFrom" method will return an existing assembly if called again on
                        //// another tab (not what you want if you are loading beta and release build tabs)
                        //AppDomain reflectionDomain = AppDomain.CreateDomain("ReflectionArea", AppDomain.CurrentDomain.Evidence, domainInfo, new PermissionSet(PermissionState.Unrestricted), null);

                        
                        //Type assemblyType = reflectionDomain.GetType().Assembly.GetType(typeof(Assembly).FullName);                       

                        //foreach (Assembly domainAssembly in reflectionDomain.GetAssemblies())
                        //{
                        //    if (domainAssembly.FullName.StartsWith("mscorlib"))
                        //    {
                        //        // Found ms core library - get the "Assembly" type
                        //        assemblyType = domainAssembly.GetType(typeof(Assembly).FullName);
                        //        break;
                        //    }
                        //}

                        // Setup a new progress dialog
                        progress = new ProgressDialog();
                        int step = 0;

                        // Display progress dialog...                      
                        progress.Show();

                        foreach (String assemblyName in assemblyNames)
                        {
                            progress.UpdateProgress(string.Format("Scanning \"{0}\" for components...", Path.GetFileName(assemblyName)), ++step, assemblyNames.Length);                            

                            // Invoke static "LoadFrom" method on Assembly type loaded into reflection domain
                            //Assembly assembly = (Assembly)assemblyType.InvokeMember("LoadFrom", BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public, null, null, new object[] { assemblyName });
                            Assembly assembly = Assembly.LoadFrom(assemblyName);

                            if (assembly != null)
                            {
                                foreach (Type possibleItem in assembly.GetTypes())
                                {
                                    // Check that the type meets criteria to be used for implementation of a toolboxItem
                                    if (!typeof(IComponent).IsAssignableFrom(possibleItem) ||  // Must implement IComponent
                                        possibleItem.IsAbstract)                               // Must not be abstract class
                                    {
                                        continue;
                                    }

                                    // Verify that that the usercontrol has appropriate constructors 
                                    // either default, or taking an argument
                                    if ((possibleItem.GetConstructor(new Type[] { typeof(Type) })) == null &&
                                         (possibleItem.GetConstructor(new Type[0])) == null)
                                    {
                                        continue;
                                    }

                                    // Get Attribute of candidate class
                                    AttributeCollection attribs = TypeDescriptor.GetAttributes(possibleItem);
                                    ToolboxItemAttribute tba = attribs[typeof(ToolboxItemAttribute)] as ToolboxItemAttribute;
                                    ToolboxItem item = null;
                                    if (tba != null && !tba.Equals(ToolboxItemAttribute.None))
                                    {
                                        if (!tba.IsDefaultAttribute())
                                        {
                                            // Handle the case for custom ToolBoxItem implementation.
                                            item = null;
                                            Type itemType = tba.ToolboxItemType;

                                            // Get the constructor for the ToolboxItem applied to the user control.
                                            ConstructorInfo ctor = itemType.GetConstructor(new Type[] { typeof(Type) });
                                            if (ctor != null && itemType != null)
                                            {
                                                item = (ToolboxItem)ctor.Invoke(new object[] { possibleItem });
                                            }
                                            else
                                            {
                                                ctor = itemType.GetConstructor(new Type[0]);
                                                if (ctor != null)
                                                {
                                                    item = (ToolboxItem)ctor.Invoke(new object[0]);
                                                    item.Initialize(possibleItem);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // Handle items created using the default constructor.
                                            item = new ToolboxItem(possibleItem);
                                        }
                                    }
                                    if (item != null)
                                    {
                                        // If there is a DisplayNameAttribute, use that as Display Name, otherwise the default name is used.
                                        DisplayNameAttribute displayName = attribs[typeof(DisplayNameAttribute)] as DisplayNameAttribute;
                                        if (displayName != null && !displayName.IsDefaultAttribute())
                                        {
                                            item.DisplayName = displayName.DisplayName;
                                        }

                                        toolboxItems.Add(item);
                                    }
                                }
                            }
                        }

                        // Load any found components onto toolbox tab - deleting any existing tab with the same name
                        if (toolboxItems.Count > 0)
                        {
                            progress.UpdateProgress("Removing old tab...", 0, 1);

                            // Add new instances of all ToolboxItems contained in ToolboxItemList
                            IToolboxService toolboxService = GetService(typeof(IToolboxService)) as IToolboxService;
                            IVsToolbox toolbox = GetService(typeof(IVsToolbox)) as IVsToolbox;

                            toolboxService.Refresh();

                            //// Remove target tab and all controls under it.
                            //foreach (ToolboxItem oldItem in toolboxService.GetToolboxItems(toolBoxTabName))
                            //{
                            //    toolboxService.RemoveToolboxItem(oldItem);
                            //}

                            toolbox.RemoveTab(toolBoxTabName);
                            
                            progress.UpdateProgress("Creating new tab...", 1, 1);
                            toolbox.AddTab(toolBoxTabName);
                            step = 0;

                            foreach (ToolboxItem itemFromList in toolboxItems)
                            {
                                progress.UpdateProgress(string.Format("Adding toolbox component \"{0}\"...", itemFromList.DisplayName), ++step, toolboxItems.Count);
                                toolboxService.AddToolboxItem(itemFromList, toolBoxTabName);

                                //Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Adding item {0} using control {1} to tab {2} on the Toolbox", itemFromList.ToString(), itemFromList.TypeName, TabCategoryName));

                                // Set the current Tab and item as selected.
                                toolboxService.SelectedCategory = toolBoxTabName;
                            }

                            toolboxService.Refresh();
                        }
                        else
                        {
                            // No components found in any of the assemblies
                            if (progress != null) progress.Hide();
                            ShowMessageBox(string.Format("Unable to refresh {0} toolbox tab. No components were found in the assemblies at \"{1}\".", toolBoxTabName, assemblyPath));
                        }

                        // Unload temporary application domain
                        //AppDomain.Unload(reflectionDomain);
                    }
                    else
                    {
                        // No DLL files found in assembly path
                        if (progress != null) progress.Hide();
                        ShowMessageBox(string.Format("Unable to refresh {0} toolbox tab. No assemblies found in code library path \"{1}\".", toolBoxTabName, assemblyPath));
                    }
                }
                else
                {
                    // Assembly path does not exist
                    if (progress != null) progress.Hide();
                    ShowMessageBox(string.Format("Unable to refresh {0} toolbox tab. Code library path \"{1}\" may not be accessible. Please try again later.", toolBoxTabName, assemblyPath));
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (progress != null) progress.Close();
            }
        }

        /*
        private void OnToolboxUpgraded(object sender, EventArgs e)
        {
            IToolboxService toolboxService = GetService(typeof(IToolboxService)) as IToolboxService;

            // Remove all current items not in upgrade item list
            foreach (ToolboxItem currentItem in toolboxService.GetToolboxItems(TabCategoryName))
            {
                toolboxService.RemoveToolboxItem(currentItem);
            }

            // Add Upgraded items.
            foreach (ToolboxItem newItem in m_toolBoxItems)
            {
                toolboxService.AddToolboxItem(newItem);
            }

            toolboxService.Refresh();
        }
        */
    }
}