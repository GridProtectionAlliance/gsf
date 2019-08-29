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

CREATE TABLE [dbo].[Device](
    [ID] [int] IDENTITY(1,1) NOT NULL,
    [UniqueID] [uniqueidentifier] NOT NULL,
    [Acronym] [varchar](200) NOT NULL,
    [Name] [varchar](200) NULL,
    [ParentAcronym] [varchar](200) NULL,
    [Protocol] [varchar](200) NULL,
    [Longitude] [decimal](9, 6) NULL,
    [Latitude] [decimal](9, 6) NULL,
    [FramesPerSecond] [int] NOT NULL DEFAULT (30)
    CONSTRAINT [PK_Device] PRIMARY KEY CLUSTERED ( [ID] ASC )
    WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
)
ON [PRIMARY]

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[MinuteStats](
    [ID] [int] IDENTITY(1,1) NOT NULL,
    [DeviceID] [int] NOT NULL,
    [Timestamp] [datetime] NOT NULL DEFAULT (getutcdate()),
    [ReceivedCount] [bigint] NOT NULL,
    [DataErrorCount] [bigint] NOT NULL,
    [TimeErrorCount] [bigint] NOT NULL,
	[MinLatency] [int] NOT NULL,
	[MaxLatency] [int] NOT NULL,
	[AvgLatency] [int] NOT NULL
    CONSTRAINT [PK_MinuteStats] PRIMARY KEY CLUSTERED ( [ID] ASC )
    WITH ( PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
)
ON [PRIMARY]
GO

ALTER TABLE [dbo].[MinuteStats] WITH CHECK ADD CONSTRAINT [FK_MinuteStats_Device] FOREIGN KEY([DeviceID])
REFERENCES [dbo].[Device] ([ID])
GO

ALTER TABLE [dbo].[MinuteStats] WITH CHECK ADD CONSTRAINT [UC_MinuteStats_DeviceID_Timestamp] UNIQUE ([DeviceID], [Timestamp])
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[DailyStats](
    [ID] [int] IDENTITY(1,1) NOT NULL,
    [DeviceID] [int] NOT NULL,
    [Timestamp] [datetime] NOT NULL DEFAULT (getutcdate()),
    [ReceivedCount] [bigint] NOT NULL DEFAULT (0),
    [DataErrorCount] [bigint] NOT NULL,
    [TimeErrorCount] [bigint] NOT NULL,
	[MinLatency] [int] NOT NULL,
	[MaxLatency] [int] NOT NULL,
	[AvgLatency] [int] NOT NULL
    CONSTRAINT [PK_DailyStats] PRIMARY KEY CLUSTERED ( [ID] ASC )
    WITH ( PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
)
ON [PRIMARY]
GO

ALTER TABLE [dbo].[DailyStats]  WITH CHECK ADD CONSTRAINT [FK_DailyStats_Device] FOREIGN KEY([DeviceID])
REFERENCES [dbo].[Device] ([ID])
GO

ALTER TABLE [dbo].[DailyStats] WITH CHECK ADD CONSTRAINT [UC_DailyStats_DeviceID_Timestamp] UNIQUE ([DeviceID], [Timestamp])
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[MinuteStatsDetail]
AS
SELECT MS.ID AS ID,
       D.Acronym,
	   D.ParentAcronym,
	   MS.Timestamp,
	   MS.ReceivedCount,
       (MS.ReceivedCount / (D.FramesPerSecond * 60.0) * 100.0) AS Completeness,
	   MS.DataErrorCount,
	   MS.TimeErrorCount,
	   MS.MinLatency,
	   MS.MaxLatency,
	   MS.AvgLatency
FROM [DeviceStats].[dbo].[MinuteStats] AS MS INNER JOIN
[DeviceStats].[dbo].[Device] D ON MS.DeviceID = D.ID
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[DailyStatsDetail]
AS
SELECT DS.ID AS ID,
       D.Acronym,
	   D.ParentAcronym,
	   DS.Timestamp,
	   DS.ReceivedCount,
	   (DS.ReceivedCount / (D.FramesPerSecond * 86400.0) * 100.0) AS Completeness,
	   DS.DataErrorCount,
	   DS.TimeErrorCount,
	   DS.MinLatency,
	   DS.MaxLatency,
	   DS.AvgLatency
FROM [DeviceStats].[dbo].[DailyStats] AS DS INNER JOIN
[DeviceStats].[dbo].[Device] D ON DS.DeviceID = D.ID