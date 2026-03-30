-- =============================================================
-- EduPath AI — Actualización de la tabla Estudiantes
-- Ejecutar en SQL Server Management Studio sobre la base de datos
-- del proyecto ANTES de arrancar la aplicación con el nuevo modelo.
-- =============================================================

USE [TU_BASE_DE_DATOS];   -- ← Cambia por el nombre real de tu BD
GO

-- -----------------------------------------------------------
-- 1. Eliminar columnas y columnas calculadas antiguas
--    (si la tabla ya existe del modelo anterior)
-- -----------------------------------------------------------

-- Primero hay que eliminar la columna calculada Promedio y Estado
-- porque dependen de las columnas de nota

IF COL_LENGTH('Estudiantes','Promedio') IS NOT NULL
    ALTER TABLE Estudiantes DROP COLUMN Promedio;

IF COL_LENGTH('Estudiantes','Estado') IS NOT NULL
    ALTER TABLE Estudiantes DROP COLUMN Estado;

-- Eliminar columna Edad (se reemplaza por FechaNacimiento)
IF COL_LENGTH('Estudiantes','Edad') IS NOT NULL
    ALTER TABLE Estudiantes DROP COLUMN Edad;

-- Eliminar NombreCompleto antiguo si era columna (ahora es solo Nombre+Apellido)
-- (Si "Nombre" ya existía como nombre completo, la renombramos)

GO

-- -----------------------------------------------------------
-- 2. Agregar / renombrar columnas de datos personales
-- -----------------------------------------------------------

-- Agregar Apellido (si no existe)
IF COL_LENGTH('Estudiantes','Apellido') IS NULL
    ALTER TABLE Estudiantes ADD Apellido NVARCHAR(100) NOT NULL DEFAULT '';

-- Agregar Codigo
IF COL_LENGTH('Estudiantes','Codigo') IS NULL
    ALTER TABLE Estudiantes ADD Codigo NVARCHAR(20) NOT NULL DEFAULT '';

-- Agregar FechaNacimiento
IF COL_LENGTH('Estudiantes','FechaNacimiento') IS NULL
    ALTER TABLE Estudiantes ADD FechaNacimiento DATE NOT NULL DEFAULT '2000-01-01';

-- Agregar Identidad
IF COL_LENGTH('Estudiantes','Identidad') IS NULL
    ALTER TABLE Estudiantes ADD Identidad NVARCHAR(20) NULL;

-- Agregar Telefono
IF COL_LENGTH('Estudiantes','Telefono') IS NULL
    ALTER TABLE Estudiantes ADD Telefono NVARCHAR(20) NULL;

-- Agregar Genero
IF COL_LENGTH('Estudiantes','Genero') IS NULL
    ALTER TABLE Estudiantes ADD Genero NVARCHAR(15) NULL;

-- Agregar Seccion
IF COL_LENGTH('Estudiantes','Seccion') IS NULL
    ALTER TABLE Estudiantes ADD Seccion NVARCHAR(20) NULL;

-- Agregar Observaciones
IF COL_LENGTH('Estudiantes','Observaciones') IS NULL
    ALTER TABLE Estudiantes ADD Observaciones NVARCHAR(500) NULL;

-- Agregar Activo
IF COL_LENGTH('Estudiantes','Activo') IS NULL
    ALTER TABLE Estudiantes ADD Activo BIT NOT NULL DEFAULT 1;

GO

-- -----------------------------------------------------------
-- 3. Agregar Nota4 y fechas de parciales
-- -----------------------------------------------------------

IF COL_LENGTH('Estudiantes','Nota4') IS NULL
    ALTER TABLE Estudiantes ADD Nota4 DECIMAL(5,2) NULL;

IF COL_LENGTH('Estudiantes','FechaNota1') IS NULL
    ALTER TABLE Estudiantes ADD FechaNota1 DATETIME2 NULL;

IF COL_LENGTH('Estudiantes','FechaNota2') IS NULL
    ALTER TABLE Estudiantes ADD FechaNota2 DATETIME2 NULL;

IF COL_LENGTH('Estudiantes','FechaNota3') IS NULL
    ALTER TABLE Estudiantes ADD FechaNota3 DATETIME2 NULL;

IF COL_LENGTH('Estudiantes','FechaNota4') IS NULL
    ALTER TABLE Estudiantes ADD FechaNota4 DATETIME2 NULL;

GO

-- -----------------------------------------------------------
-- 4. Recrear columnas calculadas con los 4 parciales
-- -----------------------------------------------------------

-- Promedio (calculado y almacenado)
ALTER TABLE Estudiantes
    ADD Promedio AS (
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

GO

-- Estado (calculado y almacenado)
ALTER TABLE Estudiantes
    ADD Estado AS (
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
                    ELSE NULL END) >= 60
                THEN N'Aprobado'
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
                    ELSE NULL END) < 60
                THEN N'Reprobado'
            ELSE N'Sin Notas'
        END
    ) PERSISTED;

GO

-- -----------------------------------------------------------
-- 5. Verificación final
-- -----------------------------------------------------------
SELECT
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Estudiantes'
ORDER BY ORDINAL_POSITION;

PRINT '✅ Tabla Estudiantes actualizada correctamente para EduPath AI.';
GO
