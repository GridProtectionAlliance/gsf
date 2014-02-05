//******************************************************************************************************
//  ISelfHostingService.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  08/21/2009 - Pinal C. Patel
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  05/28/2010 - Pinal C. Patel
//       Added an endpoint for web service help.
//  10/08/2010 - Pinal C. Patel
//       Removed REST web service help endpoint since a similar feature is now part of WCF 4.0.
//  10/14/2010 - Pinal C. Patel
//       Made changes for hosting flexibility and enabling security:
//         Deleted DataFlow since access restriction can now be imposed by enabling security.
//         Added SecurityPolicy and PublishMetadata.
//         Renamed ServiceUri to Endpoints and ServiceContract to Contract.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.IdentityModel.Policy;
using System.ServiceModel;
using GSF.Adapters;

namespace GSF.ServiceModel
{
    /// <summary>
    /// Defines a web service that can send and receive data over REST (Representational State Transfer) interface.
    /// </summary>
    [ServiceContract]
    public interface ISelfHostingService : IAdapter
    {
        #region [ Members ]

        /// <summary>
        /// Occurs when the <see cref="ServiceHost"/> has been created with the specified <see cref="Endpoints"/>.
        /// </summary>
        event EventHandler ServiceHostCreated;

        /// <summary>
        /// Occurs when the <see cref="ServiceHost"/> can process requests via all of its endpoints.
        /// </summary>
        event EventHandler ServiceHostStarted;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered when processing a request.
        /// </summary>
        event EventHandler<EventArgs<Exception>> ServiceProcessException;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a semicolon delimited list of URIs where the web service can be accessed.
        /// </summary>
        string Endpoints
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the <see cref="Type.FullName"/> of the contract interface implemented by the web service.
        /// </summary>
        string ContractInterface
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the <see cref="ServiceHost"/> will use the current instance of the web service for processing 
        /// requests or base the web service instance creation on <see cref="InstanceContextMode"/> specified in its <see cref="ServiceBehaviorAttribute"/>.
        /// </summary>
        bool Singleton
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the <see cref="Type.FullName"/> of <see cref="IAuthorizationPolicy"/> to be used for securing all web service <see cref="Endpoints"/>.
        /// </summary>
        string SecurityPolicy
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether web service metadata is to made available at all web service <see cref="Endpoints"/>.
        /// </summary>
        bool PublishMetadata
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the <see cref="ServiceHost"/> hosting the web service.
        /// </summary>
        ServiceHost ServiceHost
        {
            get;
        }

        #endregion
    }
}
