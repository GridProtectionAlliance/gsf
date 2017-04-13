//******************************************************************************************************
//  DataContext.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  02/01/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Routing;
using GSF.Collections;
using GSF.ComponentModel.DataAnnotations;
using GSF.Data;
using GSF.Data.Model;
using GSF.Reflection;
using GSF.Security;
using GSF.Web.Hubs;
using RazorEngine.Templating;

// ReSharper disable once StaticMemberInGenericType
namespace GSF.Web.Model
{
    /// <summary>
    /// Defines a data context for Razor views.
    /// </summary>
    /// <remarks>
    /// This class is used by views to render HTML form input templates, paged view model configurations,
    /// provide access to modeled table operations and general database access.
    /// </remarks>
    public class DataContext : IDisposable
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Defines the regular expression used to validate URLs. 
        /// </summary>
        public const string UrlValidation = UrlValidationAttribute.ValidationPattern;

        // Fields
        private AdoDataConnection m_connection;
        private readonly IRazorEngine m_razorEngine;
        private readonly Dictionary<Type, object> m_tableOperations;
        private readonly Action<Exception> m_exceptionHandler;
        private Dictionary<Type, KeyValuePair<string, string>[]> m_customTableOperationTokens;
        private readonly Dictionary<string, Tuple<string, string>> m_fieldValidationParameters;
        private readonly List<Tuple<string, string>> m_fieldValueInitializers;
        private readonly List<Tuple<string, string, string, bool>> m_readonlyHotLinkFields;
        private readonly List<string> m_definedDateFields; 
        private readonly string m_settingsCategory;
        private readonly bool m_disposeConnection;
        private string m_initialFocusField;
        private string m_addDateFieldTemplate;
        private string m_addInputFieldTemplate;
        private string m_addTextAreaFieldTemplate;
        private string m_addSelectFieldTemplate;
        private string m_addCheckBoxFieldTemplate;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DataContext"/>.
        /// </summary>
        /// <param name="connection"><see cref="AdoDataConnection"/> to use; defaults to a new connection.</param>
        /// <param name="disposeConnection">Set to <c>true</c> to dispose the provided <paramref name="connection"/>.</param>
        /// <param name="razorEngine">Razor engine instance to use for data context; set to <c>null</c> to use default embedded resources instance.</param>
        /// <param name="exceptionHandler">Delegate to handle exceptions.</param>
        public DataContext(AdoDataConnection connection = null, bool disposeConnection = false, IRazorEngine razorEngine = null, Action<Exception> exceptionHandler = null)
        {
            m_connection = connection;
            m_razorEngine = razorEngine ?? RazorEngine<CSharpEmbeddedResource>.Default;
            m_tableOperations = new Dictionary<Type, object>();
            m_exceptionHandler = exceptionHandler;
            m_fieldValidationParameters = new Dictionary<string, Tuple<string, string>>();
            m_fieldValueInitializers = new List<Tuple<string, string>>();
            m_readonlyHotLinkFields = new List<Tuple<string, string, string, bool>>();
            m_definedDateFields = new List<string>();
            m_settingsCategory = "systemSettings";
            m_disposeConnection = disposeConnection || connection == null;
        }

        /// <summary>
        /// Creates a new <see cref="DataContext"/> using the specified <paramref name="settingsCategory"/>.
        /// </summary>
        /// <param name="settingsCategory">Setting category that contains the connection settings.</param>
        /// <param name="razorEngine">Razor engine instance to use for data context; set to <c>null</c> to use default embedded resources instance.</param>
        /// <param name="exceptionHandler">Delegate to handle exceptions.</param>
        public DataContext(string settingsCategory, IRazorEngine razorEngine = null, Action<Exception> exceptionHandler = null)
        {
            m_razorEngine = razorEngine ?? RazorEngine<CSharpEmbeddedResource>.Default;
            m_tableOperations = new Dictionary<Type, object>();
            m_exceptionHandler = exceptionHandler;
            m_fieldValidationParameters = new Dictionary<string, Tuple<string, string>>();
            m_fieldValueInitializers = new List<Tuple<string, string>>();
            m_readonlyHotLinkFields = new List<Tuple<string, string, string, bool>>();
            m_definedDateFields = new List<string>();
            m_settingsCategory = settingsCategory;
            m_disposeConnection = true;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets reference to <see cref="IRazorEngine"/> used by this <see cref="DataContext"/>.
        /// </summary>
        public IRazorEngine RazorEngine => m_razorEngine;

        /// <summary>
        /// Gets the <see cref="AdoDataConnection"/> for this <see cref="DataContext"/>, creating a new one if needed.
        /// </summary>
        public AdoDataConnection Connection => m_connection ?? (m_connection = new AdoDataConnection(m_settingsCategory));

        /// <summary>
        /// Gets reference to user specific security provider instance.
        /// </summary>
        public AdoSecurityProvider SecurityProvider => SecurityProviderCache.CurrentProvider as AdoSecurityProvider;

        /// <summary>
        /// Gets validation pattern and error message for rendered fields, if any.
        /// </summary>
        public Dictionary<string, Tuple<string, string>> FieldValidationParameters => m_fieldValidationParameters;

        /// <summary>
        /// Gets field value initializers, if any.
        /// </summary>
        public List<Tuple<string, string>> FieldValueInitializers => m_fieldValueInitializers;

        /// <summary>
        /// Gets any text input fields that should render clickable URLs and e-mail addresses when in view mode.
        /// </summary>
        public List<Tuple<string, string, string, bool>> ReadonlyHotLinkFields => m_readonlyHotLinkFields;

        /// <summary>
        /// Gets defined date fields, if any.
        /// </summary>
        public List<string> DefinedDateFields => m_definedDateFields;

        /// <summary>
        /// Gets field name designated for initial focus.
        /// </summary>
        public string InitialFocusField => m_initialFocusField;

        /// <summary>
        /// Gets or sets the date field razor template file name.
        /// </summary>
        public string AddDateFieldTemplate
        {
            get
            {
                return m_addDateFieldTemplate ?? (m_addDateFieldTemplate = $"{m_razorEngine.TemplatePath}AddDateField.cshtml");
            }
            set
            {
                m_addDateFieldTemplate = value;
            }
        }

        /// <summary>
        /// Gets or sets the input field razor template file name.
        /// </summary>
        public string AddInputFieldTemplate
        {
            get
            {
                return m_addInputFieldTemplate ?? (m_addInputFieldTemplate = $"{m_razorEngine.TemplatePath}AddInputField.cshtml");
            }
            set
            {
                m_addInputFieldTemplate = value;
            }
        }

        /// <summary>
        /// Gets or sets the text area field razor template file name.
        /// </summary>
        public string AddTextAreaFieldTemplate
        {
            get
            {
                return m_addTextAreaFieldTemplate ?? (m_addTextAreaFieldTemplate = $"{m_razorEngine.TemplatePath}AddTextAreaField.cshtml");
            }
            set
            {
                m_addTextAreaFieldTemplate = value;
            }
        }

        /// <summary>
        /// Gets or sets the select field razor template file name.
        /// </summary>
        public string AddSelectFieldTemplate
        {
            get
            {
                return m_addSelectFieldTemplate ?? (m_addSelectFieldTemplate = $"{m_razorEngine.TemplatePath}AddSelectField.cshtml");
            }
            set
            {
                m_addSelectFieldTemplate = value;
            }
        }

        /// <summary>
        /// Gets or sets the check box field razor template file name.
        /// </summary>
        public string AddCheckBoxFieldTemplate
        {
            get
            {
                return m_addCheckBoxFieldTemplate ?? (m_addCheckBoxFieldTemplate = $"{m_razorEngine.TemplatePath}AddCheckBoxField.cshtml");
            }
            set
            {
                m_addCheckBoxFieldTemplate = value;
            }
        }

        /// <summary>
        /// Gets dictionary of per-model custom tokens to use with <see cref="AmendExpressionAttribute"/> values for <see cref="TableOperations{T}"/> in this <see cref="DataContext"/> .
        /// </summary>
        /// <remarks>
        /// The returned dictionary can be used to apply run-time tokens to any defined <see cref="AmendExpressionAttribute"/> values,
        /// for example, given the following amendment expression applied to a modeled class:
        /// <code>
        /// [AmendExpression("TOP {count}", 
        ///     TargetExpression = TargetExpression.FieldList,
        ///     AffixPosition = AffixPosition.Prefix,
        ///     StatementTypes = StatementTypes.SelectSet)]]
        /// public class MyTable
        /// {
        ///     string MyField;
        /// }
        /// </code>
        /// The per-model key/value pairs could be set as follows at run-time:
        /// <code>
        /// int count = 200;
        /// dataContext.CustomTableOperationTokens[typeof(MyTable)] = new[] { new KeyValuePair&lt;string, string&gt;("{count}", $"{count}") };
        /// </code>
        /// </remarks>        
        public Dictionary<Type, KeyValuePair<string, string>[]> CustomTableOperationTokens => m_customTableOperationTokens ?? (m_customTableOperationTokens = new Dictionary<Type, KeyValuePair<string, string>[]>());

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="DataContext"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="DataContext"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        if (m_disposeConnection)
                            m_connection?.Dispose();
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Gets the table operations for the specified modeled table <typeparamref name="TModel"/>.
        /// </summary>
        /// <typeparam name="TModel">Modeled database table (or view).</typeparam>
        /// <returns>Table operations for the specified modeled table <typeparamref name="TModel"/>.</returns>
        public TableOperations<TModel> Table<TModel>() where TModel : class, new()
        {
            KeyValuePair<string, string>[] customTokens = null;

            if ((object)m_customTableOperationTokens != null)
                m_customTableOperationTokens.TryGetValue(typeof(TModel), out customTokens);

            return m_tableOperations.GetOrAdd(typeof(TModel), type => new TableOperations<TModel>(Connection, m_exceptionHandler, customTokens)) as TableOperations<TModel>;
        }

        /// <summary>
        /// Gets the table operations for the specified modeled table type <paramref name="model"/>.
        /// </summary>
        /// <param name="model">Modeled database table (or view) type.</param>
        /// <returns>Table operations for the specified modeled table type <paramref name="model"/>.</returns>
        public ITableOperations Table(Type model)
        {
            KeyValuePair<string, string>[] customTokens = null;

            if ((object)m_customTableOperationTokens != null)
                m_customTableOperationTokens.TryGetValue(model, out customTokens);

            return m_tableOperations.GetOrAdd(model, type => Activator.CreateInstance(typeof(TableOperations<>).MakeGenericType(model), Connection, m_exceptionHandler, customTokens)) as ITableOperations;
        }

        /// <summary>
        /// Gets the field name targeted as the primary label for the modeled table.
        /// </summary>
        /// <typeparam name="TModel">Modeled database table (or view).</typeparam>
        /// <returns>Field name targeted as the primary label for the modeled table.</returns>
        public string GetPrimaryLabelField<TModel>() where TModel : class, new()
        {
            return GetPrimaryLabelField(typeof(TModel));
        }

        /// <summary>
        /// Gets the field name targeted as the primary label for the modeled table.
        /// </summary>
        /// <param name="model">Modeled database table (or view) type.</param>
        /// <returns>Field name targeted as the primary label for the modeled table.</returns>
        public string GetPrimaryLabelField(Type model)
        {
            PrimaryLabelAttribute primaryLabelAttribute;

            if (model.TryGetAttribute(out primaryLabelAttribute) && !string.IsNullOrWhiteSpace(primaryLabelAttribute.FieldName))
                return primaryLabelAttribute.FieldName;

            string[] fieldNames = Table(model).GetFieldNames(false);
            return fieldNames.Length > 0 ? fieldNames[0] : "";
        }

        /// <summary>
        /// Gets the field name targeted to mark a record as deleted.
        /// </summary>
        /// <typeparam name="TModel">Modeled database table (or view).</typeparam>
        /// <returns>Field name targeted to mark a record as deleted.</returns>
        public string GetIsDeletedFlag<TModel>() where TModel : class, new()
        {
            return GetIsDeletedFlag(typeof(TModel));
        }

        /// <summary>
        /// Gets the field name targeted to mark a record as deleted.
        /// </summary>
        /// <param name="model">Modeled database table (or view) type.</param>
        /// <returns>Field name targeted to mark a record as deleted.</returns>
        public string GetIsDeletedFlag(Type model)
        {
            IsDeletedFlagAttribute isDeletedFlagAttribute;

            if (model.TryGetAttribute(out isDeletedFlagAttribute) && !string.IsNullOrWhiteSpace(isDeletedFlagAttribute.FieldName))
                return isDeletedFlagAttribute.FieldName;

            return null;
        }

        /// <summary>
        /// Determines if user is in a specific role or list of roles (comma separated).
        /// </summary>
        /// <param name="role">Role or comma separated list of roles.</param>
        /// <returns><c>true</c> if user is in <paramref name="role"/>(s); otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// Set to * for any role.
        /// </remarks>
        public bool UserIsInRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return false;

            role = role.Trim();

            string[] roles = role.Split(',').Where(value => !string.IsNullOrWhiteSpace(value)).ToArray();

            if (roles.Length > 1)
                return UserIsInRole(roles);

            List<string> userRoles = SecurityProvider?.UserData?.Roles ?? new List<string>();

            if (role.Equals("*") && userRoles.Count > 0)
                return true;

            foreach (string userRole in userRoles)
                if (userRole.Equals(role, StringComparison.OrdinalIgnoreCase))
                    return true;

            return false;
        }

        /// <summary>
        /// Determines if user is in one of the provided of roles.
        /// </summary>
        /// <param name="roles">List of role names.</param>
        /// <returns><c>true</c> if user is in one of the <paramref name="roles"/>; otherwise, <c>false</c>.</returns>
        public bool UserIsInRole(string[] roles)
        {
            return roles.Any(UserIsInRole);
        }

        /// <summary>
        /// Determines if user is in a specific group or list of groups (comma separated).
        /// </summary>
        /// <param name="group">Group or comma separated list of groups.</param>
        /// <returns><c>true</c> if user is in <paramref name="group"/>(s); otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// Set to * for any group.
        /// </remarks>
        public bool UserIsInGroup(string group)
        {
            if (string.IsNullOrWhiteSpace(group))
                return false;

            group = group.Trim();

            string[] groups = group.Split(',').Where(value => !string.IsNullOrWhiteSpace(value)).ToArray();

            if (groups.Length > 1)
                return UserIsInGroup(groups);

            List<string> userGroups = SecurityProvider?.UserData?.Groups ?? new List<string>();

            if (group.Equals("*") && userGroups.Count > 0)
                return true;

            foreach (string userGroup in userGroups)
                if (userGroup.Equals(group, StringComparison.OrdinalIgnoreCase))
                    return true;

            return false;
        }

        /// <summary>
        /// Determines if user is in one of the provided of groups.
        /// </summary>
        /// <param name="groups">List of group names.</param>
        /// <returns><c>true</c> if user is in one of the <paramref name="groups"/>; otherwise, <c>false</c>.</returns>
        public bool UserIsInGroup(string[] groups)
        {
            return groups.Any(UserIsInGroup);
        }

        /// <summary>
        /// Configures a simple view with common view bag parameters.
        /// </summary>
        /// <param name="request">HTTP request message for view used to derive route ID, if any.</param>
        /// <param name="viewBag">Current view bag.</param>
        /// <remarks>
        /// This is normally called from controller before returning view action result.
        /// For self-hosted web servers, route ID is passed as a command line parameter, e.g.: myview.cshtml?RouteID=ShowDeleted
        /// </remarks>
        public void ConfigureView(HttpRequestMessage request, dynamic viewBag)
        {
            string routeID;
            request.QueryParameters().TryGetValue("RouteID", out routeID);
            ConfigureView(routeID, viewBag);
        }

        /// <summary>
        /// Configures a view establishing user roles based on modeled table <typeparamref name="TModel"/> and SignalR <typeparamref name="THub"/>.
        /// </summary>
        /// <param name="request">HTTP request message for view used to derive route ID, if any.</param>
        /// <param name="viewBag">Current view bag.</param>
        /// <typeparam name="TModel">Modeled database table (or view).</typeparam>
        /// <typeparam name="THub">SignalR hub that implements <see cref="IRecordOperationsHub"/>.</typeparam>
        /// <remarks>
        /// This is normally called from controller before returning view action result.
        /// For self-hosted web servers, route ID is passed as a command line parameter, e.g.: myview.cshtml?RouteID=ShowDeleted
        /// </remarks>
        public void ConfigureView<TModel, THub>(HttpRequestMessage request, dynamic viewBag) where TModel : class, new() where THub : IRecordOperationsHub, new()
        {
            ConfigureView(typeof(TModel), typeof(THub), request, viewBag);
        }

        /// <summary>
        /// Configures a view establishing user roles based on modeled table <paramref name="model"/> and SignalR <paramref name="hub"/>.
        /// </summary>
        /// <param name="model">Modeled database table (or view) type.</param>
        /// <param name="hub">Type of SignalR hub that implements <see cref="IRecordOperationsHub"/>.</param>
        /// <param name="request">HTTP request message for view used to derive route ID, if any.</param>
        /// <param name="viewBag">Current view bag.</param>
        /// <remarks>
        /// This is normally called from controller before returning view action result.
        /// For self-hosted web servers, route ID is passed as a command line parameter, e.g.: myview.cshtml?RouteID=ShowDeleted
        /// </remarks>
        public void ConfigureView(Type model, Type hub, HttpRequestMessage request, dynamic viewBag)
        {
            string routeID;
            request.QueryParameters().TryGetValue("RouteID", out routeID);
            ConfigureView(model, hub, routeID, viewBag);
        }

        /// <summary>
        /// Configures a simple view with common view bag parameters.
        /// </summary>
        /// <param name="requestContext">Url.RequestContext for view used to derive route ID, if any.</param>
        /// <param name="viewBag">Current view bag.</param>
        /// <remarks>
        /// This is normally called from controller before returning view action result.
        /// For normal MVC views the common route is "{controller}/{action}/{id}", the {id} of
        /// the route is the route ID parameter derived from the route data. In many use
        /// cases this is a primary key or action value for the page, e.g., "ShowDeleted".
        /// </remarks>
        public void ConfigureView(RequestContext requestContext, dynamic viewBag)
        {
            ConfigureView(requestContext.RouteData.Values["id"] as string, viewBag);
        }

        /// <summary>
        /// Configures a view establishing user roles based on modeled table <typeparamref name="TModel"/> and SignalR <typeparamref name="THub"/>.
        /// </summary>
        /// <param name="requestContext">Url.RequestContext for view used to derive route ID, if any.</param>
        /// <param name="viewBag">Current view bag.</param>
        /// <typeparam name="TModel">Modeled database table (or view).</typeparam>
        /// <typeparam name="THub">SignalR hub that implements <see cref="IRecordOperationsHub"/>.</typeparam>
        /// <remarks>
        /// This is normally called from controller before returning view action result.
        /// For normal MVC views the common route is "{controller}/{action}/{id}", the {id} of
        /// the route is the route ID parameter derived from the route data. In many use
        /// cases this is a primary key or action value for the page, e.g., "ShowDeleted".
        /// </remarks>
        public void ConfigureView<TModel, THub>(RequestContext requestContext, dynamic viewBag) where TModel : class, new() where THub : IRecordOperationsHub, new()
        {
            ConfigureView(typeof(TModel), typeof(THub), requestContext, viewBag);
        }

        /// <summary>
        /// Configures a view establishing user roles based on modeled table <paramref name="model"/> and SignalR <paramref name="hub"/>.
        /// </summary>
        /// <param name="model">Modeled database table (or view) type.</param>
        /// <param name="hub">Type of SignalR hub that implements <see cref="IRecordOperationsHub"/>.</param>
        /// <param name="requestContext">Url.RequestContext for view used to derive route ID, if any.</param>
        /// <param name="viewBag">Current view bag.</param>
        /// <remarks>
        /// This is normally called from controller before returning view action result.
        /// For normal MVC views the common route is "{controller}/{action}/{id}", the {id} of
        /// the route is the route ID parameter derived from the route data. In many use
        /// cases this is a primary key or action value for the page, e.g., "ShowDeleted".
        /// </remarks>
        public void ConfigureView(Type model, Type hub, RequestContext requestContext, dynamic viewBag)
        {
            ConfigureView(model, hub, requestContext.RouteData.Values["id"] as string, viewBag);
        }

        /// <summary>
        /// Configures a simple view with common view bag parameters.
        /// </summary>
        /// <param name="routeID">Route ID for view, if any (e.g., AddNew or ShowDeleted).</param>
        /// <param name="viewBag">Current view bag.</param>
        /// <remarks>
        /// This is normally called from controller before returning view action result.
        /// For normal MVC views the common route is "{controller}/{action}/{id}", the {id} of
        /// the route is what is requested as the <paramref name="routeID"/> parameter. In many
        /// use cases this is a primary key or action value for the page, e.g., "ShowDeleted".
        /// </remarks>
        public void ConfigureView(string routeID, dynamic viewBag)
        {
            viewBag.RouteID = routeID;
            viewBag.PageControlScripts = new StringBuilder();

            // Setup default roles if none are defined. Important to only check for null here as empty string will
            // be treated as none-allowed, e.g., read-only for modeled views
            if (viewBag.EditRoles == null)
                viewBag.EditRoles = "*";

            if (viewBag.AddNewRoles == null)
                viewBag.AddNewRoles = viewBag.EditRoles;

            if (viewBag.DeleteRoles == null)
                viewBag.DeleteRoles = viewBag.EditRoles;

            // Ensure that the user's roles have been properly initialized
            SecurityProviderCache.ValidateCurrentProvider();

            // Check current allowed roles for user
            viewBag.CanEdit = UserIsInRole(viewBag.EditRoles);
            viewBag.CanAddNew = UserIsInRole(viewBag.AddNewRoles);
            viewBag.CanDelete = UserIsInRole(viewBag.DeleteRoles);
        }

        /// <summary>
        /// Configures a view establishing user roles based on modeled table <typeparamref name="TModel"/> and SignalR <typeparamref name="THub"/>.
        /// </summary>
        /// <param name="routeID">Route ID for view, if any (e.g., AddNew or ShowDeleted).</param>
        /// <param name="viewBag">Current view bag.</param>
        /// <typeparam name="TModel">Modeled database table (or view).</typeparam>
        /// <typeparam name="THub">SignalR hub that implements <see cref="IRecordOperationsHub"/>.</typeparam>
        /// <remarks>
        /// This is normally called from controller before returning view action result.
        /// For normal MVC views the common route is "{controller}/{action}/{id}", the {id} of
        /// the route is what is requested as the <paramref name="routeID"/> parameter. In many
        /// use cases this is a primary key or action value for the page, e.g., "ShowDeleted".
        /// </remarks>
        public void ConfigureView<TModel, THub>(string routeID, dynamic viewBag) where TModel : class, new() where THub : IRecordOperationsHub, new()
        {
            ConfigureView(typeof(TModel), typeof(THub), routeID, viewBag);
        }

        /// <summary>
        /// Configures a view establishing user roles based on modeled table <paramref name="model"/> and SignalR <paramref name="hub"/>.
        /// </summary>
        /// <param name="model">Modeled database table (or view) type.</param>
        /// <param name="hub">Type of SignalR hub that implements <see cref="IRecordOperationsHub"/>.</param>
        /// <param name="routeID">Route ID for view, if any (e.g., AddNew or ShowDeleted).</param>
        /// <param name="viewBag">Current view bag.</param>
        /// <remarks>
        /// This is normally called from controller before returning view action result.
        /// For normal MVC views the common route is "{controller}/{action}/{id}", the {id} of
        /// the route is what is requested as the <paramref name="routeID"/> parameter. In many
        /// use cases this is a primary key or action value for the page, e.g., "ShowDeleted".
        /// </remarks>
        public void ConfigureView(Type model, Type hub, string routeID, dynamic viewBag)
        {
            // Attempt to establish roles based on hub defined security for specified modeled table
            EstablishUserRolesForPage(model, hub, viewBag);

            ConfigureView(routeID, viewBag);

            // See if modeled table has a flag field that represents a deleted row
            string isDeletedField = GetIsDeletedFlag(model);

            if (!string.IsNullOrWhiteSpace(isDeletedField))
            {
                // See if user has requested to show deleted records
                viewBag.ShowDeleted = viewBag.RouteID?.Equals("ShowDeleted", StringComparison.OrdinalIgnoreCase) ?? false;
                viewBag.IsDeletedField = isDeletedField;
            }
        }

        /// <summary>
        /// Looks up proper user roles for paged based on modeled security in <see cref="RecordOperationsCache"/>.
        /// </summary>
        /// <typeparam name="TModel">Modeled database table (or view).</typeparam>
        /// <typeparam name="THub">SignalR hub that implements <see cref="IRecordOperationsHub"/>.</typeparam>
        /// <param name="viewBag">ViewBag for the current view.</param>
        /// <remarks>
        /// Typically used in paged view model scenarios and invoked by controller prior to view rendering.
        /// Security is controlled at hub level, so failure to call will not impact security but may result
        /// in screen enabling and/or showing controls that the user does not actually have access to and
        /// upon attempted use will result in a security error.
        /// </remarks>
        public void EstablishUserRolesForPage<TModel, THub>(dynamic viewBag) where TModel : class, new() where THub : IRecordOperationsHub, new()
        {
            EstablishUserRolesForPage(typeof(TModel), typeof(THub), viewBag);
        }

        /// <summary>
        /// Looks up proper user roles for paged based on modeled security in <see cref="RecordOperationsCache"/>.
        /// </summary>
        /// <param name="model">Modeled database table (or view) type.</param>
        /// <param name="hub">Type of SignalR hub that implements <see cref="IRecordOperationsHub"/>.</param>
        /// <param name="viewBag">ViewBag for the current view.</param>
        /// <remarks>
        /// Typically used in paged view model scenarios and invoked by controller prior to view rendering.
        /// Security is controlled at hub level, so failure to call will not impact security but may result
        /// in screen enabling and/or showing controls that the user does not actually have access to and
        /// upon attempted use will result in a security error.
        /// </remarks>
        public void EstablishUserRolesForPage(Type model, Type hub, dynamic viewBag)
        {
            RecordOperationsCache cache = s_recordOperationCaches.GetOrAdd(hub, type =>
            {
                using (IRecordOperationsHub operationsHub = Activator.CreateInstance(hub) as IRecordOperationsHub)
                    return operationsHub?.RecordOperationsCache;
            });

            EstablishUserRolesForPage(model, cache, viewBag);
        }

        /// <summary>
        /// Looks up proper user roles for paged based on modeled security in <see cref="RecordOperationsCache"/>.
        /// </summary>
        /// <typeparam name="TModel">Modeled database table (or view).</typeparam>
        /// <param name="cache">Data hub record operations cache.</param>
        /// <param name="viewBag">ViewBag for the current view.</param>
        /// <remarks>
        /// Typically used in paged view model scenarios and invoked by controller prior to view rendering.
        /// Security is controlled at hub level, so failure to call will not impact security but may result
        /// in screen enabling and/or showing controls that the user does not actually have access to and
        /// upon attempted use will result in a security error.
        /// </remarks>
        public void EstablishUserRolesForPage<TModel>(RecordOperationsCache cache, dynamic viewBag) where TModel : class, new()
        {
            EstablishUserRolesForPage(typeof(TModel), cache, viewBag);
        }

        /// <summary>
        /// Looks up proper user roles for paged based on modeled security in <see cref="RecordOperationsCache"/>.
        /// </summary>
        /// <param name="model">Modeled database table (or view) type.</param>
        /// <param name="cache">Data hub record operations cache.</param>
        /// <param name="viewBag">ViewBag for the current view.</param>
        /// <remarks>
        /// Typically used in paged view model scenarios and invoked by controller prior to view rendering.
        /// Security is controlled at hub level, so failure to call will not impact security but may result
        /// in screen enabling and/or showing controls that the user does not actually have access to and
        /// upon attempted use will result in a security error.
        /// </remarks>
        public void EstablishUserRolesForPage(Type model, RecordOperationsCache cache, dynamic viewBag)
        {
            // Get any authorized roles as defined in hub for key records operations of modeled table
            Tuple<string, string>[] recordOperations = cache.GetRecordOperations(model);

            // Create a function to check if a method exists for operation - if none is defined, read-only access will be assumed (e.g. for a view)
            Func<RecordOperation, string> getRoles = operationType =>
            {
                Tuple<string, string> recordOperation = recordOperations[(int)operationType];
                return string.IsNullOrEmpty(recordOperation?.Item1) ? "" : recordOperation.Item2 ?? "";
            };

            viewBag.EditRoles = getRoles(RecordOperation.UpdateRecord);
            viewBag.AddNewRoles = getRoles(RecordOperation.AddNewRecord);
            viewBag.DeleteRoles = getRoles(RecordOperation.DeleteRecord);
        }

        /// <summary>
        /// Renders client-side configuration script for paged view model.
        /// </summary>
        /// <typeparam name="TModel">Modeled database table (or view).</typeparam>
        /// <typeparam name="THub">SignalR hub that implements <see cref="IRecordOperationsHub"/>.</typeparam>
        /// <param name="viewBag">ViewBag for the view.</param>
        /// <param name="defaultSortField">Default sort field name, defaults to first primary key field. Prefix field name with a minus, i.e., '-', to default to descending sort.</param>
        /// <param name="hubScriptName">Javascript hub name, defaults to camel-cased <typeparamref name="THub"/> type name.</param>
        /// <param name="parentKeys">Primary keys values of the parent record to load.</param>
        /// <returns>Rendered paged view model configuration script.</returns>
        public string RenderViewModelConfiguration<TModel, THub>(dynamic viewBag, string defaultSortField = null, string hubScriptName = null, params object[] parentKeys) where TModel : class, new() where THub : IRecordOperationsHub, new()
        {
            Type hubType = typeof(THub);

            RecordOperationsCache cache = s_recordOperationCaches.GetOrAdd(hubType, type =>
            {
                using (THub hub = new THub())
                    return hub.RecordOperationsCache;
            });

            return RenderViewModelConfiguration<TModel>(cache, viewBag, defaultSortField, hubType.FullName, hubScriptName ?? hubType.Name.ToCamelCase(), parentKeys);
        }

        /// <summary>
        /// Renders client-side configuration script for paged view model.
        /// </summary>
        /// <typeparam name="TModel">Modeled database table (or view).</typeparam>
        /// <param name="cache">Data hub record operations cache.</param>
        /// <param name="viewBag">ViewBag for the view.</param>
        /// <param name="defaultSortField">Default sort field name, defaults to first primary key field. Prefix field name with a minus, i.e., '-', to default to descending sort.</param>
        /// <param name="hubClassName">Full class name of hub instance, defaults to "GSF.Web.Security.SecurityHub".</param>
        /// <param name="hubScriptName">Javascript hub name, defaults to "securityHub".</param>
        /// <param name="parentKeys">Primary keys values of the parent record to load.</param>
        /// <returns>Rendered paged view model configuration script.</returns>
        public string RenderViewModelConfiguration<TModel>(RecordOperationsCache cache, dynamic viewBag, string defaultSortField = null, string hubClassName = "GSF.Web.Security.SecurityHub", string hubScriptName = "securityHub", params object[] parentKeys) where TModel : class, new()
        {
            StringBuilder javascript = new StringBuilder();
            string[] primaryKeyFields = Table<TModel>().GetPrimaryKeyFieldNames(false);
            string defaultSortAscending = "true";

            defaultSortField = defaultSortField ?? primaryKeyFields[0];

            // Handle case for desired default descending sort order
            if (defaultSortField[0] == '-')
            {
                defaultSortField = defaultSortField.Substring(1);
                defaultSortAscending = "false";
            }

            javascript.Append($@"// Configure view model
                viewModel.defaultSortField = ""{defaultSortField}"";
                viewModel.defaultSortAscending = {defaultSortAscending};
                viewModel.labelField = ""{GetPrimaryLabelField<TModel>()}"";
                viewModel.modelName = ""{typeof(TModel).FullName}"";
                viewModel.hubName = ""{hubClassName}"";
                viewModel.primaryKeyFields = [{primaryKeyFields.Select(fieldName => $"\"{fieldName}\"").ToDelimitedString(", ")}];
            ".FixForwardSpacing());

            string showDeletedValue = null;

            if (viewBag.IsDeletedField != null)
                showDeletedValue = (viewBag.ShowDeleted ?? false).ToString().ToLower();

            // Get method names for records operations of modeled table
            Tuple<string, string>[] recordOperations = cache.GetRecordOperations<TModel>();
            string queryRecordCountMethod = recordOperations[(int)RecordOperation.QueryRecordCount]?.Item1.ToCamelCase();
            string queryRecordsMethod = recordOperations[(int)RecordOperation.QueryRecords]?.Item1.ToCamelCase();
            string deleteRecordMethod = recordOperations[(int)RecordOperation.DeleteRecord]?.Item1.ToCamelCase();
            string createNewRecordMethod = recordOperations[(int)RecordOperation.CreateNewRecord]?.Item1.ToCamelCase();
            string addNewRecordMethod = recordOperations[(int)RecordOperation.AddNewRecord]?.Item1.ToCamelCase();
            string updateMethod = recordOperations[(int)RecordOperation.UpdateRecord]?.Item1.ToCamelCase();

            string parentKeyParams = null;

            if (parentKeys.Length > 0)
            {
                viewBag.ParentKeys = string.Join(",", parentKeys.Select(parentKey => parentKey.ToString().Replace(",", ','.RegexEncode())));
                parentKeyParams = string.Join(", ", parentKeys.Select(parentKey => Common.IsNumericType(parentKey) ? $"{parentKey}" : $"\"{parentKey.ToString().JavaScriptEncode()}\""));
            }

            // If modeled table has IsDeletedField marker, the showDeleted parameter should come first in DataHub operations
            if (showDeletedValue != null)
                parentKeyParams = parentKeyParams != null ? $"{showDeletedValue}, {parentKeyParams}" : showDeletedValue;

            if (!string.IsNullOrWhiteSpace(queryRecordCountMethod))
                javascript.Append($@"
                    viewModel.setQueryRecordCount(function(filterText) {{
                        return {hubScriptName}.{queryRecordCountMethod}({(parentKeyParams == null ? "" : $"{parentKeyParams}, ")}filterText);
                    }});
                ".FixForwardSpacing());

            if (!string.IsNullOrWhiteSpace(queryRecordsMethod))
                javascript.Append($@"
                    viewModel.setQueryRecords(function(sortField, ascending, page, pageSize, filterText) {{
                        return {hubScriptName}.{queryRecordsMethod}({(parentKeyParams == null ? "" : $"{parentKeyParams}, ")}sortField, ascending, page, pageSize, filterText);
                    }});
                ".FixForwardSpacing());

            if (!string.IsNullOrWhiteSpace(deleteRecordMethod))
                javascript.Append($@"
                    viewModel.setDeleteRecord(function(keyValues) {{
                        return {hubScriptName}.{deleteRecordMethod}({Enumerable.Range(0, Table<TModel>().GetPrimaryKeyFieldNames().Length).Select(index => $"keyValues[{index}]").ToDelimitedString(", ")});
                    }});
                ".FixForwardSpacing());

            if (!string.IsNullOrWhiteSpace(createNewRecordMethod))
                javascript.Append($@"
                    viewModel.setNewRecord(function() {{
                        return {hubScriptName}.{createNewRecordMethod}();
                    }});
                ".FixForwardSpacing());

            if (!string.IsNullOrWhiteSpace(addNewRecordMethod))
                javascript.Append($@"
                    viewModel.setAddNewRecord(function(record) {{
                        return {hubScriptName}.{addNewRecordMethod}(record);
                    }});
                ".FixForwardSpacing());

            if (!string.IsNullOrWhiteSpace(updateMethod))
                javascript.Append($@"
                    viewModel.setUpdateRecord(function(record) {{
                        return {hubScriptName}.{updateMethod}(record);
                    }});
                ".FixForwardSpacing());

            return javascript.ToString();
        }

        /// <summary>
        /// Renders client-side Javascript function for looking up single values from a table.
        /// </summary>
        /// <typeparam name="TModel">Modeled database table (or view).</typeparam>
        /// <param name="valueFieldName">Table field name as defined in the table.</param>
        /// <param name="keyFieldName">Name of primary key field, defaults to "ID".</param>
        /// <param name="lookupFunctionName">Name of lookup function, defaults to lookup + <typeparamref name="TModel"/> name + <paramref name="valueFieldName"/> + Value.</param>
        /// <param name="arrayName">Name of value array, defaults to camel cased <typeparamref name="TModel"/> name + <paramref name="valueFieldName"/> + Values.</param>
        /// <returns>Client-side Javascript lookup function.</returns>
        public string RenderLookupFunction<TModel>(string valueFieldName, string keyFieldName = "ID", string lookupFunctionName = null, string arrayName = null) where TModel : class, new()
        {
            StringBuilder javascript = new StringBuilder();

            if (string.IsNullOrWhiteSpace(lookupFunctionName))
                lookupFunctionName = $"lookup{typeof(TModel).Name}{valueFieldName}Value";

            if (string.IsNullOrWhiteSpace(arrayName))
                arrayName = $"{typeof(TModel).Name.ToCamelCase()}{valueFieldName}Values";

            TableOperations<TModel> operations = Table<TModel>();
            bool keyIsString = operations.GetFieldType(keyFieldName) == typeof(string);
            bool valueIsString = operations.GetFieldType(valueFieldName) == typeof(string);

            javascript.AppendLine($"var {arrayName} = [];");
            javascript.AppendLine();

            foreach (TModel record in operations.QueryRecords())
            {
                string key = operations.GetFieldValue(record, keyFieldName)?.ToString();

                if (string.IsNullOrEmpty(key))
                    continue;

                key = key.JavaScriptEncode();

                if (keyIsString)
                    key = $"\"{key}\"";

                string value = operations.GetFieldValue(record, valueFieldName)?.ToString() ?? "";

                value = value.JavaScriptEncode();

                if (valueIsString)
                    value = $"\"{value}\"";

                javascript.AppendLine($"        {arrayName}[{key}] = {value};");
            }

            javascript.AppendLine();
            javascript.AppendLine($"        function {lookupFunctionName}(value) {{");
            javascript.AppendLine($"            return {arrayName}[value];");
            javascript.AppendLine("        }");

            return javascript.ToString();
        }

        /// <summary>
        /// Adds a new pattern based validation and option error message to a field.
        /// </summary>
        /// <param name="observableFieldReference">Observable field reference (from JS view model).</param>
        /// <param name="validationPattern">Reg-ex based validation pattern.</param>
        /// <param name="errorMessage">Optional error message to display when pattern fails.</param>
        public void AddFieldValidation(string observableFieldReference, string validationPattern, string errorMessage = null)
        {
            m_fieldValidationParameters[observableFieldReference] = new Tuple<string, string>(validationPattern, errorMessage);
        }

        /// <summary>
        /// Adds a new field value initializer.
        /// </summary>
        /// <param name="fieldName">Field name (as defined in model).</param>
        /// <param name="initialValueScript">Javascript based initial value for field.</param>
        public void AddFieldValueInitializer(string fieldName, string initialValueScript = null)
        {
            m_fieldValueInitializers.Add(new Tuple<string, string>(fieldName, initialValueScript ?? "\"\""));
        }

        /// <summary>
        /// Adds a new field value initializer based on modeled table field attributes.
        /// </summary>
        /// <param name="fieldName">Field name (as defined in model).</param>
        public void AddFieldValueInitializer<TModel>(string fieldName) where TModel : class, new()
        {
            InitialValueScriptAttribute initialValueScriptAttribute;

            if (Table<TModel>().TryGetFieldAttribute(fieldName, out initialValueScriptAttribute))
                AddFieldValueInitializer(fieldName, initialValueScriptAttribute.InitialValueScript);
        }

        /// <summary>
        /// Adds field initialization, and optional validation, from a page-defined (e.g., loaded from database) parameter definition.
        /// </summary>
        /// <param name="fieldName">Target field name.</param>
        /// <param name="initialValue">Javascript based initial value for field.</param>
        /// <param name="validationPattern">Reg-ex based validation pattern, if any.</param>
        /// <param name="errorMessage">Optional error message to display when pattern fails.</param>
        public void AddPageDefinedFieldInitialization(string fieldName, string initialValue, string validationPattern = null, string errorMessage = null)
        {
            AddFieldValueInitializer(fieldName, initialValue);

            if (!string.IsNullOrEmpty(validationPattern))
                AddFieldValidation($"viewModel.currentRecord().{fieldName}", validationPattern, errorMessage);
        }

        /// <summary>
        /// Renders a read-only field with enabled hot links for a given text input field when in view mode.
        /// </summary>
        /// <param name="fieldID">ID of input field.</param>
        /// <param name="readOnlyDivID">ID of read-only div.</param>
        /// <param name="fieldName">Field name.</param>
        /// <param name="isTextArea"><c>true</c> if field is a textarea; otherwise, <c>false</c> for input[type="text"].</param>
        /// <remarks>
        /// These fields will be rendered with clickable URL and e-mail addresses when viewing a record and normal text
        /// when adding or updating a record.
        /// </remarks>
        public void AddReadOnlyHotLinkField(string fieldID, string readOnlyDivID, string fieldName, bool isTextArea)
        {
            m_readonlyHotLinkFields.Add(new Tuple<string, string, string, bool>(fieldID, readOnlyDivID, fieldName, isTextArea));
        }

        /// <summary>
        /// Generates template based input date field based on reflected modeled table field attributes.
        /// </summary>
        /// <typeparam name="TModel">Modeled table.</typeparam>
        /// <param name="fieldName">Field name for input date field.</param>
        /// <param name="inputType">Input field type, defaults to text.</param>
        /// <param name="fieldLabel">Label name for input date field, pulls from <see cref="LabelAttribute"/> if defined, otherwise defaults to <paramref name="fieldName"/>.</param>
        /// <param name="fieldID">ID to use for input field; defaults to date + <paramref name="fieldName"/>.</param>
        /// <param name="groupDataBinding">Data-bind operations to apply to outer form-group div, if any.</param>
        /// <param name="labelDataBinding">Data-bind operations to apply to label, if any.</param>
        /// <param name="requiredDataBinding">Boolean data-bind operation to apply to required state, if any.</param>
        /// <param name="customDataBinding">Extra custom data-binding operations to apply to field, if any.</param>
        /// <param name="dependencyFieldName">Defines default "enabled" subordinate data-bindings based a single boolean field, e.g., a check-box.</param>
        /// <param name="toolTip">Tool tip text to apply to field, if any.</param>
        /// <param name="initialFocus">Use field for initial focus.</param>
        /// <returns>Generated HTML for new date based input field based on modeled table field attributes.</returns>
        public string AddDateField<TModel>(string fieldName, string inputType = null, string fieldLabel = null, string fieldID = null, string groupDataBinding = null, string labelDataBinding = null, string requiredDataBinding = null, string customDataBinding = null, string dependencyFieldName = null, string toolTip = null, bool initialFocus = false) where TModel : class, new()
        {
            TableOperations<TModel> tableOperations = Table<TModel>();
            StringLengthAttribute stringLengthAttribute;
            RegularExpressionAttribute regularExpressionAttribute;

            if (string.IsNullOrEmpty(fieldLabel))
            {
                LabelAttribute labelAttribute;

                if (tableOperations.TryGetFieldAttribute(fieldName, out labelAttribute))
                    fieldLabel = labelAttribute.Label;
            }

            tableOperations.TryGetFieldAttribute(fieldName, out stringLengthAttribute);
            tableOperations.TryGetFieldAttribute(fieldName, out regularExpressionAttribute);

            if ((object)regularExpressionAttribute != null)
            {
                string observableReference;

                if (string.IsNullOrEmpty(groupDataBinding))
                    observableReference = $"viewModel.currentRecord().{fieldName}";
                else // "with: $root.connectionString"
                    observableReference = $"viewModel.{groupDataBinding.Substring(groupDataBinding.IndexOf('.') + 1)}";

                AddFieldValidation(observableReference, regularExpressionAttribute.Pattern, regularExpressionAttribute.ErrorMessage ?? "Invalid format.");
            }

            AddFieldValueInitializer<TModel>(fieldName);

            return AddDateField(fieldName, tableOperations.FieldHasAttribute<RequiredAttribute>(fieldName),
                stringLengthAttribute?.MaximumLength ?? 0, inputType, fieldLabel, fieldID, groupDataBinding, labelDataBinding, requiredDataBinding, customDataBinding, dependencyFieldName, toolTip, initialFocus);
        }

        /// <summary>
        /// Generates template based input date field based on specified parameters.
        /// </summary>
        /// <param name="fieldName">Field name for input date field.</param>
        /// <param name="required">Determines if field name is required.</param>
        /// <param name="maxLength">Defines maximum input field length.</param>
        /// <param name="inputType">Input field type, defaults to text.</param>
        /// <param name="fieldLabel">Label name for input date field, defaults to <paramref name="fieldName"/>.</param>
        /// <param name="fieldID">ID to use for input field; defaults to date + <paramref name="fieldName"/>.</param>
        /// <param name="groupDataBinding">Data-bind operations to apply to outer form-group div, if any.</param>
        /// <param name="labelDataBinding">Data-bind operations to apply to label, if any.</param>
        /// <param name="requiredDataBinding">Boolean data-bind operation to apply to required state, if any.</param>
        /// <param name="customDataBinding">Extra custom data-binding operations to apply to field, if any.</param>
        /// <param name="dependencyFieldName">Defines default "enabled" subordinate data-bindings based a single boolean field, e.g., a check-box.</param>
        /// <param name="toolTip">Tool tip text to apply to field, if any.</param>
        /// <param name="initialFocus">Use field for initial focus.</param>
        /// <returns>Generated HTML for new date based input field based on specified parameters.</returns>
        public string AddDateField(string fieldName, bool required, int maxLength = 0, string inputType = null, string fieldLabel = null, string fieldID = null, string groupDataBinding = null, string labelDataBinding = null, string requiredDataBinding = null, string customDataBinding = null, string dependencyFieldName = null, string toolTip = null, bool initialFocus = false)
        {
            RazorView addDateFieldTemplate = new RazorView(m_razorEngine, AddDateFieldTemplate, m_exceptionHandler);
            DynamicViewBag viewBag = addDateFieldTemplate.ViewBag;

            if (string.IsNullOrEmpty(fieldID))
                fieldID = $"date{fieldName}";

            if (initialFocus)
                m_initialFocusField = fieldID;

            viewBag.AddValue("FieldName", fieldName);
            viewBag.AddValue("Required", required);
            viewBag.AddValue("MaxLength", maxLength);
            viewBag.AddValue("InputType", inputType ?? "text");
            viewBag.AddValue("FieldLabel", fieldLabel ?? fieldName);
            viewBag.AddValue("FieldID", fieldID);
            viewBag.AddValue("GroupDataBinding", groupDataBinding);
            viewBag.AddValue("LabelDataBinding", labelDataBinding);
            viewBag.AddValue("RequiredDataBinding", requiredDataBinding);
            viewBag.AddValue("CustomDataBinding", customDataBinding);
            viewBag.AddValue("DependencyFieldName", dependencyFieldName);
            viewBag.AddValue("ToolTip", toolTip);

            m_definedDateFields.Add(fieldName);

            return addDateFieldTemplate.Execute();
        }

        /// <summary>
        /// Generates template based input text field based on reflected modeled table field attributes.
        /// </summary>
        /// <typeparam name="TModel">Modeled table.</typeparam>
        /// <param name="fieldName">Field name for input text field.</param>
        /// <param name="inputType">Input field type, defaults to appropriate model field type.</param>
        /// <param name="fieldLabel">Label name for input text field, pulls from <see cref="LabelAttribute"/> if defined, otherwise defaults to <paramref name="fieldName"/>.</param>
        /// <param name="fieldID">ID to use for input field; defaults to input + <paramref name="fieldName"/>.</param>
        /// <param name="groupDataBinding">Data-bind operations to apply to outer form-group div, if any.</param>
        /// <param name="labelDataBinding">Data-bind operations to apply to label, if any.</param>
        /// <param name="requiredDataBinding">Boolean data-bind operation to apply to required state, if any.</param>
        /// <param name="customDataBinding">Extra custom data-binding operations to apply to field, if any.</param>
        /// <param name="dependencyFieldName">Defines default "enabled" subordinate data-bindings based a single boolean field, e.g., a check-box.</param>
        /// <param name="toolTip">Tool tip text to apply to field, if any.</param>
        /// <param name="initialFocus">Use field for initial focus.</param>
        /// <param name="enableHotLinks">Enable clickable URLs and e-mail addresses in view mode.</param>
        /// <returns>Generated HTML for new input field based on modeled table field attributes.</returns>
        public string AddInputField<TModel>(string fieldName, string inputType = null, string fieldLabel = null, string fieldID = null, string groupDataBinding = null, string labelDataBinding = null, string requiredDataBinding = null, string customDataBinding = null, string dependencyFieldName = null, string toolTip = null, bool initialFocus = false, bool enableHotLinks = true) where TModel : class, new()
        {
            TableOperations<TModel> tableOperations = Table<TModel>();
            StringLengthAttribute stringLengthAttribute;
            RegularExpressionAttribute regularExpressionAttribute;
            bool targetedFieldType = false;

            if (string.IsNullOrEmpty(inputType))
            {
                Type fieldType = tableOperations.GetFieldType(fieldName);

                if (IsIntegerType(fieldType))
                {
                    inputType = "number";
                    customDataBinding = string.IsNullOrEmpty(customDataBinding) ? "integer" : $"integer, {customDataBinding}";
                    targetedFieldType = true;
                }
                else if (IsNumericType(fieldType))
                {
                    inputType = "number";
                    customDataBinding = string.IsNullOrEmpty(customDataBinding) ? "numeric" : $"numeric, {customDataBinding}";
                    targetedFieldType = true;
                }
                else if (fieldType == typeof(DateTime))
                {
                    inputType = "date";
                    targetedFieldType = true;
                }
            } 

            if (string.IsNullOrEmpty(fieldLabel))
            {
                LabelAttribute labelAttribute;

                if (tableOperations.TryGetFieldAttribute(fieldName, out labelAttribute))
                    fieldLabel = labelAttribute.Label;
            }

            tableOperations.TryGetFieldAttribute(fieldName, out stringLengthAttribute);
            tableOperations.TryGetFieldAttribute(fieldName, out regularExpressionAttribute);

            if ((object)regularExpressionAttribute != null)
            {
                string observableReference;

                if (string.IsNullOrEmpty(groupDataBinding))
                    observableReference = $"viewModel.currentRecord().{fieldName}";
                else // "with: $root.connectionString"
                    observableReference = $"viewModel.{groupDataBinding.Substring(groupDataBinding.IndexOf('.') + 1)}";

                AddFieldValidation(observableReference, regularExpressionAttribute.Pattern, regularExpressionAttribute.ErrorMessage ?? "Invalid format.");

                if (!targetedFieldType && regularExpressionAttribute is AcronymValidationAttribute)
                    customDataBinding = string.IsNullOrEmpty(customDataBinding) ? $"acronym: {fieldName}" : $", acronym: {fieldName}, {customDataBinding}";
            }

            AddFieldValueInitializer<TModel>(fieldName);

            return AddInputField(fieldName, tableOperations.FieldHasAttribute<RequiredAttribute>(fieldName),
                stringLengthAttribute?.MaximumLength ?? 0, inputType, fieldLabel, fieldID, groupDataBinding, labelDataBinding, requiredDataBinding, customDataBinding, dependencyFieldName, toolTip, initialFocus, enableHotLinks);
        }

        /// <summary>
        /// Generates template based input text field based on specified parameters.
        /// </summary>
        /// <param name="fieldName">Field name for input text field.</param>
        /// <param name="required">Determines if field name is required.</param>
        /// <param name="maxLength">Defines maximum input field length.</param>
        /// <param name="inputType">Input field type, defaults to text.</param>
        /// <param name="fieldLabel">Label name for input text field, defaults to <paramref name="fieldName"/>.</param>
        /// <param name="fieldID">ID to use for input field; defaults to input + <paramref name="fieldName"/>.</param>
        /// <param name="groupDataBinding">Data-bind operations to apply to outer form-group div, if any.</param>
        /// <param name="labelDataBinding">Data-bind operations to apply to label, if any.</param>
        /// <param name="requiredDataBinding">Boolean data-bind operation to apply to required state, if any.</param>
        /// <param name="customDataBinding">Extra custom data-binding operations to apply to field, if any.</param>
        /// <param name="dependencyFieldName">Defines default "enabled" subordinate data-bindings based a single boolean field, e.g., a check-box.</param>
        /// <param name="toolTip">Tool tip text to apply to field, if any.</param>
        /// <param name="initialFocus">Use field for initial focus.</param>
        /// <param name="enableHotLinks">Enable clickable URLs and e-mail addresses in view mode.</param>
        /// <returns>Generated HTML for new input field based on specified parameters.</returns>
        public string AddInputField(string fieldName, bool required, int maxLength = 0, string inputType = null, string fieldLabel = null, string fieldID = null, string groupDataBinding = null, string labelDataBinding = null, string requiredDataBinding = null, string customDataBinding = null, string dependencyFieldName = null, string toolTip = null, bool initialFocus = false, bool enableHotLinks = true)
        {
            RazorView addInputFieldTemplate = new RazorView(m_razorEngine, AddInputFieldTemplate, m_exceptionHandler);
            DynamicViewBag viewBag = addInputFieldTemplate.ViewBag;

            if (string.IsNullOrEmpty(fieldID))
                fieldID = $"input{fieldName}";

            if (initialFocus)
                m_initialFocusField = fieldID;

            if (enableHotLinks)
            {
                AddReadOnlyHotLinkField(fieldID, $"readonly{fieldID}", fieldName, false);
                customDataBinding = $"{(string.IsNullOrWhiteSpace(customDataBinding) ? "" : $"{customDataBinding}, ")}visible: $root.recordMode()!==RecordMode.View";
            }

            viewBag.AddValue("FieldName", fieldName);
            viewBag.AddValue("Required", required);
            viewBag.AddValue("MaxLength", maxLength);
            viewBag.AddValue("InputType", inputType ?? "text");
            viewBag.AddValue("FieldLabel", fieldLabel ?? fieldName);
            viewBag.AddValue("FieldID", fieldID);
            viewBag.AddValue("GroupDataBinding", groupDataBinding);
            viewBag.AddValue("LabelDataBinding", labelDataBinding);
            viewBag.AddValue("RequiredDataBinding", requiredDataBinding);
            viewBag.AddValue("CustomDataBinding", customDataBinding);
            viewBag.AddValue("DependencyFieldName", dependencyFieldName);
            viewBag.AddValue("ToolTip", toolTip);

            return addInputFieldTemplate.Execute();
        }

        /// <summary>
        /// Generates template based text area field based on reflected modeled table field attributes.
        /// </summary>
        /// <typeparam name="TModel">Modeled table.</typeparam>
        /// <param name="fieldName">Field name for text area field.</param>
        /// <param name="rows">Number of rows for text area.</param>
        /// <param name="fieldLabel">Label name for text area field, pulls from <see cref="LabelAttribute"/> if defined, otherwise defaults to <paramref name="fieldName"/>.</param>
        /// <param name="fieldID">ID to use for text area field; defaults to text + <paramref name="fieldName"/>.</param>
        /// <param name="groupDataBinding">Data-bind operations to apply to outer form-group div, if any.</param>
        /// <param name="labelDataBinding">Data-bind operations to apply to label, if any.</param>
        /// <param name="requiredDataBinding">Boolean data-bind operation to apply to required state, if any.</param>
        /// <param name="customDataBinding">Extra custom data-binding operations to apply to field, if any.</param>
        /// <param name="dependencyFieldName">Defines default "enabled" subordinate data-bindings based a single boolean field, e.g., a check-box.</param>
        /// <param name="toolTip">Tool tip text to apply to field, if any.</param>
        /// <param name="initialFocus">Use field for initial focus.</param>
        /// <param name="enableHotLinks">Enable clickable URLs and e-mail addresses in view mode.</param>
        /// <returns>Generated HTML for new text area field based on modeled table field attributes.</returns>
        public string AddTextAreaField<TModel>(string fieldName, int rows = 2, string fieldLabel = null, string fieldID = null, string groupDataBinding = null, string labelDataBinding = null, string requiredDataBinding = null, string customDataBinding = null, string dependencyFieldName = null, string toolTip = null, bool initialFocus = false, bool enableHotLinks = true) where TModel : class, new()
        {
            TableOperations<TModel> tableOperations = Table<TModel>();
            StringLengthAttribute stringLengthAttribute;
            RegularExpressionAttribute regularExpressionAttribute;

            if (string.IsNullOrEmpty(fieldLabel))
            {
                LabelAttribute labelAttribute;

                if (tableOperations.TryGetFieldAttribute(fieldName, out labelAttribute))
                    fieldLabel = labelAttribute.Label;
            }

            tableOperations.TryGetFieldAttribute(fieldName, out stringLengthAttribute);
            tableOperations.TryGetFieldAttribute(fieldName, out regularExpressionAttribute);

            if ((object)regularExpressionAttribute != null)
            {
                string observableReference;

                if (string.IsNullOrEmpty(groupDataBinding))
                    observableReference = $"viewModel.currentRecord().{fieldName}";
                else // "with: $root.connectionString"
                    observableReference = $"viewModel.{groupDataBinding.Substring(groupDataBinding.IndexOf('.') + 1)}";

                AddFieldValidation(observableReference, regularExpressionAttribute.Pattern, regularExpressionAttribute.ErrorMessage ?? "Invalid format.");
            }

            AddFieldValueInitializer<TModel>(fieldName);

            return AddTextAreaField(fieldName, tableOperations.FieldHasAttribute<RequiredAttribute>(fieldName),
                stringLengthAttribute?.MaximumLength ?? 0, rows, fieldLabel, fieldID, groupDataBinding, labelDataBinding, requiredDataBinding, customDataBinding, dependencyFieldName, toolTip, initialFocus, enableHotLinks);
        }

        /// <summary>
        /// Generates template based text area field based on specified parameters.
        /// </summary>
        /// <param name="fieldName">Field name for text area field.</param>
        /// <param name="required">Determines if field name is required.</param>
        /// <param name="maxLength">Defines maximum text area field length.</param>
        /// <param name="rows">Number of rows for text area.</param>
        /// <param name="fieldLabel">Label name for text area field, defaults to <paramref name="fieldName"/>.</param>
        /// <param name="fieldID">ID to use for text area field; defaults to text + <paramref name="fieldName"/>.</param>
        /// <param name="groupDataBinding">Data-bind operations to apply to outer form-group div, if any.</param>
        /// <param name="labelDataBinding">Data-bind operations to apply to label, if any.</param>
        /// <param name="requiredDataBinding">Boolean data-bind operation to apply to required state, if any.</param>
        /// <param name="customDataBinding">Extra custom data-binding operations to apply to field, if any.</param>
        /// <param name="dependencyFieldName">Defines default "enabled" subordinate data-bindings based a single boolean field, e.g., a check-box.</param>
        /// <param name="toolTip">Tool tip text to apply to field, if any.</param>
        /// <param name="initialFocus">Use field for initial focus.</param>
        /// <param name="enableHotLinks">Enable clickable URLs and e-mail addresses in view mode.</param>
        /// <returns>Generated HTML for new text area field based on specified parameters.</returns>
        public string AddTextAreaField(string fieldName, bool required, int maxLength = 0, int rows = 2, string fieldLabel = null, string fieldID = null, string groupDataBinding = null, string labelDataBinding = null, string requiredDataBinding = null, string customDataBinding = null, string dependencyFieldName = null, string toolTip = null, bool initialFocus = false, bool enableHotLinks = true)
        {
            RazorView addTextAreaTemplate = new RazorView(m_razorEngine, AddTextAreaFieldTemplate, m_exceptionHandler);
            DynamicViewBag viewBag = addTextAreaTemplate.ViewBag;

            if (string.IsNullOrEmpty(fieldID))
                fieldID = $"text{fieldName}";

            if (initialFocus)
                m_initialFocusField = fieldID;

            if (enableHotLinks)
            {
                AddReadOnlyHotLinkField(fieldID, $"readonly{fieldID}", fieldName, true);
                customDataBinding = $"{(string.IsNullOrWhiteSpace(customDataBinding) ? "" : $"{customDataBinding}, ")}visible: $root.recordMode()!==RecordMode.View";
            }

            viewBag.AddValue("FieldName", fieldName);
            viewBag.AddValue("Required", required);
            viewBag.AddValue("MaxLength", maxLength);
            viewBag.AddValue("Rows", rows);
            viewBag.AddValue("FieldLabel", fieldLabel ?? fieldName);
            viewBag.AddValue("FieldID", fieldID);
            viewBag.AddValue("GroupDataBinding", groupDataBinding);
            viewBag.AddValue("LabelDataBinding", labelDataBinding);
            viewBag.AddValue("RequiredDataBinding", requiredDataBinding);
            viewBag.AddValue("CustomDataBinding", customDataBinding);
            viewBag.AddValue("DependencyFieldName", dependencyFieldName);
            viewBag.AddValue("ToolTip", toolTip);

            return addTextAreaTemplate.Execute();
        }

        /// <summary>
        /// Generates template based select field based on reflected modeled table field attributes.
        /// </summary>
        /// <typeparam name="TModel">Modeled table for select field.</typeparam>
        /// <typeparam name="TOption">Modeled table for option data.</typeparam>
        /// <param name="fieldName">Field name for value of select field.</param>
        /// <param name="optionValueFieldName">Field name for ID of option data.</param>
        /// <param name="optionLabelFieldName">Field name for label of option data, defaults to <paramref name="optionValueFieldName"/></param>
        /// <param name="optionSortFieldName">Field name for sort order of option data, defaults to <paramref name="optionLabelFieldName"/></param>
        /// <param name="fieldLabel">Label name for select field, pulls from <see cref="LabelAttribute"/> if defined, otherwise defaults to <paramref name="fieldName"/>.</param>
        /// <param name="fieldID">ID to use for select field; defaults to select + <paramref name="fieldName"/>.</param>
        /// <param name="groupDataBinding">Data-bind operations to apply to outer form-group div, if any.</param>
        /// <param name="labelDataBinding">Data-bind operations to apply to label, if any.</param>
        /// <param name="requiredDataBinding">Boolean data-bind operation to apply to required state, if any.</param>
        /// <param name="customDataBinding">Extra custom data-binding operations to apply to field, if any.</param>
        /// <param name="dependencyFieldName">Defines default "enabled" subordinate data-bindings based a single boolean field, e.g., a check-box.</param>
        /// <param name="optionDataBinding">Data-bind operations to apply to each option value, if any.</param>
        /// <param name="toolTip">Tool tip text to apply to field, if any.</param>
        /// <param name="initialFocus">Use field for initial focus.</param>
        /// <param name="restriction">Record restriction to apply, if any.</param>
        /// <param name="allowUnset">Flag that determines if select can have no selected value.</param>
        /// <param name="unsetCaption">Label to show when no value is selected; defaults to "Select value...".</param>
        /// <param name="addEmptyRow">Flag that determines if an empty row should be added to options list.</param>
        /// <param name="emptyRowValue">Value to use for empty row; defaults to empty string.</param>
        /// <param name="showNoRecordOption">Flag that determines if an option representing no records should be shown if select query returns no values.</param>
        /// <param name="noRecordValue">Value for no records option when select query returns no values; defaults to "-1".</param>
        /// <param name="noRecordText">Text for no records option when select query returns no values; defaults to "No records".</param>
        /// <returns>Generated HTML for new text field based on modeled table field attributes.</returns>
        public string AddSelectField<TModel, TOption>(string fieldName, string optionValueFieldName, string optionLabelFieldName = null, string optionSortFieldName = null, string fieldLabel = null, string fieldID = null, string groupDataBinding = null, string labelDataBinding = null, string requiredDataBinding = null, string customDataBinding = null, string dependencyFieldName = null, string optionDataBinding = null, string toolTip = null, bool initialFocus = false, RecordRestriction restriction = null, bool allowUnset = false, string unsetCaption = "Select value...", bool addEmptyRow = false, string emptyRowValue = "", bool showNoRecordOption = false, string noRecordValue = "-1", string noRecordText = "No records") where TModel : class, new() where TOption : class, new()
        {
            if (string.IsNullOrEmpty(fieldLabel))
            {
                LabelAttribute labelAttribute;

                if (Table<TModel>().TryGetFieldAttribute(fieldName, out labelAttribute))
                    fieldLabel = labelAttribute.Label;
            }

            AddFieldValueInitializer<TModel>(fieldName);

            return AddSelectField<TOption>(fieldName, Table<TModel>().FieldHasAttribute<RequiredAttribute>(fieldName),
                optionValueFieldName, optionLabelFieldName, optionSortFieldName, fieldLabel, fieldID, groupDataBinding, labelDataBinding, requiredDataBinding, customDataBinding, dependencyFieldName, optionDataBinding, toolTip, initialFocus, restriction, allowUnset, unsetCaption, addEmptyRow, emptyRowValue, showNoRecordOption, noRecordValue, noRecordText);
        }

        /// <summary>
        /// Generates template based select field based on specified parameters.
        /// </summary>
        /// <typeparam name="TOption">Modeled table for option data.</typeparam>
        /// <param name="fieldName">Field name for value of select field.</param>
        /// <param name="required">Determines if field name is required.</param>
        /// <param name="optionValueFieldName">Field name for ID of option data.</param>
        /// <param name="optionLabelFieldName">Field name for label of option data, defaults to <paramref name="optionValueFieldName"/></param>
        /// <param name="optionSortFieldName">Field name for sort order of option data, defaults to <paramref name="optionLabelFieldName"/></param>
        /// <param name="fieldLabel">Label name for select field, defaults to <paramref name="fieldName"/>.</param>
        /// <param name="fieldID">ID to use for select field; defaults to select + <paramref name="fieldName"/>.</param>
        /// <param name="groupDataBinding">Data-bind operations to apply to outer form-group div, if any.</param>
        /// <param name="labelDataBinding">Data-bind operations to apply to label, if any.</param>
        /// <param name="requiredDataBinding">Boolean data-bind operation to apply to required state, if any.</param>
        /// <param name="customDataBinding">Extra custom data-binding operations to apply to field, if any.</param>
        /// <param name="dependencyFieldName">Defines default "enabled" subordinate data-bindings based a single boolean field, e.g., a check-box.</param>
        /// <param name="optionDataBinding">Data-bind operations to apply to each option value, if any.</param>
        /// <param name="toolTip">Tool tip text to apply to field, if any.</param>
        /// <param name="initialFocus">Use field for initial focus.</param>
        /// <param name="restriction">Record restriction to apply, if any.</param>
        /// <param name="allowUnset">Flag that determines if select can have no selected value.</param>
        /// <param name="unsetCaption">Label to show when no value is selected; defaults to "Select value...".</param>
        /// <param name="addEmptyRow">Flag that determines if an empty row should be added to options list.</param>
        /// <param name="emptyRowValue">Value to use for empty row; defaults to empty string.</param>
        /// <param name="showNoRecordOption">Flag that determines if an option representing no records should be shown if select query returns no values.</param>
        /// <param name="noRecordValue">Value for no records option when select query returns no values; defaults to "-1".</param>
        /// <param name="noRecordText">Text for no records option when select query returns no values; defaults to "No records".</param>
        /// <returns>Generated HTML for new text field based on specified parameters.</returns>
        public string AddSelectField<TOption>(string fieldName, bool required, string optionValueFieldName, string optionLabelFieldName = null, string optionSortFieldName = null, string fieldLabel = null, string fieldID = null, string groupDataBinding = null, string labelDataBinding = null, string requiredDataBinding = null, string customDataBinding = null, string dependencyFieldName = null, string optionDataBinding = null, string toolTip = null, bool initialFocus = false, RecordRestriction restriction = null, bool allowUnset = false, string unsetCaption = "Select value...", bool addEmptyRow = false, string emptyRowValue = "", bool showNoRecordOption = false, string noRecordValue = "-1", string noRecordText = "No records") where TOption : class, new()
        {
            RazorView addSelectFieldTemplate = new RazorView(m_razorEngine, AddSelectFieldTemplate, m_exceptionHandler);
            DynamicViewBag viewBag = addSelectFieldTemplate.ViewBag;
            TableOperations<TOption> optionTableOperations = Table<TOption>();
            Dictionary<string, string> options = new Dictionary<string, string>();
            string optionTableName = typeof(TOption).Name;

            optionLabelFieldName = optionLabelFieldName ?? optionValueFieldName;
            optionSortFieldName = optionSortFieldName ?? optionLabelFieldName;
            fieldLabel = fieldLabel ?? optionTableName;

            if (string.IsNullOrEmpty(fieldID))
                fieldID = $"select{fieldName}";

            if (initialFocus)
                m_initialFocusField = fieldID;

            viewBag.AddValue("FieldName", fieldName);
            viewBag.AddValue("Required", required);
            viewBag.AddValue("FieldLabel", fieldLabel);
            viewBag.AddValue("FieldID", fieldID);
            viewBag.AddValue("GroupDataBinding", groupDataBinding);
            viewBag.AddValue("LabelDataBinding", labelDataBinding);
            viewBag.AddValue("RequiredDataBinding", requiredDataBinding);
            viewBag.AddValue("CustomDataBinding", customDataBinding);
            viewBag.AddValue("DependencyFieldName", dependencyFieldName);
            viewBag.AddValue("ToolTip", toolTip);
            viewBag.AddValue("OptionDataBinding", optionDataBinding);
            viewBag.AddValue("AllowUnset", allowUnset);
            viewBag.AddValue("UnsetCaption", unsetCaption);

            if (addEmptyRow)
                options.Add("", emptyRowValue);

            foreach (TOption record in Table<TOption>().QueryRecords(optionSortFieldName, restriction))
            {
                if (record != null)
                    options.Add(optionTableOperations.GetFieldValue(record, optionValueFieldName).ToString(), optionTableOperations.GetFieldValue(record, optionLabelFieldName).ToNonNullString(fieldLabel));
            }

            if (options.Count == 0 && showNoRecordOption)
                options.Add(noRecordValue, noRecordText);

            viewBag.AddValue("Options", options);

            return addSelectFieldTemplate.Execute();
        }

        /// <summary>
        /// Generates template based check box field based on reflected modeled table field attributes.
        /// </summary>
        /// <typeparam name="TModel">Modeled table.</typeparam>
        /// <param name="fieldName">Field name for value of check box field.</param>
        /// <param name="fieldLabel">Label name for check box field, defaults to <paramref name="fieldName"/>.</param>
        /// <param name="fieldID">ID to use for check box field; defaults to check + <paramref name="fieldName"/>.</param>
        /// <param name="groupDataBinding">Data-bind operations to apply to outer form-group div, if any.</param>
        /// <param name="labelDataBinding">Data-bind operations to apply to label, if any.</param>
        /// <param name="customDataBinding">Extra custom data-binding operations to apply to field, if any.</param>
        /// <param name="dependencyFieldName">Defines default "enabled" subordinate data-bindings based a single boolean field, e.g., a check-box.</param>
        /// <param name="toolTip">Tool tip text to apply to field, if any.</param>
        /// <param name="initialFocus">Use field for initial focus.</param>
        /// <returns>Generated HTML for new check box field based on modeled table field attributes.</returns>
        public string AddCheckBoxField<TModel>(string fieldName, string fieldLabel = null, string fieldID = null, string groupDataBinding = null, string labelDataBinding = null, string customDataBinding = null, string dependencyFieldName = null, string toolTip = null, bool initialFocus = false) where TModel : class, new()
        {
            if (string.IsNullOrEmpty(fieldLabel))
            {
                LabelAttribute labelAttribute;

                if (Table<TModel>().TryGetFieldAttribute(fieldName, out labelAttribute))
                    fieldLabel = labelAttribute.Label;
            }

            AddFieldValueInitializer<TModel>(fieldName);

            return AddCheckBoxField(fieldName, fieldLabel, fieldID, groupDataBinding, labelDataBinding, customDataBinding, dependencyFieldName, toolTip, initialFocus);
        }

        /// <summary>
        /// Generates template based check box field based on specified parameters.
        /// </summary>
        /// <param name="fieldName">Field name for value of check box field.</param>
        /// <param name="fieldLabel">Label name for check box field, defaults to <paramref name="fieldName"/>.</param>
        /// <param name="fieldID">ID to use for check box field; defaults to check + <paramref name="fieldName"/>.</param>
        /// <param name="groupDataBinding">Data-bind operations to apply to outer form-group div, if any.</param>
        /// <param name="labelDataBinding">Data-bind operations to apply to label, if any.</param>
        /// <param name="customDataBinding">Extra custom data-binding operations to apply to field, if any.</param>
        /// <param name="dependencyFieldName">Defines default "enabled" subordinate data-bindings based a single boolean field, e.g., a check-box.</param>
        /// <param name="toolTip">Tool tip text to apply to field, if any.</param>
        /// <param name="initialFocus">Use field for initial focus.</param>
        /// <returns>Generated HTML for new check box field based on specified parameters.</returns>
        public string AddCheckBoxField(string fieldName, string fieldLabel = null, string fieldID = null, string groupDataBinding = null, string labelDataBinding = null, string customDataBinding = null, string dependencyFieldName = null, string toolTip = null, bool initialFocus = false)
        {
            RazorView addCheckBoxFieldTemplate = new RazorView(m_razorEngine, AddCheckBoxFieldTemplate, m_exceptionHandler);
            DynamicViewBag viewBag = addCheckBoxFieldTemplate.ViewBag;

            if (string.IsNullOrEmpty(fieldID))
                fieldID = $"check{fieldName}";

            if (initialFocus)
                m_initialFocusField = fieldID;

            viewBag.AddValue("FieldName", fieldName);
            viewBag.AddValue("FieldLabel", fieldLabel ?? fieldName);
            viewBag.AddValue("FieldID", fieldID);
            viewBag.AddValue("GroupDataBinding", groupDataBinding);
            viewBag.AddValue("LabelDataBinding", labelDataBinding);
            viewBag.AddValue("CustomDataBinding", customDataBinding);
            viewBag.AddValue("DependencyFieldName", dependencyFieldName);
            viewBag.AddValue("ToolTip", toolTip);

            return addCheckBoxFieldTemplate.Execute();
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly ConcurrentDictionary<Type, RecordOperationsCache> s_recordOperationCaches;

        // Static Constructor
        static DataContext()
        {
            s_recordOperationCaches = new ConcurrentDictionary<Type, RecordOperationsCache>();
        }

        // Static Methods

        private static bool IsIntegerType(Type type)
        {
            return
                type == typeof(byte) ||
                type == typeof(sbyte) ||
                type == typeof(short) ||
                type == typeof(ushort) ||
                type == typeof(Int24) ||
                type == typeof(UInt24) ||
                type == typeof(int) ||
                type == typeof(uint) ||
                type == typeof(long) ||
                type == typeof(ulong);
        }

        private static bool IsNumericType(Type type)
        {
            return
                type == typeof(float) ||
                type == typeof(double) ||
                type == typeof(decimal);
        }

        #endregion
    }
}
