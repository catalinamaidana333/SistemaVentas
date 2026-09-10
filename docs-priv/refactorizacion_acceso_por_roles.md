# 📝 Documento de Refactorización: Arquitectura en Capas y Gestión de Permisos (ABM Usuarios)

Este documento detalla los últimos cambios implementados en el módulo de Usuarios. El objetivo principal de esta refactorización fue limpiar el código, aplicar buenas prácticas y garantizar el cumplimiento estricto de la arquitectura de 3 capas exigida para el proyecto.

## 1. Implementación del Enum `Roles` (Capa `Entities`)

**¿Qué se implementó?**
Se definió una enumeración llamada `Roles` dentro de la capa transversal de `Entities`, la cual mapea los IDs de los roles de la base de datos con identificadores semánticos en C#:
- `Gerente = 1`
- `Vendedor = 2`
- `Supervisor = 3`

**¿Por qué lo hicimos?**
Para erradicar la mala práctica de usar "Números Mágicos" (Magic Numbers). Previamente, validábamos roles usando números estáticos (ej. `idRol == 1`), lo cual genera dos problemas graves que ahora están resueltos:
1. **Legibilidad:** El código ahora se explica a sí mismo. Cualquier integrante del equipo puede leer `Roles.Gerente` y entender la lógica sin necesidad de ir a consultar las tablas de la base de datos.
2. **Mantenibilidad:** Si a futuro el ID de un rol cambia por una migración en la base de datos, solo modificamos el número en el `enum` de `Entities`. Todo el sistema se actualiza automáticamente, evitando errores humanos por olvidos de reemplazar IDs aislados en distintas capas.

---

## 2. Traslado de Lógica a `UsuarioBLL` (Capa Lógica de Negocio)

**¿Qué se implementó?**
Se agregaron métodos de validación booleanos en `UsuarioBLL.cs` (ej. `PuedeAccederPantallaUsuarios(int idRol)` o `PuedeCrearOEditarUsuarios(int idRol)`). Estos métodos reciben el ID del rol extraído de la DAL y aplican la lógica de comparación utilizando el `enum Roles`.

**¿Por qué lo hicimos?**
Porque determinar *qué tipo de usuario tiene autorización para ejecutar una acción* es, por definición, una **Regla de Negocio**. Como tal, su ubicación obligatoria es la capa BLL. Si dejábamos esta lógica en la vista, estábamos violando el principio de separación de responsabilidades de la arquitectura.

---

## 3. Limpieza del Code-Behind en `UsuarioControl.xaml.cs` (Capa de Presentación)

**¿Qué eliminamos del archivo?**
Se removió por completo la evaluación directa de IDs numéricos (los `if (idRol == 2)`) y la toma de decisiones de negocio desde el archivo visual de la pantalla.

**¿Por qué lo hicimos y cómo funciona ahora?**
La capa de Presentación debe ser "ignorante" de las reglas del negocio; no necesita saber qué es un Vendedor o un Gerente. Su único trabajo es gestionar controles visuales (botones, grillas, modales). 

Ahora, el archivo `.xaml.cs` simplemente le consulta a la BLL (ej. `_usuarioLogica.PuedeCrearOEditarUsuarios(idRolActual)`). Si la BLL responde `false`, la vista se limita a acatar la orden y ocultar los elementos de la interfaz correspondientes (modificando la propiedad `Visibility` a `Collapsed` o seteando el `DataGrid` en `IsReadOnly = true`). 

**Conclusión del impacto:** 
Logramos un desacoplamiento total. Respetamos la arquitectura en capas al 100%, el archivo visual queda mucho más limpio y la lógica de seguridad del sistema está centralizada en la capa de Negocio, lista para ser evaluada sin penalizaciones.