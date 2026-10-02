---
name: wpf-minimercado-dev
description: Describa lo que hace esta capacidad y cuándo usarla. Incluya palabras clave que ayuden a los agentes a identificar las tareas pertinentes.
---

Defina la funcionalidad proporcionada por esta capacidad, incluidas instrucciones y ejemplos detallados.

Desarrollo de Minimercado WPF

Este skill garantiza que el código generado para el sistema de gestión del kiosco/minimercado cumpla estrictamente con la arquitectura de capas establecida, usando C#, .NET y SQL.

## Cuándo usar este skill
- Al crear ventanas, páginas o componentes visuales en WPF.
- Al modelar entidades, escribir lógica de negocio o redactar consultas SQL para el sistema de ventas.

## Reglas de Arquitectura Estricta

1. **Capa de Presentación (Frontend):**
   - Desarrollada en WPF.
   - **Estilos:** Toda tipografía y color DEBE referenciarse desde `App.xaml` (mediante `StaticResource` o `DynamicResource`). Prohibido colocar colores o fuentes directamente en la vista.
   - Solo se comunica con la Capa de Negocio.

2. **Capa de Entidad:**
   - Contiene los modelos de datos de dominio.
   - **Regla estricta:** Solo puede contener propiedades (getters y setters). No debe incluir ningún método, función, ni validaciones.

3. **Capa de Negocio:**
   - Procesa todas las validaciones y métodos requeridos por el negocio (ej. validación de stock, cálculo de totales).
   - Coordina la información entre la Capa de Presentación y la Capa de Acceso a Datos.

4. **Capa de Acceso a Datos:**
   - Interactúa directamente con la base de datos.
   - Aquí se alojan exclusivamente las consultas en lenguaje SQL y la configuración de comandos.
   - Devuelve información transformada en objetos de la Capa de Entidad.

## Gotchas
- **Aislamiento absoluto:** La Capa de Presentación jamás debe interactuar con Acceso a Datos ni contener sentencias SQL.
- **Sin lógica cruzada:** Ninguna validación de negocio debe filtrarse hacia el Acceso a Datos o la Presentación.