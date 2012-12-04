//******************************************************************************************************
//  TsfPopup.cs - Gbtc
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
//  10/26/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//       This code was taken from Chris Cavanagh's Blog at 
//       http://chriscavanagh.wordpress.com/2008/08/13/non-topmost-wpf-popup/
//
//******************************************************************************************************

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;

namespace GSF.TimeSeries.UI
{
    /// <summary>
    /// Represents a non-topmost <see cref="Popup"/> window.
    /// </summary>
    public class TsfPopup : Popup
    {
        #region [ Members ]

        // Nested Types

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Dependency property to define zorder of the <see cref="TsfPopup"/>.
        /// </summary>
        public static DependencyProperty TopmostProperty = Window.TopmostProperty.AddOwner(typeof(TsfPopup), new FrameworkPropertyMetadata(false, OnTopmostChanged));

        /// <summary>
        /// Gets or sets Topmost flag for <see cref="TsfPopup"/>.
        /// </summary>
        public bool Topmost
        {
            get
            {
                return (bool)GetValue(TopmostProperty);
            }
            set
            {
                SetValue(TopmostProperty, value);
            }
        }

        #endregion

        #region [ Methods ]

        private static void OnTopmostChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            (obj as TsfPopup).UpdateWindow();
        }

        /// <summary>
        /// Responds to the condition in which the value of the <see cref="System.Windows.Controls.Primitives.Popup.IsOpen"/> property changes from <c>false</c> to <c>true</c>.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnOpened(EventArgs e)
        {
            UpdateWindow();
        }

        private void UpdateWindow()
        {
            IntPtr hwnd = ((HwndSource)PresentationSource.FromVisual(this.Child)).Handle;
            RECT rect;

            if (GetWindowRect(hwnd, out rect))
                SetWindowPos(hwnd, Topmost ? -1 : -2, rect.Left, rect.Top, (int)this.Width, (int)this.Height, 0);
        }

        #endregion

        #region [ Static ]

        // Static Methods

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32", EntryPoint = "SetWindowPos")]
        private static extern int SetWindowPos(IntPtr hWnd, int hwndInsertAfter, int x, int y, int cx, int cy, int wFlags);

        #endregion
    }
}
