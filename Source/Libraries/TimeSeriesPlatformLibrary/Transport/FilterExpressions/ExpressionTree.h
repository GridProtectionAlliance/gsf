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
    InList,
    Function,
    Operator
};

// These data types are reduced to a more reasonable set of possible
// literal types that can be represented in a filter expression, as a
// result all data table column values will be mapped to these types.
// The behavior of these types has been modeled to operate similar to
// parsing of literal expressions in .NET DataSet operations, see:
// https://docs.microsoft.com/en-us/dotnet/api/system.data.datacolumn.expression#parsing-literal-expressions
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

typedef std::vector<ExpressionPtr> ExpressionCollection;
typedef SharedPtr<ExpressionCollection> ExpressionCollectionPtr;

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

const char* ExpressionUnaryTypeAcronym[];
const char* EnumName(ExpressionUnaryType type);

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
    Like,
    NotLike,
    And,
    Or
};

const char* ExpressionOperatorTypeAcronym[];
const char* EnumName(ExpressionOperatorType type);

class OperatorExpression : public Expression
{
public:
    OperatorExpression(ExpressionOperatorType operatorType, const ExpressionPtr& leftValue, const ExpressionPtr& rightValue);

    const ExpressionOperatorType OperatorType;
    const ExpressionPtr& LeftValue;
    const ExpressionPtr& RightValue;
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

class InListExpression : public Expression
{
public:
    InListExpression(const ExpressionPtr& value, const ExpressionCollectionPtr& arguments, bool notInList);

    const ExpressionPtr& Value;
    const ExpressionCollectionPtr Arguments;
    const bool NotInList;
};

typedef SharedPtr<InListExpression> InListExpressionPtr;

class FunctionExpression : public Expression
{
public:
    FunctionExpression(ExpressionFunctionType functionType, const ExpressionCollectionPtr& arguments);

    const ExpressionFunctionType FunctionType;
    const ExpressionCollectionPtr Arguments;
};

typedef SharedPtr<FunctionExpression> FunctionExpressionPtr;

class ExpressionTree
{
private:
    DataSet::DataRowPtr m_currentRow;

    ValueExpressionPtr Evaluate(const ExpressionPtr& node, ExpressionDataType targetDataType = ExpressionDataType::Boolean) const;
    ValueExpressionPtr EvaluateUnary(const ExpressionPtr& node) const;
    ValueExpressionPtr EvaluateColumn(const ExpressionPtr& node) const;
    ValueExpressionPtr EvaluateInList(const ExpressionPtr& node) const;
    ValueExpressionPtr EvaluateFunction(const ExpressionPtr& node) const;
    ValueExpressionPtr EvaluateOperator(const ExpressionPtr& node) const;
    
    template<typename T>
    T ApplyIntegerUnaryOperation(const Nullable<T>& unaryValue, ExpressionUnaryType unaryOperation) const;
    
    template<typename T>
    T ApplyNumericUnaryOperation(const Nullable<T>& unaryValue, ExpressionUnaryType unaryOperation, ExpressionDataType dataType) const;

    // Operation Data Type Selectors
    ExpressionDataType DeriveOperationDataType(ExpressionOperatorType operationType, ExpressionDataType leftDataType, ExpressionDataType rightDataType) const;
    ExpressionDataType DeriveArithmeticOperationDataType(ExpressionOperatorType operationType, ExpressionDataType leftDataType, ExpressionDataType rightDataType) const;
    ExpressionDataType DeriveBitwiseOperationDataType(ExpressionOperatorType operationType, ExpressionDataType leftDataType, ExpressionDataType rightDataType) const;
    ExpressionDataType DeriveComparisonOperationDataType(ExpressionOperatorType operationType, ExpressionDataType leftDataType, ExpressionDataType rightDataType) const;
    ExpressionDataType DeriveEqualityOperationDataType(ExpressionOperatorType operationType, ExpressionDataType leftDataType, ExpressionDataType rightDataType) const;
    ExpressionDataType DeriveBooleanOperationDataType(ExpressionOperatorType operationType, ExpressionDataType leftDataType, ExpressionDataType rightDataType) const;

    // Filter Expression Function Implementations
    const ValueExpressionPtr& Coalesce(const ValueExpressionPtr& testValue, const ValueExpressionPtr& defaultValue) const;
    ValueExpressionPtr Convert(const ValueExpressionPtr& sourceValue, const ValueExpressionPtr& targetType) const;
    ValueExpressionPtr IIf(const ValueExpressionPtr& testValue, const ExpressionPtr& leftResultValue, const ExpressionPtr& rightResultValue) const;
    ValueExpressionPtr IsRegExMatch(const ValueExpressionPtr& regexValue, const ValueExpressionPtr& testValue) const;
    ValueExpressionPtr Len(const ValueExpressionPtr& sourceValue) const;
    ValueExpressionPtr RegExVal(const ValueExpressionPtr& regexValue, const ValueExpressionPtr& testValue) const;
    ValueExpressionPtr SubString(const ValueExpressionPtr& sourceValue, const ValueExpressionPtr& indexValue, const ValueExpressionPtr& lengthValue) const;
    ValueExpressionPtr Trim(const ValueExpressionPtr& sourceValue) const;

    // Filter Expression Operator Implementations
    ValueExpressionPtr Multiply(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionDataType dataType) const;
    ValueExpressionPtr Divide(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionDataType dataType) const;
    ValueExpressionPtr Modulus(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionDataType dataType) const;
    ValueExpressionPtr Add(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionDataType dataType) const;
    ValueExpressionPtr Subtract(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionDataType dataType) const;
    ValueExpressionPtr BitShiftLeft(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue) const;
    ValueExpressionPtr BitShiftRight(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue) const;
    ValueExpressionPtr BitwiseAnd(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionDataType dataType) const;
    ValueExpressionPtr BitwiseOr(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionDataType dataType) const;
    ValueExpressionPtr LessThan(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionDataType dataType) const;
    ValueExpressionPtr LessThanOrEqual(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionDataType dataType) const;
    ValueExpressionPtr GreaterThan(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionDataType dataType) const;
    ValueExpressionPtr GreaterThanOrEqual(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionDataType dataType) const;
    ValueExpressionPtr Equal(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionDataType dataType) const;
    ValueExpressionPtr NotEqual(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionDataType dataType) const;
    ValueExpressionPtr IsNull(const ValueExpressionPtr& leftValue) const;
    ValueExpressionPtr IsNotNull(const ValueExpressionPtr& leftValue) const;
    ValueExpressionPtr Like(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue) const;
    ValueExpressionPtr NotLike(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue) const;
    ValueExpressionPtr And(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue) const;
    ValueExpressionPtr Or(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue) const;

    template<typename T>
    static T Multiply(const Nullable<T>& leftValue, const Nullable<T>& rightValue);

    template<typename T>
    static T Divide(const Nullable<T>& leftValue, const Nullable<T>& rightValue);

    template<typename T>
    static T Modulus(const Nullable<T>& leftValue, const Nullable<T>& rightValue);

    template<typename T>
    static T Add(const Nullable<T>& leftValue, const Nullable<T>& rightValue);

    template<typename T>
    static T Subtract(const Nullable<T>& leftValue, const Nullable<T>& rightValue);

    template<typename T>
    static T BitShiftLeft(const Nullable<T>& operandValue, int32_t shiftValue);

    template<typename T>
    static T BitShiftRight(const Nullable<T>& operandValue, int32_t shiftValue);

    template<typename T>
    static T BitwiseAnd(const Nullable<T>& leftValue, const Nullable<T>& rightValue);

    template<typename T>
    static T BitwiseOr(const Nullable<T>& leftValue, const Nullable<T>& rightValue);

    template<typename T>
    static bool LessThan(const Nullable<T>& leftValue, const Nullable<T>& rightValue);

    template<typename T>
    static bool LessThanOrEqual(const Nullable<T>& leftValue, const Nullable<T>& rightValue);

    template<typename T>
    static bool GreaterThan(const Nullable<T>& leftValue, const Nullable<T>& rightValue);

    template<typename T>
    static bool GreaterThanOrEqual(const Nullable<T>& leftValue, const Nullable<T>& rightValue);

    template<typename T>
    static bool Equal(const Nullable<T>& leftValue, const Nullable<T>& rightValue);

    template<typename T>
    static bool NotEqual(const Nullable<T>& leftValue, const Nullable<T>& rightValue);

    ValueExpressionPtr Convert(const ValueExpressionPtr& sourceValue, ExpressionDataType targetDataType) const;
    ValueExpressionPtr EvaluateRegEx(const std::string& functionName, const ValueExpressionPtr& regexValue, const ValueExpressionPtr& testValue, bool returnMatchedValue) const;
public:
    ExpressionTree(std::string measurementTableName, const DataSet::DataTablePtr& measurements);

    const std::string MeasurementTableName;
    const DataSet::DataTablePtr& Measurements;
    ExpressionPtr Root = nullptr;

    ValueExpressionPtr Evaluate(const DataSet::DataRowPtr& row);

    static const ValueExpressionPtr True;
    static const ValueExpressionPtr False;
    static const ValueExpressionPtr EmptyString;
    static ValueExpressionPtr NullValue(ExpressionDataType targetDataType);
};

typedef SharedPtr<ExpressionTree> ExpressionTreePtr;

}}}

#endif