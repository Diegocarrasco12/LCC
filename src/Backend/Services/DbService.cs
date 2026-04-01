using MySqlConnector;
using Microsoft.Data.SqlClient;
using System;
using System.IO;
using System.Text.Json;

namespace LogisticControlCenter.Services
{
    /// <summary>
    /// DbService
    /// -------------------------------------------
    /// Servicio central de conexiones a bases de datos
    ///
    /// Bases soportadas:
    /// - MySQL consumo_papel
    /// - MySQL control_bins
    /// - SQL Server SAP
    /// </summary>
    public class DbService
    {
        private readonly DbSettings _settings;

        public DbService(DbSettings settings)
        {
            _settings = settings;
        }

        /* =========================================
           MYSQL - CONSUMO PAPEL
        ========================================= */

        public MySqlConnection GetConsumoPapelConnection()
        {
            var builder = new MySqlConnectionStringBuilder
            {
                Server = _settings.MySqlHost,
                Database = "consumo_papel",
                UserID = _settings.MySqlUser,
                Password = _settings.MySqlPassword,

                CharacterSet = "utf8mb4",

                AllowPublicKeyRetrieval = true,
                AllowUserVariables = true,

                SslMode = MySqlSslMode.None,

                Pooling = true,
                MinimumPoolSize = 0,
                MaximumPoolSize = 25,

                ConnectionTimeout = 5,
                DefaultCommandTimeout = 30
            };

            return new MySqlConnection(builder.ConnectionString);
        }

        /* =========================================
           MYSQL - CONTROL BINS
        ========================================= */

        public MySqlConnection GetBinsConnection()
        {
            var builder = new MySqlConnectionStringBuilder
            {
                Server = _settings.MySqlHost,
                Database = "control_bins",
                UserID = _settings.MySqlUser,
                Password = _settings.MySqlPassword,

                CharacterSet = "utf8mb4",

                AllowPublicKeyRetrieval = true,
                AllowUserVariables = true,

                SslMode = MySqlSslMode.None,

                Pooling = true,
                MinimumPoolSize = 0,
                MaximumPoolSize = 25,

                ConnectionTimeout = 5,
                DefaultCommandTimeout = 30
            };

            return new MySqlConnection(builder.ConnectionString);
        }

        /* =========================================
           SQL SERVER - SAP
        ========================================= */

        public SqlConnection GetSapConnection()
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = _settings.SapHost,
                InitialCatalog = _settings.SapDatabase,
                UserID = _settings.SapUser,
                Password = _settings.SapPassword,

                Encrypt = false,
                TrustServerCertificate = true,

                ConnectTimeout = 5,

                ApplicationName = "LogisticControlCenter"
            };

            return new SqlConnection(builder.ConnectionString);
        }

        /* =========================================
           CONFIGURAR SESIÓN SAP (igual que PHP)
        ========================================= */

        public void ConfigureSapSession(SqlConnection conn)
        {
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
                SET NOCOUNT ON;
            ";

            cmd.ExecuteNonQuery();
        }
    }

    /* =========================================
       CONFIGURACIÓN
    ========================================= */

    public class DbSettings
    {
        /* MYSQL */

        public string MySqlHost { get; set; } = "192.168.1.70";
        public string MySqlUser { get; set; } = "tickera";
        public string MySqlPassword { get; set; } = "admin123";

        /* SAP */

        public string SapHost { get; set; } = "192.168.1.230";
        public string SapDatabase { get; set; } = "FPS_PRODUCCION";
        public string SapUser { get; set; } = "sa";
        public string SapPassword { get; set; } = "Sa123456";

        /* =========================================
           LOAD DESDE config.json (SEGURO)
        ========================================= */

        public static DbSettings Load()
        {
            try
            {
var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
                if (!File.Exists(path))
                {
                    Console.WriteLine("⚠️ config.json no encontrado, usando valores por defecto");
                    return new DbSettings();
                }

                var json = File.ReadAllText(path);

                var settings = JsonSerializer.Deserialize<DbSettings>(json);

                if (settings == null)
                {
                    Console.WriteLine("⚠️ config.json inválido, usando valores por defecto");
                    return new DbSettings();
                }

                Console.WriteLine("✅ Config cargado desde config.json");

                return settings;
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error leyendo config.json:");
                Console.WriteLine(ex.Message);

                return new DbSettings(); // fallback seguro
            }
        }
    }
}