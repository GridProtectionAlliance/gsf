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
#include "tree/ParseTreeWalker.h"
#include "../../Common/Convert.h"

#include <boost/algorithm/string.hpp>

using namespace std;
using namespace GSF::DataSet;
using namespace GSF::TimeSeries;
using namespace GSF::TimeSeries::Transport;
using namespace antlr4;
using namespace antlr4::tree;
using namespace boost;

// Mapped type for boost UUID (ANTLR4 also defines a Guid type)
typedef GSF::TimeSeries::Guid guid;

FilterExpressionParser::FilterExpressionParser(const string& filterExpression) :
    m_inputStream(filterExpression),
    m_lexer(nullptr),
    m_tokens(nullptr),
    m_parser(nullptr),
    m_dataset(nullptr),
    m_primaryMeasurementTableName("ActiveMeasurements"),
    m_signalIDColumnName("SignalID"),
    m_measurementKeyColumnName("ID"),
    m_pointTagColumnName("PointTag")
{
    m_lexer = new FilterExpressionSyntaxLexer(&m_inputStream);
    m_tokens = new CommonTokenStream(m_lexer);
    m_parser = new FilterExpressionSyntaxParser(m_tokens);
}

FilterExpressionParser::~FilterExpressionParser()
{
    delete m_lexer;
    delete m_tokens;
    delete m_parser;
}

bool FilterExpressionParser::TryGetExpr(const ParserRuleContext* context, ExpressionPtr& expression) const
{
    return TryGetValue<const ParserRuleContext*, ExpressionPtr>(m_expressions, context, expression, nullptr);
}

void FilterExpressionParser::AddExpr(const ParserRuleContext* context, const ExpressionPtr& expression)
{
    m_expressions.insert(pair<const ParserRuleContext*, ExpressionPtr>(context, expression));
}

void FilterExpressionParser::MapMeasurement(const DataTablePtr& measurements, const int32_t signalIDColumnIndex, const string& columnName, const string& columnValue)
{
    const auto column = measurements->Column(columnName);

    if (column == nullptr)
        return;

    const int32_t columnIndex = column->Index();

    for (int32_t i = 0; i < measurements->RowCount(); i++)
    {
        const auto row = measurements->Row(i);

        if (row != nullptr)
        {
            if (iequals(columnValue, row->ValueAsString(columnIndex)))
            {
                m_signalIDs.push_back(row->ValueAsGuid(signalIDColumnIndex));
                return;
            }
        }
    }
}

const DataSetPtr& FilterExpressionParser::CurrentDataSet() const
{
    return m_dataset;
}

void FilterExpressionParser::AssignDataSet(const DataSetPtr& dataset)
{
    m_dataset = dataset;
}

const string& FilterExpressionParser::GetPrimaryMeasurementTableName() const
{
    return m_primaryMeasurementTableName;
}

void FilterExpressionParser::SetPrimaryMeasurementTableName(const string& tableName)
{
    m_primaryMeasurementTableName = tableName;
}

const string& FilterExpressionParser::GetSignalIDColumnName() const
{
    return m_signalIDColumnName;
}

void FilterExpressionParser::SetSignalIDColumnName(const string& columnName)
{
    m_signalIDColumnName = columnName;
}

const string& FilterExpressionParser::GetMeasurementKeyColumnName() const
{
    return m_measurementKeyColumnName;
}

void FilterExpressionParser::SetMeasurementKeyColumnName(const string& columnName)
{
    m_measurementKeyColumnName = columnName;
}

const string& FilterExpressionParser::GetPointTagColumnName() const
{
    return m_pointTagColumnName;
}

void FilterExpressionParser::SetPointTagColumnName(const string& columnName)
{
    m_pointTagColumnName = columnName;
}

void FilterExpressionParser::Evaluate()
{
    if (m_dataset == nullptr)
        throw RuntimeException("Dataset is undefined");

    m_signalIDs.clear();
    m_expressions.clear();

    ParseTreeWalker walker;
    walker.walk(this, m_parser->parse());
}

const vector<guid>& FilterExpressionParser::FilteredSignalIDs() const
{
    return m_signalIDs;
}

/*
    parse
     : ( filterExpressionStatementList | error ) EOF
     ;
 */
void FilterExpressionParser::exitParse(FilterExpressionSyntaxParser::ParseContext* context)
{
}

/*
    identifierStatement
     : GUID_LITERAL
     | MEASUREMENT_KEY_LITERAL
     | POINT_TAG_LITERAL
     ;
 */
void FilterExpressionParser::exitIdentifierStatement(FilterExpressionSyntaxParser::IdentifierStatementContext* context)
{
    const auto guidLiteral = context->GUID_LITERAL();

    if (guidLiteral)
    {
        m_signalIDs.push_back(ToGuid(guidLiteral->getText().c_str()));
        return;
    }

    const auto measurements = m_dataset->Table(m_primaryMeasurementTableName);

    if (measurements == nullptr)
        return;

    const auto signalIDColumn = measurements->Column(m_signalIDColumnName);

    if (signalIDColumn == nullptr)
        return;

    const int32_t signalIDColumnIndex = signalIDColumn->Index();

    const auto measurementKeyLiteral = context->MEASUREMENT_KEY_LITERAL();

    if (measurementKeyLiteral)
    {
        MapMeasurement(measurements, signalIDColumnIndex, m_measurementKeyColumnName, measurementKeyLiteral->getText());
        return;
    }

    const auto pointTagLiteral = context->POINT_TAG_LITERAL();
    
    if (pointTagLiteral)
        MapMeasurement(measurements, signalIDColumnIndex, m_pointTagColumnName, pointTagLiteral->getText());
}

/*
    expression
     : literalValue
     | columnName
     | unaryOperator expression
     | expression ( '*' | '/' | '%' ) expression
     | expression ( '+' | '-' ) expression
     | expression ( '<<' | '>>' | '&' | '|' ) expression
     | expression ( '<' | '<=' | '>' | '>=' ) expression
     | expression ( '=' | '==' | '!=' | '<>' ) expression
     | expression K_IS K_NOT? K_NULL
     | expression K_NOT? K_IN ( '(' ( expression ( ',' expression )* )? ')' )
     | expression K_NOT? K_LIKE expression
     | expression K_AND expression
     | expression K_OR expression
     | functionName '(' ( expression ( ',' expression )* | '*' )? ')'
     | '(' expression ')'
     ;
 */
void FilterExpressionParser::exitExpression(FilterExpressionSyntaxParser::ExpressionContext* context)
{
    const ExpressionPtr result = NewSharedPtr<Expression>();
    ExpressionPtr value;

    result->Context = context;

    // : literalValue
    if (TryGetExpr(context->literalValue(), value))
    {
        result->Type = value->Type;
        result->Value = value->Value;
        AddExpr(context, result);
        return;
    }

    // | columnName
    if (TryGetExpr(context->columnName(), value))
    {
        result->Type = value->Type;
        result->Value = value->Value;
        AddExpr(context, result);
        return;
    }

    // | unaryOperator expression
    const auto unaryOperator = context->unaryOperator();

    if (unaryOperator != nullptr && TryGetExpr(context->expression(0), value))
    {

        return;
    }

    if (context->expression().size() == 2)
    {
        
    }

    if (TryGetExpr(context->functionName(), value))
    {
        return;
    }
}

/*
    literalValue
     : NUMERIC_LITERAL
     | STRING_LITERAL
     | DATETIME_LITERAL
     | K_NULL
     ;
 */
void FilterExpressionParser::exitLiteralValue(FilterExpressionSyntaxParser::LiteralValueContext* context)
{
}

/*
    columnName
     : IDENTIFIER
     ;
 */
void FilterExpressionParser::exitColumnName(FilterExpressionSyntaxParser::ColumnNameContext* context)
{
}

/*
    unaryOperator
     : '-'
     | '+'
     | '~'
     | K_NOT
     ;
 */
void FilterExpressionParser::exitUnaryOperator(FilterExpressionSyntaxParser::UnaryOperatorContext* context)
{
}

/*
    functionName
     : K_CONVERT
     | K_IIF
     | K_LEN
     | K_ISNULL
     | K_REGEXP
     | K_SUBSTRING
     | K_TRIM
     ;
 */
void FilterExpressionParser::exitFunctionName(FilterExpressionSyntaxParser::FunctionNameContext* context)
{
}
