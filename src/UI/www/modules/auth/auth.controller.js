window.AuthController = class AuthController {

    init() {
        console.log("🔐 AuthController iniciado");

        this.form = document.getElementById("auth-login-form");
        this.message = document.getElementById("auth-message");

        if (!this.form) {
            console.error("❌ Formulario no encontrado");
            return;
        }

        this.bindEvents();
    }

    bindEvents() {
        this.form.addEventListener("submit", async (e) => {
            e.preventDefault();

            const codigoUsuario = document.getElementById("codigoUsuario").value.trim();
            const password = document.getElementById("password").value;

            if (!codigoUsuario || !password) {
                this.showMessage("Completa todos los campos", false);
                return;
            }

            try {
                const response = await window.PhotinoBridge.send({
                    action: "auth.login",
                    data: {
                        CodigoUsuario: codigoUsuario,
                        Password: password
                    }
                });

                console.log("🔐 LOGIN RESPONSE:", response);

                if (response.ok) {
                    const meResponse = await window.PhotinoBridge.send({
                        action: "auth.me",
                        data: {}
                    });

                    console.log("👤 AUTH ME RESPONSE:", meResponse);

                    if (meResponse.ok && meResponse.data) {
                        sessionStorage.setItem("isLoggedIn", "true");
                        sessionStorage.setItem("codigoUsuario", meResponse.data.CodigoUsuario || "");
                        sessionStorage.setItem("nombreUsuario", meResponse.data.NombreCompleto || "");
                        sessionStorage.setItem("rolUsuario", meResponse.data.Rol || "");
                        sessionStorage.setItem("usuarioActivo", String(meResponse.data.Activo ?? true));
                    } else {
                        sessionStorage.setItem("isLoggedIn", "true");
                        sessionStorage.setItem("codigoUsuario", codigoUsuario);
                    }

                    this.showMessage("Ingreso correcto", true);

                    setTimeout(() => {
                        window.App.loadModule("inicio");
                    }, 500);

                } else {
                    this.showMessage(response.error || "Credenciales inválidas", false);
                }

            } catch (error) {
                console.error("❌ Error login:", error);
                this.showMessage("Error de conexión", false);
            }
        });
    }

    showMessage(text, success) {
        if (!this.message) return;

        this.message.innerText = text;
        this.message.style.color = success ? "#16a34a" : "#dc2626";
    }

    destroy() {
        console.log("🧹 AuthController destruido");
    }
};