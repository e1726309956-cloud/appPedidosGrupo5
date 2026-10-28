namespace appPedidosGrupo5.Views;

public partial class vRegistro : ContentPage
{
    private readonly Services.FirebaseAuthService servicio = new();
    private bool ocupado;

    public vRegistro()
    {
        InitializeComponent();
    }

    private async void btnRegistrar_Clicked(object? sender, EventArgs e)
    {
        if (ocupado) return;
        if (!ValidacionSeguridad.CorreoValido(txtCorreo.Text))
        {
            await DisplayAlertAsync("Revisa tus datos", "Ingresa un correo electrónico válido.", "Aceptar");
            return;
        }
        if (string.IsNullOrWhiteSpace(txtContrasena.Text) || txtContrasena.Text.Length < 6)
        {
            await DisplayAlertAsync("Revisa tu contraseña", "Usa al menos 6 caracteres.", "Aceptar");
            return;
        }
        if (txtContrasena.Text != txtConfirmacion.Text)
        {
            await DisplayAlertAsync("Revisa tu contraseña", "Las contraseñas no coinciden.", "Aceptar");
            return;
        }
        if (!ValidacionSeguridad.HayInternet())
        {
            await DisplayAlertAsync("Sin conexión a Internet", "No tienes acceso a Internet. Conéctate a una red e inténtalo nuevamente.", "Aceptar");
            return;
        }

        CambiarEstado(true);
        try
        {
            await servicio.RegistrarAsync(txtCorreo.Text.Trim(), txtContrasena.Text);
            txtContrasena.Text = txtConfirmacion.Text = string.Empty;
            await DisplayAlertAsync("Cuenta creada", "Ya puedes iniciar sesión con tu correo y contraseña.", "Aceptar");
            await Navigation.PopAsync();
        }
        catch (Exception error)
        {
            await DisplayAlertAsync("No se pudo crear la cuenta", ValidacionSeguridad.MensajeError(error), "Aceptar");
        }
        finally { CambiarEstado(false); }
    }

    private void CambiarEstado(bool valor)
    {
        ocupado = valor;
        Formulario.IsEnabled = !valor;
        Indicador.IsVisible = Indicador.IsRunning = valor;
    }
}
