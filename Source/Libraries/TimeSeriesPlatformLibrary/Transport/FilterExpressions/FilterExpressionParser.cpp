//******************************************************************************************************
//  FilterExpressionParser.cpp - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
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
//  11/01/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

#include "FilterExpressionParser.h"

using namespace GSF::TimeSeries;
using namespace GSF::TimeSeries::Transport;

FilterExpressionParser::FilterExpressionParser()
{
}


FilterExpressionParser::~FilterExpressionParser()
{
}

void FilterExpressionParser::exitParse(FilterExpressionSyntaxParser::ParseContext* context)
{
}

void FilterExpressionParser::exitError(FilterExpressionSyntaxParser::ErrorContext* context)
{
}

void FilterExpressionParser::exitExpression(FilterExpressionSyntaxParser::ExpressionContext* context)
{
}

void FilterExpressionParser::exitLiteralValue(FilterExpressionSyntaxParser::LiteralValueContext* context)
{
}

void FilterExpressionParser::exitColumnName(FilterExpressionSyntaxParser::ColumnNameContext* context)
{
}

void FilterExpressionParser::exitUnaryOperator(FilterExpressionSyntaxParser::UnaryOperatorContext* context)
{
}

void FilterExpressionParser::exitFunctionName(FilterExpressionSyntaxParser::FunctionNameContext* context)
{
}
