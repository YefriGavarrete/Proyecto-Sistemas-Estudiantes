-- ================================================================
-- EduPath AI — Script completo de creación/actualización de tablas
-- Ejecutar en SQL Server Management Studio sobre tu base de datos.
-- Es seguro ejecutarlo múltiples veces (usa IF NOT EXISTS).
-- ================================================================

-- ⚠️ Cambia el nombre de la BD si es necesario:
-- USE [TU_BASE_DE_DATOS];
-- GO

-- ================================================================
-- 1. TABLA: Cursos
--    (debe existir antes que Estudiantes por la FK)
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Cursos')
BEGIN
    CREATE TABLE Cursos (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        Codigo      NVARCHAR(20)  NOT NULL,
        Nombre      NVARCHAR(200) NOT NULL,
        Descripcion NVARCHAR(500) NULL,
        Periodo     NVARCHAR(20)  NULL,
        Activo      BIT           NOT NULL DEFAULT 1,
        DocenteId   INT           NOT NULL,
        CONSTRAINT UQ_Cursos_Codigo UNIQUE (Codigo),
        CONSTRAINT FK_Cursos_Docentes FOREIGN KEY (DocenteId)
            REFERENCES Docentes(Id) ON DELETE NO ACTION
    );
    PRINT '✅ Tabla Cursos creada.';
END
ELSE
BEGIN
    PRINT '⚠️  Tabla Cursos ya existía, no se modificó.';
END
GO

-- ================================================================
-- 2. TABLA: Estudiantes (creación completa desde cero)
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Estudiantes')
BEGIN
    CREATE TABLE Estudiantes (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        Codigo          NVARCHAR(20)   NOT NULL DEFAULT '',

        -- Datos personales
        Nombre          NVARCHAR(100)  NOT NULL,
        Apellido        NVARCHAR(100)  NOT NULL DEFAULT '',
        FechaNacimiento DATE           NOT NULL DEFAULT '2000-01-01',
        Identidad       NVARCHAR(20)   NULL,
        Correo          NVARCHAR(150)  NOT NULL,
        Telefono        NVARCHAR(20)   NULL,
        Genero          NVARCHAR(15)   NULL,
        Seccion         NVARCHAR(20)   NULL,
        Observaciones   NVARCHAR(500)  NULL,
        Activo          BIT            NOT NULL DEFAULT 1,

        -- Notas de los 4 parciales
        Nota1           DECIMAL(5,2)   NULL,
        FechaNota1      DATETIME2      NULL,
        Nota2           DECIMAL(5,2)   NULL,
        FechaNota2      DATETIME2      NULL,
        Nota3           DECIMAL(5,2)   NULL,
        FechaNota3      DATETIME2      NULL,
        Nota4           DECIMAL(5,2)   NULL,
        FechaNota4      DATETIME2      NULL,

        -- Columnas calculadas (PERSISTED = guardadas físicamente)
        Promedio AS (
            CASE
                WHEN Nota1 IS NOT NULL AND Nota2 IS NOT NULL
                 AND Nota3 IS NOT NULL AND Nota4 IS NOT NULL
                    THEN ROUND((Nota1+Nota2+Nota3+Nota4)/4.0, 2)
                WHEN Nota1 IS NOT NULL AND Nota2 IS NOT NULL
                 AND Nota3 IS NOT NULL
                    THEN ROUND((Nota1+Nota2+Nota3)/3.0, 2)
                WHEN Nota1 IS NOT NULL AND Nota2 IS NOT NULL
                    THEN ROUND((Nota1+Nota2)/2.0, 2)
                WHEN Nota1 IS NOT NULL THEN Nota1
                ELSE NULL
            END
        ) PERSISTED,

        Estado AS (
            CASE
                WHEN (CASE
                        WHEN Nota1 IS NOT NULL AND Nota2 IS NOT NULL
                         AND Nota3 IS NOT NULL AND Nota4 IS NOT NULL
                            THEN (Nota1+Nota2+Nota3+Nota4)/4.0
                        WHEN Nota1 IS NOT NULL AND Nota2 IS NOT NULL
                         AND Nota3 IS NOT NULL
                            THEN (Nota1+Nota2+Nota3)/3.0
                        WHEN Nota1 IS NOT NULL AND Nota2 IS NOT NULL
                            THEN (Nota1+Nota2)/2.0
                        WHEN Nota1 IS NOT NULL THEN Nota1
                        ELSE NULL
                      END) >= 60 THEN N'Aprobado'
                WHEN (CASE
                        WHEN Nota1 IS NOT NULL AND Nota2 IS NOT NULL
                         AND Nota3 IS NOT NULL AND Nota4 IS NOT NULL
                            THEN (Nota1+Nota2+Nota3+Nota4)/4.0
                        WHEN Nota1 IS NOT NULL AND Nota2 IS NOT NULL
                         AND Nota3 IS NOT NULL
                            THEN (Nota1+Nota2+Nota3)/3.0
                        WHEN Nota1 IS NOT NULL AND Nota2 IS NOT NULL
                            THEN (Nota1+Nota2)/2.0
                        WHEN Nota1 IS NOT NULL THEN Nota1
                        ELSE NULL
                      END) < 60 THEN N'Reprobado'
                ELSE N'Sin Notas'
            END
        ) PERSISTED,

        -- Predicción IA (campo manual)
        EnRiesgoIA      BIT            NULL,

        -- Relación con Cursos
        CursoId         INT            NOT NULL,
        FechaRegistro   DATETIME2      NOT NULL DEFAULT GETDATE(),

        -- Restricciones
        CONSTRAINT UQ_Estudiante_Correo_Curso UNIQUE (Correo, CursoId),
        CONSTRAINT FK_Estudiantes_Cursos FOREIGN KEY (CursoId)
            REFERENCES Cursos(Id) ON DELETE CASCADE
    );
    PRINT '✅ Tabla Estudiantes creada correctamente.';
END


ELSE


BEGIN
    PRINT '⚠️  Tabla Estudiantes ya existe — aplicando columnas faltantes...';

    -- Por si la tabla existía con esquema antiguo, agregar columnas nuevas:
    IF COL_LENGTH('Estudiantes','Apellido')        IS NULL ALTER TABLE Estudiantes ADD Apellido        NVARCHAR(100) NOT NULL DEFAULT '';
    IF COL_LENGTH('Estudiantes','Codigo')          IS NULL ALTER TABLE Estudiantes ADD Codigo          NVARCHAR(20)  NOT NULL DEFAULT '';
    IF COL_LENGTH('Estudiantes','FechaNacimiento') IS NULL ALTER TABLE Estudiantes ADD FechaNacimiento DATE          NOT NULL DEFAULT '2000-01-01';
    IF COL_LENGTH('Estudiantes','Identidad')       IS NULL ALTER TABLE Estudiantes ADD Identidad       NVARCHAR(20)  NULL;
    IF COL_LENGTH('Estudiantes','Telefono')        IS NULL ALTER TABLE Estudiantes ADD Telefono        NVARCHAR(20)  NULL;
    IF COL_LENGTH('Estudiantes','Genero')          IS NULL ALTER TABLE Estudiantes ADD Genero          NVARCHAR(15)  NULL;
    IF COL_LENGTH('Estudiantes','Seccion')         IS NULL ALTER TABLE Estudiantes ADD Seccion         NVARCHAR(20)  NULL;
    IF COL_LENGTH('Estudiantes','Observaciones')   IS NULL ALTER TABLE Estudiantes ADD Observaciones   NVARCHAR(500) NULL;
    IF COL_LENGTH('Estudiantes','Activo')          IS NULL ALTER TABLE Estudiantes ADD Activo          BIT           NOT NULL DEFAULT 1;
    IF COL_LENGTH('Estudiantes','Nota4')           IS NULL ALTER TABLE Estudiantes ADD Nota4           DECIMAL(5,2)  NULL;
    IF COL_LENGTH('Estudiantes','FechaNota1')      IS NULL ALTER TABLE Estudiantes ADD FechaNota1      DATETIME2     NULL;
    IF COL_LENGTH('Estudiantes','FechaNota2')      IS NULL ALTER TABLE Estudiantes ADD FechaNota2      DATETIME2     NULL;
    IF COL_LENGTH('Estudiantes','FechaNota3')      IS NULL ALTER TABLE Estudiantes ADD FechaNota3      DATETIME2     NULL;
    IF COL_LENGTH('Estudiantes','FechaNota4')      IS NULL ALTER TABLE Estudiantes ADD FechaNota4      DATETIME2     NULL;
    IF COL_LENGTH('Estudiantes','EnRiesgoIA')      IS NULL ALTER TABLE Estudiantes ADD EnRiesgoIA      BIT           NULL;
    IF COL_LENGTH('Estudiantes','FechaRegistro')   IS NULL ALTER TABLE Estudiantes ADD FechaRegistro   DATETIME2     NOT NULL DEFAULT GETDATE();

    -- Eliminar columnas calculadas antiguas para recrearlas
    IF COL_LENGTH('Estudiantes','Promedio') IS NOT NULL ALTER TABLE Estudiantes DROP COLUMN Promedio;
    IF COL_LENGTH('Estudiantes','Estado')   IS NOT NULL ALTER TABLE Estudiantes DROP COLUMN Estado;
    IF COL_LENGTH('Estudiantes','Edad')     IS NOT NULL ALTER TABLE Estudiantes DROP COLUMN Edad;

    -- Recrear Promedio
    ALTER TABLE Estudiantes ADD Promedio AS (
        CASE
            WHEN Nota1 IS NOT NULL AND Nota2 IS NOT NULL
             AND Nota3 IS NOT NULL AND Nota4 IS NOT NULL
                THEN ROUND((Nota1+Nota2+Nota3+Nota4)/4.0, 2)
            WHEN Nota1 IS NOT NULL AND Nota2 IS NOT NULL
             AND Nota3 IS NOT NULL
                THEN ROUND((Nota1+Nota2+Nota3)/3.0, 2)
            WHEN Nota1 IS NOT NULL AND Nota2 IS NOT NULL
                THEN ROUND((Nota1+Nota2)/2.0, 2)
            WHEN Nota1 IS NOT NULL THEN Nota1
            ELSE NULL
        END
    ) PERSISTED;

    -- Recrear Estado
    ALTER TABLE Estudiantes ADD Estado AS (
        CASE
            WHEN (CASE
                    WHEN Nota1 IS NOT NULL AND Nota2 IS NOT NULL
                     AND Nota3 IS NOT NULL AND Nota4 IS NOT NULL
                        THEN (Nota1+Nota2+Nota3+Nota4)/4.0
                    WHEN Nota1 IS NOT NULL AND Nota2 IS NOT NULL
                     AND Nota3 IS NOT NULL
                        THEN (Nota1+Nota2+Nota3)/3.0
                    WHEN Nota1 IS NOT NULL AND Nota2 IS NOT NULL
                        THEN (Nota1+Nota2)/2.0
                    WHEN Nota1 IS NOT NULL THEN Nota1
                    ELSE NULL END) >= 60 THEN N'Aprobado'
            WHEN (CASE
                    WHEN Nota1 IS NOT NULL AND Nota2 IS NOT NULL
                     AND Nota3 IS NOT NULL AND Nota4 IS NOT NULL
                        THEN (Nota1+Nota2+Nota3+Nota4)/4.0
                    WHEN Nota1 IS NOT NULL AND Nota2 IS NOT NULL
                     AND Nota3 IS NOT NULL
                        THEN (Nota1+Nota2+Nota3)/3.0
                    WHEN Nota1 IS NOT NULL AND Nota2 IS NOT NULL
                        THEN (Nota1+Nota2)/2.0
                    WHEN Nota1 IS NOT NULL THEN Nota1
                    ELSE NULL END) < 60 THEN N'Reprobado'
            ELSE N'Sin Notas'
        END
    ) PERSISTED;

    PRINT '✅ Columnas faltantes agregadas y columnas calculadas recreadas.';
END
GO

-- ================================================================
-- 3. VERIFICACIÓN FINAL
-- ================================================================
SELECT
    COLUMN_NAME         AS Columna,
    DATA_TYPE           AS Tipo,
    IS_NULLABLE         AS Nullable,
    COLUMNPROPERTY(OBJECT_ID('Estudiantes'), COLUMN_NAME, 'IsComputed') AS EsCalculada
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Estudiantes'
ORDER BY ORDINAL_POSITION;

PRINT '✅ Script completado. Tablas listas para EduPath AI.';
GO
