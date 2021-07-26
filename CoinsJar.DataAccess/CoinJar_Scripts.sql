USE [CoinJar]
GO
/****** Object:  Table [dbo].[Coins]    Script Date: 2021-07-26 03:34:11 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Coins](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Amount] [decimal](18, 0) NOT NULL,
	[Volume] [int] NOT NULL,
 CONSTRAINT [PK_Coin] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[Coins] ON 
GO
INSERT [dbo].[Coins] ([Id], [Amount], [Volume]) VALUES (1, CAST(0 AS Decimal(18, 0)), 0)
GO
INSERT [dbo].[Coins] ([Id], [Amount], [Volume]) VALUES (2, CAST(0 AS Decimal(18, 0)), 0)
GO
INSERT [dbo].[Coins] ([Id], [Amount], [Volume]) VALUES (3, CAST(0 AS Decimal(18, 0)), 0)
GO
INSERT [dbo].[Coins] ([Id], [Amount], [Volume]) VALUES (4, CAST(0 AS Decimal(18, 0)), 0)
GO
INSERT [dbo].[Coins] ([Id], [Amount], [Volume]) VALUES (5, CAST(0 AS Decimal(18, 0)), 0)
GO
SET IDENTITY_INSERT [dbo].[Coins] OFF
GO
