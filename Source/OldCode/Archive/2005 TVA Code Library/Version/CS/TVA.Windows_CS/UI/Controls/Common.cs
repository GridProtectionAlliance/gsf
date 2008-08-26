using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Windows.Forms;
using System.Reflection;

//*******************************************************************************************************
//  TVA.Windows.UI.Controls.Commmon.vb - Common Functions for Windows UI Controls
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  03/16/2007 - J. Ritchie Carroll
//       Original version of source code generated
//
//*******************************************************************************************************


namespace TVA.Windows
{
	namespace UI
	{
		namespace Controls
		{
			
			
			public sealed class Common
			{
				
				
				private Common()
				{
					
					// This class contains only global functions and is not meant to be instantiated
					
				}
				
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
				public static void AdjustPropertyGridLabelRatio(PropertyGrid grid, double ratio)
				{
					
					typeof(PropertyGrid).GetField("gridView", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(grid).labelRatio = ratio;
					
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
				public static void AdjustPropertyGridCommentAreaHeight(PropertyGrid grid, int lines)
				{
					
					Type itemType;
					
					foreach (Control item in ((Control.ControlCollection) (typeof(PropertyGrid).GetProperty("Controls").GetValue(grid, null))))
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
	}
	
}
