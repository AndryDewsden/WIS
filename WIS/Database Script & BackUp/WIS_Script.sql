USE [master]
GO
/****** Object:  Database [WIS_database]    Script Date: 18.06.2025 10:58:11 ******/
CREATE DATABASE [WIS_database]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'WIS_database', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\WIS_database.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'WIS_database_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\WIS_database_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT
GO
ALTER DATABASE [WIS_database] SET COMPATIBILITY_LEVEL = 150
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [WIS_database].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [WIS_database] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [WIS_database] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [WIS_database] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [WIS_database] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [WIS_database] SET ARITHABORT OFF 
GO
ALTER DATABASE [WIS_database] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [WIS_database] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [WIS_database] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [WIS_database] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [WIS_database] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [WIS_database] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [WIS_database] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [WIS_database] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [WIS_database] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [WIS_database] SET  DISABLE_BROKER 
GO
ALTER DATABASE [WIS_database] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [WIS_database] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [WIS_database] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [WIS_database] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [WIS_database] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [WIS_database] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [WIS_database] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [WIS_database] SET RECOVERY FULL 
GO
ALTER DATABASE [WIS_database] SET  MULTI_USER 
GO
ALTER DATABASE [WIS_database] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [WIS_database] SET DB_CHAINING OFF 
GO
ALTER DATABASE [WIS_database] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [WIS_database] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [WIS_database] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [WIS_database] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
EXEC sys.sp_db_vardecimal_storage_format N'WIS_database', N'ON'
GO
ALTER DATABASE [WIS_database] SET QUERY_STORE = OFF
GO
USE [WIS_database]
GO
/****** Object:  Table [dbo].[WIS_Asset_Disposals]    Script Date: 18.06.2025 10:58:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WIS_Asset_Disposals](
	[ID_disposal] [int] IDENTITY(1,1) NOT NULL,
	[disposal_asset_ID] [int] NOT NULL,
	[disposal_date] [date] NOT NULL,
	[disposal_reason] [nvarchar](250) NULL,
	[disposal_user_ID] [int] NOT NULL,
 CONSTRAINT [PK_WIS_Asset_Disposals] PRIMARY KEY CLUSTERED 
(
	[ID_disposal] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[WIS_Asset_Histories]    Script Date: 18.06.2025 10:58:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WIS_Asset_Histories](
	[ID_asset_history] [int] IDENTITY(1,1) NOT NULL,
	[history_asset_ID] [int] NOT NULL,
	[history_event_date] [datetime] NOT NULL,
	[history_event_type] [nvarchar](50) NOT NULL,
	[history_description] [nvarchar](50) NULL,
	[history_user_ID] [int] NOT NULL,
 CONSTRAINT [PK_WIS_Asset_Historys] PRIMARY KEY CLUSTERED 
(
	[ID_asset_history] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[WIS_Asset_Locations]    Script Date: 18.06.2025 10:58:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WIS_Asset_Locations](
	[ID_asset_location] [int] IDENTITY(1,1) NOT NULL,
	[location_name] [nvarchar](50) NOT NULL,
	[location_note] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_WIS_Asset_Location] PRIMARY KEY CLUSTERED 
(
	[ID_asset_location] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[WIS_Asset_Statuses]    Script Date: 18.06.2025 10:58:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WIS_Asset_Statuses](
	[ID_asset_status] [int] IDENTITY(1,1) NOT NULL,
	[status_name] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_WIS_Asset_Statuses_1] PRIMARY KEY CLUSTERED 
(
	[ID_asset_status] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[WIS_Asset_Types]    Script Date: 18.06.2025 10:58:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WIS_Asset_Types](
	[ID_asset_type] [int] IDENTITY(1,1) NOT NULL,
	[asset_type_name] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_WIS_Asset_Types] PRIMARY KEY CLUSTERED 
(
	[ID_asset_type] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[WIS_Assets]    Script Date: 18.06.2025 10:58:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WIS_Assets](
	[ID_asset] [int] IDENTITY(1,1) NOT NULL,
	[asset_name] [nvarchar](50) NOT NULL,
	[asset_model] [nvarchar](50) NULL,
	[asset_serial_number] [nvarchar](50) NOT NULL,
	[asset_type_ID] [int] NOT NULL,
	[asset_purchase_date] [date] NULL,
	[asset_purchase_price] [decimal](18, 2) NULL,
	[asset_warranty_expiration_date] [date] NULL,
	[asset_note] [nvarchar](250) NULL,
	[asset_status_ID] [int] NOT NULL,
	[asset_location_ID] [int] NULL,
	[asset_user_ID] [int] NULL,
 CONSTRAINT [PK_WIS_Assets] PRIMARY KEY CLUSTERED 
(
	[ID_asset] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[WIS_Report_Types]    Script Date: 18.06.2025 10:58:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WIS_Report_Types](
	[ID_report_type] [int] IDENTITY(1,1) NOT NULL,
	[report_type_name] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_WIS_Report_Types] PRIMARY KEY CLUSTERED 
(
	[ID_report_type] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[WIS_Reports]    Script Date: 18.06.2025 10:58:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WIS_Reports](
	[ID_report] [int] IDENTITY(1,1) NOT NULL,
	[report_name] [nvarchar](50) NOT NULL,
	[report_type_ID] [int] NOT NULL,
	[report_creation_date] [datetime] NOT NULL,
	[report_user_ID] [int] NOT NULL,
 CONSTRAINT [PK_WIS_Reports] PRIMARY KEY CLUSTERED 
(
	[ID_report] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[WIS_Request_Statuses]    Script Date: 18.06.2025 10:58:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WIS_Request_Statuses](
	[ID_request_status] [int] IDENTITY(1,1) NOT NULL,
	[request_status_name] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_WIS_Request_Statuses] PRIMARY KEY CLUSTERED 
(
	[ID_request_status] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[WIS_Requests]    Script Date: 18.06.2025 10:58:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WIS_Requests](
	[ID_request] [int] IDENTITY(1,1) NOT NULL,
	[request_asset_ID] [int] NOT NULL,
	[request_user_ID] [int] NOT NULL,
	[request_date] [datetime] NOT NULL,
	[request_status_ID] [int] NOT NULL,
	[request_approved_by_user_ID] [int] NULL,
	[request_approval_date] [datetime] NULL,
	[request_note] [nvarchar](250) NULL,
 CONSTRAINT [PK_WIS_Requests] PRIMARY KEY CLUSTERED 
(
	[ID_request] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[WIS_User_Roles]    Script Date: 18.06.2025 10:58:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WIS_User_Roles](
	[ID_user_role] [int] IDENTITY(1,1) NOT NULL,
	[user_role_name] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_WIS_User_Roles] PRIMARY KEY CLUSTERED 
(
	[ID_user_role] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[WIS_Users]    Script Date: 18.06.2025 10:58:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WIS_Users](
	[ID_user] [int] IDENTITY(1,1) NOT NULL,
	[user_firstname] [nvarchar](50) NOT NULL,
	[user_lastname] [nvarchar](50) NULL,
	[user_login] [varchar](50) NOT NULL,
	[user_password_hash] [varchar](64) NULL,
	[user_email] [varchar](50) NULL,
	[user_role_ID] [int] NOT NULL,
	[user_department] [nvarchar](50) NULL,
 CONSTRAINT [PK_WIS_Users] PRIMARY KEY CLUSTERED 
(
	[ID_user] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[WIS_Asset_Disposals]  WITH CHECK ADD  CONSTRAINT [FK_WIS_Asset_Disposals_WIS_Assets] FOREIGN KEY([disposal_asset_ID])
REFERENCES [dbo].[WIS_Assets] ([ID_asset])
GO
ALTER TABLE [dbo].[WIS_Asset_Disposals] CHECK CONSTRAINT [FK_WIS_Asset_Disposals_WIS_Assets]
GO
ALTER TABLE [dbo].[WIS_Asset_Disposals]  WITH CHECK ADD  CONSTRAINT [FK_WIS_Asset_Disposals_WIS_Users] FOREIGN KEY([disposal_user_ID])
REFERENCES [dbo].[WIS_Users] ([ID_user])
GO
ALTER TABLE [dbo].[WIS_Asset_Disposals] CHECK CONSTRAINT [FK_WIS_Asset_Disposals_WIS_Users]
GO
ALTER TABLE [dbo].[WIS_Asset_Histories]  WITH CHECK ADD  CONSTRAINT [FK_WIS_Asset_Historys_WIS_Assets] FOREIGN KEY([history_asset_ID])
REFERENCES [dbo].[WIS_Assets] ([ID_asset])
GO
ALTER TABLE [dbo].[WIS_Asset_Histories] CHECK CONSTRAINT [FK_WIS_Asset_Historys_WIS_Assets]
GO
ALTER TABLE [dbo].[WIS_Asset_Histories]  WITH CHECK ADD  CONSTRAINT [FK_WIS_Asset_Historys_WIS_Users] FOREIGN KEY([history_user_ID])
REFERENCES [dbo].[WIS_Users] ([ID_user])
GO
ALTER TABLE [dbo].[WIS_Asset_Histories] CHECK CONSTRAINT [FK_WIS_Asset_Historys_WIS_Users]
GO
ALTER TABLE [dbo].[WIS_Assets]  WITH CHECK ADD  CONSTRAINT [FK_WIS_Assets_WIS_Asset_Locations] FOREIGN KEY([asset_location_ID])
REFERENCES [dbo].[WIS_Asset_Locations] ([ID_asset_location])
GO
ALTER TABLE [dbo].[WIS_Assets] CHECK CONSTRAINT [FK_WIS_Assets_WIS_Asset_Locations]
GO
ALTER TABLE [dbo].[WIS_Assets]  WITH CHECK ADD  CONSTRAINT [FK_WIS_Assets_WIS_Asset_Statuses] FOREIGN KEY([asset_status_ID])
REFERENCES [dbo].[WIS_Asset_Statuses] ([ID_asset_status])
GO
ALTER TABLE [dbo].[WIS_Assets] CHECK CONSTRAINT [FK_WIS_Assets_WIS_Asset_Statuses]
GO
ALTER TABLE [dbo].[WIS_Assets]  WITH CHECK ADD  CONSTRAINT [FK_WIS_Assets_WIS_Asset_Types] FOREIGN KEY([asset_type_ID])
REFERENCES [dbo].[WIS_Asset_Types] ([ID_asset_type])
GO
ALTER TABLE [dbo].[WIS_Assets] CHECK CONSTRAINT [FK_WIS_Assets_WIS_Asset_Types]
GO
ALTER TABLE [dbo].[WIS_Assets]  WITH CHECK ADD  CONSTRAINT [FK_WIS_Assets_WIS_Users] FOREIGN KEY([asset_user_ID])
REFERENCES [dbo].[WIS_Users] ([ID_user])
GO
ALTER TABLE [dbo].[WIS_Assets] CHECK CONSTRAINT [FK_WIS_Assets_WIS_Users]
GO
ALTER TABLE [dbo].[WIS_Reports]  WITH CHECK ADD  CONSTRAINT [FK_WIS_Reports_WIS_Report_Types] FOREIGN KEY([report_type_ID])
REFERENCES [dbo].[WIS_Report_Types] ([ID_report_type])
GO
ALTER TABLE [dbo].[WIS_Reports] CHECK CONSTRAINT [FK_WIS_Reports_WIS_Report_Types]
GO
ALTER TABLE [dbo].[WIS_Reports]  WITH CHECK ADD  CONSTRAINT [FK_WIS_Reports_WIS_Users1] FOREIGN KEY([report_user_ID])
REFERENCES [dbo].[WIS_Users] ([ID_user])
GO
ALTER TABLE [dbo].[WIS_Reports] CHECK CONSTRAINT [FK_WIS_Reports_WIS_Users1]
GO
ALTER TABLE [dbo].[WIS_Requests]  WITH CHECK ADD  CONSTRAINT [FK_WIS_Requests_WIS_Assets] FOREIGN KEY([request_asset_ID])
REFERENCES [dbo].[WIS_Assets] ([ID_asset])
GO
ALTER TABLE [dbo].[WIS_Requests] CHECK CONSTRAINT [FK_WIS_Requests_WIS_Assets]
GO
ALTER TABLE [dbo].[WIS_Requests]  WITH CHECK ADD  CONSTRAINT [FK_WIS_Requests_WIS_Request_Statuses] FOREIGN KEY([request_status_ID])
REFERENCES [dbo].[WIS_Request_Statuses] ([ID_request_status])
GO
ALTER TABLE [dbo].[WIS_Requests] CHECK CONSTRAINT [FK_WIS_Requests_WIS_Request_Statuses]
GO
ALTER TABLE [dbo].[WIS_Requests]  WITH CHECK ADD  CONSTRAINT [FK_WIS_Requests_WIS_Users] FOREIGN KEY([request_user_ID])
REFERENCES [dbo].[WIS_Users] ([ID_user])
GO
ALTER TABLE [dbo].[WIS_Requests] CHECK CONSTRAINT [FK_WIS_Requests_WIS_Users]
GO
ALTER TABLE [dbo].[WIS_Requests]  WITH CHECK ADD  CONSTRAINT [FK_WIS_Requests_WIS_Users1] FOREIGN KEY([request_approved_by_user_ID])
REFERENCES [dbo].[WIS_Users] ([ID_user])
GO
ALTER TABLE [dbo].[WIS_Requests] CHECK CONSTRAINT [FK_WIS_Requests_WIS_Users1]
GO
ALTER TABLE [dbo].[WIS_Users]  WITH CHECK ADD  CONSTRAINT [FK_WIS_Users_WIS_User_Roles] FOREIGN KEY([user_role_ID])
REFERENCES [dbo].[WIS_User_Roles] ([ID_user_role])
GO
ALTER TABLE [dbo].[WIS_Users] CHECK CONSTRAINT [FK_WIS_Users_WIS_User_Roles]
GO
USE [master]
GO
ALTER DATABASE [WIS_database] SET  READ_WRITE 
GO
