# CineTracker

Aplicación web para descubrir películas, gestionar una lista personal de seguimiento y llevar el control de lo que has visto y tus favoritos. Cada usuario tiene su propia cuenta y su propia lista independiente.

---

## Tecnologías utilizadas

| Capa | Tecnología |
|---|---|
| Framework | ASP.NET Core + Blazor Server (.NET 10) |
| Base de datos | PostgreSQL en la nube via [Supabase](https://supabase.com) |
| ORM | Entity Framework Core 9 + Npgsql |
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
├── Migrations/                          # Migraciones generadas por EF Core para PostgreSQL
│
├── Models/
│   ├── Usuario.cs                       # Modelo de usuario (tabla usuarios)
│   ├── WatchListItem.cs                 # Modelo de ítem en la lista (tabla watch_list_items)
│   ├── Movie.cs                         # Modelo de película local (tabla movies)
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
├── appsettings.json                     # Configuración (URLs de TMDB, opciones generales)
├── TmdbSettings.cs                      # Clase de configuración tipada para TMDB
└── Program.cs                           # Punto de entrada: servicios, middleware y rutas
```

---

## Base de datos

La base de datos se gestiona con **Entity Framework Core + Npgsql** (Code First) sobre **PostgreSQL en Supabase**. Los nombres de tablas y columnas siguen la convención `snake_case` de PostgreSQL, configurada automáticamente con `UseSnakeCaseNamingConvention()`.

### Tablas

#### `usuarios`
Almacena las cuentas de usuario registradas en la aplicación.

| Columna | Tipo | Descripción |
|---|---|---|
| `id` | integer (PK) | Clave primaria autoincremental |
| `nombre_usuario` | varchar(50) UNIQUE | Nombre visible del usuario |
| `email` | varchar(200) UNIQUE | Email usado para iniciar sesión |
| `password_hash` | text | Contraseña hasheada con PBKDF2 |
| `fecha_creacion` | timestamptz | Fecha de creación de la cuenta (UTC) |

#### `watch_list_items`
Guarda las películas que cada usuario ha agregado a su lista personal.

| Columna | Tipo | Descripción |
|---|---|---|
| `id` | integer (PK) | Clave primaria |
| `usuario_id` | integer (FK → usuarios.id) | Usuario dueño del ítem |
| `tmdb_id` | integer | ID de la película en la API de TMDB |
| `title` | varchar(200) | Título de la película |
| `poster_path` | varchar(500) | Ruta relativa del poster en TMDB |
| `release_date` | text | Fecha de estreno (formato YYYY-MM-DD) |
| `vote_average` | double precision | Calificación promedio de TMDB (0-10) |
| `is_favorite` | boolean | Marcada como favorita |
| `is_watched` | boolean | Marcada como vista |
| `date_added` | timestamptz | Fecha en que se agregó a la lista (UTC) |

> La relación `usuario_id → usuarios.id` tiene `ON DELETE CASCADE`: si se elimina un usuario, se eliminan automáticamente todos sus ítems de watchlist.

#### `movies`
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

El sistema de autenticación es **propio** (sin ASP.NET Core Identity completo). Usa Cookie Authentication del framework con lógica de usuarios personalizada sobre PostgreSQL.

### Registro

1. El usuario completa el formulario en `/registro`
2. Se valida que el email y nombre de usuario no existan en la base de datos
3. La contraseña se hashea con `PasswordHasher<T>` (algoritmo PBKDF2 con sal aleatoria y 100.000 iteraciones)
4. Se inserta el nuevo usuario en la tabla `usuarios`
5. Se inicia sesión automáticamente y se redirige al inicio

### Inicio de sesión

1. El usuario completa el formulario en `/login`
2. Se busca el usuario por email en la base de datos
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
  → Busca usuario por email en PostgreSQL
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
- Proyecto en [Supabase](https://supabase.com) (plan gratuito es suficiente)
- Cuenta en [TMDB](https://www.themoviedb.org/) para obtener una API Key gratuita

### 1. Clonar el repositorio

```bash
git clone https://github.com/KnNgZmN/ProyectoCineTracker.git
cd ProyectoCineTracker/CineTracker
```

### 2. Configurar el connection string de Supabase

El connection string **nunca** debe ir en `appsettings.json`. Se configura con User Secrets:

```bash
dotnet user-secrets set "ConnectionStrings:CineTrackerDB" \
  "Host=aws-X-us-east-1.pooler.supabase.com;Database=postgres;Username=postgres.TU_PROJECT_REF;Password=TU_PASSWORD;Port=6543;SSL Mode=Require;Trust Server Certificate=true;No Reset On Close=true;Max Auto Prepare=0"
```

> Los valores exactos (`Host`, `Username`, `Password`) se obtienen desde **Project Settings → Database → Connection pooling** en el dashboard de Supabase. Usa el modo **Transaction (puerto 6543)** para soportar múltiples usuarios simultáneos en el plan gratuito.

### 3. Configurar la API Key de TMDB

```bash
dotnet user-secrets set "TmdbSettings:ApiKey" "TU_API_KEY_DE_TMDB"
```

Para obtener una API Key gratuita: https://www.themoviedb.org/settings/api

### 4. Aplicar las migraciones

```bash
dotnet ef database update
```

> **Nota:** Si usas el pooler en Transaction Mode (puerto 6543), algunas migraciones DDL pueden fallar. En ese caso, cambia temporalmente el puerto a `5432` (Session Mode) solo para ejecutar las migraciones y luego vuelve a `6543`.

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

# Aplicar migraciones pendientes a Supabase
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
Maneja el registro y login de usuarios contra PostgreSQL (Supabase). Usa `PasswordHasher<Usuario>` de ASP.NET Core para hash seguro de contraseñas. Se registra como `Scoped` (una instancia por solicitud HTTP).

### `WatchlistService`
CRUD completo de la lista de películas. Todos sus métodos reciben `int usuarioId` para que cada usuario solo pueda ver y modificar sus propios datos. Usa `IDbContextFactory<CineTrackerContext>` para operaciones seguras en el entorno concurrente de Blazor Server.

---

## Seguridad

- Las contraseñas se almacenan con **PBKDF2 + sal aleatoria** — nunca en texto plano
- Las cookies de sesión están **cifradas** por ASP.NET Core
- Los formularios incluyen **token antifalsificación (CSRF)** con `<AntiforgeryToken />`
- La API Key de TMDB y el connection string se gestionan con **User Secrets** — no se suben al repositorio
- Cada usuario solo puede acceder y modificar **sus propios datos**, ya que todas las consultas filtran por `usuario_id`

---

## Autores

| Nombre         | Rol       |
|---|---|
| Alejandra Toro | Developer |
| Kevin Guzmán   | Developer |
