//******************************************************************************************************
//  IRazorView.cs - Gbtc
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
//  05/05/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace GSF.Web.Model
{
    /// <summary>
    /// Defines an interface for <see cref="RazorView{TLanguage}"/> implementations.
    /// </summary>
    public interface IRazorView
    {
        /// <summary>
        /// Gets or sets name of template file.
        /// </summary>
        string TemplateName
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets reference to model to use when rendering template.
        /// </summary>
        object Model
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets type of <see cref="Model"/>.
        /// </summary>
        Type ModelType
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets delegate used to handle exceptions.
        /// </summary>
        Action<Exception> ExceptionHandler
        {
            get; set;
        }

        /// <summary>
        /// Gets reference to view bag used when rendering template.
        /// </summary>
        dynamic ViewBag
        {
            get;
        }

        /// <summary>
        /// Gets query string parameter specified by <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Name of query string parameter to retrieve.</param>
        /// <returns>Query string parameter specified by <paramref name="key"/>.</returns>
        string this[string key]
        {
            get;
        }
        
        /// <summary>
        /// Gets a dictionary of query string parameters passed to rendered view.
        /// </summary>
        Dictionary<string, string> Parameters
        {
            get;
        }

        /// <summary>
        /// Compiles and executes view template.
        /// </summary>
        /// <returns>Rendered result.</returns>
        string Execute();

        /// <summary>
        /// Compiles and executes view template for specified request message and post data.
        /// </summary>
        /// <returns>Rendered result.</returns>
        string Execute(HttpRequestMessage requestMessage, dynamic postData);

        /// <summary>
        /// Asynchronously compiles and executes view template for specified request message and post data.
        /// </summary>
        /// <returns>Task that will provide rendered result.</returns>
        Task ExecuteAsync(HttpRequestMessage requestMessage, dynamic postData);
    }
}
