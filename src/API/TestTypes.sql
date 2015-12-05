USE [TestTypes]
GO

/****** Object:  Table [dbo].[TestTypeTable]    Script Date: 03/23/2012 19:21:40 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[TestTypeTable](
	[testint] [int] NOT NULL,
	[testbit] [bit] NOT NULL,
	[testbigint] [bigint] NOT NULL,
	[testbinary] [binary](50) NOT NULL,
	[testchar] [char](10) NOT NULL,
	[testdate] [date] NOT NULL,
	[testdatetime] [datetime] NOT NULL,
	[testdatetime2] [datetime2](7) NOT NULL,
	[testdatetimeoffset] [datetimeoffset](7) NOT NULL,
	[testdecimal] [decimal](18, 2) NOT NULL,
	[testfloat] [float] NOT NULL,
	[testgeography] [geography] NOT NULL,
	[testgeometry] [geometry] NOT NULL,
	[testheirarchyid] [hierarchyid] NOT NULL,
	[testimage] [image] NOT NULL,
	[testmoney] [money] NOT NULL,
	[testnchar] [nchar](10) NOT NULL,
	[testntext] [ntext] NOT NULL,
	[testnumeric] [numeric](18, 0) NOT NULL,
	[testnvarchar] [nvarchar](50) NOT NULL,
	[testnvarcharmax] [nvarchar](max) NOT NULL,
	[testreal] [real] NOT NULL,
	[testsmalldatetime] [smalldatetime] NOT NULL,
	[testsmallint] [smallint] NOT NULL,
	[testsmallmoney] [smallmoney] NOT NULL,
	[testsql_variant] [sql_variant] NOT NULL,
	[testtext] [text] NOT NULL,
	[testtime] [time](7) NOT NULL,
	[testtimestamp] [timestamp] NOT NULL,
	[testtinyint] [tinyint] NOT NULL,
	[testuniqueidentifier] [uniqueidentifier] NOT NULL,
	[testvarbinary] [varbinary](50) NOT NULL,
	[testvarbinarymax] [varbinary](max) NOT NULL,
	[testvarchar] [varchar](50) NOT NULL,
	[testvarcharmax] [varchar](max) NOT NULL,
	[testxml] [xml] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

INSERT INTO [TestTypes].[dbo].[TestTypeTable]
           ([testint]
           ,[testbit]
           ,[testbigint]
           ,[testbinary]
           ,[testchar]
           ,[testdate]
           ,[testdatetime]
           ,[testdatetime2]
           ,[testdatetimeoffset]
           ,[testdecimal]
           ,[testfloat]
           ,[testgeography]
           ,[testgeometry]
           ,[testheirarchyid]
           ,[testimage]
           ,[testmoney]
           ,[testnchar]
           ,[testntext]
           ,[testnumeric]
           ,[testnvarchar]
           ,[testnvarcharmax]
           ,[testreal]
           ,[testsmalldatetime]
           ,[testsmallint]
           ,[testsmallmoney]
           ,[testsql_variant]
           ,[testtext]
           ,[testtime]
           ,[testtinyint]
           ,[testuniqueidentifier]
           ,[testvarbinary]
           ,[testvarbinarymax]
           ,[testvarchar]
           ,[testvarcharmax]
           ,[testxml])
     VALUES
           (1234567890
           ,1
           ,11223344556677
           ,0xFF33
           ,'1234567890'
           ,GETDATE()
           ,GETDATE()
           ,GETDATE()
           ,GETUTCDATE()
           ,12324567.89
           ,1234567.89
           ,NULL
           ,NULL
           ,0x
           ,0xFF5544
           ,1234.567
           ,N'1234567890'
           ,N'jkajsfdjkjfjsaflkjfsakljf'
           ,12345678.99
           ,N'jsdjfksjafsjfsalfjk'
           ,N'sadfjdsajfksalfkls'
           ,1234567.8877
           ,GETDATE()
           ,1000
           ,100.00
           ,N'xxx'
           ,'test'
           ,'12:00'
           ,254
           ,NEWID()
           ,0xFF665544
           ,0xFF665544
           ,'dasdjakjdkls'
           ,'fkasdjlkas'
           ,N'XML'
           )
GO


