//******************************************************************************************************
//  FilterExpressionSyntax.g4 - Gbtc
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
//  10/27/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

grammar FilterExpressionSyntax;

parse
 : ( filterExpressionStatementList | error ) EOF
 ;

error
 : UNEXPECTED_CHAR 
   { 
     throw RuntimeException("Unexpected character: " + $UNEXPECTED_CHAR.text); 
   }
 ;

filterExpressionStatementList
 : ';'* filterExpressionStatement ( ';'+ filterExpressionStatement )* ';'*
 ;

filterExpressionStatement
 : identifierStatement
 | filterStatement
 | expression
 ;

identifierStatement
 : GUID_LITERAL
 | MEASUREMENT_KEY_LITERAL
 | POINT_TAG_LITERAL
 ;

filterStatement
 : K_FILTER ( K_TOP topLimit )? tableName K_WHERE expression ( K_ORDER K_BY orderingTerm ( ',' orderingTerm )* )?
 ;

topLimit
 : ( '-' | '+' )? INTEGER_LITERAL
 ;

orderingTerm
 : exactMatchModifier? orderByColumnName ( K_ASC | K_DESC )?
 ;

expressionList
 : expression ( ',' expression )*
 ;

/*
    Expressions understand the following binary operators,
    in order from highest to lowest precedence:

    *    /    %
    +    -
    <<   >>   &    |
    <    <=   >    >=
    =    ==   ===  !=  !==  <>
    IS NULL   IN   LIKE
    AND  &&
    OR   ||
*/
expression
 : notOperator expression
 | expression logicalOperator expression
 | predicateExpression
 ;

predicateExpression
 : predicateExpression notOperator? K_IN exactMatchModifier? '(' expressionList ')'
 | predicateExpression K_IS notOperator? K_NULL
 | predicateExpression comparisonOperator predicateExpression
 | predicateExpression notOperator? K_LIKE exactMatchModifier? predicateExpression
 | valueExpression
 ;

valueExpression
 : literalValue
 | columnName
 | functionExpression
 | unaryOperator valueExpression
 | '(' expression ')'
 | valueExpression mathOperator valueExpression
 | valueExpression bitwiseOperator valueExpression
 ;

notOperator
 : K_NOT
 | '!'
 ;

unaryOperator
 : '-' | '+'
 | '~' | '!' | K_NOT
 ;

exactMatchModifier
 : K_BINARY
 | '==='
 ;

// === and !== are for case sensitive string match
comparisonOperator
 : '<' | '<=' | '>' | '>='
 | '=' | '==' | '==='
 | '!='| '!==' | '<>'
 ;

logicalOperator
 : K_AND | '&&'
 | K_OR | '||'
 ;

bitwiseOperator
 : '<<' | '>>'
 | '&' | '|'
 | K_XOR | '^'
 ;

mathOperator
 : '*' | '/' | '%'
 | '+' | '-'
 ;

functionName
 : K_ABS
 | K_CEILING
 | K_COALESCE
 | K_CONVERT
 | K_CONTAINS
 | K_DATEADD
 | K_DATEDIFF
 | K_DATEPART
 | K_ENDSWITH
 | K_FLOOR
 | K_IIF
 | K_INDEXOF
 | K_ISDATE
 | K_ISINTEGER
 | K_ISGUID
 | K_ISNULL
 | K_ISNUMERIC
 | K_LASTINDEXOF
 | K_LEN
 | K_LOWER
 | K_MAXOF
 | K_MINOF
 | K_NOW
 | K_NTHINDEXOF
 | K_POWER
 | K_REGEXMATCH
 | K_REGEXVAL
 | K_REPLACE
 | K_REVERSE
 | K_ROUND
 | K_SPLIT
 | K_SQRT
 | K_STARTSWITH
 | K_STRCOUNT
 | K_STRCMP
 | K_SUBSTR
 | K_TRIM
 | K_TRIMLEFT
 | K_TRIMRIGHT
 | K_UPPER
 | K_UTCNOW
 ;

functionExpression
 : functionName '(' expressionList? ')'
 ;

literalValue
 : INTEGER_LITERAL
 | NUMERIC_LITERAL
 | STRING_LITERAL
 | DATETIME_LITERAL
 | GUID_LITERAL
 | BOOLEAN_LITERAL
 | K_NULL
 ;

tableName 
 : IDENTIFIER
 ;

columnName 
 : IDENTIFIER
 ;

orderByColumnName 
 : IDENTIFIER
 ;

// Terminals for keywords should come before terminals with pattern expressions

// Keywords
K_ABS: A B S;
K_AND : A N D;
K_ASC : A S C;
K_BINARY : B I N A R Y;
K_BY : B Y;
K_CEILING : C E I L I N G;
K_COALESCE : C O A L E S C E;
K_CONVERT : C O N V E R T;
K_CONTAINS : C O N T A I N S;
K_DATEADD : D A T E A D D;
K_DATEDIFF : D A T E D I F F;
K_DATEPART : D A T E P A R T;
K_DESC : D E S C;
K_ENDSWITH : E N D S W I T H;
K_FILTER : F I L T E R;
K_FLOOR : F L O O R;
K_IIF : I I F;
K_IN : I N;
K_INDEXOF : I N D E X O F;
K_IS : I S;
K_ISDATE : I S D A T E;
K_ISINTEGER : I S I N T E G E R;
K_ISGUID : I S G U I D;
K_ISNULL : I S N U L L;
K_ISNUMERIC : I S N U M E R I C;
K_LASTINDEXOF : L A S T I N D E X O F;
K_LEN : L E N;
K_LIKE : L I K E;
K_LOWER : L O W E R;
K_MAXOF : M A X O F;
K_MINOF : M I N O F;
K_NOT : N O T;
K_NOW : N O W;
K_NTHINDEXOF : N T H I N D E X O F;
K_NULL : N U L L;
K_OR : O R;
K_ORDER : O R D E R;
K_POWER : P O W E R;
K_REGEXMATCH : R E G E X M A T C H;
K_REGEXVAL : R E G E X V A L;
K_REPLACE : R E P L A C E;
K_REVERSE : R E V E R S E;
K_ROUND : R O U N D;
K_SQRT : S Q R T;
K_SPLIT : S P L I T;
K_STARTSWITH : S T A R T S W I T H;
K_STRCOUNT : S T R C O U N T;
K_STRCMP: S T R C M P;
K_SUBSTR: S U B S T R;
K_TOP : T O P;
K_TRIM : T R I M;
K_TRIMLEFT : T R I M L E F T;
K_TRIMRIGHT : T R I M R I G H T;
K_UPPER : U P P E R;
K_UTCNOW : U T C N O W;
K_WHERE : W H E R E;
K_XOR : X O R;

BOOLEAN_LITERAL
  : T R U E
  | F A L S E
  ;

IDENTIFIER
 : '`' ( ~'`' )+ '`'
 | '[' ( ~']' )+ ']'
 | [a-zA-Z_] [a-zA-Z_0-9]* // TODO check: needs more chars in set
 ;

INTEGER_LITERAL
 : DIGIT+
 | '0' X HEX_DIGIT+
 ;

NUMERIC_LITERAL
 : DIGIT+ ( '.' DIGIT* )? ( E [-+]? DIGIT+ )?
 | '.' DIGIT+ ( E [-+]? DIGIT+ )?
 ;

GUID_LITERAL
 : '\'' GUID_VALUE '\''
 | '{' GUID_VALUE '}'
 | GUID_VALUE
 ;

MEASUREMENT_KEY_LITERAL
 : ACRONYM_DIGIT+ ':' DIGIT+
 ;

POINT_TAG_LITERAL
 : '"' ACRONYM_DIGIT+ '"'
 ;

STRING_LITERAL
 : '\'' ( ~'\'' | '\'\'' )* '\''
 ;

DATETIME_LITERAL
 : '#' ( ~'#' )+ '#'
 ;

SINGLE_LINE_COMMENT
 : '--' ~[\r\n]* -> channel(HIDDEN)
 ;

MULTILINE_COMMENT
 : '/*' .*? ( '*/' | EOF ) -> channel(HIDDEN)
 ;

SPACES
 : [ \u000B\t\r\n] -> channel(HIDDEN)
 ;

UNEXPECTED_CHAR
 : .
 ;

fragment DIGIT : [0-9];
fragment HEX_DIGIT : [0-9a-fA-F];
fragment ACRONYM_DIGIT : ( [a-zA-Z0-9] | '-' | '!' | '_' | '.' | '@' | '#' | '$' );

fragment GUID_VALUE
 : HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT '-'?
   HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT '-'?
   HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT '-'?
   HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT '-'?
   HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT
 ;

fragment A : [aA];
fragment B : [bB];
fragment C : [cC];
fragment D : [dD];
fragment E : [eE];
fragment F : [fF];
fragment G : [gG];
fragment H : [hH];
fragment I : [iI];
fragment J : [jJ];
fragment K : [kK];
fragment L : [lL];
fragment M : [mM];
fragment N : [nN];
fragment O : [oO];
fragment P : [pP];
fragment Q : [qQ];
fragment R : [rR];
fragment S : [sS];
fragment T : [tT];
fragment U : [uU];
fragment V : [vV];
fragment W : [wW];
fragment X : [xX];
fragment Y : [yY];
fragment Z : [zZ];