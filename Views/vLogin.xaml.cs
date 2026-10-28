namespace appPedidosGrupo5.Views;

public partial class vLogin : ContentPage
{
    private readonly Services.FirebaseAuthService servicio = new();
    private bool ocupado;

    public vLogin()
    {
        InitializeComponent();
    }

    private async void btnIngresar_Clicked(object? sender, EventArgs e)
    {
        if (ocupado) return;
        if (!ValidacionSeguridad.CorreoValido(txtCorreo.Text) ||
            string.IsNullOrEmpty(txtContrasena.Text))
        {
            await DisplayAlertAsync("Revisa tus datos", "Ingresa un correo válido y tu contraseña.", "Aceptar");
            return;
        }

        CambiarEstado(true);
        var autenticado = false;
        try
        {
            var sesion = await servicio.IniciarSesionAsync(txtCorreo.Text.Trim(), txtContrasena.Text);
            autenticado = true;
            txtContrasena.Text = string.Empty;
            var rol = await servicio.ObtenerRolAsync(sesion);
            await DisplayAlertAsync("Inicio de sesión correcto", $"Tu rol es: {rol}", "Aceptar");
        }
        catch (Exception error)
        {
            await DisplayAlertAsync(
                autenticado ? "Sesión iniciada, pero no se pudo consultar el rol" : "No se pudo iniciar sesión",
                ValidacionSeguridad.MensajeError(error), "Aceptar");
        }
        finally { CambiarEstado(false); }
    }

    private async void btnRegistro_Clicked(object? sender, EventArgs e)
    {
        if (!ocupado) await Navigation.PushAsync(new vRegistro());
    }

    private async void btnRecuperar_Clicked(object? sender, EventArgs e)
    {
        if (!ocupado) await Navigation.PushAsync(new vRecuperarContrasena());
    }

    private void CambiarEstado(bool valor)
    {
        ocupado = valor;
        Formulario.IsEnabled = !valor;
        Indicador.IsVisible = Indicador.IsRunning = valor;
    }
}
