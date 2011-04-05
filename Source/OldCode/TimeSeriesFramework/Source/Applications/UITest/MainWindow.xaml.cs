//******************************************************************************************************
//  MainWindow.xaml.cs - Gbtc
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
//  03/29/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Serialization;
using TimeSeriesFramework.UI.Utilities;

namespace UITest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<MenuDataItem> m_menuDataItems; 

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);            
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            XmlRootAttribute xmlRootAttribute = new XmlRootAttribute("MenuDataItems");
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<MenuDataItem>), xmlRootAttribute);
            using (XmlReader reader = XmlReader.Create("Menu.xml"))
                m_menuDataItems = (ObservableCollection<MenuDataItem>)serializer.Deserialize(reader);

            MenuMain.DataContext = m_menuDataItems;
        }

        //void SerializeToXML(ObservableCollection<MenuDataItem> menus)
        //{            
        //    try
        //    {                
        //        XmlRootAttribute xmlRootAttribute = new XmlRootAttribute("MenuDataItems");                
        //        XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<MenuDataItem>), xmlRootAttribute);
        //        using(TextWriter textWriter = new StreamWriter("Menu.xml"))
        //            serializer.Serialize(textWriter, menus);
                
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine(ex.ToString());
        //    }
        //}

        private void MenuMain_Click(object sender, RoutedEventArgs e)
        {
            MenuDataItem menuDataItem;
            try
            {
                menuDataItem = (MenuDataItem)((MenuItem)e.OriginalSource).Header;
                var assembly = Assembly.LoadFrom(menuDataItem.UserControlAssembly);
                foreach (var type in assembly.GetTypes())
                {
                    if (type.Name == menuDataItem.UserControlPath)
                    {
                        var userControl = Activator.CreateInstance(type) as UserControl;                        
                        FrameContent.Navigate(userControl);
                        TextBlockTitle.Text = menuDataItem.Description;
                    }
                }
            }
            catch
            {

            }
        }
    }
}
