//******************************************************************************************************
//  FilterExpressions.h - Gbtc
//
//  Copyright © 2019, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  01/22/2019 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

#ifndef __FILTER_EXPRESSIONS_H
#define __FILTER_EXPRESSIONS_H

// When using filter expressions this header file must be included first, before all others, otherwise
// "antlr4-runtime/ATNSimulator.h" causes an "E0040 expected an identifier" error in Visual Studio here:

/*
  class ANTLR4CPP_PUBLIC ATNSimulator {
  public:
    /// Must distinguish between missing edge and edge we know leads nowhere.
    static const Ref<dfa::DFAState> ERROR;
 */

#include "FilterExpressionSyntaxBaseListener.h"

// For some reason this symbol gets undefined? So, we re-define it...
#ifndef EOF
#define EOF (-1)
#endif

#endif