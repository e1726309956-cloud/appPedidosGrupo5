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
            await DisplayAlertAsync(Constantes.TituloRevisarDatos, Constantes.CorreoRequerido, Constantes.Aceptar);
            return;
        }
        if (string.IsNullOrWhiteSpace(txtContrasena.Text) || txtContrasena.Text.Length < 6)
        {
            await DisplayAlertAsync(Constantes.TituloRevisarContrasena, Constantes.ContrasenaCorta, Constantes.Aceptar);
            return;
        }
        if (txtContrasena.Text != txtConfirmacion.Text)
        {
            await DisplayAlertAsync(Constantes.TituloRevisarContrasena, Constantes.ContrasenasDistintas, Constantes.Aceptar);
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
            await servicio.RegistrarAsync(txtCorreo.Text.Trim(), txtContrasena.Text);
            txtContrasena.Text = txtConfirmacion.Text = string.Empty;
            await DisplayAlertAsync("Cuenta creada", "Tu cuenta se registró como cliente. Ya puedes iniciar sesión con tu correo y contraseña.", Constantes.Aceptar);
            await Navigation.PopAsync();
        }
        catch (Exception error)
        {
            await DisplayAlertAsync(Constantes.TituloErrorRegistro, ValidacionSeguridad.MensajeError(error), Constantes.Aceptar);
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
