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
            await DisplayAlertAsync(Constantes.TituloRevisarDatos, Constantes.CredencialesRequeridas, Constantes.Aceptar);
            return;
        }

        if (!ValidacionSeguridad.HayInternet())
        {
            await DisplayAlertAsync(Constantes.TituloSinInternet, Constantes.SinInternet, Constantes.Aceptar);
            return;
        }

        CambiarEstado(true);
        var autenticado = false;
        try
        {
            var sesion = await servicio.IniciarSesionAsync(txtCorreo.Text.Trim(), txtContrasena.Text);
            autenticado = true;
            txtContrasena.Text = string.Empty;
            var permisos = new Services.PermisosService(sesion);
            await permisos.ActualizarAsync();
            if (Window is not null)
                Window.Page = new NavigationPage(new vMenu(permisos));
        }
        catch (Exception error)
        {
            await DisplayAlertAsync(
                autenticado ? Constantes.TituloErrorAccesos : Constantes.TituloErrorLogin,
                ValidacionSeguridad.MensajeError(error), Constantes.Aceptar);
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
