IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE name = 'Grados' And xtype= 'U')
BEGIN
	CREATE TABLE dbo.Grados(
	  [Id]          INT IDENTITY(1,1) NOT NULL,
        [Nombre]      NVARCHAR(100)     NOT NULL,  -- "Primer Grado", "Segundo Grado"...
        [Codigo]      NVARCHAR(10)      NOT NULL,  -- "1°", "2°", ... "9°"
        [Nivel]       NVARCHAR(50)      NOT NULL,  -- "Primaria" | "Tercer Ciclo"
        [Orden]       INT               NOT NULL,  -- 1 al 9, para ordenar correctamente
        [Descripcion] NVARCHAR(300)     NULL,
        [Activo]      BIT               NOT NULL DEFAULT 1,
        CONSTRAINT [PK_Grados] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

END
GO


INSERT INTO [dbo].[Grados] ([Nombre], [Codigo], [Nivel], [Orden], [Activo])
VALUES
    ('Primer Grado',   '1°', 'Primaria',      1, 1),
    ('Segundo Grado',  '2°', 'Primaria',      2, 1),
    ('Tercer Grado',   '3°', 'Primaria',      3, 1),
    ('Cuarto Grado',   '4°', 'Primaria',      4, 1),
    ('Quinto Grado',   '5°', 'Primaria',      5, 1),
    ('Sexto Grado',    '6°', 'Primaria',      6, 1),
    ('Séptimo Grado',  '7°', 'Tercer Ciclo',  7, 1),
    ('Octavo Grado',   '8°', 'Tercer Ciclo',  8, 1),
    ('Noveno Grado',   '9°', 'Tercer Ciclo',  9, 1);
GO

IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE name='Asignaturas' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[Asignaturas] (
        [Id]          INT IDENTITY(1,1) NOT NULL,
        [Nombre]      NVARCHAR(150)     NOT NULL,
        [Codigo]      NVARCHAR(20)      NOT NULL,  -- "ESP", "MAT", etc.
        [NivelAplicacion] NVARCHAR(20) NOT NULL DEFAULT 'Todos',       -- útil para horarios futuros
        [Activo]      BIT               NOT NULL DEFAULT 1,
        CONSTRAINT [PK_Asignaturas] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    PRINT 'Tabla Asignaturas creada.';
END
GO


SELECT * FROM Cursos

SELECT * FROM Asignaturas
SELECT * FROM dbo.Grados





INSERT INTO [dbo].[Asignaturas] ([Nombre], [Codigo],[NivelAplicacion], [Activo])
VALUES
    -- Aplican para todos los grados (1° al 9°)
    ('Español',              'ESP',  'Todos',       1),
    ('Matemáticas',          'MAT',  'Todos',       1),
    ('Ciencias Naturales',   'CN',   'Todos',       1),
    ('Ciencias Sociales',    'CS',   'Todos',       1),
    ('Inglés',               'ING',  'Todos',       1),
    ('Educación Artística',  'ART',  'Todos',       1),
    ('Educación Física',     'EDF',  'Todos',       1),
    ('Educación Cívica',     'CIV',  'Todos', 1),
    -- Solo para Tercer Ciclo (7°, 8°, 9°)
    ('Tecnología',           'TEC',  'Todos', 1);
GO


IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Cursos' AND COLUMN_NAME = 'Seccion'
)
BEGIN
    ALTER TABLE [dbo].[Cursos] ADD [Seccion] NVARCHAR(5) NULL;
    PRINT 'Columna Seccion agregada a Cursos.';
END
GO

SELECT * FROM CURSOS


--Eliminar la relacion de docentes con cursos ya que ahora tendra relacion con otra, pero ahora dependiendo segun la asiganaturas
IF EXISTS (
    SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Cursos_Docentes'
)
    ALTER TABLE [dbo].[Cursos] DROP CONSTRAINT [FK_Cursos_Docentes];

IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Cursos' AND COLUMN_NAME = 'DocenteId'
)
    ALTER TABLE [dbo].[Cursos] DROP COLUMN [DocenteId];
GO




IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE name='AsignacionDocente' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[AsignacionDocente] (
        [Id]           INT IDENTITY(1,1) NOT NULL,
        [CursoId]      INT NOT NULL,
        [AsignaturaId] INT NOT NULL,
        [DocenteId]    INT NOT NULL,
        [Activo]       BIT NOT NULL DEFAULT 1,
        CONSTRAINT [PK_AsignacionDocente] PRIMARY KEY ([Id]),

        CONSTRAINT [FK_AsignDoc_Curso]
            FOREIGN KEY ([CursoId])      REFERENCES [dbo].[Cursos]([Id])      ON DELETE CASCADE,
        CONSTRAINT [FK_AsignDoc_Asignatura]
            FOREIGN KEY ([AsignaturaId]) REFERENCES [dbo].[Asignaturas]([Id]),
        CONSTRAINT [FK_AsignDoc_Docente]
            FOREIGN KEY ([DocenteId])    REFERENCES [dbo].[Docentes]([Id]),

        -- Un docente no puede estar asignado dos veces a la misma
        -- asignatura en el mismo curso
        CONSTRAINT [UQ_AsignDoc_Curso_Asignatura]
            UNIQUE ([CursoId], [AsignaturaId])
    );
END
GO