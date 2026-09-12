USE SistemaVentas;
MERGE dbo.Categoria AS target
USING (VALUES    
    ('Bebidas Sin Alcohol', 1),    
    ('Bebidas con Alcohol', 1),    
    ('Golosinas y Snacks', 1),
    ('Infusiones y Desayuno', 1),
    ('Galletitas y Panificados', 1),
    ('Lácteos y Frescos', 1),
    ('Fiambres y Embutidos', 1),
    ('Comestibles Secos', 1),
    ('Conservas y Aderezos', 1),
    ('Limpieza', 1),    
    ('Higiene Personal', 1),
    ('Tabaquería', 1),
    ('Varios y Bazar', 1)
) AS source (nombre, activa)
ON target.nombre = source.nombre
WHEN NOT MATCHED THEN    
    INSERT (nombre, activa) VALUES (source.nombre, source.activa);