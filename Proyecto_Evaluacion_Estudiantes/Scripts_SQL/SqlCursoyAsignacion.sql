USE [EvaluacionEstudiantes]
GO

/****** Object:  Table [dbo].[AsignacionDocente]    Script Date: 05-Apr-26 1:08:03 PM ******/
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

ALTER TABLE [dbo].[AsignacionDocente] ADD  DEFAULT ((1)) FOR [Activo]
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






	USE [EvaluacionEstudiantes]
	GO

	/****** Object:  Table [dbo].[Cursos]    Script Date: 05-Apr-26 1:07:25 PM ******/
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

	ALTER TABLE [dbo].[Cursos] ADD  DEFAULT ((1)) FOR [Activo]
	GO

	ALTER TABLE [dbo].[Cursos]  WITH CHECK ADD  CONSTRAINT [FK_Cursos_DocenteTutor] FOREIGN KEY([DocenteTutorId])
	REFERENCES [dbo].[Docentes] ([Id])
	GO

	ALTER TABLE [dbo].[Cursos] CHECK CONSTRAINT [FK_Cursos_DocenteTutor]
	GO


