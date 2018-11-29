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
 : filterStatement
 | identifierStatement
 ;

identifierStatement
 : GUID_LITERAL
 | MEASUREMENT_KEY_LITERAL
 | POINT_TAG_LITERAL
 ;

filterStatement
 : K_FILTER ( K_TOP INTEGER_LITERAL )? tableName K_WHERE expression ( K_ORDER K_BY orderingTerm ( ',' orderingTerm )* )?
 ;
 
orderingTerm
 : columnName ( K_ASC | K_DESC )?
 ;

/*
    Filter expressions understand the following binary operators,
    in order from highest to lowest precedence:

    *    /    %
    +    -
    <<   >>   &    |
    <    <=   >    >=
    =    ==   !=   <>   IS NULL   IN   LIKE
    AND
    OR
*/
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

literalValue
 : NUMERIC_LITERAL
 | STRING_LITERAL
 | DATETIME_LITERAL
 | K_NULL
 ;

unaryOperator
 : '-'
 | '+'
 | '~'
 | K_NOT
 ;

keyword
 : K_AND        // Boolean operator
 | K_ASC        // FILTER expression keyword (part of optional ORDER BY expression)
 | K_BY         // FILTER expression keyword (part of optional ORDER BY expression)
 | K_CONVERT    // Function
 | K_DESC       // FILTER expression keyword (part of optional ORDER BY expression)
 | K_FILTER     // FILTER expression keyword
 | K_IIF        // Function
 | K_IN         // IN expression keyword
 | K_IS         // IS expression keyword
 | K_ISNULL     // Function
 | K_LEN        // Function
 | K_LIKE       // LIKE expression keyword
 | K_NOT        // Boolean operator
 | K_NULL       // NULL operand
 | K_OR         // Boolean operator
 | K_ORDER      // FILTER expression keyword (part of optional ORDER BY expression)
 | K_REGEXP     // Function
 | K_SUBSTRING  // Function
 | K_TOP        // FILTER expression keyword
 | K_TRIM       // Function
 | K_WHERE      // FILTER expression keyword
 ;

functionName
 : K_CONVERT
 | K_IIF
 | K_LEN
 | K_ISNULL
 | K_REGEXP
 | K_SUBSTRING
 | K_TRIM
 ;

databaseName
 : IDENTIFIER
 ;

tableName 
 : IDENTIFIER
 ;

columnName 
 : IDENTIFIER
 ;

// Keywords
K_AND : A N D;
K_ASC : A S C;
K_BY : B Y;
K_CONVERT : C O N V E R T;
K_DESC : D E S C;
K_FILTER : F I L T E R;
K_IIF : I I F;
K_IN : I N;
K_IS : I S;
K_ISNULL : I S N U L L;
K_LEN: L E N;
K_LIKE : L I K E;
K_NOT : N O T;
K_NULL : N U L L;
K_OR : O R;
K_ORDER : O R D E R;
K_REGEXP : R E G E X P;
K_SUBSTRING: S U B S T R I N G;
K_TOP : T O P;
K_TRIM: T R I M;
K_WHERE : W H E R E;

IDENTIFIER
 : '"' (  ~'"' | '""' )* '"'
 | '[' ~']'* ']'
 | [a-zA-Z_] [a-zA-Z_0-9]* // TODO check: needs more chars in set
 ;

INTEGER_LITERAL
 : DIGIT+
 ;

NUMERIC_LITERAL
 : DIGIT+ ( '.' DIGIT* )? ( E [-+]? DIGIT+ )?
 | '.' DIGIT+ ( E [-+]? DIGIT+ )?
 ;

STRING_LITERAL
 : '\'' ( ~'\'' | '\'\'' )* '\''
 ;

DATETIME_LITERAL
 : '#' ( ~'#' )+ '#'
 ;

GUID_VALUE
 : HEX_DIGIT+ '-' HEX_DIGIT+ '-' HEX_DIGIT+ '-' HEX_DIGIT+ '-' HEX_DIGIT+
 ;

GUID_LITERAL
 : '{' GUID_VALUE '}'
 | '\'' GUID_VALUE '\''
 ;

MEASUREMENT_KEY_LITERAL
 : ACRONYM_DIGIT+ ':' DIGIT+
 ;

POINT_TAG_LITERAL
 : ACRONYM_DIGIT+
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