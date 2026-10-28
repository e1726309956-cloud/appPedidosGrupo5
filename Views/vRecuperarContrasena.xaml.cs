namespace appPedidosGrupo5.Views;

public partial class vRecuperarContrasena : ContentPage
{
    private readonly Services.FirebaseAuthService servicio = new();
    private bool ocupado;

    public vRecuperarContrasena()
    {
        InitializeComponent();
    }

    private async void btnEnviar_Clicked(object? sender, EventArgs e)
    {
        if (ocupado) return;
        if (!ValidacionSeguridad.CorreoValido(txtCorreo.Text))
        {
            await DisplayAlertAsync("Revisa tus datos", "Ingresa un correo electrónico válido.", "Aceptar");
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
            await servicio.RecuperarContrasenaAsync(txtCorreo.Text.Trim());
            await DisplayAlertAsync("Solicitud recibida",
                "Si existe una cuenta con ese correo, recibirás un enlace para cambiar tu contraseña. Revisa también el correo no deseado.", "Aceptar");
            await Navigation.PopAsync();
        }
        catch (Exception error)
        {
            await DisplayAlertAsync("No se pudo enviar el enlace", ValidacionSeguridad.MensajeError(error), "Aceptar");
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
