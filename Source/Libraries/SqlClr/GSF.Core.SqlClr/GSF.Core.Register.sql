-- TODO: Set proper target database name
ALTER DATABASE [MyDatabase] SET TRUSTWORTHY ON
EXEC sp_configure 'clr enabled', 1
RECONFIGURE
GO

CREATE ASSEMBLY [GSF.Core]
FROM 'GSF.Core.SqlClr.dll'
WITH PERMISSION_SET = UNSAFE;
GO

CREATE TYPE COMPLEX
EXTERNAL NAME [GSF.Core].[GSF.Core.SqlClr.ComplexNumber];
GO