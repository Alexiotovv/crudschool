using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
// Habilitamos soporte para que la API sea fácil de probar en el navegador (Swagger)

// 📌 Tu misma cadena de conexión de Windows Forms
string conexionString = "Data Source=mi_base_datos.db";

// 📌 Inicialización: Crear la tabla al arrancar la API (Tu método CrearTablaSiNoExiste)
using (var conexion = new SqliteConnection(conexionString))
{
    conexion.Open();
    string query = @"CREATE TABLE IF NOT EXISTS Persona (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Nombre TEXT NOT NULL,
                        Edad INTEGER NOT NULL
                     );";
    using (var comando = new SqliteCommand(query, conexion))
    {
        comando.ExecuteNonQuery();
    }
}

// ==========================================
// 🚀 ENDPOINTS DEL CRUD (API WEB)
// ==========================================

// 1. LEER TODO (Tu método CargarDatos)
app.MapGet("/personas", () =>
{
    var lista = new List<Persona>();
    using (var conexion = new SqliteConnection(conexionString))
    {
        conexion.Open();
        string query = "SELECT * FROM Persona";
        using (var comando = new SqliteCommand(query, conexion))
        {
            using (var lector = comando.ExecuteReader())
            {
                while (lector.Read())
                {
                    lista.Add(new Persona(
                        lector.GetInt32(0),
                        lector.GetString(1),
                        lector.GetInt32(2)
                    ));
                }
            }
        }
    }
    return Results.Ok(lista);
});

// 2. CREAR / AGREGAR (Tu método btnAgregar_Click)
app.MapPost("/personas", (PersonaNueva nuevaPersona) =>
{
    using (var conexion = new SqliteConnection(conexionString))
    {
        conexion.Open();
        string query = "INSERT INTO Persona (Nombre, Edad) VALUES (@nombre, @edad)";
        using (var comando = new SqliteCommand(query, conexion))
        {
            comando.Parameters.AddWithValue("@nombre", nuevaPersona.Nombre);
            comando.Parameters.AddWithValue("@edad", nuevaPersona.Edad);
            comando.ExecuteNonQuery();
        }
    }
    return Results.Created("/personas", nuevaPersona);
});

// 3. MODIFICAR / ACTUALIZAR (Tu método btnModificar_Click)
app.MapPut("/personas/{id}", (int id, PersonaNueva datosActualizados) =>
{
    using (var conexion = new SqliteConnection(conexionString))
    {
        conexion.Open();
        string query = "UPDATE Persona SET Nombre = @nombre, Edad = @edad WHERE Id = @id";
        using (var comando = new SqliteCommand(query, conexion))
        {
            comando.Parameters.AddWithValue("@id", id);
            comando.Parameters.AddWithValue("@nombre", datosActualizados.Nombre);
            comando.Parameters.AddWithValue("@edad", datosActualizados.Edad);

            int filasAfectadas = comando.ExecuteNonQuery();
            if (filasAfectadas == 0) return Results.NotFound("Persona no encontrada.");
        }
    }
    return Results.Ok("Persona actualizada con éxito.");
});

// 4. ELIMINAR (Tu método btnEliminar_Click)
app.MapDelete("/personas/{id}", (int id) =>
{
    using (var conexion = new SqliteConnection(conexionString))
    {
        conexion.Open();
        string query = "DELETE FROM Persona WHERE Id = @id";
        using (var comando = new SqliteCommand(query, conexion))
        {
            comando.Parameters.AddWithValue("@id", id);

            int filasAfectadas = comando.ExecuteNonQuery();
            if (filasAfectadas == 0) return Results.NotFound("Persona no encontrada.");
        }
    }
    return Results.Ok("Persona eliminada con éxito.");
});

app.Run();

// ==========================================
// 📦 MODELOS DE DATOS (REEMPLAZAN LOS TEXTBOX)
// ==========================================
public record Persona(int Id, string Nombre, int Edad);
public record PersonaNueva(string Nombre, int Edad);
