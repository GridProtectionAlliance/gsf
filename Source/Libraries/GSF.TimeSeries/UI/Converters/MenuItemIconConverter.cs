//******************************************************************************************************
//  MenuItemIconConverter.cs - Gbtc
//
//  Copyright © 2026, Grid Protection Alliance.  All Rights Reserved.
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
//  06/19/2026 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GSF.IO;
using DrawingIcon = System.Drawing.Icon;

namespace GSF.TimeSeries.UI.Converters
{
    /// <summary>
    /// Represents an <see cref="IMultiValueConverter"/> that produces the image to display in the icon
    /// region of a menu item from an icon reference and/or an external process path.
    /// </summary>
    /// <remarks>
    /// The converter expects two bound values: the first is the explicit icon reference (i.e., the menu item's
    /// <c>Icon</c> value) and the second is the external process path (i.e., the menu item's
    /// <c>ExternalProcessPath</c> value). When the icon reference is defined, the referenced image is loaded and
    /// displayed. When no icon is defined but an external process path is set, the embedded application icon for
    /// the external process is extracted and used instead. The converter returns a new <see cref="Image"/> instance
    /// on each evaluation so that the result can be safely assigned to the <see cref="MenuItem.Icon"/> property from
    /// within a shared <see cref="Style"/> setter (a single <see cref="FrameworkElement"/> instance cannot be the
    /// logical child of multiple menu items).
    /// </remarks>
    public class MenuItemIconConverter : IMultiValueConverter
    {
        /// <summary>
        /// Default width and height, in device-independent pixels, of a generated menu item icon.
        /// </summary>
        public const double DefaultIconSize = 16.0D;

        /// <summary>
        /// Returns an <see cref="Image"/> element representing the icon for a menu item.
        /// </summary>
        /// <param name="values">
        /// The bound values: <c>values[0]</c> is the explicit icon reference and <c>values[1]</c> is the external
        /// process path.
        /// </param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">Optional icon size override; when parsable as a <see cref="double"/>, overrides <see cref="DefaultIconSize"/>.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>An <see cref="Image"/> when an icon could be resolved; otherwise, <c>null</c>.</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if ((object)values == null)
                return null;

            string icon = values.Length > 0 ? values[0] as string : null;
            string externalProcessPath = values.Length > 1 ? values[1] as string : null;

            ImageSource source = null;

            // (1) Use the explicitly defined icon when one is specified.
            if (!string.IsNullOrWhiteSpace(icon))
                source = LoadImageSource(icon);

            // (2) Otherwise, fall back to the embedded icon of the external process, when defined.
            if ((object)source == null && !string.IsNullOrWhiteSpace(externalProcessPath))
                source = ExtractAssociatedIcon(externalProcessPath);

            if ((object)source == null)
                return null;

            double size = DefaultIconSize;

            if (parameter != null && double.TryParse(parameter.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedSize) && parsedSize > 0.0D)
                size = parsedSize;

            return new Image
            {
                Source = source,
                Width = size,
                Height = size,
                Stretch = Stretch.Uniform,
                SnapsToDevicePixels = true
            };
        }

        /// <summary>
        /// Not supported; this converter only supports one-way conversion.
        /// </summary>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Loads an <see cref="ImageSource"/> from an icon reference that may be a WPF pack/resource URI or a file path.
        /// </summary>
        /// <remarks>
        /// Example to load an embedded image from the GSF.TimeSeries.UI assembly:
        /// <code>
        /// &lt;MenuDataItem Icon="/GSF.TimeSeries.UI;component/images/Configure.png" MenuText="Measurements" ... /&gt;
        /// </code>
        /// </remarks>
        private static ImageSource LoadImageSource(string icon)
        {
            try
            {
                icon = Environment.ExpandEnvironmentVariables(icon.Trim());

                Uri uri;

                if (icon.StartsWith("pack://", StringComparison.OrdinalIgnoreCase))
                {
                    // Absolute pack URI, e.g.: pack://application:,,,/GSF.TimeSeries.UI;component/images/Icon.png
                    uri = new Uri(icon, UriKind.Absolute);
                }
                else if (icon.StartsWith("/", StringComparison.Ordinal))
                {
                    // Relative pack URI, e.g.: /GSF.TimeSeries.UI;component/images/Icon.png - resolved against the application.
                    uri = new Uri(icon, UriKind.Relative);
                }
                else if (Uri.TryCreate(icon, UriKind.Absolute, out Uri absoluteUri) && !absoluteUri.IsFile)
                {
                    // Any other absolute URI (e.g. http://) is used as-is.
                    uri = absoluteUri;
                }
                else
                {
                    // Treat as a file path, resolving relative paths against the application directory.
                    string path = FilePath.GetAbsolutePath(icon);

                    if (!File.Exists(path))
                        return null;

                    uri = new Uri(path, UriKind.Absolute);
                }

                BitmapImage bitmap = new BitmapImage();

                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = uri;
                bitmap.EndInit();
                bitmap.Freeze();

                return bitmap;
            }
            catch
            {
                // Icon is best-effort: a bad reference simply yields no icon rather than failing the menu render.
                return null;
            }
        }

        /// <summary>
        /// Extracts the embedded application icon associated with an external process executable.
        /// </summary>
        private static ImageSource ExtractAssociatedIcon(string processPath)
        {
            try
            {
                processPath = Environment.ExpandEnvironmentVariables(processPath.Trim());

                // System.Drawing.Icon.ExtractAssociatedIcon requires an existing local file path.
                if (!File.Exists(processPath))
                {
                    string resolvedPath = FilePath.GetAbsolutePath(processPath);

                    if (!File.Exists(resolvedPath))
                        return null;

                    processPath = resolvedPath;
                }

                using (DrawingIcon icon = DrawingIcon.ExtractAssociatedIcon(processPath))
                {
                    if ((object)icon == null)
                        return null;

                    // CreateBitmapSourceFromHIcon copies the icon data, so the source GDI icon can be disposed afterward.
                    ImageSource source = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    source.Freeze();

                    return source;
                }
            }
            catch
            {
                // Icon extraction is best-effort: failures simply yield no icon.
                return null;
            }
        }
    }
}
