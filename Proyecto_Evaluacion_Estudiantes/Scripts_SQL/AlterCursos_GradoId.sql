
USE [EvaluacionEstudiantes]
GO

-- Agregar GradoId si no existe
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Cursos' AND COLUMN_NAME = 'GradoId'
)
BEGIN
    ALTER TABLE [dbo].[Cursos]
        ADD [GradoId] INT NULL;
    PRINT 'Columna GradoId agregada a Cursos.';
END
ELSE
BEGIN
    PRINT 'La columna GradoId ya existia en Cursos.';
END
GO


SELECT * FROM CURSOS
-- Agregar la FK de GradoId -> Grados (si la tabla Grados ya existe)
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS
    WHERE CONSTRAINT_NAME = 'FK_Cursos_Grado'
)
BEGIN
    ALTER TABLE [dbo].[Cursos]
        ADD CONSTRAINT [FK_Cursos_Grado]
        FOREIGN KEY ([GradoId]) REFERENCES [dbo].[Grados]([Id]);
    PRINT 'FK FK_Cursos_Grado creada.';
END
GO

-- Verificar columnas actuales de Cursos
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Cursos'
ORDER BY ORDINAL_POSITION;
GO
