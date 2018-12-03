//******************************************************************************************************
//  ExpressionTree.cpp - Gbtc
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
//  12/02/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

#include "ExpressionTree.h"


using namespace std;
using namespace GSF::DataSet;
using namespace GSF::TimeSeries;
using namespace Transport;

const LiteralExpressionPtr ExpressionTree::True = NewSharedPtr<LiteralExpression>(ExpressionDataType::Boolean, true);

const LiteralExpressionPtr ExpressionTree::False = NewSharedPtr<LiteralExpression>(ExpressionDataType::Boolean, false);

const LiteralExpressionPtr ExpressionTree::Null = NewSharedPtr<LiteralExpression>(ExpressionDataType::Null, nullptr);

ExpressionTreeException::ExpressionTreeException(string message) noexcept :
    m_message(std::move(message))
{
}

const char* ExpressionTreeException::what() const noexcept
{
    return &m_message[0];
}

Expression::Expression(ExpressionType type, ExpressionDataType dataType) :
    Type(type),
    DataType(dataType)
{
}

LiteralExpression::LiteralExpression(ExpressionDataType dataType, const Object& value) :
    Expression(ExpressionType::Literal, dataType),
    Value(value)
{
}

UnaryExpression::UnaryExpression(ExpressionUnaryType unaryType, const ExpressionPtr& value) :
    Expression(ExpressionType::Unary, value->DataType),
    UnaryType(unaryType),
    Value(value)
{
}

ColumnExpression::ColumnExpression(ExpressionDataType dataType, int32_t columnIndex) :
    Expression(ExpressionType::Column, dataType),
    ColumnIndex(columnIndex)
{
}

OperatorExpression::OperatorExpression(ExpressionDataType dataType, ExpressionOperatorType operatorType) :
    Expression(ExpressionType::Operator, dataType),
    OperatorType(operatorType)
{
}

FunctionExpression::FunctionExpression(ExpressionDataType dataType, ExpressionFunctionType functionType, const std::vector<ExpressionPtr>& arguments) :
    Expression(ExpressionType::Function, dataType),
    FunctionType(functionType),
    Arguments(arguments)
{
}

ExpressionPtr ExpressionTree::Evaluate(ExpressionPtr node)
{
    if (node == nullptr)
        return False;

    
}

ExpressionTree::ExpressionTree(std::string measurementTableName, const DataTablePtr& measurements) :
    MeasurementTableName(std::move(measurementTableName)),
    Measurements(measurements)
{
}

bool ExpressionTree::Evaluate(const DataRowPtr& row)
{
    m_currentRow = row;

    const ExpressionPtr result = Evaluate(Root);

    // Final expression should have a boolean data type
    if (result->DataType == ExpressionDataType::Boolean)
        return Cast<bool>(CastSharedPtr<LiteralExpression>(result)->Value);

    throw ExpressionTreeException("Final expression tree evaluation did not result in a boolean value");
}
