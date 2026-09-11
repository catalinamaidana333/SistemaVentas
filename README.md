# 🛒 Sistema de Gestión para Minimercado y Kiosco

![.NET](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white)
![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=c-sharp&logoColor=white)
![WPF](https://img.shields.io/badge/WPF-blue?style=for-the-badge)

Proyecto desarrollado para la cátedra **Taller de Programación 2**. Este sistema es una aplicación de escritorio diseñada para administrar de manera eficiente las operaciones diarias de un minimercado o kiosco, abarcando tanto el flujo de ventas como el reabastecimiento (compras).

## Características Principales

El sistema está diseñado con una arquitectura robusta y cuenta con control de acceso basado en roles para garantizar la seguridad y correcta delegación de tareas.

### 👥 Gestión de Roles
El acceso a los módulos está restringido según el nivel de usuario:
*   **Gerente:** Acceso total al sistema. Gestión de usuarios, visualización completa de métricas, y control absoluto de compras y ventas.
*   **Supervisor:** Perfil orientado a la auditoría. Capacidad para visualizar operaciones, monitorear el stock y revisar el registro de ventas y compras sin permisos de modificación estructural.
*   **Vendedor:** Perfil operativo. Acceso exclusivo al módulo de punto de venta (POS) para registrar las transacciones diarias con los clientes.

### 📦 Módulos Operativos
*   **Módulo de Ventas:** Interfaz ágil para la facturación a clientes, cálculo de totales y actualización de stock en tiempo real.
*   **Módulo de Compras:** Registro de ingreso de mercadería y gestión de reabastecimiento del inventario.

## 🎨 Diseño y UI/UX

La interfaz gráfica fue construida utilizando **Windows Presentation Foundation (WPF)**, priorizando una experiencia de usuario limpia y moderna. 
La paleta de colores institucional del proyecto se basa en:

*   **Azul y Oro (Boca Juniors):** Colores de acento utilizados estratégicamente en botones, alertas y elementos interactivos principales para guiar la atención del usuario (Call to Action).

## 🛠️ Tecnologías Utilizadas

*   **Lenguaje:** C#
*   **Framework:** .NET 
*   **Interfaz Gráfica:** WPF (Windows Presentation Foundation)


## 🚀 Instalación y Uso

1. Clonar este repositorio en tu máquina local:
   ```bash
   git clone