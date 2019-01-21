@echo off
echo Compiling grammar...
java -jar antlr-4.7.1-complete.jar -Dlanguage=Cpp FilterExpressionSyntax.g4
echo Finished.
pause