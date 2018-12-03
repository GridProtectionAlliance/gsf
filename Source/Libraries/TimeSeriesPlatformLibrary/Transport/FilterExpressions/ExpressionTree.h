//******************************************************************************************************
//  ExpressionTree.h - Gbtc
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

#ifndef __EXPRESSION_TREE_H
#define __EXPRESSION_TREE_H

#include "../../Common/CommonTypes.h"
#include "../../DataSet/DataSet.h"

namespace GSF {
namespace TimeSeries {
namespace Transport
{

enum class ExpressionType
{
    Literal,
    Unary,
    Column,
    Operator,
    Function
};

enum class ExpressionDataType
{
    Boolean,    // bool
    Int32,      // int32_t
    Int64,      // int64_t
    Decimal,    // decimal_t
    Double,     // float64_t
    String,     // string
    Guid,       // Guid
    DateTime,   // time_t
    Null        // nullptr
};

class Expression
{
public:
    Expression(ExpressionType type) :
        Type(type)
    {
    }
    
    ExpressionType Type;
    ExpressionDataType DataType = ExpressionDataType::Null;
};

typedef SharedPtr<Expression> ExpressionPtr;

class LiteralExpression : public Expression
{
public:
    LiteralExpression() :
        Expression(ExpressionType::Literal)
    {
    }

    GSF::TimeSeries::Object Value = nullptr;
};

typedef SharedPtr<LiteralExpression> LiteralExpressionPtr;

enum class ExpressionUnaryType
{
    Plus,
    Minus,
    Not
};

class UnaryExpression : public Expression
{
public:
    UnaryExpression(ExpressionUnaryType unaryType) :
        Expression(ExpressionType::Unary),
        UnaryType(unaryType)
    {
    }

    ExpressionUnaryType UnaryType;
    ExpressionPtr Value = nullptr;
};

typedef SharedPtr<UnaryExpression> UnaryExpressionPtr;

class ColumnExpression : public Expression
{
public:
    ColumnExpression() :
        Expression(ExpressionType::Column)
    {
    }

    std::string ColumnName = nullptr;
    int32_t ColumnIndex = -1;
};

typedef SharedPtr<UnaryExpression> UnaryExpressionPtr;

enum class ExpressionOperatorType
{
    Multiply,
    Divide,
    Modulus,
    Add,
    Subtract,
    BitShiftLeft,
    BitShiftRight,
    BitwiseAnd,
    BitwiseOr,
    LessThan,
    LessThanOrEqual,
    GreaterThan,
    GreaterThanOrEqual,
    Equal,
    NotEqual,
    IsNull,
    IsNotNull,
    And,
    Or
};

class OperatorExpression : public Expression
{
public:
    OperatorExpression(ExpressionOperatorType operatorType) :
        Expression(ExpressionType::Operator),
        OperatorType(operatorType)
    {
    }

    ExpressionOperatorType OperatorType;
    ExpressionPtr Left = nullptr;
    ExpressionPtr Right = nullptr;
};

enum class ExpressionFunctionType
{
    Coalesce,
    Convert,
    ImmediateIf,
    Len,
    RegExp,
    SubString,
    Trim
};

class FunctionExpression : public Expression
{
public:
    FunctionExpression(ExpressionFunctionType functionType) :
        Expression(ExpressionType::Function),
        FunctionType(functionType)
    {
    }

    ExpressionFunctionType FunctionType;
    std::vector<ExpressionPtr> Arguments;
};

class ExpressionTree
{
public:
    ExpressionPtr Root = nullptr;

    GSF::TimeSeries::Guid Evaluate(const DataSet::DataRowPtr& row, const int32_t signalIDColumnIndex);
};

typedef SharedPtr<ExpressionTree> ExpressionTreePtr;

}}}

#endif