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
            await DisplayAlertAsync(Constantes.TituloRevisarDatos, Constantes.CorreoRequerido, Constantes.Aceptar);
            return;
        }
        if (!ValidacionSeguridad.HayInternet())
        {
            await DisplayAlertAsync(Constantes.TituloSinInternet, Constantes.SinInternet, Constantes.Aceptar);
            return;
        }

        CambiarEstado(true);
        try
        {
            await servicio.RecuperarContrasenaAsync(txtCorreo.Text.Trim());
            await DisplayAlertAsync("Solicitud recibida",
                "Si existe una cuenta con ese correo, recibirás un enlace para cambiar tu contraseña. Revisa también el correo no deseado.", Constantes.Aceptar);
            await Navigation.PopAsync();
        }
        catch (Exception error)
        {
            await DisplayAlertAsync(Constantes.TituloErrorRecuperacion, ValidacionSeguridad.MensajeError(error), Constantes.Aceptar);
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
