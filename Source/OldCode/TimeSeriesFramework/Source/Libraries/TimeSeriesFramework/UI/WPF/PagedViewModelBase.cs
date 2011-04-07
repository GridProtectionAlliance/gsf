//******************************************************************************************************
//  PagedViewModelBase.cs - Gbtc
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
//  04/07/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using TimeSeriesFramework.UI.Commands;

namespace TimeSeriesFramework.UI
{
    /// <summary>
    /// Represents an abstract class with paging support which all ViewModel objects derive from.
    /// </summary>
    public abstract class PagedViewModelBase : INotifyPropertyChanged
    {
        #region [ Members ]

        //Events
        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        //Fields
        private int m_pageCount, m_currentPageNumber, m_itemsPerPage;
        private ObservableCollection<object> m_currentPage, m_itemsSource;
        private ObservableCollection<ObservableCollection<object>> m_pages;
        private ICommand m_firstCommand, m_previousCommand, m_nextCommand, m_lastCommand;
        private object m_currentItem;

        #endregion

        #region [ Properties ]
        
        /// <summary>
        /// Gets a message box to display message to users.
        /// </summary>
        public Action<string, string, MessageBoxImage> Popup
        {
            get
            {
                return (Action<string, string, MessageBoxImage>)((message, caption, messageBoxImage) => MessageBox.Show(Application.Current.MainWindow, message, caption, MessageBoxButton.OK, messageBoxImage));
            }
        }

        /// <summary>
        /// Gets message box to request confirmation from users.
        /// </summary>
        public Func<string, string, bool> Confirm
        {
            get
            {
                return (Func<string, string, bool>)((message, caption) => MessageBox.Show(Application.Current.MainWindow, message, caption, MessageBoxButton.YesNo) == MessageBoxResult.Yes);
            }
        }

        /// <summary>
        /// Gets or sets number of items to be displayed per page.
        /// </summary>
        public int ItemsPerPage
        {
            get 
            {
                if (m_itemsPerPage != null && m_itemsPerPage > 0)
                    return m_itemsPerPage;
                else
                    return 20;
            }
            set 
            {
                m_itemsPerPage = value;
            }
        }

        /// <summary>
        /// Gets or sets the entire collection retrieved from the database.
        /// </summary>
        public ObservableCollection<object> ItemsSource
        {
            get { return m_itemsSource; }
            set
            {
                m_itemsSource = value;
                GeneratePages();
            }
        }

        /// <summary>
        /// Gets or sets the current item displayed in a form for edit.
        /// </summary>
        public object CurrentItem
        {
            get { return m_currentItem; }
            set
            {
                m_currentItem = value;
                NotifyPropertyChanged("CurrentItem");
            }
        }

        /// <summary>
        /// Gets or sets collection displayed on the current page.
        /// </summary>
        public ObservableCollection<Object> CurrentPage
        {
            get { return m_currentPage; }
            set
            {
                m_currentPage = value;
                NotifyPropertyChanged("CurrentPage");
                if (m_currentPage.Count > 0)
                    CurrentItem = m_currentPage[0];
            }
        }

        /// <summary>
        /// Gets or sets an index of <see cref="CurrentPage"/> to be displayed.
        /// </summary>
        public int CurrentPageNumber
        {
            get { return m_currentPageNumber; }
            set
            {
                m_currentPageNumber = value;
                NotifyPropertyChanged("CurrentPageNumber");
            }
        }

        /// <summary>
        /// Gets or sets total number of pages.
        /// </summary>
        public int PageCount
        {
            get { return m_pageCount; }
            set
            {
                m_pageCount = value;
                NotifyPropertyChanged("PageCount");
            }
        }

        /// <summary>
        /// Gets the command for moving to the first page.
        /// </summary>
        public ICommand FirstCommand
        {
            get
            {
                if (m_firstCommand == null)
                {
                    m_firstCommand = new RelayCommand
                    (
                        param =>
                        {
                            if (m_pages != null)
                            {
                                CurrentPage = m_pages[0];
                                CurrentPageNumber = 1;
                            }

                        },
                        param =>
                        {
                            return (CurrentPageNumber - 1) < 1 ? false : true;
                        }
                    );
                }
                return m_firstCommand;
            }
        }

        /// <summary>
        /// Gets the command for moving to the previous page.
        /// </summary>
        public ICommand PreviousCommand
        {
            get
            {
                if (m_previousCommand == null)
                {
                    m_previousCommand = new RelayCommand
                    (
                        param =>
                        {
                            if (m_pages != null)
                            {
                                CurrentPageNumber = (CurrentPageNumber - 1) < 1 ? 1 : CurrentPageNumber - 1;
                                CurrentPage = m_pages[CurrentPageNumber - 1];
                            }
                        },
                        param =>
                        {
                            return (CurrentPageNumber - 1) < 1 ? false : true;
                        }
                    );
                }
                return m_previousCommand;
            }
        }

        /// <summary>
        /// Gets the command for moving to the next page.
        /// </summary>
        public ICommand NextCommand
        {
            get
            {
                if (m_nextCommand == null)
                {
                    m_nextCommand = new RelayCommand
                    (
                        param =>
                        {
                            if (m_pages != null)
                            {
                                CurrentPageNumber = (CurrentPageNumber + 1) > m_pageCount ? m_pageCount : CurrentPageNumber + 1;
                                CurrentPage = m_pages[CurrentPageNumber - 1];
                            }
                        },
                        param =>
                        {
                            return (CurrentPageNumber + 1) > m_pageCount ? false : true;
                        }
                    );
                }
                return m_nextCommand;
            }
        }

        /// <summary>
        /// Gets the command for moving to the last page.
        /// </summary>
        public ICommand LastCommand
        {
            get
            {
                if (m_lastCommand == null)
                {
                    m_lastCommand = new RelayCommand
                    (
                        param =>
                        {
                            if (m_pages != null)
                            {
                                CurrentPage = m_pages[m_pageCount - 1];
                                CurrentPageNumber = m_pageCount;
                            }
                        },
                        param =>
                        {
                            return (CurrentPageNumber + 1) > m_pageCount ? false : true;
                        }
                    );
                }
                return m_lastCommand;
            }
        }

        #endregion

        #region [ Methods ]

        #region [ INotifyPropertyChanged Implementation ]

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion

        /// <summary>
        /// Method to create a list of pages based on the <see cref="ItemsSource"/>.
        /// </summary>
        void GeneratePages()
        {
            if (ItemsSource != null)
            {
                PageCount = (int)Math.Ceiling(ItemsSource.Count / (double)ItemsPerPage);
                m_pages = new ObservableCollection<ObservableCollection<object>>();
                for (int i = 0; i < m_pageCount; i++)
                {
                    ObservableCollection<object> page = new ObservableCollection<object>();
                    for (int j = 0; j < ItemsPerPage; j++)
                    {
                        if (i * ItemsPerPage + j > ItemsSource.Count - 1) break;
                        page.Add(ItemsSource[i * ItemsPerPage + j]);
                    }
                    m_pages.Add(page);
                }

                if (CurrentPage == null || CurrentPageNumber == 0)
                {
                    CurrentPage = m_pages[0];
                    CurrentPageNumber = 1;
                }
                else
                {
                    //This will help retain current page when user deletes any record from the collection.
                    CurrentPageNumber = (CurrentPageNumber + 1) > m_pageCount ? m_pageCount : CurrentPageNumber;
                    CurrentPage = m_pages[CurrentPageNumber - 1];
                }
            }
            else
            {
                PageCount = 0;
                CurrentPage = new ObservableCollection<object>();
                CurrentPageNumber = 0;
            }
        }

        #endregion

    }
}
