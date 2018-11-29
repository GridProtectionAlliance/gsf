//******************************************************************************************************
//  FilterExpressionParser.h - Gbtc
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

#ifndef __FILTER_EXPRESSION_PARSER_H
#define __FILTER_EXPRESSION_PARSER_H

#include "FilterExpressionSyntaxBaseListener.h"

#ifndef EOF
#define EOF (-1)
#endif

#include "../../Common/CommonTypes.h"
#include "../../DataSet/DataSet.h"

namespace GSF {
namespace TimeSeries {
namespace Transport
{

enum class ExpressionType
{
    Boolean,
    Numeric,
    String,
    DateTime,
    Null
};

struct Expression
{
    ExpressionType Type = ExpressionType::Null;
    antlr4::ParserRuleContext* Context = nullptr;
    antlrcpp::Any Value = nullptr;
};

typedef SharedPtr<Expression> ExpressionPtr;

class FilterExpressionParser : public FilterExpressionSyntaxBaseListener
{
private:
    GSF::DataSet::DataSetPtr m_dataset;
    std::map<antlr4::ParserRuleContext*, ExpressionPtr> m_expressions;

public:
    FilterExpressionParser();
    ~FilterExpressionParser();

    void exitParse(FilterExpressionSyntaxParser::ParseContext*) override;
    void exitExpression(FilterExpressionSyntaxParser::ExpressionContext*) override;
    void exitLiteralValue(FilterExpressionSyntaxParser::LiteralValueContext*) override;
    void exitColumnName(FilterExpressionSyntaxParser::ColumnNameContext*) override;
    void exitUnaryOperator(FilterExpressionSyntaxParser::UnaryOperatorContext*) override;
    void exitFunctionName(FilterExpressionSyntaxParser::FunctionNameContext*) override;
};

}}}

#endif