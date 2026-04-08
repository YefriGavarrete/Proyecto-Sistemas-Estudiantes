
USE [EvaluacionEstudiantes]
GO

-- ============================================================
-- Parte 2: Tabla NotasParciales
-- Almacena la nota de cada estudiante por asignatura y parcial.
-- Nota1-4 en Estudiantes pasan a ser el promedio de parcial
-- calculado por el sistema al subir notas.
-- Script seguro: usa IF NOT EXISTS en cada paso.
-- ============================================================

-- ── 1. Crear tabla si no existe ─────────────────────────────
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'NotasParciales'
)
BEGIN
    CREATE TABLE [dbo].[NotasParciales]
    (
        [Id]             INT IDENTITY(1,1)   NOT NULL,
        [EstudianteId]   INT                 NOT NULL,
        [AsignaturaId]   INT                 NOT NULL,
        [Parcial]        TINYINT             NOT NULL,   -- 1, 2, 3 o 4
        [Nota]           DECIMAL(5,2)        NOT NULL,   -- 0.00 – 100.00
        [FechaRegistro]  DATETIME2           NOT NULL
                         CONSTRAINT [DF_NotasParciales_FechaRegistro]
                         DEFAULT SYSUTCDATETIME(),
        CONSTRAINT [PK_NotasParciales]
            PRIMARY KEY CLUSTERED ([Id] ASC),

        -- Un estudiante no puede tener dos notas para la misma
        -- asignatura y el mismo parcial
        CONSTRAINT [UQ_NotasParciales_Est_Asig_Parcial]
            UNIQUE ([EstudianteId], [AsignaturaId], [Parcial]),

        -- Parcial solo puede ser 1, 2, 3 o 4
        CONSTRAINT [CK_NotasParciales_Parcial]
            CHECK ([Parcial] IN (1, 2, 3, 4)),

        -- Nota en rango 0 – 100
        CONSTRAINT [CK_NotasParciales_Nota]
            CHECK ([Nota] >= 0 AND [Nota] <= 100),

        -- FK -> Estudiantes (si el estudiante se elimina, sus notas se eliminan)
        CONSTRAINT [FK_NotasParciales_Estudiante]
            FOREIGN KEY ([EstudianteId])
            REFERENCES [dbo].[Estudiantes]([Id])
            ON DELETE CASCADE,

        -- FK -> Asignaturas (restrict: no se puede borrar una asignatura con notas)
        CONSTRAINT [FK_NotasParciales_Asignatura]
            FOREIGN KEY ([AsignaturaId])
            REFERENCES [dbo].[Asignaturas]([Id])
            ON DELETE NO ACTION
    );

    PRINT 'Tabla NotasParciales creada correctamente.';
END
ELSE
BEGIN
    PRINT 'La tabla NotasParciales ya existia — no se modifico.';
END
GO




-- ── 2. Índice de consulta rápida por estudiante ─────────────
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.NotasParciales')
      AND name = 'IX_NotasParciales_EstudianteId'
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_NotasParciales_EstudianteId]
        ON [dbo].[NotasParciales] ([EstudianteId] ASC)
        INCLUDE ([AsignaturaId], [Parcial], [Nota]);

    PRINT 'Indice IX_NotasParciales_EstudianteId creado.';
END
ELSE
BEGIN
    PRINT 'El indice IX_NotasParciales_EstudianteId ya existia.';
END
GO

-- ── 3. Índice de consulta rápida por asignatura+parcial ─────
--    Util cuando el docente carga la grilla de un parcial
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.NotasParciales')
      AND name = 'IX_NotasParciales_Asignatura_Parcial'
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_NotasParciales_Asignatura_Parcial]
        ON [dbo].[NotasParciales] ([AsignaturaId] ASC, [Parcial] ASC);

    PRINT 'Indice IX_NotasParciales_Asignatura_Parcial creado.';
END
ELSE
BEGIN
    PRINT 'El indice IX_NotasParciales_Asignatura_Parcial ya existia.';
END
GO

-- ── 4. Verificar estructura creada ──────────────────────────
SELECT
    c.COLUMN_NAME,
    c.DATA_TYPE,
    c.CHARACTER_MAXIMUM_LENGTH,
    c.NUMERIC_PRECISION,
    c.NUMERIC_SCALE,
    c.IS_NULLABLE,
    c.COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS c
WHERE c.TABLE_NAME = 'NotasParciales'
ORDER BY c.ORDINAL_POSITION;
GO

-- ── 5. Verificar constraints creados ───────────────────────
SELECT
    tc.CONSTRAINT_TYPE,
    tc.CONSTRAINT_NAME
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
WHERE tc.TABLE_NAME = 'NotasParciales'
ORDER BY tc.CONSTRAINT_TYPE, tc.CONSTRAINT_NAME;
GO
