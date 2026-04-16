using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using LogisticControlCenter.Config;
using LogisticControlCenter.Modules.Auth;
using LogisticControlCenter.Repositories.Auth;
using LogisticControlCenter.Services;
using Photino.NET;

namespace LogisticControlCenter
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("🚀 Iniciando Logistic Control Center...");

            try
            {
                // =========================
                // 🔧 CONFIG + DB
                // =========================
                var settings = DbSettings.Load();
                var db = new DbService(settings);

                // =========================
                // 🔐 AUTH + SESSION
                // =========================
                var session = new CurrentUserSessionService();
                var authRepository = new AuthRepository(db);
                var authService = new AuthService(authRepository, session);
                var authHandler = new AuthHandler(authService);

                // =========================
                // 🧠 ROUTER CENTRAL
                // =========================
                var router = new MessageRouter(db, authHandler, session);

                // =========================
                // 📂 RUTA INDEX.HTML
                // =========================
                var root = Directory.GetCurrentDirectory();
                var indexPath = Path.Combine(root, "src", "UI", "www", "index.html");

                Console.WriteLine($"📂 Cargando HTML desde: {indexPath}");

                if (!File.Exists(indexPath))
                {
                    Console.WriteLine("❌ ERROR: index.html no encontrado");
                    return;
                }

                // =========================
                // 🖥 VENTANA
                // =========================
                var window = new PhotinoWindow()
                    .SetTitle("Logistic Control Center")
                    .SetUseOsDefaultSize(true)
                    .Center()
                    .SetChromeless(false)
                    .Load(indexPath);

                // =========================
                // 🔥 BRIDGE JS ↔ C#
                // =========================
                window.RegisterWebMessageReceivedHandler(
                    async (sender, message) =>
                    {
                        try
                        {
                            Console.WriteLine($"📥 RAW: {message}");

                            using var doc = JsonDocument.Parse(message);
                            var rootJson = doc.RootElement;

                            // =========================
                            // VALIDAR FORMATO
                            // =========================
                            if (
                                !rootJson.TryGetProperty("id", out var idProp)
                                || !rootJson.TryGetProperty("payload", out var payloadProp)
                            )
                            {
                                SendError(window, 0, "Formato inválido (id/payload faltante)");
                                return;
                            }

                            var requestId = idProp.GetInt32();
                            var payloadJson = payloadProp.GetRawText();

                            Console.WriteLine($"🎯 Request ID: {requestId}");

                            // =========================
                            // PROCESAR EN ROUTER
                            // =========================
                            var result = await router.Handle(payloadJson);

                            // =========================
                            // RESPUESTA FINAL
                            // =========================
                            var response = new
                            {
                                id = requestId,
                                data = JsonSerializer.Deserialize<JsonElement>(result),
                            };

                            var responseJson = JsonSerializer.Serialize(response);

                            Console.WriteLine($"📤 RESPONSE: {responseJson}");

                            window.SendWebMessage(responseJson);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"❌ ERROR BRIDGE: {ex}");

                            SendError(window, 0, "Error interno servidor");
                        }
                    }
                );

                // =========================
                // 🚀 RUN
                // =========================
                window.WaitForClose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR FATAL: {ex}");
            }
        }

        // =========================
        // 🔴 ERROR STANDARD
        // =========================
        static void SendError(PhotinoWindow window, int id, string message)
        {
            var errorResponse = new { id = id, data = new { ok = false, error = message } };

            window.SendWebMessage(JsonSerializer.Serialize(errorResponse));
        }
    }
}
