//******************************************************************************************************
//  Searcher.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  10/25/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

namespace GSF.Searching
{
    /// <summary>
    /// Uses reflection to search a collection of items for a given search string.
    /// </summary>
    /// <typeparam name="TSearch">The type of the objects to be searched.</typeparam>
    public class AsyncSearcher<TSearch>
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Event triggered when the searcher finds a match.
        /// </summary>
        public event EventHandler<EventArgs<IEnumerable<TSearch>>> MatchesFound;

        /// <summary>
        /// Event triggered when the searcher has finished searching all the items in its queue.
        /// </summary>
        public event EventHandler SearchComplete;

        // Fields
        private string m_searchText;
        private ICollection<string> m_searchCategories;
        private bool m_ignoreCase;
        private bool m_useWildcards;
        private bool m_useRegex;

        private string[] m_tokens;
        private PropertyInfo[] m_searchProperties;
        private RegexOptions m_regexOptions;
        private bool m_usingWildcards;

        private ConcurrentQueue<IEnumerable<TSearch>> m_itemsToSearch;
        private int m_searching;
        private int m_cancel;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="AsyncSearcher{T}"/> class.
        /// </summary>
        public AsyncSearcher()
        {
            m_searchCategories = new List<string>();
            m_itemsToSearch = new ConcurrentQueue<IEnumerable<TSearch>>();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the text to be searched for.
        /// </summary>
        public string SearchText
        {
            get
            {
                return m_searchText;
            }
            set
            {
                m_searchText = value;
            }
        }

        /// <summary>
        /// Gets the names of the properties to which
        /// the search string will be applied.
        /// </summary>
        public ICollection<string> SearchCategories
        {
            get
            {
                return m_searchCategories;
            }
        }

        /// <summary>
        /// Gets or sets the flag that determines whether
        /// to ignore case when running the search.
        /// </summary>
        public bool IgnoreCase
        {
            get
            {
                return m_ignoreCase;
            }
            set
            {
                m_ignoreCase = value;
            }
        }

        /// <summary>
        /// Gets or sets the flag that determines whether to process
        /// <c>*</c>, <c>?</c>, and <c>-</c> characters as wildcards.
        /// </summary>
        /// <remarks>
        /// The <see cref="UseRegex"/> property supercedes this property.
        /// In order to use wildcards, <c>UseRegex</c> must be set to <c>false</c>.
        /// </remarks>
        public bool UseWildcards
        {
            get
            {
                return m_useWildcards;
            }
            set
            {
                m_useWildcards = value;
            }
        }

        /// <summary>
        /// Gets or sets the flag that determines whether to
        /// process the search tokens as regular expressions.
        /// </summary>
        /// <remarks>
        /// This property supercedes the <see cref="UseWildcards"/> property.
        /// In order to use wildcards, this property must be set to <c>false</c>.
        /// </remarks>
        public bool UseRegex
        {
            get
            {
                return m_useRegex;
            }
            set
            {
                m_useRegex = value;
            }
        }

        /// <summary>
        /// Gets the flag that indicates whether the <see cref="AsyncSearcher{T}"/>
        /// is in the middle of a search operation.
        /// </summary>
        public bool Searching
        {
            get
            {
                return (Interlocked.CompareExchange(ref m_searching, 0, 0) != 0);
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Adds a collection of items to the search.
        /// </summary>
        /// <param name="itemsToSearch">The items to be searched.</param>
        public void Add(IEnumerable<TSearch> itemsToSearch)
        {
            IEnumerable<TSearch> dequeuedItems;

            if (!Searching)
            {
                // Parse the search text into tokens
                m_tokens = m_searchText.Split(' ');

                // Build the collection of properties to be searched
                m_searchProperties = m_searchCategories.Select(category => typeof(TSearch).GetProperty(category)).ToArray();

                // Assign regex options based on the ignore case option
                m_regexOptions = m_ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
                m_usingWildcards = false;

                if (!m_useRegex)
                {
                    // We are not using regex so escape the tokens...
                    m_tokens = m_tokens.Select(token => Regex.Escape(token)).ToArray();

                    if (m_useWildcards)
                    {
                        // ... however we are using wildcards so replace asterisks and question marks with their regex equivalents
                        m_tokens = m_tokens.Select(token => token.Replace(@"\*", ".*").Replace(@"\?", ".?")).ToArray();
                        m_usingWildcards = true;
                    }
                }
            }

            // Queue up the items in the search queue
            m_itemsToSearch.Enqueue(itemsToSearch);

            if (Interlocked.CompareExchange(ref m_searching, 1, 0) == 0)
            {
                // If we are not already searching, then begin the search
                if (m_itemsToSearch.TryDequeue(out dequeuedItems))
                    ThreadPool.QueueUserWorkItem(state => Search((IEnumerable<TSearch>)state), dequeuedItems);
            }
        }

        /// <summary>
        /// Clears the queue of items to search.
        /// </summary>
        /// <remarks>
        /// After a search is cancelled, there may still
        /// be items lingering in the queue. This method
        /// allows the user to clear them out.
        /// </remarks>
        public void Clear()
        {
            m_itemsToSearch = new ConcurrentQueue<IEnumerable<TSearch>>();
        }

        /// <summary>
        /// Cancels currently running searches.
        /// </summary>
        public void Cancel()
        {
            if (Searching)
                Interlocked.Exchange(ref m_cancel, 1);
        }

        private void Search(IEnumerable<TSearch> itemsToSearch)
        {
            IEnumerable<TSearch> dequeuedItems;
            List<TSearch> matches;
            string pattern;
            bool inverse;
            bool isMatch;

            matches = new List<TSearch>();

            foreach (TSearch item in itemsToSearch)
            {
                isMatch = true;

                foreach (string token in m_tokens)
                {
                    // Determine whether this token is being used for normal matching or inverse matching
                    inverse = m_usingWildcards && token.StartsWith("-");
                    pattern = inverse ? token.Substring(1) : token;

                    // If any of the properties that we are searching
                    // match the expression, then it counts as a match
                    isMatch = true; 

                    //m_searchProperties
                    //    .Select(property => property.GetValue(item).ToNonNullString())
                    //    .Select(value => Regex.IsMatch(value, pattern, m_regexOptions))
                    //    .Any(match => match);

                    // If the token is inverted,
                    // invert the match
                    if (inverse)
                        isMatch = !isMatch;

                    // If any one token fails to match, then
                    // the item does not match the search text
                    if (!isMatch)
                        break;
                }

                // If the item matches the search text,
                // add it to the collection of matches
                if (isMatch)
                    matches.Add(item);
            }

            // Notify if matches are found
            if (matches.Count > 0)
                OnMatchesFound(matches);

            if (Interlocked.CompareExchange(ref m_cancel, 1, 0) == 0 && m_itemsToSearch.TryDequeue(out dequeuedItems))
            {
                // If there are more items to be searched, continue the search
                ThreadPool.QueueUserWorkItem(state => Search((IEnumerable<TSearch>)state), dequeuedItems);
            }
            else
            {
                // ... otherwise, complete the search and notify
                Interlocked.Exchange(ref m_searching, 0);
                OnSearchComplete();
            }
        }

        private void OnMatchesFound(IEnumerable<TSearch> matches)
        {
            if ((object)MatchesFound != null)
                MatchesFound(this, new EventArgs<IEnumerable<TSearch>>(matches));
        }

        private void OnSearchComplete()
        {
            if ((object)SearchComplete != null)
                SearchComplete(this, EventArgs.Empty);
        }

        #endregion
    }
}
