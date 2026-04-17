USE [master]
GO
/****** Object:  Database [EvaluacionEstudiantes]    Script Date: 15-Apr-26 9:52:55 AM ******/
CREATE DATABASE [EvaluacionEstudiantes]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'EvaluacionEstudiantes', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER02\MSSQL\DATA\EvaluacionEstudiantes.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'EvaluacionEstudiantes_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER02\MSSQL\DATA\EvaluacionEstudiantes_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [EvaluacionEstudiantes] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [EvaluacionEstudiantes].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [EvaluacionEstudiantes] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [EvaluacionEstudiantes] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [EvaluacionEstudiantes] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [EvaluacionEstudiantes] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [EvaluacionEstudiantes] SET ARITHABORT OFF 
GO
ALTER DATABASE [EvaluacionEstudiantes] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [EvaluacionEstudiantes] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [EvaluacionEstudiantes] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [EvaluacionEstudiantes] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [EvaluacionEstudiantes] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [EvaluacionEstudiantes] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [EvaluacionEstudiantes] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [EvaluacionEstudiantes] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [EvaluacionEstudiantes] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [EvaluacionEstudiantes] SET  ENABLE_BROKER 
GO
ALTER DATABASE [EvaluacionEstudiantes] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [EvaluacionEstudiantes] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [EvaluacionEstudiantes] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [EvaluacionEstudiantes] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [EvaluacionEstudiantes] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [EvaluacionEstudiantes] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [EvaluacionEstudiantes] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [EvaluacionEstudiantes] SET RECOVERY FULL 
GO
ALTER DATABASE [EvaluacionEstudiantes] SET  MULTI_USER 
GO
ALTER DATABASE [EvaluacionEstudiantes] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [EvaluacionEstudiantes] SET DB_CHAINING OFF 
GO
ALTER DATABASE [EvaluacionEstudiantes] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [EvaluacionEstudiantes] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [EvaluacionEstudiantes] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [EvaluacionEstudiantes] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
EXEC sys.sp_db_vardecimal_storage_format N'EvaluacionEstudiantes', N'ON'
GO
ALTER DATABASE [EvaluacionEstudiantes] SET QUERY_STORE = ON
GO
ALTER DATABASE [EvaluacionEstudiantes] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [EvaluacionEstudiantes]
GO
/****** Object:  Table [dbo].[Administradores]    Script Date: 15-Apr-26 9:52:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Administradores](
	[Id_Administradores] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [nvarchar](200) NOT NULL,
	[Apellido] [nvarchar](200) NOT NULL,
	[NombreUsuario] [nvarchar](80) NOT NULL,
	[Contrasena] [nvarchar](256) NOT NULL,
	[Activo] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id_Administradores] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[NombreUsuario] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AsignacionDocente]    Script Date: 15-Apr-26 9:52:56 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AsignacionDocente](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CursoId] [int] NOT NULL,
	[AsignaturaId] [int] NOT NULL,
	[DocenteId] [int] NOT NULL,
	[Activo] [bit] NOT NULL,
 CONSTRAINT [PK_AsignacionDocente] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_AsignDoc_Curso_Asignatura] UNIQUE NONCLUSTERED 
(
	[CursoId] ASC,
	[AsignaturaId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Asignaturas]    Script Date: 15-Apr-26 9:52:56 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Asignaturas](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [nvarchar](150) NOT NULL,
	[Codigo] [nvarchar](20) NOT NULL,
	[NivelAplicacion] [nvarchar](20) NOT NULL,
	[Activo] [bit] NOT NULL,
 CONSTRAINT [PK_Asignaturas] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Cursos]    Script Date: 15-Apr-26 9:52:56 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Cursos](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Codigo] [nvarchar](20) NOT NULL,
	[Nombre] [nvarchar](200) NOT NULL,
	[Descripcion] [nvarchar](500) NULL,
	[Periodo] [nvarchar](20) NULL,
	[Activo] [bit] NOT NULL,
	[DocenteTutorId] [int] NULL,
	[Seccion] [nvarchar](5) NULL,
	[GradoId] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_Cursos_Codigo] UNIQUE NONCLUSTERED 
(
	[Codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Docentes]    Script Date: 15-Apr-26 9:52:56 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Docentes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[NombreCompleto] [nvarchar](150) NOT NULL,
	[Titulo] [nvarchar](20) NOT NULL,
	[Usuario] [nvarchar](80) NOT NULL,
	[Contrasena] [nvarchar](256) NOT NULL,
	[Correo] [nvarchar](150) NOT NULL,
	[Activo] [bit] NOT NULL,
	[FechaCreacion] [datetime] NOT NULL,
	[UltimoAcceso] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Usuario] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Estudiantes]    Script Date: 15-Apr-26 9:52:56 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Estudiantes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Codigo] [nvarchar](20) NOT NULL,
	[Nombre] [nvarchar](100) NOT NULL,
	[Apellido] [nvarchar](100) NOT NULL,
	[FechaNacimiento] [date] NOT NULL,
	[Identidad] [nvarchar](20) NULL,
	[Correo] [nvarchar](150) NOT NULL,
	[Telefono] [nvarchar](20) NULL,
	[Genero] [nvarchar](15) NULL,
	[Seccion] [nvarchar](20) NULL,
	[Observaciones] [nvarchar](500) NULL,
	[Activo] [bit] NOT NULL,
	[Nota1] [decimal](5, 2) NULL,
	[FechaNota1] [datetime2](7) NULL,
	[Nota2] [decimal](5, 2) NULL,
	[FechaNota2] [datetime2](7) NULL,
	[Nota3] [decimal](5, 2) NULL,
	[FechaNota3] [datetime2](7) NULL,
	[Nota4] [decimal](5, 2) NULL,
	[FechaNota4] [datetime2](7) NULL,
	[Promedio]  AS (case when [Nota1] IS NOT NULL AND [Nota2] IS NOT NULL AND [Nota3] IS NOT NULL AND [Nota4] IS NOT NULL then round(((([Nota1]+[Nota2])+[Nota3])+[Nota4])/(4.0),(2)) when [Nota1] IS NOT NULL AND [Nota2] IS NOT NULL AND [Nota3] IS NOT NULL then round((([Nota1]+[Nota2])+[Nota3])/(3.0),(2)) when [Nota1] IS NOT NULL AND [Nota2] IS NOT NULL then round(([Nota1]+[Nota2])/(2.0),(2)) when [Nota1] IS NOT NULL then [Nota1]  end) PERSISTED,
	[Estado]  AS (case when case when [Nota1] IS NOT NULL AND [Nota2] IS NOT NULL AND [Nota3] IS NOT NULL AND [Nota4] IS NOT NULL then ((([Nota1]+[Nota2])+[Nota3])+[Nota4])/(4.0) when [Nota1] IS NOT NULL AND [Nota2] IS NOT NULL AND [Nota3] IS NOT NULL then (([Nota1]+[Nota2])+[Nota3])/(3.0) when [Nota1] IS NOT NULL AND [Nota2] IS NOT NULL then ([Nota1]+[Nota2])/(2.0) when [Nota1] IS NOT NULL then [Nota1]  end>=(60) then N'Aprobado' when case when [Nota1] IS NOT NULL AND [Nota2] IS NOT NULL AND [Nota3] IS NOT NULL AND [Nota4] IS NOT NULL then ((([Nota1]+[Nota2])+[Nota3])+[Nota4])/(4.0) when [Nota1] IS NOT NULL AND [Nota2] IS NOT NULL AND [Nota3] IS NOT NULL then (([Nota1]+[Nota2])+[Nota3])/(3.0) when [Nota1] IS NOT NULL AND [Nota2] IS NOT NULL then ([Nota1]+[Nota2])/(2.0) when [Nota1] IS NOT NULL then [Nota1]  end<(60) then N'Reprobado' else N'Sin Notas' end) PERSISTED NOT NULL,
	[EnRiesgoIA] [bit] NULL,
	[CursoId] [int] NOT NULL,
	[FechaRegistro] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_Estudiante_Correo_Curso] UNIQUE NONCLUSTERED 
(
	[Correo] ASC,
	[CursoId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Grados]    Script Date: 15-Apr-26 9:52:56 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Grados](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [nvarchar](100) NOT NULL,
	[Codigo] [nvarchar](10) NOT NULL,
	[Nivel] [nvarchar](50) NOT NULL,
	[Orden] [int] NOT NULL,
	[Descripcion] [nvarchar](300) NULL,
	[Activo] [bit] NOT NULL,
 CONSTRAINT [PK_Grados] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NotasParciales]    Script Date: 15-Apr-26 9:52:56 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NotasParciales](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EstudianteId] [int] NOT NULL,
	[AsignaturaId] [int] NOT NULL,
	[Parcial] [tinyint] NOT NULL,
	[Nota] [decimal](5, 2) NOT NULL,
	[FechaRegistro] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_NotasParciales] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_NotasParciales_Est_Asig_Parcial] UNIQUE NONCLUSTERED 
(
	[EstudianteId] ASC,
	[AsignaturaId] ASC,
	[Parcial] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Administradores] ADD  DEFAULT ((1)) FOR [Activo]
GO
ALTER TABLE [dbo].[AsignacionDocente] ADD  DEFAULT ((1)) FOR [Activo]
GO
ALTER TABLE [dbo].[Asignaturas] ADD  DEFAULT ('Todos') FOR [NivelAplicacion]
GO
ALTER TABLE [dbo].[Asignaturas] ADD  DEFAULT ((1)) FOR [Activo]
GO
ALTER TABLE [dbo].[Cursos] ADD  DEFAULT ((1)) FOR [Activo]
GO
ALTER TABLE [dbo].[Docentes] ADD  DEFAULT ('Lic.') FOR [Titulo]
GO
ALTER TABLE [dbo].[Docentes] ADD  DEFAULT ((1)) FOR [Activo]
GO
ALTER TABLE [dbo].[Docentes] ADD  DEFAULT (getdate()) FOR [FechaCreacion]
GO
ALTER TABLE [dbo].[Estudiantes] ADD  DEFAULT ('') FOR [Codigo]
GO
ALTER TABLE [dbo].[Estudiantes] ADD  DEFAULT ('') FOR [Apellido]
GO
ALTER TABLE [dbo].[Estudiantes] ADD  DEFAULT ('2000-01-01') FOR [FechaNacimiento]
GO
ALTER TABLE [dbo].[Estudiantes] ADD  DEFAULT ((1)) FOR [Activo]
GO
ALTER TABLE [dbo].[Estudiantes] ADD  DEFAULT (getdate()) FOR [FechaRegistro]
GO
ALTER TABLE [dbo].[Grados] ADD  DEFAULT ((1)) FOR [Activo]
GO
ALTER TABLE [dbo].[NotasParciales] ADD  CONSTRAINT [DF_NotasParciales_FechaRegistro]  DEFAULT (sysutcdatetime()) FOR [FechaRegistro]
GO
ALTER TABLE [dbo].[AsignacionDocente]  WITH CHECK ADD  CONSTRAINT [FK_AsignDoc_Asignatura] FOREIGN KEY([AsignaturaId])
REFERENCES [dbo].[Asignaturas] ([Id])
GO
ALTER TABLE [dbo].[AsignacionDocente] CHECK CONSTRAINT [FK_AsignDoc_Asignatura]
GO
ALTER TABLE [dbo].[AsignacionDocente]  WITH CHECK ADD  CONSTRAINT [FK_AsignDoc_Curso] FOREIGN KEY([CursoId])
REFERENCES [dbo].[Cursos] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AsignacionDocente] CHECK CONSTRAINT [FK_AsignDoc_Curso]
GO
ALTER TABLE [dbo].[AsignacionDocente]  WITH CHECK ADD  CONSTRAINT [FK_AsignDoc_Docente] FOREIGN KEY([DocenteId])
REFERENCES [dbo].[Docentes] ([Id])
GO
ALTER TABLE [dbo].[AsignacionDocente] CHECK CONSTRAINT [FK_AsignDoc_Docente]
GO
ALTER TABLE [dbo].[Cursos]  WITH CHECK ADD  CONSTRAINT [FK_Cursos_DocenteTutor] FOREIGN KEY([DocenteTutorId])
REFERENCES [dbo].[Docentes] ([Id])
GO
ALTER TABLE [dbo].[Cursos] CHECK CONSTRAINT [FK_Cursos_DocenteTutor]
GO
ALTER TABLE [dbo].[Cursos]  WITH CHECK ADD  CONSTRAINT [FK_Cursos_Grado] FOREIGN KEY([GradoId])
REFERENCES [dbo].[Grados] ([Id])
GO
ALTER TABLE [dbo].[Cursos] CHECK CONSTRAINT [FK_Cursos_Grado]
GO
ALTER TABLE [dbo].[Estudiantes]  WITH CHECK ADD  CONSTRAINT [FK_Estudiantes_Cursos] FOREIGN KEY([CursoId])
REFERENCES [dbo].[Cursos] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Estudiantes] CHECK CONSTRAINT [FK_Estudiantes_Cursos]
GO
ALTER TABLE [dbo].[NotasParciales]  WITH CHECK ADD  CONSTRAINT [FK_NotasParciales_Asignatura] FOREIGN KEY([AsignaturaId])
REFERENCES [dbo].[Asignaturas] ([Id])
GO
ALTER TABLE [dbo].[NotasParciales] CHECK CONSTRAINT [FK_NotasParciales_Asignatura]
GO
ALTER TABLE [dbo].[NotasParciales]  WITH CHECK ADD  CONSTRAINT [FK_NotasParciales_Estudiante] FOREIGN KEY([EstudianteId])
REFERENCES [dbo].[Estudiantes] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[NotasParciales] CHECK CONSTRAINT [FK_NotasParciales_Estudiante]
GO
ALTER TABLE [dbo].[NotasParciales]  WITH CHECK ADD  CONSTRAINT [CK_NotasParciales_Nota] CHECK  (([Nota]>=(0) AND [Nota]<=(100)))
GO
ALTER TABLE [dbo].[NotasParciales] CHECK CONSTRAINT [CK_NotasParciales_Nota]
GO
ALTER TABLE [dbo].[NotasParciales]  WITH CHECK ADD  CONSTRAINT [CK_NotasParciales_Parcial] CHECK  (([Parcial]=(4) OR [Parcial]=(3) OR [Parcial]=(2) OR [Parcial]=(1)))
GO
ALTER TABLE [dbo].[NotasParciales] CHECK CONSTRAINT [CK_NotasParciales_Parcial]
GO
USE [master]
GO
ALTER DATABASE [EvaluacionEstudiantes] SET  READ_WRITE 
GO
