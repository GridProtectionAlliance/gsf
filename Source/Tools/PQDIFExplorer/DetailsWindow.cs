//******************************************************************************************************
//  DetailsWindow.cs - Gbtc
//
//  Copyright © 2015, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  12/14/2015 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Drawing;
using System.Windows.Forms;

namespace PQDIFExplorer
{
    /// <summary>
    /// A simple window used to display the details about a node in the main window's tree view.
    /// </summary>
    public partial class DetailsWindow : Form
    {
        /// <summary>
        /// Creates a new instance of the <see cref="DetailsWindow"/> class.
        /// </summary>
        public DetailsWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Sets the text in the text box of the details window.
        /// </summary>
        /// <param name="text"></param>
        public void SetText(string text)
        {
            DetailsTextBox.Clear();
            DetailsTextBox.AppendText(text);
        }

        // Fixes the size of the contents of the window when the window is resized.
        private void FixSize()
        {
            const int pad = 15;
            int x = ClientRectangle.X + pad;
            int y = ClientRectangle.Y + pad;
            int width = ClientRectangle.Width - (2 * pad);
            int height = ClientRectangle.Height - (2 * pad);
            DetailsTextBox.SetBounds(x, y, width, height);
        }

        private void FixScrollBars()
        {
            Size textSize = TextRenderer.MeasureText(DetailsTextBox.Text, DetailsTextBox.Font);
            bool showVertical = DetailsTextBox.ClientSize.Height < textSize.Height + Convert.ToInt32(DetailsTextBox.Font.Size);
            bool showHorizontal = DetailsTextBox.ClientSize.Width < textSize.Width;

            if (showVertical && showHorizontal)
                DetailsTextBox.ScrollBars = ScrollBars.Both;
            else if (showVertical)
                DetailsTextBox.ScrollBars = ScrollBars.Vertical;
            else if (showHorizontal)
                DetailsTextBox.ScrollBars = ScrollBars.Horizontal;
            else
                DetailsTextBox.ScrollBars = ScrollBars.None;
        }

        // Handler called when the details window is initially loaded.
        private void DetailsWindow_Load(object sender, EventArgs e)
        {
            Icon = new Icon(typeof(MainWindow), "Icons.explorer.ico");
            FixSize();
        }

        // Handler called when the details window is resized.
        private void DetailsWindow_Resize(object sender, EventArgs e)
        {
            FixSize();
        }

        // Handler called when the text changes in the details window's text box.
        private void DetailsTextBox_TextChanged(object sender, EventArgs e)
        {
            FixScrollBars();
        }

        // Handler called when the details text box is resized.
        private void DetailsTextBox_Resize(object sender, EventArgs e)
        {
            if (IsHandleCreated)
                BeginInvoke(new Action(FixScrollBars));
        }

        // Closes the window when the escape key is pressed.
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
