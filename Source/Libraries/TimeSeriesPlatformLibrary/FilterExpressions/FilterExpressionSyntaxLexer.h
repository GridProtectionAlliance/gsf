
// Generated from FilterExpressionSyntax.g4 by ANTLR 4.7.1

#pragma once


#include "antlr4-runtime.h"




class  FilterExpressionSyntaxLexer : public antlr4::Lexer {
public:
  enum {
    T__0 = 1, T__1 = 2, T__2 = 3, T__3 = 4, T__4 = 5, T__5 = 6, T__6 = 7, 
    T__7 = 8, T__8 = 9, T__9 = 10, T__10 = 11, T__11 = 12, T__12 = 13, T__13 = 14, 
    T__14 = 15, T__15 = 16, T__16 = 17, T__17 = 18, T__18 = 19, T__19 = 20, 
    T__20 = 21, T__21 = 22, T__22 = 23, T__23 = 24, T__24 = 25, T__25 = 26, 
    T__26 = 27, T__27 = 28, K_ABS = 29, K_AND = 30, K_ASC = 31, K_BINARY = 32, 
    K_BY = 33, K_CEILING = 34, K_COALESCE = 35, K_CONVERT = 36, K_CONTAINS = 37, 
    K_DATEADD = 38, K_DATEDIFF = 39, K_DATEPART = 40, K_DESC = 41, K_ENDSWITH = 42, 
    K_FILTER = 43, K_FLOOR = 44, K_IIF = 45, K_IN = 46, K_INDEXOF = 47, 
    K_IS = 48, K_ISDATE = 49, K_ISINTEGER = 50, K_ISGUID = 51, K_ISNULL = 52, 
    K_ISNUMERIC = 53, K_LASTINDEXOF = 54, K_LEN = 55, K_LIKE = 56, K_LOWER = 57, 
    K_MAXOF = 58, K_MINOF = 59, K_NOT = 60, K_NOW = 61, K_NTHINDEXOF = 62, 
    K_NULL = 63, K_OR = 64, K_ORDER = 65, K_POWER = 66, K_REGEXMATCH = 67, 
    K_REGEXVAL = 68, K_REPLACE = 69, K_REVERSE = 70, K_ROUND = 71, K_SQRT = 72, 
    K_SPLIT = 73, K_STARTSWITH = 74, K_STRCOUNT = 75, K_STRCMP = 76, K_SUBSTR = 77, 
    K_TOP = 78, K_TRIM = 79, K_TRIMLEFT = 80, K_TRIMRIGHT = 81, K_UPPER = 82, 
    K_UTCNOW = 83, K_WHERE = 84, K_XOR = 85, BOOLEAN_LITERAL = 86, IDENTIFIER = 87, 
    INTEGER_LITERAL = 88, NUMERIC_LITERAL = 89, GUID_LITERAL = 90, MEASUREMENT_KEY_LITERAL = 91, 
    POINT_TAG_LITERAL = 92, STRING_LITERAL = 93, DATETIME_LITERAL = 94, 
    SINGLE_LINE_COMMENT = 95, MULTILINE_COMMENT = 96, SPACES = 97, UNEXPECTED_CHAR = 98
  };

  FilterExpressionSyntaxLexer(antlr4::CharStream *input);
  ~FilterExpressionSyntaxLexer();

  virtual std::string getGrammarFileName() const override;
  virtual const std::vector<std::string>& getRuleNames() const override;

  virtual const std::vector<std::string>& getChannelNames() const override;
  virtual const std::vector<std::string>& getModeNames() const override;
  virtual const std::vector<std::string>& getTokenNames() const override; // deprecated, use vocabulary instead
  virtual antlr4::dfa::Vocabulary& getVocabulary() const override;

  virtual const std::vector<uint16_t> getSerializedATN() const override;
  virtual const antlr4::atn::ATN& getATN() const override;

private:
  static std::vector<antlr4::dfa::DFA> _decisionToDFA;
  static antlr4::atn::PredictionContextCache _sharedContextCache;
  static std::vector<std::string> _ruleNames;
  static std::vector<std::string> _tokenNames;
  static std::vector<std::string> _channelNames;
  static std::vector<std::string> _modeNames;

  static std::vector<std::string> _literalNames;
  static std::vector<std::string> _symbolicNames;
  static antlr4::dfa::Vocabulary _vocabulary;
  static antlr4::atn::ATN _atn;
  static std::vector<uint16_t> _serializedATN;


  // Individual action functions triggered by action() above.

  // Individual semantic predicate functions triggered by sempred() above.

  struct Initializer {
    Initializer();
  };
  static Initializer _init;
};

