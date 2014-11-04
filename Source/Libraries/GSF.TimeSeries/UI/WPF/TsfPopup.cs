//******************************************************************************************************
//  TsfPopup.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

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
            public readonly int Left;
            public readonly int Top;
            public readonly int Right;
            public readonly int Bottom;
        }

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="TsfPopup"/> class.
        /// </summary>
        public TsfPopup()
        {
            PreviewMouseDown += TsfPopup_PreviewMouseDown;
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
            TsfPopup tsfPopup = obj as TsfPopup;

            if ((object)tsfPopup != null && tsfPopup.IsOpen)
                tsfPopup.UpdateWindow();
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
            HwndSource hwndSource;
            IntPtr hwnd;
            RECT rect;

            // Get the source of the window handle
            hwndSource = PresentationSource.FromVisual(Child) as HwndSource;

            if ((object)hwndSource != null)
            {
                // Get the window handle
                hwnd = hwndSource.Handle;

                // Set the position of the window
                if (GetWindowRect(hwnd, out rect))
                    SetWindowPos(hwnd, Topmost ? -1 : -2, rect.Left, rect.Top, (int)Width, (int)Height, 0);
            }
        }

        private void TsfPopup_PreviewMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(this);
            Window window = parent as Window;

            // Find the parent window of this popup
            while ((object)parent != null && (object)window == null)
            {
                parent = VisualTreeHelper.GetParent(parent);
                window = parent as Window;
            }

            // Make sure the window is active
            if ((object)window != null)
                window.Activate();
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
