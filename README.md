# CineTracker

Aplicación web para descubrir películas, gestionar una lista personal de seguimiento y llevar el control de lo que has visto y tus favoritos. Cada usuario tiene su propia cuenta y su propia lista independiente.

---

## Tecnologías utilizadas

| Capa | Tecnología |
|---|---|
| Framework | ASP.NET Core + Blazor Server (.NET 10) |
| Base de datos | SQL Server |
| ORM | Entity Framework Core 10 |
| API externa | TMDB (The Movie Database) |
| Autenticación | Cookie Authentication (ASP.NET Core) |
| Hash de contraseñas | PasswordHasher PBKDF2 (ASP.NET Core Identity) |
| Estilos | CSS personalizado (tema oscuro estilo Netflix) |

---

## Funcionalidades

- **Página principal** — películas populares y mejor valoradas traídas en tiempo real desde la API de TMDB
- **Buscador** — búsqueda de películas por nombre usando la API de TMDB
- **Detalle de película** — sinopsis, géneros, duración, calificación y poster
- **Mi Lista** — lista personal por usuario: agregar, marcar como vista, marcar como favorita, eliminar
- **Registro de cuenta** — crea una cuenta con nombre de usuario, email y contraseña
- **Inicio de sesión** — acceso con email y contraseña, sesión persistente de 7 días
- **Protección de rutas** — Mi Lista requiere inicio de sesión; el resto de páginas son públicas

---

## Estructura del proyecto

```
CineTracker/
│
├── Components/
│   ├── App.razor                        # Componente raíz: estructura HTML global
│   ├── Routes.razor                     # Router con AuthorizeRouteView
│   ├── _Imports.razor                   # Usings globales para todos los componentes
│   │
│   ├── Layout/
│   │   ├── MainLayout.razor             # Layout principal con navbar y estado de sesión
│   │   ├── AuthLayout.razor             # Layout vacío para páginas de login/registro
│   │   └── MainLayout.razor.css         # Estilos del layout principal
│   │
│   ├── Pages/
│   │   ├── Home.razor                   # Página principal: populares y mejor valoradas
│   │   ├── Buscar.razor                 # Buscador de películas
│   │   ├── DetallePelicula.razor        # Detalle completo de una película
│   │   ├── MiLista.razor                # Lista personal del usuario [Authorize]
│   │   ├── Login.razor                  # Formulario de inicio de sesión (SSR)
│   │   ├── Registro.razor               # Formulario de registro (SSR)
│   │   ├── Error.razor                  # Página de error
│   │   └── NotFound.razor               # Página 404
│   │
│   └── Shared/
│       ├── MovieCard.razor              # Tarjeta reutilizable de película
│       └── RedirectToLogin.razor        # Componente auxiliar de redirección
│
├── Data/
│   └── CineTrackerContext.cs            # DbContext de Entity Framework
│
├── Migrations/                          # Migraciones generadas automáticamente por EF Core
│
├── Models/
│   ├── Usuario.cs                       # Modelo de usuario (tabla Usuarios)
│   ├── WatchListItem.cs                 # Modelo de ítem en la lista (tabla WatchlistItems)
│   ├── Movie.cs                         # Modelo de película local
│   └── TmdbModels.cs                    # Modelos que mapean la respuesta JSON de TMDB
│
├── Services/
│   ├── AuthService.cs                   # Registro, login y hash de contraseñas
│   ├── WatchlistService.cs              # CRUD de la lista personal por usuario
│   └── TmdbService.cs                   # Consumo de la API REST de TMDB
│
├── wwwroot/
│   └── app.css                          # Estilos globales de la aplicación
│
├── appsettings.json                     # Configuración (cadena de conexión, URLs de TMDB)
├── TmdbSettings.cs                      # Clase de configuración tipada para TMDB
└── Program.cs                           # Punto de entrada: servicios, middleware y rutas
```

---

## Base de datos

La base de datos se llama `CineTrackerDB` y se gestiona con **Entity Framework Core** (Code First). Las migraciones se generan y aplican con la CLI de EF.

### Tablas

#### `Usuarios`
Almacena las cuentas de usuario registradas en la aplicación.

| Columna | Tipo | Descripción |
|---|---|---|
| `Id` | int (PK, IDENTITY) | Clave primaria autoincremental |
| `NombreUsuario` | nvarchar(50) UNIQUE | Nombre visible del usuario |
| `Email` | nvarchar(200) UNIQUE | Email usado para iniciar sesión |
| `PasswordHash` | nvarchar(max) | Contraseña hasheada con PBKDF2 |
| `FechaCreacion` | datetime2 | Fecha de creación de la cuenta (UTC) |

#### `WatchlistItems`
Guarda las películas que cada usuario ha agregado a su lista personal.

| Columna | Tipo | Descripción |
|---|---|---|
| `Id` | int (PK, IDENTITY) | Clave primaria |
| `UsuarioId` | int (FK → Usuarios.Id) | Usuario dueño del ítem |
| `TmdbId` | int | ID de la película en la API de TMDB |
| `Title` | nvarchar(200) | Título de la película |
| `PosterPath` | nvarchar(500) | Ruta relativa del poster en TMDB |
| `ReleaseDate` | nvarchar | Fecha de estreno (formato YYYY-MM-DD) |
| `VoteAverage` | float | Calificación promedio de TMDB (0-10) |
| `IsFavorite` | bit | Marcada como favorita |
| `IsWatched` | bit | Marcada como vista |
| `DateAdded` | datetime2 | Fecha en que se agregó a la lista |

> La relación `UsuarioId → Usuarios.Id` tiene `ON DELETE CASCADE`: si se elimina un usuario, se eliminan automáticamente todos sus ítems de watchlist.

#### `Movies`
Tabla de películas locales (uso interno de EF Core).

---

## API externa: TMDB (The Movie Database)

La aplicación consume la API REST pública de TMDB para obtener información de películas en tiempo real. No se almacenan películas localmente; solo se guarda una copia de los datos básicos cuando el usuario agrega una película a su lista.

**URL base de la API:** `https://api.themoviedb.org/3`
**URL base de imágenes:** `https://image.tmdb.org/t/p/w500`
**Documentación oficial:** https://developer.themoviedb.org/docs

### Endpoints utilizados

| Método | Endpoint | Descripción |
|---|---|---|
| GET | `/movie/popular` | Películas más populares del momento |
| GET | `/movie/top_rated` | Películas mejor calificadas por usuarios de TMDB |
| GET | `/movie/now_playing` | Películas actualmente en cartelera |
| GET | `/movie/{id}` | Detalle completo: sinopsis, géneros, duración, tagline |
| GET | `/search/movie?query=` | Búsqueda de películas por nombre |

Todos los endpoints incluyen los parámetros:
- `api_key` — clave de autenticación de TMDB
- `language=es-ES` — respuestas en español

### Modelos de respuesta TMDB

| Clase | Descripción |
|---|---|
| `TmdbResponse` | Envuelve los resultados paginados (page, results, total_pages, total_results) |
| `TmdbMovie` | Película en listado: id, title, overview, poster_path, release_date, vote_average |
| `TmdbMovieDetail` | Detalle completo: agrega runtime, genres, tagline, backdrop_path |
| `TmdbGenre` | Género de película con id y name |

Las propiedades usan `[JsonPropertyName]` para mapear los nombres `snake_case` del JSON de TMDB a `PascalCase` de C#. Por ejemplo: `"poster_path"` → `PosterPath`.

---

## Autenticación

El sistema de autenticación es **propio** (sin ASP.NET Core Identity completo). Usa Cookie Authentication del framework con lógica de usuarios personalizada en SQL Server.

### Registro

1. El usuario completa el formulario en `/registro`
2. Se valida que el email y nombre de usuario no existan en la base de datos
3. La contraseña se hashea con `PasswordHasher<T>` (algoritmo PBKDF2 con sal aleatoria y 100.000 iteraciones)
4. Se inserta el nuevo usuario en la tabla `Usuarios`
5. Se inicia sesión automáticamente y se redirige al inicio

### Inicio de sesión

1. El usuario completa el formulario en `/login`
2. Se busca el usuario por email en SQL Server
3. Se verifica la contraseña con `VerifyHashedPassword`
4. Si es correcta, se crean los **Claims** del usuario:
   - `NameIdentifier` → ID en la base de datos
   - `Name` → nombre de usuario (mostrado en la navbar)
   - `Email` → email del usuario
5. Se escribe una cookie cifrada en el navegador con `HttpContext.SignInAsync`
6. La sesión dura **7 días** con renovación automática (`SlidingExpiration`)

### Cierre de sesión

El endpoint `GET /logout` elimina la cookie con `SignOutAsync` y redirige a la página principal.

### Protección de rutas

- `MiLista.razor` tiene el atributo `[Authorize]`
- Si un usuario no autenticado intenta acceder, `RedirectToLogin.razor` lo redirige a `/login`
- El resto de páginas (Home, Buscar, DetallePelicula) son públicas

### Flujo completo

```
Usuario visita /login
       │
       ▼
  Llena el formulario y envía (HTTP POST)
       │
       ▼
  AuthService.LoginAsync()
  → Busca usuario por email en SQL Server
  → VerifyHashedPassword (PBKDF2)
       │
    ┌──┴─────────────────┐
  Falla                 OK
    │                    │
  Muestra error        Crea Claims
                       SignInAsync() → Cookie cifrada en el navegador
                       NavigateTo("/", forceLoad: true)
```

---

## Configuración y puesta en marcha

### Requisitos previos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (local o remoto)
- Cuenta en [TMDB](https://www.themoviedb.org/) para obtener una API Key gratuita

### 1. Clonar el repositorio

```bash
git clone https://github.com/KnNgZmN/ProyectoCineTracker.git
cd ProyectoCineTracker/CineTracker
```

### 2. Configurar la cadena de conexión

Editar `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "CineTrackerDB": "Server=TU_SERVIDOR;Database=CineTrackerDB;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "TmdbSettings": {
    "BaseUrl": "https://api.themoviedb.org/3",
    "ImageBaseUrl": "https://image.tmdb.org/t/p/w500"
  }
}
```

### 3. Configurar la API Key de TMDB

La API Key **nunca** debe ir en `appsettings.json` ni subirse al repositorio. Se configura con User Secrets:

```bash
dotnet user-secrets set "TmdbSettings:ApiKey" "TU_API_KEY_DE_TMDB"
```

Para obtener una API Key gratuita: https://www.themoviedb.org/settings/api

### 4. Aplicar las migraciones

```bash
dotnet ef database update
```

Esto crea automáticamente la base de datos `CineTrackerDB` con todas las tablas y relaciones.

### 5. Ejecutar la aplicación

```bash
dotnet run
```

La aplicación estará disponible en `https://localhost:5001`.

---

## Comandos útiles de Entity Framework

```bash
# Crear una nueva migración después de cambiar un modelo
dotnet ef migrations add NombreDeLaMigracion

# Aplicar migraciones pendientes a la base de datos
dotnet ef database update

# Ver el estado de todas las migraciones (Applied / Pending)
dotnet ef migrations list

# Revertir la última migración (antes de aplicarla)
dotnet ef migrations remove
```

---

## Servicios principales

### `TmdbService`
Consume la API REST de TMDB usando `HttpClient`. Serializa y deserializa JSON con `System.Text.Json`. Se registra con `AddHttpClient<TmdbService>()` para gestión automática del pool de conexiones HTTP.

### `AuthService`
Maneja el registro y login de usuarios contra SQL Server. Usa `PasswordHasher<Usuario>` de ASP.NET Core para hash seguro de contraseñas. Se registra como `Scoped` (una instancia por solicitud HTTP).

### `WatchlistService`
CRUD completo de la lista de películas. Todos sus métodos reciben `int usuarioId` para que cada usuario solo pueda ver y modificar sus propios datos. Usa `IDbContextFactory<CineTrackerContext>` para operaciones seguras en el entorno concurrente de Blazor Server.

---

## Seguridad

- Las contraseñas se almacenan con **PBKDF2 + sal aleatoria** — nunca en texto plano
- Las cookies de sesión están **cifradas** por ASP.NET Core
- Los formularios incluyen **token antifalsificación (CSRF)** con `<AntiforgeryToken />`
- La API Key de TMDB se gestiona con **User Secrets** — no se sube al repositorio
- Cada usuario solo puede acceder y modificar **sus propios datos** en la base de datos, ya que todas las consultas filtran por `UsuarioId`

---

## Autores

| Nombre         | Rol       |
| Alejandra Toro | Developer |
| Kevin Guzmán   | Developer |
