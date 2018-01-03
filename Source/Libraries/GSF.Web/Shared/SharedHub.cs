//******************************************************************************************************
//  SecurityHub.cs - Gbtc
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
//  03/03/2016 - Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using GSF.Data.Model;
using GSF.Identity;
using GSF.Security;
using GSF.Security.Model;
using GSF.Web.Hubs;
using GSF.Web.Model;
using Microsoft.AspNet.SignalR.Hubs;
using GSF.Web.Security;
using System.Threading.Tasks;
using GSF.Web.Shared.Model;

namespace GSF.Web.Shared
{
    /// <summary>
    /// Defines a SignalR security hub for managing users, groups and SID management.
    /// </summary>
    [AuthorizeHubRole]
    public class SharedHub : RecordOperationsHub<SharedHub>
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="SharedHub"/>.
        /// </summary>
        public SharedHub() : 
            this(null, null, null)
        {
        }

        /// <summary>
        /// Creates a new <see cref="SharedHub"/> with the specified logging functions.
        /// </summary>
        /// <param name="logStatusMessageFunction">Delegate to use to log status messages, if any.</param>
        /// <param name="logExceptionFunction">Delegate to use to log exceptions, if any.</param>
        public SharedHub(Action<string, UpdateType> logStatusMessageFunction, Action<Exception> logExceptionFunction) :
            this(null, logStatusMessageFunction, logExceptionFunction)
        {
        }

        /// <summary>
        /// Creates a new <see cref="SharedHub"/> with the specified <see cref="DataContext"/> and logging functions.
        /// </summary>
        /// <param name="settingsCategory">Setting category that contains the connection settings. Defaults to "securityProvider".</param>
        /// <param name="logStatusMessageFunction">Delegate to use to log status messages, if any.</param>
        /// <param name="logExceptionFunction">Delegate to use to log exceptions, if any.</param>
        public SharedHub(string settingsCategory, Action<string, UpdateType> logStatusMessageFunction, Action<Exception> logExceptionFunction) : 
            this(settingsCategory, logStatusMessageFunction, logExceptionFunction, true)
        {
            // Capture initial defaults
            if ((object)logStatusMessageFunction != null && (object)s_logStatusMessageFunction == null)
                s_logStatusMessageFunction = logStatusMessageFunction;

            if ((object)logExceptionFunction != null && (object)s_logExceptionFunction == null)
                s_logExceptionFunction = logExceptionFunction;
        }

        // ReSharper disable once UnusedParameter.Local
        private SharedHub(string settingsCategory, Action<string, UpdateType> logStatusMessageFunction, Action<Exception> logExceptionFunction, bool overload) :
            base(settingsCategory ?? "securityProvider", logStatusMessageFunction ?? s_logStatusMessageFunction, logExceptionFunction ?? s_logExceptionFunction)
        {
        }


        #endregion

        #region [ Methods ]
        /// <summary>
        /// Overrides base OnConnected method to provide logging
        /// </summary>
        /// <returns></returns>
        public override Task OnConnected()
        {
            LogStatusMessage($"SharedHub connect by {Context.User?.Identity?.Name ?? "Undefined User"} [{Context.ConnectionId}] - count = {ConnectionCount}", UpdateType.Information, false);
            return base.OnConnected();
        }

        /// <summary>
        /// Overrides base OnDisconnected method to provide logging
        /// </summary>
        /// <param name="stopCalled"></param>
        /// <returns></returns>
        public override Task OnDisconnected(bool stopCalled)
        {
            if (stopCalled)
            {
                // Dispose any associated hub operations associated with current SignalR client
                LogStatusMessage($"SharedHub disconnect by {Context.User?.Identity?.Name ?? "Undefined User"} [{Context.ConnectionId}] - count = {ConnectionCount}", UpdateType.Information, false);
            }

            return base.OnDisconnected(stopCalled);
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static Action<string, UpdateType> s_logStatusMessageFunction;
        private static Action<Exception> s_logExceptionFunction;

        // Static Properties

        /// <summary>
        /// Gets current default Node ID for security.
        /// </summary>
        public static readonly Guid DefaultNodeID = AdoSecurityProvider.DefaultNodeID;

        #endregion

        // Client-side script functionality

        #region [ Company Table Operations ]
        /// <summary>
        /// Queries company table counts
        /// </summary>
        /// <param name="filterText">Text to filter search down to.</param>
        /// <returns>The count of the rows in the table matching the query.</returns>
        [RecordOperation(typeof(Company), RecordOperation.QueryRecordCount)]
        public int QueryCompanyCount(string filterText)
        {
            return DataContext.Table<Company>().QueryRecordCount(filterText);
        }

        /// <summary>
        /// Queries company table.
        /// </summary>
        /// <param name="sortField">Table field label to sort on.</param>
        /// <param name="ascending">Boolean denoting direction of sort.</param>
        /// <param name="page">The page number to return.</param>
        /// <param name="pageSize">The size of the page.</param>
        /// <param name="filterText">Text to filter search down to.</param>
        /// <returns>The rows in the table matching the query.</returns>
        [RecordOperation(typeof(Company), RecordOperation.QueryRecords)]
        public IEnumerable<Company> QueryCompanies(string sortField, bool ascending, int page, int pageSize, string filterText)
        {
            return DataContext.Table<Company>().QueryRecords(sortField, ascending, page, pageSize, filterText);
        }

        /// <summary>
        /// Deletes record from company table.
        /// </summary>
        /// <param name="id">The id of the record to delete.</param>
        [AuthorizeHubRole("Administrator, Editor")]
        [RecordOperation(typeof(Company), RecordOperation.DeleteRecord)]
        public void DeleteCompany(int id)
        {
            DataContext.Table<Company>().DeleteRecord(id);
        }

        /// <summary>
        /// Creates a new Company object.
        /// </summary>
        /// <returns>A new company instance</returns>
        [RecordOperation(typeof(Company), RecordOperation.CreateNewRecord)]
        public Company NewCompany()
        {
            return DataContext.Table<Company>().NewRecord();
        }

        /// <summary>
        /// Adds new record to company table.
        /// </summary>
        /// <param name="company">The record to add.</param>
        [AuthorizeHubRole("Administrator, Editor")]
        [RecordOperation(typeof(Company), RecordOperation.AddNewRecord)]
        public void AddNewCompany(Company company)
        {
            DataContext.Table<Company>().AddNewRecord(company);
        }

        /// <summary>
        /// Updates a record in company table.
        /// </summary>
        /// <param name="company">The record to update.</param>
        [AuthorizeHubRole("Administrator, Editor")]
        [RecordOperation(typeof(Company), RecordOperation.UpdateRecord)]
        public void UpdateCompany(Company company)
        {
            DataContext.Table<Company>().UpdateRecord(company);
        }

        #endregion

        #region [ Vendor Table Operations ]
        /// <summary>
        /// Queries vender table count.
        /// </summary>
        /// <param name="filterText">Text to filter search down to.</param>
        /// <returns>The count of the rows in the table matching the query.</returns>
        [RecordOperation(typeof(Vendor), RecordOperation.QueryRecordCount)]
        public int QueryVendorCount(string filterText)
        {
            return DataContext.Table<Vendor>().QueryRecordCount(filterText);
        }

        /// <summary>
        /// Queries vendor table records.
        /// </summary>
        /// <param name="sortField">Table field label to sort on.</param>
        /// <param name="ascending">Boolean denoting direction of sort.</param>
        /// <param name="page">The page number to return.</param>
        /// <param name="pageSize">The size of the page.</param>
        /// <param name="filterText">Text to filter search down to.</param>
        /// <returns>The rows in the table matching the query.</returns>
        [RecordOperation(typeof(Vendor), RecordOperation.QueryRecords)]
        public IEnumerable<Vendor> QueryVendors(string sortField, bool ascending, int page, int pageSize, string filterText)
        {
            return DataContext.Table<Vendor>().QueryRecords(sortField, ascending, page, pageSize, filterText);
        }

        /// <summary>
        /// Deletes vendor table record.
        /// </summary>
        /// <param name="id">The id of the record to delete.</param>
        [AuthorizeHubRole("Administrator, Editor")]
        [RecordOperation(typeof(Vendor), RecordOperation.DeleteRecord)]
        public void DeleteVendor(int id)
        {
            DataContext.Table<Vendor>().DeleteRecord(id);
        }

        /// <summary>
        /// Creates new vendor object.
        /// </summary>
        /// <returns>A new Vendor model instance.</returns>
        [RecordOperation(typeof(Vendor), RecordOperation.CreateNewRecord)]
        public Vendor NewVendor()
        {
            return DataContext.Table<Vendor>().NewRecord();
        }

        /// <summary>
        /// Adds new vendor record to table.
        /// </summary>
        /// <param name="vendor">The record to add.</param>
        [AuthorizeHubRole("Administrator, Editor")]
        [RecordOperation(typeof(Vendor), RecordOperation.AddNewRecord)]
        public void AddNewVendor(Vendor vendor)
        {
            DataContext.Table<Vendor>().AddNewRecord(vendor);
        }

        /// <summary>
        /// Updates record in vendor table.
        /// </summary>
        /// <param name="vendor">The record to update.</param>
        [AuthorizeHubRole("Administrator, Editor")]
        [RecordOperation(typeof(Vendor), RecordOperation.UpdateRecord)]
        public void UpdateVendor(Vendor vendor)
        {
            DataContext.Table<Vendor>().UpdateRecord(vendor);
        }

        #endregion

        #region [ VendorDevice Table Operations ]
        /// <summary>
        /// Queries vendor device table count.
        /// </summary>
        /// <param name="filterText">Text to filter search down to.</param>
        /// <returns>The count of the rows in the table matching the query.</returns>
        [RecordOperation(typeof(VendorDevice), RecordOperation.QueryRecordCount)]
        public int QueryVendorDeviceCount(string filterText)
        {
            return DataContext.Table<VendorDevice>().QueryRecordCount(filterText);
        }

        /// <summary>
        /// Queries vendor device table records.
        /// </summary>
        /// <param name="sortField">Table field label to sort on.</param>
        /// <param name="ascending">Boolean denoting direction of sort.</param>
        /// <param name="page">The page number to return.</param>
        /// <param name="pageSize">The size of the page.</param>
        /// <param name="filterText">Text to filter search down to.</param>
        /// <returns>The rows in the table matching the query.</returns>
        [RecordOperation(typeof(VendorDevice), RecordOperation.QueryRecords)]
        public IEnumerable<VendorDevice> QueryVendorDevices(string sortField, bool ascending, int page, int pageSize, string filterText)
        {
            return DataContext.Table<VendorDevice>().QueryRecords(sortField, ascending, page, pageSize, filterText);
        }

        /// <summary>
        /// Deletes record from vendor device table.
        /// </summary>
        /// <param name="id">The id of the record to delete.</param>
        [AuthorizeHubRole("Administrator, Editor")]
        [RecordOperation(typeof(VendorDevice), RecordOperation.DeleteRecord)]
        public void DeleteVendorDevice(int id)
        {
            DataContext.Table<VendorDevice>().DeleteRecord(id);
        }

        /// <summary>
        /// Creates new VendorDevice object.
        /// </summary>
        /// <returns>A new VendorDevice model instance.</returns>
        [RecordOperation(typeof(VendorDevice), RecordOperation.CreateNewRecord)]
        public VendorDevice NewVendorDevice()
        {
            return DataContext.Table<VendorDevice>().NewRecord();
        }

        /// <summary>
        /// Adds new record to vendor device table.
        /// </summary>
        /// <param name="vendorDevice">The record to add.</param>
        [AuthorizeHubRole("Administrator, Editor")]
        [RecordOperation(typeof(VendorDevice), RecordOperation.AddNewRecord)]
        public void AddNewVendorDevice(VendorDevice vendorDevice)
        {
            DataContext.Table<VendorDevice>().AddNewRecord(vendorDevice);
        }

        /// <summary>
        /// Updates record in vendor device table.
        /// </summary>
        /// <param name="vendorDevice">The record to update.</param>
        [AuthorizeHubRole("Administrator, Editor")]
        [RecordOperation(typeof(VendorDevice), RecordOperation.UpdateRecord)]
        public void UpdateVendorDevice(VendorDevice vendorDevice)
        {
            DataContext.Table<VendorDevice>().UpdateRecord(vendorDevice);
        }

        #endregion
    }
}
