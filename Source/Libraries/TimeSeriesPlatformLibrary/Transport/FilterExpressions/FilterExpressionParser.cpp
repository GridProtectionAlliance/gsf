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

using namespace std;
using namespace GSF::DataSet;
using namespace GSF::TimeSeries;
using namespace GSF::TimeSeries::Transport;
using namespace antlr4;
using namespace antlr4::tree;
using namespace boost;

// Mapped type for TimeSeries Guid (ANTLR4 also defines a Guid type)
typedef GSF::TimeSeries::Guid guid;

static guid ParseGuidLiteral(string guidLiteral)
{
    // Remove any quotes from GUID (boost currently only handles optional braces),
    // ANTLR grammar already ensures GUID starting with quote also ends with one
    if (guidLiteral.front() == '\'')
    {
        guidLiteral.erase(0, 1);
        guidLiteral.erase(guidLiteral.size() - 1);
    }

    return ToGuid(guidLiteral.c_str());
}

time_t ParseDateTimeLiteral(string time)
{
    // Remove any surrounding '#' symbols from date/time, ANTLR grammar already
    // ensures date/time starting with '#' symbol will also end with one
    if (time.front() == '#')
    {
        time.erase(0, 1);
        time.erase(time.size() - 1);
    }

    return ParseTimestamp(time.c_str());
}

FilterExpressionException::FilterExpressionException(string message) noexcept :
    m_message(std::move(message))
{
}

const char* FilterExpressionException::what() const noexcept
{
    return &m_message[0];
}

FilterExpressionParser::FilterExpressionParser(const string& filterExpression) :
    m_inputStream(filterExpression),
    m_lexer(nullptr),
    m_tokens(nullptr),
    m_parser(nullptr),
    m_dataset(nullptr),
    m_primaryMeasurementTableName("ActiveMeasurements")
{
    m_lexer = new FilterExpressionSyntaxLexer(&m_inputStream);
    m_tokens = new CommonTokenStream(m_lexer);
    m_parser = new FilterExpressionSyntaxParser(m_tokens);

    MeasurementTableIDFieldsPtr measurementTableIDFields = NewSharedPtr<MeasurementTableIDFields>();

    measurementTableIDFields->SignalIDFieldName = "SignalID";
    measurementTableIDFields->MeasurementKeyFieldName = "ID";
    measurementTableIDFields->PointTagFieldName = "PointTag";

    SetMeasurementTableIDFields(m_primaryMeasurementTableName, measurementTableIDFields);
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

void FilterExpressionParser::MapMeasurement(const DataTablePtr& measurements, const int32_t signalIDColumnIndex, const string& columnName, const string& mappingValue)
{
    const DataColumnPtr column = measurements->Column(columnName);

    if (column == nullptr)
        return;

    const int32_t columnIndex = column->Index();

    for (int32_t i = 0; i < measurements->RowCount(); i++)
    {
        const DataRowPtr row = measurements->Row(i);

        if (row)
        {
            const Nullable<string> field = row->ValueAsString(columnIndex);

            if (field.HasValue() && IsEqual(mappingValue, field.Value))
            {
                const Nullable<guid> signalIDField = row->ValueAsGuid(signalIDColumnIndex);

                if (signalIDField.HasValue())
                    m_signalIDs.insert(signalIDField.Value);

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

MeasurementTableIDFieldsPtr FilterExpressionParser::GetMeasurementTableIDFields(const std::string& measurementTableName) const
{
    MeasurementTableIDFieldsPtr measurementTableFields;
    
    TryGetValue<const std::string, MeasurementTableIDFieldsPtr>(m_measurementTableIDFields, measurementTableName, measurementTableFields, nullptr);

    return measurementTableFields;
}

void FilterExpressionParser::SetMeasurementTableIDFields(const std::string& measurementTableName, const MeasurementTableIDFieldsPtr& measurementTableIDFields)
{
    m_measurementTableIDFields.insert_or_assign(measurementTableName, measurementTableIDFields);
}

const string& FilterExpressionParser::GetPrimaryMeasurementTableName() const
{
    return m_primaryMeasurementTableName;
}

void FilterExpressionParser::SetPrimaryMeasurementTableName(const string& tableName)
{
    m_primaryMeasurementTableName = tableName;
}

void FilterExpressionParser::Evaluate()
{
    m_signalIDs.clear();
    m_expressionTrees.clear();
    m_expressions.clear();

    if (m_dataset == nullptr)
        throw FilterExpressionException("Cannot evaluate filter expression, no dataset has been defined");

    // Create parse tree and visit listener methods
    ParseTreeWalker walker;
    const auto parseTree = m_parser->parse();
    walker.walk(this, parseTree);

    // Each filter expression statement will have its own expression tree, evaluate each
    for (size_t x = 0; x < m_expressionTrees.size(); x++)
    {
        const ExpressionTreePtr& expressionTree = m_expressionTrees[x];
        const DataTablePtr& measurements = expressionTree->Measurements;
        const MeasurementTableIDFieldsPtr measurementTableIDFields = GetMeasurementTableIDFields(expressionTree->MeasurementTableName);

        if (measurementTableIDFields == nullptr)
            throw FilterExpressionException("Failed to find ID fields record for measurement table \"" + expressionTree->MeasurementTableName + "\"");

        const DataColumnPtr signalIDColumn = measurements->Column(measurementTableIDFields->SignalIDFieldName);

        if (signalIDColumn == nullptr)
            throw FilterExpressionException("Failed to find signal ID field for measurement table \"" + expressionTree->MeasurementTableName + "\"");

        const int32_t signalIDColumnIndex = signalIDColumn->Index();

        for (int32_t y = 0; y < measurements->RowCount(); y++)
        {
            const DataRowPtr row = measurements->Row(y);

            if (row == nullptr)
                continue;

            const ValueExpressionPtr result = expressionTree->Evaluate(row);

            // Final expression should have a boolean data type (it's part of a WHERE clause)
            if (result->DataType != ExpressionDataType::Boolean)
                throw FilterExpressionException("Final expression tree evaluation did not result in a boolean value, result data type is " + string(EnumName(result->DataType)));

            bool expressionValue;

            // If final result is Null (due to Null propagation), treat result as False
            if (result->IsNullable)
            {
                Nullable<bool> value = Cast<Nullable<bool>>(result->Value);

                if (value.HasValue())
                    expressionValue = value.Value;
                else
                    expressionValue = false;
            }
            else
            {
                expressionValue = Cast<bool>(result->Value);
            }

            if (expressionValue)
            {
                Nullable<guid> signalIDField = row->ValueAsGuid(signalIDColumnIndex);

                if (signalIDField.HasValue())
                    m_signalIDs.insert(signalIDField.Value);
            }
        }
    }
}

const unordered_set<guid>& FilterExpressionParser::FilteredSignalIDs() const
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
    filterExpressionStatement
     : filterStatement
     | identifierStatement
     ;
 */
void FilterExpressionParser::enterFilterStatement(FilterExpressionSyntaxParser::FilterStatementContext* context)
{
    // One filter expression can contain multiple filter statements separated by semi-colon,
    // so we track each as an independent expression tree
    m_expressions.clear();

    const string& measurementTableName = context->tableName()->getText();
    const DataTablePtr measurements = m_dataset->Table(measurementTableName);

    if (measurements == nullptr)
        throw FilterExpressionException("Failed to find measurement table \"" + measurementTableName + "\"");

    m_activeExpressionTree = NewSharedPtr<ExpressionTree>(measurementTableName, measurements);
    m_expressionTrees.push_back(m_activeExpressionTree);
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
    if (context->GUID_LITERAL())
    {
        m_signalIDs.insert(ParseGuidLiteral(context->GUID_LITERAL()->getText()));
        return;
    }

    const DataTablePtr measurements = m_dataset->Table(m_primaryMeasurementTableName);

    if (measurements == nullptr)
        return;

    const MeasurementTableIDFieldsPtr measurementTableIDFields = GetMeasurementTableIDFields(m_primaryMeasurementTableName);

    if (measurementTableIDFields == nullptr)
        return;

    const DataColumnPtr signalIDColumn = measurements->Column(measurementTableIDFields->SignalIDFieldName);

    if (signalIDColumn == nullptr)
        return;

    const int32_t signalIDColumnIndex = signalIDColumn->Index();

    if (context->MEASUREMENT_KEY_LITERAL())
    {
        MapMeasurement(measurements, signalIDColumnIndex, measurementTableIDFields->MeasurementKeyFieldName, context->MEASUREMENT_KEY_LITERAL()->getText());
        return;
    }

    if (context->POINT_TAG_LITERAL())
        MapMeasurement(measurements, signalIDColumnIndex, measurementTableIDFields->PointTagFieldName, context->POINT_TAG_LITERAL()->getText());
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
    //const ExpressionPtr result = NewSharedPtr<Expression>();
    ExpressionPtr value;

    //result->Context = context;

    //if (TryGetExpr(context->literalValue(), value))
    //{
    //    result->Value = value->Value;
    //    result->DataType = value->DataType;
    //    AddExpr(context, result);
    //    return;
    //}

    //if (TryGetExpr(context->columnName(), value))
    //{
    //    result->Value = value->Value;
    //    result->DataType = value->DataType;
    //    AddExpr(context, result);
    //    return;
    //}

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
     : INTEGER_LITERAL
     | NUMERIC_LITERAL
     | STRING_LITERAL
     | DATETIME_LITERAL
     | GUID_LITERAL
     | BOOLEAN_LITERAL
     | K_NULL
     ;
 */
void FilterExpressionParser::exitLiteralValue(FilterExpressionSyntaxParser::LiteralValueContext* context)
{
    ValueExpressionPtr result = nullptr;

    if (context->INTEGER_LITERAL())
    {
        const double_t value = stod(context->INTEGER_LITERAL()->getText());

        if (value > Int64::MaxValue)
            result = NewSharedPtr<ValueExpression>(ExpressionDataType::Double, value);
        else if (value > Int32::MaxValue)
            result = NewSharedPtr<ValueExpression>(ExpressionDataType::Int64, static_cast<int64_t>(value));
        else
            result = NewSharedPtr<ValueExpression>(ExpressionDataType::Int32, static_cast<int32_t>(value));
    }
    else if (context->NUMERIC_LITERAL())
    {
        const string literal = context->NUMERIC_LITERAL()->getText();
        
        if (literal.find('e') || literal.find('E'))
        {
            // Real literals using scientific notation are parsed as double
            result = NewSharedPtr<ValueExpression>(ExpressionDataType::Double, stod(literal));
        }
        else
        {
            // Real literals without scientific notation are parsed as decimal, if
            // the number fails to parse as decimal, then it is parsed as a double
            try
            {
                result = NewSharedPtr<ValueExpression>(ExpressionDataType::Decimal, decimal_t(literal));
            }
            catch (const std::runtime_error&)
            {
                result = NewSharedPtr<ValueExpression>(ExpressionDataType::Double, stod(literal));
            }
        }
    }
    else if (context->STRING_LITERAL())
    {
        result = NewSharedPtr<ValueExpression>(ExpressionDataType::String, context->STRING_LITERAL()->getText());
    }
    else if (context->DATETIME_LITERAL())
    {
        result = NewSharedPtr<ValueExpression>(ExpressionDataType::DateTime, ParseDateTimeLiteral(context->DATETIME_LITERAL()->getText()));
    }
    else if (context->GUID_LITERAL())
    {
        result = NewSharedPtr<ValueExpression>(ExpressionDataType::Guid, ParseGuidLiteral(context->GUID_LITERAL()->getText()));
    }
    else if (context->BOOLEAN_LITERAL())
    {        
        result = IsEqual(context->BOOLEAN_LITERAL()->getText(), "true") ? ExpressionTree::True : ExpressionTree::False;
    }
    else if (context->K_NULL())
    {
        result = ExpressionTree::NullValue(ExpressionDataType::Undefined);
    }

    if (result)
        AddExpr(context, CastSharedPtr<Expression>(result));
}

/*
    columnName
     : IDENTIFIER
     ;
 */
void FilterExpressionParser::exitColumnName(FilterExpressionSyntaxParser::ColumnNameContext* context)
{
    const string& columnName = context->IDENTIFIER()->getText();
    const DataColumnPtr column = m_activeExpressionTree->Measurements->Column(columnName);

    if (column == nullptr)
        return;

    AddExpr(context, NewSharedPtr<ColumnExpression>(column));
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
     : K_COALESCE
     | K_CONVERT
     | K_IIF
     | K_ISNULL
     | K_ISREGEXMATCH
     | K_LEN
     | K_REGEXVAL
     | K_SUBSTR
     | K_SUBSTRING
     | K_TRIM
     ;
 */
void FilterExpressionParser::exitFunctionName(FilterExpressionSyntaxParser::FunctionNameContext* context)
{
}
