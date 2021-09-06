USE [master]
GO
CREATE DATABASE [DeviceStats];
GO
ALTER DATABASE [DeviceStats] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [DeviceStats] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [DeviceStats] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [DeviceStats] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [DeviceStats] SET ARITHABORT OFF 
GO
ALTER DATABASE [DeviceStats] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [DeviceStats] SET AUTO_CREATE_STATISTICS ON 
GO
ALTER DATABASE [DeviceStats] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [DeviceStats] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [DeviceStats] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [DeviceStats] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [DeviceStats] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [DeviceStats] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [DeviceStats] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [DeviceStats] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [DeviceStats] SET  ENABLE_BROKER 
GO
ALTER DATABASE [DeviceStats] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [DeviceStats] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [DeviceStats] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [DeviceStats] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [DeviceStats] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [DeviceStats] SET  READ_WRITE 
GO
ALTER DATABASE [DeviceStats] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [DeviceStats] SET  MULTI_USER 
GO
ALTER DATABASE [DeviceStats] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [DeviceStats] SET DB_CHAINING OFF 
GO
USE [DeviceStats]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DailyStats](
    [DeviceID] [int] NOT NULL,
    [Timestamp] [date] NOT NULL,
    [ReceivedCount] [int] NOT NULL,
    [DataErrorCount] [int] NOT NULL,
    [TimeErrorCount] [int] NOT NULL,
    [MinLatency] [int] NULL,
    [MaxLatency] [int] NULL,
    [AvgLatency] [int] NULL,
 CONSTRAINT [PK_DailyStats] PRIMARY KEY CLUSTERED 
(
    [DeviceID] ASC,
    [Timestamp] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Device](
    [ID] [int] IDENTITY(1,1) NOT NULL,
    [UniqueID] [uniqueidentifier] NOT NULL,
    [Name] [varchar](200) NULL,
    [NameWISP] [varchar](200) NULL,
    [NamePMU] [varchar](200) NULL,
    [NamePDC] [varchar](200) NULL,
    [SubstationID] [int] NULL,
    [Acronym] [varchar](200) NOT NULL,
    [ParentAcronym] [varchar](200) NULL,
    [Protocol] [varchar](200) NULL,
    [Longitude] [decimal](9, 6) NULL,
    [Latitude] [decimal](9, 6) NULL,
    [FramesPerSecond] [int] NOT NULL,
 CONSTRAINT [PK_Device] PRIMARY KEY CLUSTERED 
(
    [ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MinuteStats](
    [DeviceID] [int] NOT NULL,
    [Timestamp] [smalldatetime] NOT NULL,
    [ReceivedCount] [smallint] NOT NULL,
    [DataErrorCount] [smallint] NOT NULL,
    [TimeErrorCount] [smallint] NOT NULL,
    [MinLatency] [int] NULL,
    [MaxLatency] [int] NULL,
    [AvgLatency] [int] NULL,
 CONSTRAINT [PK_MinuteStats14] PRIMARY KEY CLUSTERED 
(
    [DeviceID] ASC,
    [Timestamp] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PMULines](
    [NameEMS] [nvarchar](50) NOT NULL,
    [NameEMSClean] [nvarchar](50) NULL,
    [NameLong] [nvarchar](100) NOT NULL,
    [FromSubstationID] [int] NULL,
    [ToSubstationID] [int] NULL,
    [NominalVoltage] [float] NOT NULL
) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Substation](
    [ID] [int] IDENTITY(1,1) NOT NULL,
    [Name] [varchar](200) NOT NULL,
    [NameShort] [varchar](200) NOT NULL,
    [NameEMS] [varchar](200) NOT NULL,
    [PMUPrefix] [varchar](200) NOT NULL,
    [Longitude] [decimal](9, 6) NOT NULL,
    [Latitude] [decimal](9, 6) NOT NULL,
    [HasPMUs] [bit] NOT NULL,
 CONSTRAINT [PK_Substation] PRIMARY KEY CLUSTERED 
(
    [ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Device] ADD  CONSTRAINT [DF__Device__FramesPerSec]  DEFAULT ((30)) FOR [FramesPerSecond]
GO
ALTER TABLE [dbo].[MinuteStats] ADD  CONSTRAINT [DF_MinuteStats_DataErrorCount]  DEFAULT ((0)) FOR [DataErrorCount]

GO
ALTER TABLE [dbo].[MinuteStats] ADD  CONSTRAINT [DF_MinuteStats_TimeErrorCount]  DEFAULT ((0)) FOR [TimeErrorCount]

GO
ALTER TABLE [dbo].[DailyStats]  WITH CHECK ADD  CONSTRAINT [FK_DailyStats_Device] FOREIGN KEY([DeviceID])
REFERENCES [dbo].[Device] ([ID])
GO

ALTER TABLE [dbo].[DailyStats] CHECK CONSTRAINT [FK_DailyStats_Device]
GO

ALTER TABLE [dbo].[Device]  WITH CHECK ADD  CONSTRAINT [FK_Device_Substation] FOREIGN KEY([SubstationID])
REFERENCES [dbo].[Substation] ([ID])
GO
ALTER TABLE [dbo].[Device] CHECK CONSTRAINT [FK_Device_Substation]
GO

ALTER TABLE [dbo].[PMULines]  WITH CHECK ADD  CONSTRAINT [FK_PMULines__From_Substation] FOREIGN KEY([FromSubstationID])
REFERENCES [dbo].[Substation] ([ID])
GO
ALTER TABLE [dbo].[PMULines] CHECK CONSTRAINT [FK_PMULines__From_Substation]
GO

ALTER TABLE [dbo].[PMULines]  WITH CHECK ADD  CONSTRAINT [FK_PMULines_To_Substation] FOREIGN KEY([ToSubstationID])
REFERENCES [dbo].[Substation] ([ID])
GO

ALTER TABLE [dbo].[PMULines] CHECK CONSTRAINT [FK_PMULines_To_Substation]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/********************************/
/* STEP 3                       */
/* Create new stored procedures */
/********************************/
CREATE PROCEDURE [dbo].[InsertMissingMinuteStats]
AS
BEGIN
    DECLARE @StartTimestamp SMALLDATETIME;
    DECLARE @EndTimestamp SMALLDATETIME;

    SELECT @StartTimeStamp = MIN(Timestamp) FROM MinuteStats;
    SELECT @EndTimestamp = DATEADD(MINUTE, -2, GETDATE());

    ;WITH MinutesCTE (Timestamp) AS
    (
        SELECT @StartTimestamp UNION ALL
        SELECT DateAdd(MINUTE, 1, Timestamp)
        FROM   MinutesCTE WHERE Timestamp < @EndTimestamp
    )
    INSERT MinuteStats (DeviceID, Timestamp, ReceivedCount, DataErrorCount, TimeErrorCount)
    SELECT d.ID, m.Timestamp, 0, 0, 0
    FROM MinutesCTE m
    JOIN Device d ON 1=1
    LEFT OUTER JOIN MinuteStats mm ON mm.DeviceID=d.ID AND mm.Timestamp=m.Timestamp
    WHERE m.Timestamp < @EndTimestamp AND mm.DeviceID IS NULL
    ORDER BY 1,2
    OPTION  (MaxRecursion 0)
END
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[QueryPDCDailyStats] (@PDC varchar(20), @FromMS BIGINT, @ToMS BIGINT)
AS
BEGIN
    DECLARE @DeviceID int;

    SELECT @DeviceID=MIN(ID) FROM Device WHERE COALESCE(ParentAcronym, Acronym) = @PDC;

    SELECT DATEDIFF(SECOND, '1970-01-01', [Timestamp]) AS time
        , (ds.ReceivedCount / (d.FramesPerSecond * 86400.0) * 100.0) as value
        , @PDC as metric
    FROM  [DailyStats] ds
    JOIN [Device] d on d.ID=ds.DeviceID
    WHERE [DeviceID] = @DeviceID
      AND DATEDIFF(SECOND, '1970-01-01', [Timestamp]) BETWEEN @FromMS/1000 AND @ToMS/1000
    ORDER BY [Timestamp];
END
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[QueryPDCMinuteStats] (@PDC varchar(20), @From DATETIME, @To DATETIME)
AS
BEGIN
    DECLARE @DeviceID int;

    SELECT @DeviceID=MIN(ID) FROM Device WHERE COALESCE(ParentAcronym,Acronym) = @PDC;

    SELECT DATEDIFF(second, '1970-01-01', [Timestamp]) AS time, [ReceivedCount] as value, @PDC as metric
    FROM  [MinuteStats]
    WHERE [DeviceID] = @DeviceID
      AND [Timestamp] BETWEEN @From AND @To
    ORDER BY [Timestamp];
END
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[QueryPDCNames] 
AS
BEGIN
    SET NOCOUNT ON;

    -- Find all PDCs with valid DailyStats data in the last month
    SELECT DISTINCT COALESCE(ParentAcronym, Acronym) PDC
    FROM Device d
    JOIN DailyStats s on s.DeviceID=d.ID
    WHERE [Timestamp]>DATEADD(MONTH, -1, CONVERT(date,GETUTCDATE()))
    ORDER BY 1
END
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[QueryPMUDataErrors] (@ToMS BIGINT, @NumMinutes INT, @Filter varchar(max), @Substation varchar(max))
AS
BEGIN
    DECLARE @Last SMALLDATETIME; 
    DECLARE @To SMALLDATETIME;
    DECLARE @From SMALLDATETIME;

    SELECT @Last=MAX(Timestamp) FROM MinuteStats WHERE AvgLatency IS NOT NULL;
    SELECT @To=DATEADD(s, @ToMS/1000, '1970-01-01');

    IF (@To > @Last) SELECT @To=@LAST

    SELECT @From=DATEADD(MINUTE, -@NumMinutes, @To)

    SELECT COALESCE(Acronym, ParentAcronym) PMU,
        MAX(Timestamp) LastTimestamp,
        AVG(ReceivedCount) ReceivedCount,
        AVG(DataErrorCount) DataErrorCount,
        AVG(TimeErrorCount) TimeErrorCount,
        MAX(s.Name) Substation
        FROM MinuteStats ms
        JOIN Device d on d.ID=ms.DeviceID
        JOIN Substation s on s.ID=d.SubstationID
        WHERE [Timestamp] BETWEEN @From AND DATEADD(MINUTE, -1, @To)
        AND (@Filter='all' OR DataErrorCount>0)
        AND (@Substation='' OR s.Name=@Substation)
        GROUP BY COALESCE(Acronym, ParentAcronym)
        ORDER BY 1, 2
END
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[QuerySubstationNames] 
AS
BEGIN
    SET NOCOUNT ON;

    -- Find all Substations with valid DailyStats data in the last month
    SELECT DISTINCT s.Name AS Substation --COALESCE(ParentAcronym, Acronym) PDC
    FROM Device d
    JOIN DailyStats ds on ds.DeviceID=d.ID
    JOIN Substation s on s.ID=d.SubstationID
    WHERE [Timestamp]>DATEADD(MONTH, -1, CONVERT(date,GETUTCDATE()))
    ORDER BY 1
END
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[UpdateDailyDeviceStats]
AS
BEGIN
    SET NOCOUNT ON;
    SET ANSI_WARNINGS OFF

    DELETE FROM dbo.MinuteStats WHERE Timestamp < DATEADD(YEAR, -1, GETDATE());

    -- Insert dummy (empty) missing MinuteStats records for any active devices on days with zero records in MinuteStats
    EXEC [dbo].[InsertMissingMinuteStats];

    INSERT INTO DailyStats (DeviceID, Timestamp, ReceivedCount, DataErrorCount, TimeErrorCount, MaxLatency, MinLatency, AvgLatency) 
        SELECT m.DeviceID, 
        CONVERT(date, m.Timestamp), 
        SUM(m.[ReceivedCount]), 
        SUM(m.[DataErrorCount]),
        SUM(m.[TimeErrorCount]),
        MAX(m.[MaxLatency]),
        MIN(m.[MinLatency]),
        CAST(AVG(CAST(m.[AvgLatency] AS FLOAT)) AS INT)
        FROM MinuteStats m
        LEFT OUTER JOIN DailyStats dd on dd.DeviceID=m.DeviceID AND dd.Timestamp=CONVERT(date, m.Timestamp)
        WHERE CONVERT(date, m.Timestamp) < CONVERT(date, getdate()) AND dd.DeviceID IS NULL
        GROUP BY m.DeviceID, CONVERT(date, m.Timestamp);

    SET ANSI_WARNINGS ON
END
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[DailyStatsDetail]
AS
SELECT dbo.Device.Acronym AS PMU, dbo.Device.ParentAcronym AS PDC, dbo.DailyStats.ReceivedCount, dbo.DailyStats.DataErrorCount, dbo.DailyStats.TimeErrorCount
FROM   dbo.DailyStats INNER JOIN
       dbo.Device ON dbo.DailyStats.DeviceID = dbo.Device.ID
WHERE  dbo.DailyStats.Timestamp = (SELECT MAX(Timestamp) AS MaxTime FROM dbo.DailyStats)
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[MinuteStatsDetail]
AS
SELECT D.Acronym AS PMU, D.ParentAcronym AS PDC, MS.ReceivedCount AS Received, MS.DataErrorCount AS DataErrors, MS.TimeErrorCount AS TimeErrors
FROM   dbo.MinuteStats AS MS INNER JOIN
       dbo.Device AS D ON MS.DeviceID = D.ID
WHERE MS.Timestamp = (SELECT MAX(Timestamp) AS MaxTime FROM dbo.MinuteStats)
