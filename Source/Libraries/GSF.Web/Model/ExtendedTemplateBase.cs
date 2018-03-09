//******************************************************************************************************
//  ExtendedTemplateBase.cs - Gbtc
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
//  08/05/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Web;
using RazorEngine.Templating;

namespace GSF.Web.Model
{
    /// <summary>
    /// Represents an extended RazorEngine template base with helper functions.
    /// </summary>
    public abstract class ExtendedTemplateBase : TemplateBase
    {
        #region [ Members ]

        // Fields
        private readonly HtmlHelper m_htmlHelper;
        private readonly UrlHelper m_urlHelper;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ExtendedTemplateBase"/>.
        /// </summary>
        protected ExtendedTemplateBase()
        {
            m_htmlHelper = new HtmlHelper(this);
            m_urlHelper = new UrlHelper();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets HTML based helper functions.
        /// </summary>
        public HtmlHelper Html => m_htmlHelper;

        /// <summary>
        /// Gets URL based helper functions.
        /// </summary>
        public UrlHelper Url => m_urlHelper;

        /// <summary>
        /// Gets the current <see cref="HttpContext"/>.
        /// </summary>
        protected HttpContext Context => HttpContext.Current;

        #endregion
    }

    /// <summary>
    /// Represents an extended RazorEngine modeled template base with helper functions.
    /// </summary>
    public abstract class ExtendedTemplateBase<T> : TemplateBase<T>
    {
        #region [ Members ]

        // Fields
        private readonly HtmlHelper m_htmlHelper;
        private readonly UrlHelper m_urlHelper;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ExtendedTemplateBase"/>.
        /// </summary>
        protected ExtendedTemplateBase()
        {
            m_htmlHelper = new HtmlHelper(this);
            m_urlHelper = new UrlHelper();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets HTML based helper functions.
        /// </summary>
        public HtmlHelper Html => m_htmlHelper;

        /// <summary>
        /// Gets URL based helper functions.
        /// </summary>
        public UrlHelper Url => m_urlHelper;

        /// <summary>
        /// Gets the current <see cref="HttpContext"/>.
        /// </summary>
        protected HttpContext Context => HttpContext.Current;

        #endregion
    }
}
