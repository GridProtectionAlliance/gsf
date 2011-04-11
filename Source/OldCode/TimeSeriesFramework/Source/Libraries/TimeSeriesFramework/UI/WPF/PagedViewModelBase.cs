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
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using TimeSeriesFramework.UI.Commands;
using TVA.Data;

namespace TimeSeriesFramework.UI
{
    /// <summary>
    /// Represents an abstract class with paging support which all ViewModel objects derive from.
    /// </summary>
    public abstract class PagedViewModelBase<TDataModel, TPrimaryKey> : IViewModel where TDataModel : IDataModel, new()
    {
        #region [ Members ]

        //Events

        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        //Fields
        private int m_pageCount, m_currentPageNumber, m_itemsPerPage;
        private ObservableCollection<TDataModel> m_currentPage, m_itemsSource;
        private ObservableCollection<ObservableCollection<TDataModel>> m_pages;
        private ICommand m_firstCommand, m_previousCommand, m_nextCommand, m_lastCommand;
        private RelayCommand m_saveCommand, m_deleteCommand, m_clearCommand;
        private TDataModel m_currentItem;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="PagedViewModelBase{T1,T2}"/> class.
        /// </summary>
        protected PagedViewModelBase()
        {
            Load();
        }

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
                if (m_itemsPerPage > 0)
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
        public ObservableCollection<TDataModel> ItemsSource
        {
            get
            {
                return m_itemsSource;
            }
            set
            {
                m_itemsSource = value;
                GeneratePages();
            }
        }

        /// <summary>
        /// Gets or sets the current item displayed in a form for edit.
        /// </summary>
        public TDataModel CurrentItem
        {
            get
            {
                return m_currentItem;
            }
            set
            {
                m_currentItem = value;
                OnPropertyChanged("CurrentItem");
            }
        }

        /// <summary>
        /// Gets or sets collection displayed on the current page.
        /// </summary>
        public ObservableCollection<TDataModel> CurrentPage
        {
            get
            {
                return m_currentPage;
            }
            set
            {
                m_currentPage = value;
                OnPropertyChanged("CurrentPage");
                if (m_currentPage.Count > 0)
                    CurrentItem = m_currentPage[0];
            }
        }

        /// <summary>
        /// Gets or sets an index of <see cref="CurrentPage"/> to be displayed.
        /// </summary>
        public int CurrentPageNumber
        {
            get
            {
                return m_currentPageNumber;
            }
            set
            {
                m_currentPageNumber = value;
                OnPropertyChanged("CurrentPageNumber");
            }
        }

        /// <summary>
        /// Gets or sets total number of pages.
        /// </summary>
        public int PageCount
        {
            get
            {
                return m_pageCount;
            }
            set
            {
                m_pageCount = value;
                OnPropertyChanged("PageCount");
            }
        }

        /// <summary>
        /// Gets <see cref="ICommand"/> for moving to the first page.
        /// </summary>
        public ICommand FirstCommand
        {
            get
            {
                if (m_firstCommand == null)
                {
                    m_firstCommand = new RelayCommand
                    (
                        () =>
                        {
                            if (m_pages != null)
                            {
                                CurrentPage = m_pages[0];
                                CurrentPageNumber = 1;
                            }

                        },
                        () =>
                        {
                            return (CurrentPageNumber - 1) < 1 ? false : true;
                        }
                    );
                }
                return m_firstCommand;
            }
        }

        /// <summary>
        /// Gets <see cref="ICommand"/> for moving to the previous page.
        /// </summary>
        public ICommand PreviousCommand
        {
            get
            {
                if (m_previousCommand == null)
                {
                    m_previousCommand = new RelayCommand
                    (
                        () =>
                        {
                            if (m_pages != null)
                            {
                                CurrentPageNumber = (CurrentPageNumber - 1) < 1 ? 1 : CurrentPageNumber - 1;
                                CurrentPage = m_pages[CurrentPageNumber - 1];
                            }
                        },
                        () =>
                        {
                            return (CurrentPageNumber - 1) < 1 ? false : true;
                        }
                    );
                }
                return m_previousCommand;
            }
        }

        /// <summary>
        /// Gets <see cref="ICommand"/> for moving to the next page.
        /// </summary>
        public ICommand NextCommand
        {
            get
            {
                if (m_nextCommand == null)
                {
                    m_nextCommand = new RelayCommand
                    (
                        () =>
                        {
                            if (m_pages != null)
                            {
                                CurrentPageNumber = (CurrentPageNumber + 1) > m_pageCount ? m_pageCount : CurrentPageNumber + 1;
                                CurrentPage = m_pages[CurrentPageNumber - 1];
                            }
                        },
                        () =>
                        {
                            return (CurrentPageNumber + 1) > m_pageCount ? false : true;
                        }
                    );
                }
                return m_nextCommand;
            }
        }

        /// <summary>
        /// Gets <see cref="ICommand"/> for moving to the last page.
        /// </summary>
        public ICommand LastCommand
        {
            get
            {
                if (m_lastCommand == null)
                {
                    m_lastCommand = new RelayCommand
                    (
                        () =>
                        {
                            if (m_pages != null)
                            {
                                CurrentPage = m_pages[m_pageCount - 1];
                                CurrentPageNumber = m_pageCount;
                            }
                        },
                        () =>
                        {
                            return (CurrentPageNumber + 1) > m_pageCount ? false : true;
                        }
                    );
                }
                return m_lastCommand;
            }
        }

        /// <summary>
        /// Gets save <see cref="ICommand"/>.
        /// </summary>
        public ICommand SaveCommand
        {
            get
            {
                if (m_saveCommand == null)
                    m_saveCommand = new RelayCommand(Save, () => CanSave);

                return m_saveCommand;
            }
        }

        /// <summary>
        /// Gets delete <see cref="ICommand"/>.
        /// </summary>
        public ICommand DeleteCommand
        {
            get
            {
                if (m_deleteCommand == null)
                    m_deleteCommand = new RelayCommand(Delete, () => CanDelete);

                return m_deleteCommand;
            }
        }

        /// <summary>
        /// Gets clear <see cref="ICommand"/>.
        /// </summary>
        public ICommand ClearCommand
        {
            get
            {
                if (m_clearCommand == null)
                    m_clearCommand = new RelayCommand(Clear, () => CanClear);

                return m_clearCommand;
            }
        }

        /// <summary>
        /// Gets flag that indicates if <see cref="CurrentItem"/> state is valid and can be saved.
        /// </summary>
        /// <returns><c>true</c> if <see cref="CurrentItem"/> can be saved; otherwise <c>false</c>.</returns>
        public virtual bool CanSave
        {
            get
            {
                return CurrentItem.IsValid;
            }
        }

        /// <summary>
        /// Gets flag that indicates if <see cref="CurrentItem"/> can be deleted.
        /// </summary>
        /// <returns><c>true</c> if <see cref="CurrentItem"/> can be deleted; otherwise <c>false</c>.</returns>
        public virtual bool CanDelete
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets flag that indicates if <see cref="CurrentItem"/> can be cleared.
        /// </summary>
        /// <returns><c>true</c> if <see cref="CurrentItem"/> can be cleared; otherwise <c>false</c>.</returns>
        public virtual bool CanClear
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets name of associated <see cref="IDataModel"/>.
        /// </summary>
        public virtual string DataModelName
        {
            get
            {
                return s_dataModelType.Name;
            }
        }

        /// <summary>
        /// Gets flag that determines if <see cref="CurrentItem"/> is a new record.
        /// </summary>
        public abstract bool IsNewRecord
        {
            get;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the primary key value of the <see cref="CurrentItem"/>.
        /// </summary>
        /// <returns>The primary key value of the <see cref="CurrentItem"/>.</returns>
        public abstract TPrimaryKey GetCurrentItemKey();

        /// <summary>
        /// Gets the string based named identifier of the <see cref="CurrentItem"/>.
        /// </summary>
        /// <returns>The string based named identifier of the <see cref="CurrentItem"/>.</returns>
        public abstract string GetCurrentItemName();

        /// <summary>
        /// Loads the records for the associated <see cref="IDataModel"/>.
        /// </summary>
        public virtual void Load()
        {
            ItemsSource = (ObservableCollection<TDataModel>)s_loadRecords.Invoke(this, new object[] { (AdoDataConnection)null });
        }

        /// <summary>
        /// Saves the record for the associated <see cref="IDataModel"/>.
        /// </summary>
        public virtual void Save()
        {
            if (CanSave)
            {
                try
                {
                    string result = (string)s_saveRecord.Invoke(this, new object[] { (AdoDataConnection)null, CurrentItem, IsNewRecord });
                    Popup(result, "Save " + DataModelName, MessageBoxImage.Information);
                    Load();
                }
                catch (Exception ex)
                {
                    Popup(ex.Message, "Save " + DataModelName + " Exception:", MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Deletes the record for the associated <see cref="IDataModel"/>.
        /// </summary>
        public virtual void Delete()
        {
            if (CanDelete && !IsNewRecord && Confirm("Are you sure you want to delete \'" + GetCurrentItemName() + "\'?", "Delete " + DataModelName))
            {
                try
                {
                    string result = (string)s_deleteRecord.Invoke(this, new object[] { (AdoDataConnection)null, GetCurrentItemKey() });
                    Load();
                    Popup(result, "Delete " + DataModelName, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    Popup(ex.Message, "Delete " + DataModelName + " Exception:", MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Clears the record for the associated <see cref="IDataModel"/>.
        /// </summary>
        public virtual void Clear()
        {
            if (CanClear)
                CurrentItem = new TDataModel();
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">Property name that has changed.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Method to create a list of pages based on the <see cref="ItemsSource"/>.
        /// </summary>
        private void GeneratePages()
        {
            if (ItemsSource != null)
            {
                PageCount = (int)Math.Ceiling(ItemsSource.Count / (double)ItemsPerPage);
                m_pages = new ObservableCollection<ObservableCollection<TDataModel>>();

                for (int i = 0; i < m_pageCount; i++)
                {
                    ObservableCollection<TDataModel> page = new ObservableCollection<TDataModel>();
                    for (int j = 0; j < ItemsPerPage; j++)
                    {
                        if (i * ItemsPerPage + j > ItemsSource.Count - 1)
                            break;
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
                    // Retain current page when user deletes any record from the collection
                    CurrentPageNumber = (CurrentPageNumber + 1) > m_pageCount ? m_pageCount : CurrentPageNumber;
                    CurrentPage = m_pages[CurrentPageNumber - 1];
                }
            }
            else
            {
                PageCount = 0;
                CurrentPage = new ObservableCollection<TDataModel>();
                CurrentPageNumber = 0;
            }
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static Type s_dataModelType = typeof(TDataModel);
        private static MethodInfo s_loadRecords = s_dataModelType.GetMethod("Load");
        private static MethodInfo s_saveRecord = s_dataModelType.GetMethod("Save");
        private static MethodInfo s_deleteRecord = s_dataModelType.GetMethod("Delete");

        #endregion


    }
}
