CREATE TABLE HistorialBackups (
    IdBackup INT IDENTITY(1,1) PRIMARY KEY,
    NombreArchivo VARCHAR(255) NOT NULL,
    FechaHora DATETIME NOT NULL,
    TipoOperacion VARCHAR(50) NOT NULL,
    Estado VARCHAR(50) NOT NULL
);