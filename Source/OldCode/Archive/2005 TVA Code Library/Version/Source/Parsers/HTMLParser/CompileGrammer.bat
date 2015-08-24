@ECHO OFF
ECHO Rebuilding HTML Parser
CALL JavaCC HTMLParser.jj
ECHO Generating BNF docs for HTML Parser
CALL JJDoc HTMLParser.jj
PAUSE