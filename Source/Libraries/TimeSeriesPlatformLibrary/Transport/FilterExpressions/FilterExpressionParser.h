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

#include "FilterExpressionSyntaxLexer.h"
#include "FilterExpressionSyntaxBaseListener.h"

#ifndef EOF
#define EOF (-1)
#endif

#include "ExpressionTree.h"

namespace GSF {
namespace TimeSeries {
namespace Transport
{

// Simple exception type thrown by the filter expression parser
class FilterExpressionException : public Exception
{
private:
    std::string m_message;

public:
    FilterExpressionException(std::string message) noexcept;
    const char* what() const noexcept;
};

struct MeasurementTableIDFields
{
    std::string SignalIDFieldName;
    std::string MeasurementKeyFieldName;
    std::string PointTagFieldName;
};

typedef SharedPtr<MeasurementTableIDFields> MeasurementTableIDFieldsPtr;

class FilterExpressionParser : public FilterExpressionSyntaxBaseListener // NOLINT
{
private:
    antlr4::ANTLRInputStream m_inputStream;
    FilterExpressionSyntaxLexer* m_lexer;
    antlr4::CommonTokenStream* m_tokens;
    FilterExpressionSyntaxParser* m_parser;
    GSF::DataSet::DataSetPtr m_dataset;
    ExpressionTreePtr m_activeExpressionTree;

    std::string m_primaryMeasurementTableName;
    std::unordered_set<GSF::TimeSeries::Guid> m_signalIDs;
    std::vector<ExpressionTreePtr> m_expressionTrees;
    std::map<const antlr4::ParserRuleContext*, ExpressionPtr> m_expressions;
    std::map<const std::string, MeasurementTableIDFieldsPtr> m_measurementTableIDFields;

    bool TryGetExpr(const antlr4::ParserRuleContext* context, ExpressionPtr& expression) const;
    void AddExpr(const antlr4::ParserRuleContext* context, const ExpressionPtr& expression);
    void MapMeasurement(const GSF::DataSet::DataTablePtr& measurements, const int32_t signalIDColumnIndex, const std::string& columnName, const std::string& mappingValue);
public:
    FilterExpressionParser(const std::string& filterExpression);
    ~FilterExpressionParser();

    const GSF::DataSet::DataSetPtr& CurrentDataSet() const;
    void AssignDataSet(const GSF::DataSet::DataSetPtr& dataset);

    MeasurementTableIDFieldsPtr GetMeasurementTableIDFields(const std::string& measurementTableName) const;
    void SetMeasurementTableIDFields(const std::string& measurementTableName, const MeasurementTableIDFieldsPtr& measurementTableIDFields);

    const std::string& GetPrimaryMeasurementTableName() const;
    void SetPrimaryMeasurementTableName(const std::string& tableName);

    void Evaluate();

    const std::unordered_set<GSF::TimeSeries::Guid>& FilteredSignalIDs() const;

    void exitParse(FilterExpressionSyntaxParser::ParseContext*) override;
    void enterFilterStatement(FilterExpressionSyntaxParser::FilterStatementContext*) override;
    void exitIdentifierStatement(FilterExpressionSyntaxParser::IdentifierStatementContext*) override;
    void exitExpression(FilterExpressionSyntaxParser::ExpressionContext*) override;
    void exitLiteralValue(FilterExpressionSyntaxParser::LiteralValueContext*) override;
    void exitColumnName(FilterExpressionSyntaxParser::ColumnNameContext*) override;
};

typedef SharedPtr<FilterExpressionParser> FilterExpressionParserPtr;

}}}

#endif