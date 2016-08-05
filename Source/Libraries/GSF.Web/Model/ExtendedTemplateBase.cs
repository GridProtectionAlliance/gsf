using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        #endregion
    }

    /// <summary>
    /// Represents an extended RazorEngine modeled template base with helper functions.
    /// </summary>
    public abstract class ExtendedTemplateBase<T> : TemplateBase<T>
    {
        #region [ Members ]

        // Fields
        private readonly HtmlHelper<T> m_htmlHelper;
        private readonly UrlHelper m_urlHelper;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ExtendedTemplateBase"/>.
        /// </summary>
        protected ExtendedTemplateBase()
        {
            m_htmlHelper = new HtmlHelper<T>(this);
            m_urlHelper = new UrlHelper();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets HTML based helper functions.
        /// </summary>
        public HtmlHelper<T> Html => m_htmlHelper;

        /// <summary>
        /// Gets URL based helper functions.
        /// </summary>
        public UrlHelper Url => m_urlHelper;

        #endregion
    }
}
