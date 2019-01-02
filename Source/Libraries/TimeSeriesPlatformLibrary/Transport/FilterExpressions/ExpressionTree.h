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
#include "../../Data/DataSet.h"

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

// Base class for all expression types
class Expression // NOLINT
{
public:
    explicit Expression(ExpressionType type);
    virtual ~Expression();

    const ExpressionType Type;
};

typedef SharedPtr<Expression> ExpressionPtr;
typedef std::vector<ExpressionPtr> ExpressionCollection;
typedef SharedPtr<ExpressionCollection> ExpressionCollectionPtr;

// These expression value data types are reduced to a reasonable set of possible types
// that can be represented in a filter expression. All data table column values will be
// mapped to these types. The behavior of these types has been modeled to operate similar
// to parsing of literal expressions in .NET DataSet operations, see:
// https://docs.microsoft.com/en-us/dotnet/api/system.data.datacolumn.expression#parsing-literal-expressions
enum class ExpressionValueType
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

const extern int32_t ExpressionValueTypeLength;
const char* ExpressionValueTypeAcronym[];
const char* EnumName(ExpressionValueType valueType);

bool IsIntegerType(ExpressionValueType valueType);
bool IsNumericType(ExpressionValueType valueType);

class ValueExpression : public Expression
{
private:
    void ValidateValueType(ExpressionValueType valueType) const;

public:
    ValueExpression(ExpressionValueType valueType, const GSF::TimeSeries::Object& value, bool valueIsNullable = false);

    const GSF::TimeSeries::Object& Value;
    const ExpressionValueType ValueType;
    const bool ValueIsNullable;

    bool IsNull() const;
    std::string ToString() const;

    // The following functions are value type validated
    bool ValueAsBoolean() const;
    Nullable<bool> ValueAsNullableBoolean() const;

    int32_t ValueAsInt32() const;
    Nullable<int32_t> ValueAsNullableInt32() const;

    int64_t ValueAsInt64() const;
    Nullable<int64_t> ValueAsNullableInt64() const;

    decimal_t ValueAsDecimal() const;
    Nullable<decimal_t> ValueAsNullableDecimal() const;

    float64_t ValueAsDouble() const;
    Nullable<float64_t> ValueAsNullableDouble() const;

    std::string ValueAsString() const;
    Nullable<std::string> ValueAsNullableString() const;

    Guid ValueAsGuid() const;
    Nullable<Guid> ValueAsNullableGuid() const;

    time_t ValueAsDateTime() const;
    Nullable<time_t> ValueAsNullableDateTime() const;
};

typedef SharedPtr<ValueExpression> ValueExpressionPtr;

enum class ExpressionUnaryType
{
    Plus,
    Minus,
    Not
};

const char* ExpressionUnaryTypeAcronym[];
const char* EnumName(ExpressionUnaryType unaryType);

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
    ColumnExpression(const GSF::Data::DataColumnPtr& dataColumn);

    const GSF::Data::DataColumnPtr& DataColumn;
};

typedef SharedPtr<ColumnExpression> ColumnExpressionPtr;

class InListExpression : public Expression
{
public:
    InListExpression(const ExpressionPtr& value, ExpressionCollectionPtr arguments, bool hasNotKeyword);

    const ExpressionPtr& Value;
    const ExpressionCollectionPtr Arguments;
    const bool HasNotKeyword;
};

typedef SharedPtr<InListExpression> InListExpressionPtr;

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
    FunctionExpression(ExpressionFunctionType functionType, ExpressionCollectionPtr arguments);

    const ExpressionFunctionType FunctionType;
    const ExpressionCollectionPtr Arguments;
};

typedef SharedPtr<FunctionExpression> FunctionExpressionPtr;

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
const char* EnumName(ExpressionOperatorType operatorType);

class OperatorExpression : public Expression
{
public:
    OperatorExpression(ExpressionOperatorType operatorType, const ExpressionPtr& leftValue, const ExpressionPtr& rightValue);

    const ExpressionOperatorType OperatorType;
    const ExpressionPtr& LeftValue;
    const ExpressionPtr& RightValue;
};

typedef SharedPtr<OperatorExpression> OperatorExpressionPtr;

class ExpressionTree
{
private:
    Data::DataRowPtr m_currentRow;
    Data::DataTablePtr m_measurements;

    ValueExpressionPtr Evaluate(const ExpressionPtr& expression, ExpressionValueType targetValueType = ExpressionValueType::Boolean) const;
    ValueExpressionPtr EvaluateUnary(const ExpressionPtr& expression) const;
    ValueExpressionPtr EvaluateColumn(const ExpressionPtr& expression) const;
    ValueExpressionPtr EvaluateInList(const ExpressionPtr& expression) const;
    ValueExpressionPtr EvaluateFunction(const ExpressionPtr& expression) const;
    ValueExpressionPtr EvaluateOperator(const ExpressionPtr& expression) const;
    
    template<typename T>
    static T ApplyIntegerUnaryOperation(const T& unaryValue, ExpressionUnaryType unaryOperation);
    
    template<typename T>
    static T ApplyFloatingPointUnaryOperation(const T& unaryValue, ExpressionUnaryType unaryOperation, ExpressionValueType unaryValueType);

    // Operation Value Type Selectors
    ExpressionValueType DeriveOperationValueType(ExpressionOperatorType operationType, ExpressionValueType leftValueType, ExpressionValueType rightValueType) const;
    ExpressionValueType DeriveArithmeticOperationValueType(ExpressionOperatorType operationType, ExpressionValueType leftValueType, ExpressionValueType rightValueType) const;
    ExpressionValueType DeriveIntegerOperationValueType(ExpressionOperatorType operationType, ExpressionValueType leftValueType, ExpressionValueType rightValueType) const;
    ExpressionValueType DeriveComparisonOperationValueType(ExpressionOperatorType operationType, ExpressionValueType leftValueType, ExpressionValueType rightValueType) const;
    ExpressionValueType DeriveBooleanOperationValueType(ExpressionOperatorType operationType, ExpressionValueType leftValueType, ExpressionValueType rightValueType) const;

    // Filter Expression Function Implementations
    static const ValueExpressionPtr& Coalesce(const ValueExpressionPtr& testValue, const ValueExpressionPtr& defaultValue);
    ValueExpressionPtr Convert(const ValueExpressionPtr& sourceValue, const ValueExpressionPtr& targetType) const;
    ValueExpressionPtr IIf(const ValueExpressionPtr& testValue, const ExpressionPtr& leftResultValue, const ExpressionPtr& rightResultValue) const;
    ValueExpressionPtr IsRegExMatch(const ValueExpressionPtr& regexValue, const ValueExpressionPtr& testValue) const;
    ValueExpressionPtr Len(const ValueExpressionPtr& sourceValue) const;
    ValueExpressionPtr RegExVal(const ValueExpressionPtr& regexValue, const ValueExpressionPtr& testValue) const;
    ValueExpressionPtr SubString(const ValueExpressionPtr& sourceValue, const ValueExpressionPtr& indexValue, const ValueExpressionPtr& lengthValue) const;
    ValueExpressionPtr Trim(const ValueExpressionPtr& sourceValue) const;

    // Filter Expression Operator Implementations
    ValueExpressionPtr Multiply(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionValueType valueType) const;
    ValueExpressionPtr Divide(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionValueType valueType) const;
    ValueExpressionPtr Modulus(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionValueType valueType) const;
    ValueExpressionPtr Add(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionValueType valueType) const;
    ValueExpressionPtr Subtract(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionValueType valueType) const;
    ValueExpressionPtr BitShiftLeft(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue) const;
    ValueExpressionPtr BitShiftRight(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue) const;
    ValueExpressionPtr BitwiseAnd(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionValueType valueType) const;
    ValueExpressionPtr BitwiseOr(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionValueType valueType) const;
    ValueExpressionPtr LessThan(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionValueType valueType) const;
    ValueExpressionPtr LessThanOrEqual(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionValueType valueType) const;
    ValueExpressionPtr GreaterThan(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionValueType valueType) const;
    ValueExpressionPtr GreaterThanOrEqual(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionValueType valueType) const;
    ValueExpressionPtr Equal(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionValueType valueType) const;
    ValueExpressionPtr NotEqual(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue, ExpressionValueType valueType) const;
    ValueExpressionPtr IsNull(const ValueExpressionPtr& leftValue) const;
    ValueExpressionPtr IsNotNull(const ValueExpressionPtr& leftValue) const;
    ValueExpressionPtr Like(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue) const;
    ValueExpressionPtr NotLike(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue) const;
    ValueExpressionPtr And(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue) const;
    ValueExpressionPtr Or(const ValueExpressionPtr& leftValue, const ValueExpressionPtr& rightValue) const;

    template<typename T>
    static T Multiply(const T& leftValue, const T& rightValue);

    template<typename T>
    static T Divide(const T& leftValue, const T& rightValue);

    template<typename T>
    static T Modulus(const T& leftValue, const T& rightValue);

    template<typename T>
    static T Add(const T& leftValue, const T& rightValue);

    template<typename T>
    static T Subtract(const T& leftValue, const T& rightValue);

    template<typename T>
    static T BitShiftLeft(const T& operandValue, int32_t shiftValue);

    template<typename T>
    static T BitShiftRight(const T& operandValue, int32_t shiftValue);

    template<typename T>
    static T BitwiseAnd(const T& leftValue, const T& rightValue);

    template<typename T>
    static T BitwiseOr(const T& leftValue, const T& rightValue);

    template<typename T>
    static bool LessThan(const T& leftValue, const T& rightValue);

    template<typename T>
    static bool LessThanOrEqual(const T& leftValue, const T& rightValue);

    template<typename T>
    static bool GreaterThan(const T& leftValue, const T& rightValue);

    template<typename T>
    static bool GreaterThanOrEqual(const T& leftValue, const T& rightValue);

    template<typename T>
    static bool Equal(const T& leftValue, const T& rightValue);

    template<typename T>
    static bool NotEqual(const T& leftValue, const T& rightValue);

    ValueExpressionPtr Convert(const ValueExpressionPtr& sourceValue, ExpressionValueType targetValueType) const;
    ValueExpressionPtr EvaluateRegEx(const std::string& functionName, const ValueExpressionPtr& regexValue, const ValueExpressionPtr& testValue, bool returnMatchedValue) const;
public:
    ExpressionTree(Data::DataTablePtr measurements);

    const Data::DataTablePtr& Measurements() const;
    int32_t TopLimit;
    std::vector<std::pair<Data::DataColumnPtr, bool>> OrderByTerms;

    ExpressionPtr Root = nullptr;

    ValueExpressionPtr Evaluate(const Data::DataRowPtr& row);

    static const ValueExpressionPtr True;
    static const ValueExpressionPtr False;
    static const ValueExpressionPtr EmptyString;
    static ValueExpressionPtr NullValue(ExpressionValueType targetValueType);
};

typedef SharedPtr<ExpressionTree> ExpressionTreePtr;

}}}

#endif