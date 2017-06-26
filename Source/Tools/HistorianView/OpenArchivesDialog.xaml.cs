//******************************************************************************************************
//  OpenArchivesDialog.xaml.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  03/14/2014 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Forms;
using GSF.IO;
using HistorianView.Annotations;
using Button = System.Windows.Controls.Button;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace HistorianView
{
    internal class Archive : INotifyPropertyChanged
    {
        #region [ Members ]

        // Events
        public event PropertyChangedEventHandler PropertyChanged;

        // Fields
        private int m_id;
        private string m_name;
        private string m_location;
        private string m_offloadLocation;

        #endregion

        #region [ Constructors ]

        public Archive()
        {
            m_id = s_id;
            s_id++;
        }

        #endregion

        #region [ Properties ]

        public int ID
        {
            get
            {
                return m_id;
            }
        }

        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                if (value == m_name)
                    return;

                m_name = value;
                OnPropertyChanged();
            }
        }

        public string Location
        {
            get
            {
                return m_location;
            }
            set
            {
                if (value == m_location)
                    return;

                m_location = value;
                OnPropertyChanged();
            }
        }

        public string OffloadLocation
        {
            get
            {
                return m_offloadLocation;
            }
            set
            {
                if (value == m_offloadLocation)
                    return;

                m_offloadLocation = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region [ Methods ]

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if ((object)handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static int s_id;

        #endregion
    }

    internal class ArchiveCollection : ObservableCollection<Archive> { }

    /// <summary>
    /// Interaction logic for OpenArchivesDialog.xaml
    /// </summary>
    public partial class OpenArchivesDialog : Window
    {
        #region [ Constructors ]

        public OpenArchivesDialog()
        {
            InitializeComponent();
        }

        #endregion

        #region [ Properties ]

        public List<string> ArchiveLocations
        {
            get
            {
                return Archives
                    .Select(archive => string.Format("{0}|{1}", Path.Combine(archive.Location, archive.Name), archive.OffloadLocation))
                    .ToList();
            }
            set
            {
                string[] paths;

                Archives.Clear();

                foreach (string archiveLocation in value)
                {
                    paths = archiveLocation.Split('|');

                    if (paths.Length > 1)
                        AddLocation(paths[0], paths[1]);
                    else
                        AddLocation(paths[0]);
                }
            }
        }

        #endregion

        #region [ Methods ]

        private void AddLocation(string archiveLocation)
        {
            AddLocation(archiveLocation, string.Empty);
        }

        private void AddLocation(string archiveLocation, string offloadLocation)
        {
            if (Archives.All(archive => Path.Combine(archive.Location, archive.Name) != archiveLocation))
            {
                Archives.Add(new Archive()
                {
                    Name = FilePath.GetFileName(archiveLocation),
                    Location = FilePath.GetDirectoryName(archiveLocation),
                    OffloadLocation = offloadLocation
                });
            }
        }

        private void OpenArchiveButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog archiveDialog = new OpenFileDialog();

            archiveDialog.Filter = "Archive files|*_archive.d|Metadata files|*_dbase.dat;*_dbase.dat2";
            archiveDialog.CheckFileExists = true;

            if (archiveDialog.ShowDialog() == true)
                AddLocation(archiveDialog.FileName);
        }

        private void CloseArchiveButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = e.OriginalSource as Button;

            if ((object)button != null)
            {
                foreach (Archive archive in Archives.Where(archive => (int)button.DataContext == archive.ID).ToList())
                    Archives.Remove(archive);
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = e.OriginalSource as Button;

            FolderBrowserDialog dialog;

            if ((object)button != null)
            {
                dialog = new FolderBrowserDialog();

                foreach (Archive archive in Archives.Where(archive => (int)button.DataContext == archive.ID))
                {
                    dialog.SelectedPath = archive.OffloadLocation;

                    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        archive.OffloadLocation = dialog.SelectedPath;
                }
            }
        }

        #endregion
    }
}
