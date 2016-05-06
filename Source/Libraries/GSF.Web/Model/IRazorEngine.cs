//******************************************************************************************************
//  IRazorEngine.cs - Gbtc
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
using System.Threading.Tasks;
using RazorEngine.Templating;

namespace GSF.Web.Model
{
    /// <summary>
    /// Defines an interface for <see cref="RazorEngine{TLanguage}"/> implementations.
    /// </summary>
    public interface IRazorEngine : IRazorEngineService
    {
        /// <summary>
        /// Gets the template path defined for this <see cref="IRazorEngine"/>.
        /// </summary>
        string TemplatePath
        {
            get;
        }

        /// <summary>
        /// Kicks off a task to pre-compile Razor templates.
        /// </summary>
        /// <param name="exceptionHandler">Exception handler used to report issues, if any.</param>
        Task PreCompile(Action<Exception> exceptionHandler = null);
    }
}
