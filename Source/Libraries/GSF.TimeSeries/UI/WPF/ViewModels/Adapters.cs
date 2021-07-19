//******************************************************************************************************
//  Adapters.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/05/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using GSF.IO;
using GSF.Reflection;
using GSF.TimeSeries.Adapters;
using GSF.TimeSeries.UI.Commands;
using GSF.TimeSeries.UI.DataModels;

// ReSharper disable VirtualMemberCallInConstructor
namespace GSF.TimeSeries.UI.ViewModels
{
    /// <summary>
    /// Class to hold bindable <see cref="Adapter"/> collection and current selection information for UI.
    /// </summary>
    internal class Adapters : PagedViewModelBase<Adapter, int>
    {
        #region [ Members ]

        // Nested Types
        public class AdapterTypeDescription
        {
            public string Header { get; set; }

            public string Description { get; set; }

            public Visibility HeaderVisibility => 
                Header is null ? Visibility.Collapsed : Visibility.Visible;

            public override string ToString() => 
                Header is null ? Description : $"{Header}: {Description}";
        }

        // Fields
        private List<Tuple<Type, AdapterTypeDescription>> m_adapterTypeList;
        private List<AdapterConnectionStringParameter> m_parameterList;
        private readonly AdapterType m_adapterType;
        private string m_searchDirectory;
        private AdapterConnectionStringParameter m_selectedParameter;
        private RelayCommand m_initializeCommand;
        private string m_runtimeID;

        private bool m_suppressParameterValueUpdates;
        private bool m_suppressConnectionStringUpdates;

        #endregion

        #region [ Constructor ]

        /// <summary>
        /// Creates an instance of <see cref="Adapters"/> class.
        /// </summary>
        /// <param name="itemsPerPage">Integer value to determine number of items per page.</param>
        /// <param name="autoSave">Boolean value to determine is user changes should be saved automatically.</param>
        /// <param name="adapterType"><see cref="AdapterType"/> to determine type.</param>
        public Adapters(int itemsPerPage, AdapterType adapterType, bool autoSave = true)
            : base(0, autoSave) // Set items per page to zero to avoid load in the base class.
        {
            AppDomain.CurrentDomain.AssemblyResolve += AppDomain_AssemblyResolve;
            ItemsPerPage = itemsPerPage;

            // Must attach to PropertyChanged event after setting m_adapterType!
            // Do not override OnPropertyChanged or you will regret it!
            m_adapterType = adapterType;
            PropertyChanged += Adapters_PropertyChanged;

            SearchDirectory = FilePath.GetAbsolutePath("").EnsureEnd(Path.DirectorySeparatorChar);
            Load();
        }

        #endregion

        #region [ Properties ]

        public string RuntimeID
        {
            get => m_runtimeID;
            set
            {
                m_runtimeID = value;
                OnPropertyChanged("RuntimeID");
            }
        }

        /// <summary>
        /// Gets flag that determines if <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/> is a new record.
        /// </summary>
        public override bool IsNewRecord => CurrentItem.ID == 0;

        /// <summary>
        /// Gets or sets the collection containing the adapter types found
        /// in the assemblies residing in the <see cref="SearchDirectory"/>.
        /// </summary>
        public List<Tuple<Type, AdapterTypeDescription>> AdapterTypeList
        {
            get => m_adapterTypeList;
            set
            {
                m_adapterTypeList = value;
                OnPropertyChanged("AdapterTypeList");
            }
        }

        /// <summary>
        /// Gets or sets the index of the selected item in the <see cref="AdapterTypeList"/>.
        /// </summary>
        public int AdapterTypeSelectedIndex
        {
            get
            {
                int index = m_adapterTypeList
                    .Select(tuple => tuple.Item1)
                    .TakeWhile(type => type.FullName != CurrentItem.TypeName)
                    .Count();

                if (index == m_adapterTypeList.Count)
                    index = -1;

                return index;
            }
            set
            {
                if (value >= 0 && value < AdapterTypeList.Count)
                    CurrentItem.TypeName = AdapterTypeList[value].Item1.FullName;

                OnAdapterTypeSelectedIndexChanged();
            }
        }

        /// <summary>
        /// Gets additional information about the type selected from the drop-down.
        /// </summary>
        public string TypeInfo => string.IsNullOrEmpty(CurrentItem.TypeName) ? "Adapter Type" : 
            $"Adapter Type: {CurrentItem.TypeName} from {Path.GetFileName(CurrentItem.AssemblyName)}";

        /// <summary>
        /// Gets or sets the list of <see cref="Adapter.ConnectionString"/> parameters found by
        /// searching the class defined by <see cref="Adapter.AssemblyName"/> and
        /// <see cref="Adapter.TypeName"/> plus the parameters defined by the ConnectionString.
        /// </summary>
        public List<AdapterConnectionStringParameter> ParameterList
        {
            get => m_parameterList;
            set
            {
                try
                {
                    // Attempting to update parameter values without suppressing connection
                    // string updates would result in an infinite loop. We should not be modifying
                    // the connection string directly at the same time the user is, anyway.
                    m_suppressConnectionStringUpdates = true;

                    // Getting this ahead of time allows us to reselect the previously
                    // selected parameter if it still exists in the parameter list.
                    AdapterConnectionStringParameter selectedParameter = m_selectedParameter;

                    m_parameterList = value;
                    OnPropertyChanged("ParameterList");

                    // Reselect the previously selected parameter, and update
                    // the connection string parameters if necessary.
                    UpdateConnectionStringParameters(m_parameterList, CurrentItem.ConnectionString.ToNonNullString().ParseKeyValuePairs());
                    SelectedParameter = selectedParameter;
                }
                finally
                {
                    // Indicate that we want to stop suppressing connection string updates.
                    m_suppressConnectionStringUpdates = false;
                }
            }
        }

        /// <summary>
        /// Gets or sets the directory in which the application will search for
        /// assemblies containing adapters. The resulting list of adapters populates
        /// the <see cref="AdapterTypeList"/>.
        /// </summary>
        public string SearchDirectory
        {
            get => m_searchDirectory;
            set
            {
                if (m_searchDirectory?.Equals(value, StringComparison.OrdinalIgnoreCase) ?? false)
                    return;

                m_searchDirectory = value;
                OnPropertyChanged("SearchDirectory");

                if (string.IsNullOrEmpty(m_searchDirectory) || Directory.Exists(m_searchDirectory))
                {
                    // When the adapter type list is updated, the selected type will change.
                    // Since that is bound to the type name which in turn updates the
                    // assembly name, we need to store the current values ahead of time
                    // and restore them after updating the adapter type list.
                    string assemblyName = CurrentItem.AssemblyName;
                    string typeName = CurrentItem.TypeName;
                    string ext = string.IsNullOrEmpty(m_searchDirectory) ? string.Empty : Path.DirectorySeparatorChar.ToString();

                    AdapterTypeList = GetAdapterTypeList(m_searchDirectory + ext, GetAdapterInterfaceType(m_adapterType));
                    CurrentItem.TypeName = typeName;
                    CurrentItem.AssemblyName = assemblyName;
                }
            }
        }

        /// <summary>
        /// Gets or sets the currently selected parameter from the
        /// <see cref="ParameterList"/>. This is used to reselect previously
        /// selected parameters when the ParameterList is updated.
        /// </summary>
        public AdapterConnectionStringParameter SelectedParameter
        {
            get => m_selectedParameter;
            set
            {
                if (!(m_selectedParameter is null))
                    m_selectedParameter.PropertyChanged -= ConnectionStringParameter_PropertyChanged;

                m_selectedParameter = value;

                if (!(m_selectedParameter is null))
                    m_selectedParameter.PropertyChanged += ConnectionStringParameter_PropertyChanged;

                OnPropertyChanged("SelectedParameter");
            }
        }

        /// <summary>
        /// Gets the command whose action is evoked when the user clicks the initialize button.
        /// </summary>
        public ICommand InitializeCommand => m_initializeCommand ?? (m_initializeCommand = new RelayCommand(InitializeAdapter, () => CanSave));

        /// <summary>
        /// Determines whether the custom configuration button is visible.
        /// </summary>
        public Visibility CustomConfigurationButtonVisibility => AdapterTypeSelectedIndex >= 0 && 
            AdapterTypeList[AdapterTypeSelectedIndex].Item1.TryGetAttribute(out CustomConfigurationEditorAttribute _) ? 
                Visibility.Visible : 
                Visibility.Collapsed;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the primary key value of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.
        /// </summary>
        /// <returns>The primary key value of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.</returns>
        public override int GetCurrentItemKey() => 
            CurrentItem.ID;

        /// <summary>
        /// Gets the string based named identifier of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.
        /// </summary>
        /// <returns>The string based named identifier of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.</returns>
        public override string GetCurrentItemName() => 
            CurrentItem.AdapterName;

        /// <summary>
        /// Creates a new instance of <see cref="Historian"/> and assigns it to CurrentItem.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            CurrentItem.Type = m_adapterType;
            ParameterList = null;
        }

        /// <summary>
        /// Loads collection of <see cref="Adapter"/> defined in the database.
        /// </summary>
        public override void Load()
        {
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                if (ItemsKeys is null)
                {
                    ItemsKeys = Adapter.LoadIDs(null, m_adapterType, SortMember, SortDirection);

                    if (!(SortSelector is null))
                    {
                        ItemsKeys = SortDirection == "ASC" ? 
                            ItemsKeys.OrderBy(SortSelector).ToList() : 
                            ItemsKeys.OrderByDescending(SortSelector).ToList();
                    }
                }

                List<int> pageKeys = ItemsKeys.Skip((CurrentPageNumber - 1) * ItemsPerPage).Take(ItemsPerPage).ToList();
                ItemsSource = Adapter.Load(null, m_adapterType, pageKeys);
                CurrentItem.Type = m_adapterType;
            }
            catch (Exception ex)
            {
                if (ex.InnerException is null)
                {
                    Popup(ex.Message, $"Load {DataModelName} Exception:", MessageBoxImage.Error);
                    CommonFunctions.LogException(null, $"Load {DataModelName}", ex);
                }
                else
                {
                    Popup($"{ex.Message}{Environment.NewLine}Inner Exception: {ex.InnerException.Message}", $"Load {DataModelName} Exception:", MessageBoxImage.Error);
                    CommonFunctions.LogException(null, $"Load {DataModelName}", ex.InnerException);
                }
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        /// <summary>
        /// Deletes associated <see cref="Adapter"/> record.
        /// </summary>
        public override void Delete()
        {
            if (!CanDelete || !Confirm($"Are you sure you want to delete '{GetCurrentItemName()}'?", $"Delete {DataModelName}"))
                return;

            try
            {
                if (OnBeforeDeleteCanceled())
                    throw new OperationCanceledException("Delete was canceled.");

                int currentItemKey = GetCurrentItemKey();
                string result = Adapter.Delete(null, m_adapterType, GetCurrentItemKey());
                ItemsKeys.Remove(currentItemKey);

                OnDeleted();

                Load();
                DisplayStatusMessage(result);
            }
            catch (Exception ex)
            {
                if (ex.InnerException is null)
                {
                    Popup(ex.Message, $"Delete {DataModelName} Exception:", MessageBoxImage.Error);
                    CommonFunctions.LogException(null, $"Delete {DataModelName}", ex);
                }
                else
                {
                    Popup($"{ex.Message}{Environment.NewLine}Inner Exception: {ex.InnerException.Message}", $"Delete {DataModelName} Exception:", MessageBoxImage.Error);
                    CommonFunctions.LogException(null, $"Delete {DataModelName}", ex.InnerException);
                }
            }
        }

        /// <summary>
        /// Unloads the <see cref="Adapters"/> class.
        /// </summary>
        public void Unload() => 
            AppDomain.CurrentDomain.AssemblyResolve -= AppDomain_AssemblyResolve;

        /// <summary>
        /// Handles PropertyChanged event on CurrentItem.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        protected override void m_currentItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.m_currentItem_PropertyChanged(sender, e);

            switch (e.PropertyName)
            {
                // Occurs when CurrentItem.TypeName changes.
                case "TypeName":
                {
                    // If there exists a type in the adapter type list that
                    // matches the type name, also update the assembly name.
                    Type selectedType = m_adapterTypeList
                                        .Select(tuple => tuple.Item1)
                                        .SingleOrDefault(type => type.FullName == CurrentItem.TypeName);

                    if (!(selectedType is null))
                    {
                        string pattern = Regex.Escape(FilePath.GetAbsolutePath("").EnsureEnd(Path.DirectorySeparatorChar));
                        CurrentItem.AssemblyName = Regex.Replace(selectedType.Assembly.Location, $"^{pattern}", "", RegexOptions.IgnoreCase);
                    }

                    // Also update the parameter list. Since the type has changed,
                    // the current list of parameters generated by searching the
                    // selected type is no longer valid.
                    ParameterList = GetParameterList(CurrentItem.AssemblyName, CurrentItem.TypeName);

                    OnAdapterTypeSelectedIndexChanged();
                    OnPropertyChanged("TypeInfo");
                    break;
                }
                // Occurs when CurrentItem.AssemblyName changes.
                case "AssemblyName":
                // Occurs when CurrentItem.ConnectionString changes.
                // When the connection string changes, we need to keep all related
                // elements synchronized. Since we can't easily determine exactly
                // what has changed, simply update everything.
                case "ConnectionString":
                    // When the assembly name changes, we may be able to
                    // find connection string parameters in the class defined
                    // by the assembly name and type name.
                    ParameterList = GetParameterList(CurrentItem.AssemblyName, CurrentItem.TypeName);
                    break;
            }
        }

        /// <summary>
        /// Handles PropertyChanged event on <see cref="SelectedParameter"/>.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        protected virtual void ConnectionStringParameter_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (m_suppressConnectionStringUpdates || !(sender is AdapterConnectionStringParameter parameter) || parameter.Value is null || e.PropertyName != "Value")
                return;

            try
            {
                m_suppressParameterValueUpdates = true;

                // The easiest way to update is to break the connection string into key
                // value pairs, update the value of the pair corresponding to the parameter
                // that fired the event, and then rejoin the key value pairs.
                Dictionary<string, string> settings = CurrentItem.ConnectionString.ToNonNullString().ParseKeyValuePairs();
                settings[parameter.Name] = parameter.Value.ToString();

                // Build the new connection string and validate that it can still be parsed
                string connectionString = settings.JoinKeyValuePairs();
                connectionString.ParseKeyValuePairs();

                CurrentItem.ConnectionString = connectionString;
            }
            finally
            {
                m_suppressParameterValueUpdates = false;
            }
        }

        private void Adapters_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "CurrentItem")
                return;

            if (CurrentItem is null)
            {
                RuntimeID = string.Empty;
            }
            else
            {
                switch (m_adapterType)
                {
                    case AdapterType.Action:
                        RuntimeID = CommonFunctions.GetRuntimeID("CustomActionAdapter", CurrentItem.ID);
                        break;
                    case AdapterType.Filter:
                        RuntimeID = CommonFunctions.GetRuntimeID("CustomFilterAdapter", CurrentItem.ID);
                        break;
                    case AdapterType.Input:
                        RuntimeID = CommonFunctions.GetRuntimeID("CustomInputAdapter", CurrentItem.ID);
                        break;
                    case AdapterType.Output:
                        RuntimeID = CommonFunctions.GetRuntimeID("CustomOutputAdapter", CurrentItem.ID);
                        break;
                    default:
                        RuntimeID = string.Empty;
                        break;
                }

                // Modify search path to the path of the assembly of the selected item
                SearchDirectory = FilePath.GetDirectoryName(FilePath.GetAbsolutePath(CurrentItem.AssemblyName ?? ""));

                // If the current item changes, but the connection string does not, the
                // parameter list and parameters won't be updated. We take care of that here.
                ParameterList = GetParameterList(CurrentItem.AssemblyName, CurrentItem.TypeName);
            }

            OnAdapterTypeSelectedIndexChanged();
            OnPropertyChanged("TypeInfo");
        }

        private void InitializeAdapter()
        {
            try
            {
                if (Confirm($"Do you want to Initialize {GetCurrentItemName()}?", "Confirm Initialize"))
                    Popup(CommonFunctions.SendCommandToService($"Initialize {RuntimeID}"), "Initialize", MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Popup($"ERROR: {ex.Message}", "Failed To Initialize", MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Gets the interface type of the adapter based on
        /// the given <see cref="AdapterType"/>.
        /// </summary>
        /// <param name="type">The adapter type.</param>
        /// <returns>The interface type corresponding to the adapter type.</returns>
        private Type GetAdapterInterfaceType(AdapterType type)
        {
            switch (type)
            {
                case AdapterType.Filter:
                    return typeof(IFilterAdapter);
                case AdapterType.Input:
                    return typeof(IInputAdapter);
                case AdapterType.Action:
                    return typeof(IActionAdapter);
                case AdapterType.Output:
                    return typeof(IOutputAdapter);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Searches the <paramref name="searchDirectory"/> for assemblies containing
        /// implementations of the given adapter type. It then returns a collection
        /// containing all the implementations it found as well as their descriptions.
        /// </summary>
        /// <param name="searchDirectory">The directory in which to search for assemblies.</param>
        /// <param name="adapterType">The type to be searched for in the assemblies.</param>
        /// <returns>The collection of types found as well as their descriptions.</returns>
        private List<Tuple<Type, AdapterTypeDescription>> GetAdapterTypeList(string searchDirectory, Type adapterType)
        {
            return adapterType
                .LoadImplementations(searchDirectory, true, false, true)
                .Distinct()
                .Where(type => GetEditorBrowsableState(type) == EditorBrowsableState.Always)
                .Select(type => Tuple.Create(type, GetDescription(type)))
                .OrderByDescending(pair => pair.Item1.TryGetAttribute(out DescriptionAttribute _))
                .ThenBy(pair => pair.Item2.ToString())
                .ToList();
        }

        /// <summary>
        /// Gets the editor browsable state of the given type. This method will
        /// search for a <see cref="EditorBrowsableAttribute"/> using reflection.
        /// If none is found, it will default to <see cref="EditorBrowsableState.Always"/>.
        /// </summary>
        /// <param name="type">The type for which an editor browsable state is found.</param>
        /// <returns>
        /// Either the editor browsable state as defined by an <see cref="EditorBrowsableAttribute"/>
        /// or else <see cref="EditorBrowsableState.Always"/>.
        /// </returns>
        private EditorBrowsableState GetEditorBrowsableState(Type type) => 
            type.TryGetAttribute(out EditorBrowsableAttribute editorBrowsableAttribute) ? 
                editorBrowsableAttribute.State : 
                EditorBrowsableState.Always;

        /// <summary>
        /// Gets a description of the given type. This method will search for a
        /// <see cref="DescriptionAttribute"/> using reflection. If none is found,
        /// it will default to the <see cref="Type.FullName"/> of the type.
        /// </summary>
        /// <param name="type">The type for which a description is found.</param>
        /// <returns>
        /// Either the description as defined by a <see cref="DescriptionAttribute"/>
        /// or else the <see cref="Type.FullName"/> of the parameter.
        /// </returns>
        private static AdapterTypeDescription GetDescription(Type type)
        {
            AdapterTypeDescription adapterTypeDescription = new AdapterTypeDescription();

            string[] splitDescription = type.TryGetAttribute(out DescriptionAttribute descriptionAttribute) ? 
                descriptionAttribute.Description.ToNonNullNorEmptyString(type.FullName).Split(':') : 
                new[] { type.FullName };

            if (splitDescription.Length > 1)
            {
                adapterTypeDescription.Header = splitDescription[0].Trim();
                adapterTypeDescription.Description = splitDescription[1].Trim();
            }
            else
            {
                adapterTypeDescription.Description = splitDescription[0].Trim();
            }

            return adapterTypeDescription;
        }

        /// <summary>
        /// Gets the list of connection string parameters found in the assembly
        /// defined by the given assembly name and type name. Also included in
        /// the list are the keys defined in the <see cref="Adapter.ConnectionString"/>
        /// which do not match parameters found in the aforementioned type.
        /// </summary>
        /// <param name="assemblyName">The name of the assembly in which the type resides.</param>
        /// <param name="typeName">The name of the type to be found in the assembly.</param>
        /// <returns>
        /// The list of connection string parameters found by
        /// searching the given type as well as the connection string.
        /// </returns>
        private List<AdapterConnectionStringParameter> GetParameterList(string assemblyName, string typeName)
        {
            if (!(assemblyName is null) && !(typeName is null))
            {
                // For convenience, start by searching the type
                // list for a type matching the parameters.
                Type adapterType = m_adapterTypeList
                    .Select(tuple => tuple.Item1)
                    .Where(type => type.Assembly.Location.Equals(FilePath.GetAbsolutePath(assemblyName), StringComparison.OrdinalIgnoreCase))
                    .SingleOrDefault(type => type.FullName == typeName);

                // Attempt to find that assembly and retrieve the type.
                if (adapterType is null && File.Exists(assemblyName))
                    adapterType = Assembly.LoadFrom(assemblyName).GetType(typeName);

                if (!(adapterType is null))
                {
                    // Get the list of properties with ConnectionStringParameterAttribute annotations.
                    IEnumerable<PropertyInfo> infoList = adapterType.GetProperties()
                        .Where(info => info.TryGetAttribute(typeof(ConnectionStringParameterAttribute).FullName, out Attribute _));

                    // Get the list of connection string keys which do not match
                    // the names of the properties in the previously obtained list.
                    IEnumerable<string> keyList = CurrentItem.ConnectionString
                        .ToNonNullString().ParseKeyValuePairs().Keys
                        .Where(key => !infoList.Any(info => info.Name.Equals(key, StringComparison.OrdinalIgnoreCase)));

                    // Convert both lists into ConnectionStringParameter lists, combine
                    // the two lists, and then order them lexically while giving precedence
                    // to "required" parameters (those lacking a default value).
                    return infoList.Select(GetParameter)
                        .Union(keyList.Select(GetParameter))
                        .OrderByDescending(parameter => parameter.IsRequired)
                        .ThenBy(parameter => parameter.Name)
                        .ToList();
                }
            }

            // If the adapter type could not be found, then we
            // can only search the connection string for parameters.
            return CurrentItem.ConnectionString.ToNonNullString().ParseKeyValuePairs()
                .Select(pair => GetParameter(pair.Key))
                .OrderBy(parameter => parameter.Name)
                .ToList();
        }

        /// <summary>
        /// Retrieves an existing parameter from the <see cref="ParameterList"/>. If no
        /// parameter exists with a name that matches the given <see cref="MemberInfo.Name"/>,
        /// then a new one is created. There is also a special case in which the parameter is
        /// already defined, but no <see cref="PropertyInfo"/> exists for it. In that case,
        /// the property info is added, as well as any other new information, and the parameter
        /// is returned.
        /// </summary>
        /// <param name="info">The property info that defines the connection string parameter.</param>
        /// <returns>The parameter defined by the given property info.</returns>
        private AdapterConnectionStringParameter GetParameter(PropertyInfo info)
        {
            AdapterConnectionStringParameter parameter = null;

            bool isRequired = false;
            object defaultValue = null;
            string description = null;

            if (!(m_parameterList is null))
                parameter = m_parameterList.SingleOrDefault(param => param.Name.Equals(info.Name, StringComparison.OrdinalIgnoreCase));

            // These are different cases, but we need to extract the description
            // and default value in both. In cases where we already know this
            // information, we can skip this step.
            if (parameter?.Info is null)
            {
                isRequired = !info.TryGetAttribute(out DefaultValueAttribute defaultValueAttribute);
                defaultValue = isRequired ? null : defaultValueAttribute.Value;
                description = info.TryGetAttribute(out DescriptionAttribute descriptionAttribute) ? descriptionAttribute.Description : string.Empty;
            }

            if (parameter is null)
            {
                // Create a brand new parameter to be returned.
                parameter = new AdapterConnectionStringParameter
                {
                    Info = info,
                    Name = info.Name,
                    Description = description,
                    Value = null,
                    DefaultValue = defaultValue,
                    IsRequired = isRequired
                };
            }
            else if (parameter.Info is null)
            {
                // Update the existing parameter with newly obtained information.
                parameter.Info = info;
                parameter.Description = description;
                parameter.DefaultValue = defaultValue;
            }

            return parameter;
        }

        /// <summary>
        /// Retrieves an existing parameter from the <see cref="ParameterList"/>. If no
        /// parameter exists with a name that matches the one given, then a new one is created.
        /// </summary>
        /// <param name="name">The name of the parameter to be retrieved or created.</param>
        /// <returns>The parameter with the given name.</returns>
        private AdapterConnectionStringParameter GetParameter(string name)
        {
            AdapterConnectionStringParameter parameter = null;

            if (!(m_parameterList is null))
                parameter = m_parameterList.SingleOrDefault(param => param.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            return parameter ?? new AdapterConnectionStringParameter()
            {
                Name = name,
                DefaultValue = string.Empty,
                IsRequired = false
            };
        }

        /// <summary>
        /// Updates the values of the <see cref="AdapterConnectionStringParameter"/>s in the
        /// given parameter list using the values in the given connection string settings.
        /// </summary>
        /// <param name="parameters">The list of parameters to be updated.</param>
        /// <param name="settings">
        /// The connection string settings that is used to
        /// determine whether the parameters should be bold or not.
        /// </param>
        private void UpdateConnectionStringParameters(List<AdapterConnectionStringParameter> parameters, Dictionary<string, string> settings)
        {
            if (m_suppressParameterValueUpdates || parameters is null)
                return;

            foreach (AdapterConnectionStringParameter parameter in parameters)
                parameter.Value = settings.TryGetValue(parameter.Name, out string value) ? value : null;
        }

        private void OnAdapterTypeSelectedIndexChanged()
        {
            OnPropertyChanged("AdapterTypeSelectedIndex");
            OnPropertyChanged("CustomConfigurationButtonVisibility");
        }

        private Assembly AppDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                AssemblyName assemblyName = new AssemblyName(args.Name);
                string assemblyFile = Path.Combine(m_searchDirectory, $"{assemblyName.Name}.dll");

                if (!File.Exists(assemblyFile))
                    assemblyFile = Path.Combine(m_searchDirectory, $"{assemblyName.Name}.exe");

                if (!File.Exists(assemblyFile))
                    return null;

                Assembly assembly = Assembly.LoadFrom(assemblyFile);

                // JRC: Changed to allow more UI assembly load flexibility, like different versions
                return !assembly.GetName().Name.Equals(assemblyName.Name) ? null : assembly;
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}
