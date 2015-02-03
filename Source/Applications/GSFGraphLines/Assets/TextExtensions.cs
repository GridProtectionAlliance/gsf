//******************************************************************************************************
//  TextExtensions.cs - Gbtc
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
//  01/14/2012 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using UnityEngine;

// Defines extension functions for text objects
// ReSharper disable once CheckNamespace
public static class TextExtensions
{
    /// <summary>
    /// Updates the text of a 3D Text object, even from a non-UI thread.
    /// </summary>
    /// <param name='mesh'>The text mesh to update.</param>
    /// <param name='text'>The new text to apply to the mesh.</param>
    public static void UpdateText(this TextMesh mesh, string text)
    {
        UIThread.Invoke(UpdateText, mesh, text);
    }

    // Text updates must occur on main UI thread
    private static void UpdateText(object[] args)
    {
        if ((object)args == null || args.Length < 2)
            return;

        TextMesh mesh = args[0] as TextMesh;
        string text = args[1] as string;

        if ((object)mesh != null && (object)text != null)
            mesh.text = text;
    }
}
