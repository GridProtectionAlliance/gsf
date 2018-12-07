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

// Simple exception type thrown by the expression tree
class ExpressionTreeException : public Exception
{
private:
    std::string m_message;

public:
    ExpressionTreeException(std::string message) noexcept;
    const char* what() const noexcept;
};

enum class ExpressionType
{
    Value,
    Unary,
    Column,
    Operator,
    Function
};

// These data types are reduced to a more reasonable set of possible
// literal types that can be represented in a filter expression, as a
// result all data table column values will be mapped to these types.
// The behavior has been targeted to match that of parsed literal
// expressions in .NET DataSet operations. See:
// https://docs.microsoft.com/en-us/dotnet/api/system.data.datacolumn.expression?redirectedfrom=MSDN&view=netframework-4.7.2#parsing-literal-expressions
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
    Undefined   // nullptr (make sure value is always last in enum)
};

const extern int32_t ExpressionDataTypeLength;
const char* ExpressionDataTypeAcronym[];
const char* EnumName(ExpressionDataType type);

bool IsIntegerType(ExpressionDataType type);
bool IsNumericType(ExpressionDataType type);

// Base class for all expression types
class Expression;
typedef SharedPtr<Expression> ExpressionPtr;

class Expression
{
public:
    Expression(ExpressionType type, ExpressionDataType dataType, bool isNullable = false);

    const ExpressionType Type;
    const ExpressionDataType DataType;
    const bool IsNullable;
};

class ValueExpression : public Expression
{
private:
    void ValidateDataType(ExpressionDataType targetType) const;

public:
    ValueExpression(ExpressionDataType dataType, const GSF::TimeSeries::Object& value, bool isNullable = false);

    const GSF::TimeSeries::Object& Value;

    bool IsNull() const;
    std::string ToString() const;

    // The following functions are data type validated
    Nullable<bool> ValueAsBoolean() const;
    Nullable<int32_t> ValueAsInt32() const;
    Nullable<int64_t> ValueAsInt64() const;
    Nullable<decimal_t> ValueAsDecimal() const;
    Nullable<float64_t> ValueAsDouble() const;
    Nullable<std::string> ValueAsString() const;
    Nullable<Guid> ValueAsGuid() const;
    Nullable<time_t> ValueAsDateTime() const;
};

typedef SharedPtr<ValueExpression> ValueExpressionPtr;

enum class ExpressionUnaryType
{
    Plus,
    Minus,
    Not
};

class UnaryExpression : public Expression
{
public:
    UnaryExpression(ExpressionUnaryType unaryType, const ExpressionPtr& value);

    const ExpressionUnaryType UnaryType;
    const ExpressionPtr& Value;
};

typedef SharedPtr<UnaryExpression> UnaryExpressionPtr;

class ColumnExpression : public Expression
{
public:
    ColumnExpression(const GSF::DataSet::DataColumnPtr& column);

    const GSF::DataSet::DataColumnPtr& Column;
};

typedef SharedPtr<ColumnExpression> ColumnExpressionPtr;

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
    OperatorExpression(ExpressionDataType dataType, ExpressionOperatorType operatorType);

    const ExpressionOperatorType OperatorType;
    ExpressionPtr Left;
    ExpressionPtr Right;
};

typedef SharedPtr<OperatorExpression> OperatorExpressionPtr;

enum class ExpressionFunctionType
{
    Coalesce,
    Convert,
    IIf,
    IsRegExMatch,
    Len,
    RegExVal,
    SubString,
    Trim
};

class FunctionExpression : public Expression
{
public:
    FunctionExpression(ExpressionDataType dataType, ExpressionFunctionType functionType, const std::vector<ExpressionPtr>& arguments);

    const ExpressionFunctionType FunctionType;
    const std::vector<ExpressionPtr>& Arguments;
};

typedef SharedPtr<FunctionExpression> FunctionExpressionPtr;

class ExpressionTree
{
private:
    DataSet::DataRowPtr m_currentRow;

    ValueExpressionPtr Evaluate(const ExpressionPtr& node, ExpressionDataType targetType = ExpressionDataType::Undefined) const;
    ValueExpressionPtr EvaluateUnary(const ExpressionPtr& node) const;
    ValueExpressionPtr EvaluateColumn(const ExpressionPtr& node) const;
    ValueExpressionPtr EvaluateFunction(const ExpressionPtr& node) const;
    ValueExpressionPtr EvaluateOperator(const ExpressionPtr& node) const;
    
    template<typename T>
    T ApplyIntegerUnaryOperation(const UnaryExpressionPtr& unaryNode) const;
    
    template<typename T>
    T ApplyNumericUnaryOperation(const UnaryExpressionPtr& unaryNode) const;

    const ValueExpressionPtr& Coalesce(const ValueExpressionPtr& testValue, const ValueExpressionPtr& defaultValue) const;
    ValueExpressionPtr Convert(const ValueExpressionPtr& sourceValue, const ValueExpressionPtr& targetType) const;
    ValueExpressionPtr IIf(const ValueExpressionPtr& testValue, const ExpressionPtr& leftResultValue, const ExpressionPtr& rightResultValue) const;
    ValueExpressionPtr IsRegExMatch(const ValueExpressionPtr& regexValue, const ValueExpressionPtr& testValue) const;
    ValueExpressionPtr Len(const ValueExpressionPtr& sourceValue) const;
    ValueExpressionPtr RegExVal(const ValueExpressionPtr& regexValue, const ValueExpressionPtr& testValue) const;
    ValueExpressionPtr SubString(const ValueExpressionPtr& sourceValue, const ValueExpressionPtr& indexValue, const ValueExpressionPtr& lengthValue) const;
    ValueExpressionPtr Trim(const ValueExpressionPtr& sourceValue) const;
    ValueExpressionPtr EvaluateRegEx(const std::string& functionName, const ValueExpressionPtr& regexValue, const ValueExpressionPtr& testValue, bool returnValue) const;
    ValueExpressionPtr NullableValue(ExpressionDataType targetDataType) const;
public:
    ExpressionTree(std::string measurementTableName, const DataSet::DataTablePtr& measurements);

    const std::string MeasurementTableName;
    const DataSet::DataTablePtr& Measurements;
    ExpressionPtr Root = nullptr;

    ValueExpressionPtr Evaluate(const DataSet::DataRowPtr& row);

    static const ValueExpressionPtr True;
    static const ValueExpressionPtr False;
    static const ValueExpressionPtr Null;
    static const ValueExpressionPtr Empty;
};

typedef SharedPtr<ExpressionTree> ExpressionTreePtr;

}}}

#endif