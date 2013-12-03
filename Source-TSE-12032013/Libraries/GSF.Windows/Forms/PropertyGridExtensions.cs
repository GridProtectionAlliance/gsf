//******************************************************************************************************
//  PropertyGridExtensions.cs - Gbtc
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
//  03/16/2007 - J. Ritchie Carroll
//       Generated original version of source code.
//  10/01/2008 - J. Ritchie Carroll
//       Converted to C# extensions.
//  03/20/2009 - Pinal C. Patel
//       Edited code comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Reflection;
using System.Windows.Forms;

namespace GSF.Windows.Forms
{
    /// <summary>
    /// Defines extension functions for the <see cref="PropertyGrid"/> control.
    /// </summary>
    public static class PropertyGridExtensions
    {
        /// <summary>
        /// Adjusts a property grid's label ratio
        /// </summary>
        /// <param name="grid">Property grid to adjust</param>
        /// <param name="ratio">Ratio to use use for label column</param>
        /// <remarks>
        /// <para>Smaller ratios (e.g., 1.75) produce a wider label column.</para>
        /// <para>
        /// This function only has an effect on property grids when their Visible property is set to True.  To use
        /// this on an initially hidden property grid - set the property grid's Visible property to True at design
        /// time, call this function during form load, then set the Visible property to False.
        /// </para>
        /// <para>
        /// This function was written to work with the .NET 2.0 PropertyGrid control.  Note that reflection is used
        /// to set private properties of the property grid and as a result this function may not work with future
        /// versions of the .NET property grid.
        /// </para>
        /// </remarks>
        public static void AdjustLabelRatio(this PropertyGrid grid, double ratio)
        {
            object gridView = typeof(PropertyGrid).GetField("gridView", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(grid);
            gridView.GetType().GetField("labelRatio").SetValue(gridView, ratio);
        }

        /// <summary>
        /// Adjusts a property grid's comment area height
        /// </summary>
        /// <param name="grid">Property grid to adjust</param>
        /// <param name="lines">Number of lines to display in comment area</param>
        /// <remarks>
        /// <para>
        /// This function only has an effect on property grids when their Visible property is set to True.  To use
        /// this on an initially hidden property grid - set the property grid's Visible property to True at design
        /// time, call this function during form load, then set the Visible property to False.
        /// </para>
        /// <para>
        /// This function was written to work with the .NET 2.0 PropertyGrid control.  Note that reflection is used
        /// to set private properties of the property grid and as a result this function may not work with future
        /// versions of the .NET property grid.
        /// </para>
        /// </remarks>
        public static void AdjustCommentAreaHeight(this PropertyGrid grid, int lines)
        {
            Control.ControlCollection propertyGridControls = (Control.ControlCollection)typeof(PropertyGrid).GetProperty("Controls").GetValue(grid, null);
            Type itemType;

            foreach (Control item in propertyGridControls)
            {
                itemType = item.GetType();

                if (string.Compare(itemType.Name, "DocComment", true) == 0)
                {
                    itemType.GetField("userSized", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(item, true);
                    itemType.GetProperty("Lines").SetValue(item, lines, null);
                    break;
                }
            }
        }
    }
}