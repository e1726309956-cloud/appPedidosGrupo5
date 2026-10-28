using appPedidosGrupo5.Services;

namespace appPedidosGrupo5.Views;

public partial class vMenu : ContentPage
{
    private readonly PermisosService permisos;
    private bool ocupado;
    private bool cerrada;

    public vMenu(PermisosService permisos)
    {
        InitializeComponent();
        this.permisos = permisos;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarAsync();
    }

    private void AlternarMenu_Clicked(object? sender, EventArgs e) => PanelMenu.IsVisible = !PanelMenu.IsVisible;

    protected override bool OnBackButtonPressed()
    {
        if (!PanelMenu.IsVisible) return base.OnBackButtonPressed();
        PanelMenu.IsVisible = false;
        return true;
    }

    private async void Actualizar_Clicked(object? sender, EventArgs e) => await CargarAsync();

    private void CerrarSesion_Clicked(object? sender, EventArgs e)
    {
        cerrada = true;
        if (Window is not null) Window.Page = new NavigationPage(new vLogin());
    }

    private void CambiarEstado(bool valor)
    {
        ocupado = valor;
        Indicador.IsVisible = Indicador.IsRunning = valor;
        BotonActualizar.IsEnabled = !valor;
        Tarjetas.IsEnabled = OpcionesMenu.IsEnabled = !valor;
    }

    private void LimpiarOpciones(string mensaje)
    {
        Tarjetas.Children.Clear();
        OpcionesMenu.Children.Clear();
        Estado.Text = EstadoMenu.Text = mensaje;
    }

    private async Task CargarAsync()
    {
        if (ocupado || cerrada) return;
        CambiarEstado(true);
        LimpiarOpciones("Consultando tus accesos…");
        try
        {
            await permisos.ActualizarAsync();
            if (cerrada) return;
            MostrarOpciones();
        }
        catch (Exception error)
        {
            if (cerrada) return;
            LimpiarOpciones(Constantes.ErrorCargarAccesos);
            await DisplayAlertAsync(Constantes.TituloAccesos, ValidacionSeguridad.MensajeError(error), Constantes.Aceptar);
        }
        finally { CambiarEstado(false); }
    }

    private void MostrarOpciones()
    {
        var (nombre, saludo, descripcion) = permisos.Rol switch
        {
            "cliente" => ("Cliente", "Un detalle para cada ocasión", "Descubre tu próximo detalle y encuentra las opciones disponibles para ti."),
            "vendedor" => ("Vendedor", "Cada detalle comienza contigo", "Accede a tus herramientas para acompañar a nuestros clientes."),
            _ => ("Administrador", "Tu florería, en un solo lugar", "Accede a las secciones habilitadas para organizar tu trabajo.")
        };
        EtiquetaRol.Text = "ESPACIO " + nombre.ToUpperInvariant();
        RolMenu.Text = "Sesión de " + nombre.ToLowerInvariant();
        Saludo.Text = saludo;
        DescripcionRol.Text = descripcion;
        LimpiarOpciones("");
        foreach (var pantalla in RegistroPantallas.Todas.Where(p => permisos.PuedeAcceder(p.Id)))
        {
            var abrir = new Button { Text = "Abrir " + pantalla.Nombre.ToLowerInvariant() };
            abrir.SetDynamicResource(StyleProperty, "BotonPrincipalFloral");
            abrir.Clicked += async (_, _) => await AbrirAsync(pantalla);
            var titulo = new Label { Text = pantalla.Nombre, FontSize = 20 };
            titulo.SetDynamicResource(StyleProperty, "TituloFloral");
            titulo.FontSize = 20;
            var descripcionPantalla = new Label { Text = pantalla.Descripcion };
            descripcionPantalla.SetDynamicResource(StyleProperty, "TextoFloral");
            var tarjeta = new Border
            {
                Padding = 20,
                Content = new VerticalStackLayout { Spacing = 12, Children = { titulo, descripcionPantalla, abrir } }
            };
            tarjeta.SetDynamicResource(StyleProperty, "TarjetaFloral");
            Tarjetas.Children.Add(tarjeta);
            var opcion = new Button { Text = pantalla.Nombre };
            opcion.SetDynamicResource(StyleProperty, "BotonSecundarioFloral");
            opcion.Clicked += async (_, _) => await AbrirAsync(pantalla);
            OpcionesMenu.Children.Add(opcion);
        }
        var mensaje = Tarjetas.Children.Count == 0
            ? "Todavía no tienes secciones habilitadas. Puedes actualizar tus accesos o consultar al administrador."
            : "Elige una sección para comenzar.";
        Estado.Text = EstadoMenu.Text = mensaje;
    }

    private async Task AbrirAsync(PantallaMenu pantalla)
    {
        if (ocupado || cerrada) return;
        CambiarEstado(true);
        try
        {
            await permisos.ActualizarAsync();
            if (cerrada) return;
            MostrarOpciones();
            if (!permisos.PuedeAcceder(pantalla.Id))
            {
                await DisplayAlertAsync(Constantes.TituloAccesoRestringido, Constantes.AccesoDenegado, Constantes.Aceptar);
                return;
            }
            PanelMenu.IsVisible = false;
            await Navigation.PushAsync(pantalla.CrearPagina());
        }
        catch (Exception error)
        {
            if (cerrada) return;
            LimpiarOpciones(Constantes.ReintentarAccesos);
            await DisplayAlertAsync(Constantes.TituloAccesos, ValidacionSeguridad.MensajeError(error), Constantes.Aceptar);
        }
        finally { CambiarEstado(false); }
    }
}
