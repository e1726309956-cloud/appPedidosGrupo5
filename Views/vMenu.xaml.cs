using appPedidosGrupo5.Services;

namespace appPedidosGrupo5.Views;

public partial class vMenu : ContentPage
{
    private readonly PermisosService? permisos;
    private bool ModoDemostracion => permisos is null;
    private bool ocupado;
    private bool cerrada;

    public vMenu()
    {
        InitializeComponent();
        BotonCerrarSesion.IsVisible = BotonCerrarSesionMenu.IsVisible = false;
        BotonActualizar.Text = "Actualizar inicio";
    }

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
        if (Window is not null) Window.Page = App.CrearPaginaInicial();
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
            if (permisos is not null) await permisos.ActualizarAsync();
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
        var (nombre, saludo, descripcion) = (permisos?.Rol ?? "demostracion") switch
        {
            "demostracion" => ("Demostración", "Bienvenido a tu florería", "Explora las pantallas de ejemplo sin iniciar sesión."),
            "cliente" => ("Cliente", "Un detalle para cada ocasión", "Descubre tu próximo detalle y encuentra las opciones disponibles para ti."),
            "vendedor" => ("Vendedor", "Cada detalle comienza contigo", "Accede a tus herramientas para acompañar a nuestros clientes."),
            _ => ("Administrador", "Tu florería, en un solo lugar", "Accede a las secciones habilitadas para organizar tu trabajo.")
        };
        EtiquetaRol.Text = "ESPACIO " + nombre.ToUpperInvariant();
        RolMenu.Text = ModoDemostracion ? "Modo de demostración · Sin sesión" : "Sesión de " + nombre.ToLowerInvariant();
        Saludo.Text = saludo;
        DescripcionRol.Text = descripcion;
        LimpiarOpciones("");
        foreach (var pantalla in RegistroPantallas.Todas.Where(p => ModoDemostracion || permisos!.PuedeAcceder(p.Id)))
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
            : ModoDemostracion ? "Vista previa local. Las opciones abren ejemplos sin datos de Firebase." : "Elige una sección para comenzar.";
        Estado.Text = EstadoMenu.Text = mensaje;
    }

    private async Task AbrirAsync(PantallaMenu pantalla)
    {
        if (ocupado || cerrada) return;
        CambiarEstado(true);
        try
        {
            if (permisos is not null) await permisos.ActualizarAsync();
            if (cerrada) return;
            MostrarOpciones();
            if (permisos is not null && !permisos.PuedeAcceder(pantalla.Id))
            {
                await DisplayAlertAsync(Constantes.TituloAccesoRestringido, Constantes.AccesoDenegado, Constantes.Aceptar);
                return;
            }
            PanelMenu.IsVisible = false;
            // La vista previa siempre abre ejemplos, incluso cuando se añadan páginas reales al registro.
            await Navigation.PushAsync(ModoDemostracion
                ? new vSeccionEjemplo(pantalla.Nombre, pantalla.Descripcion)
                : pantalla.CrearPagina());
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
