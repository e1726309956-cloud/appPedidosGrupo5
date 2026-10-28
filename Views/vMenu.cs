using appPedidosGrupo5.Services;

namespace appPedidosGrupo5.Views;

public sealed class vMenu : ContentPage
{
    private readonly PermisosService permisos;
    private readonly VerticalStackLayout opciones = new() { Spacing = 12 };
    private readonly Label estado = new();
    private bool ocupado;

    // Añadir aquí cada pantalla nueva y utilizar el mismo identificador en Firestore.
    private static readonly (string Id, string Nombre)[] Pantallas =
    [
        ("catalogo", "Catálogo de ejemplo"),
        ("pedidos", "Pedidos de ejemplo"),
        ("administracion", "Administración de ejemplo")
    ];

    public vMenu(PermisosService permisos)
    {
        this.permisos = permisos;
        Title = "Menú principal";
        this.SetDynamicResource(BackgroundColorProperty, "FloralCream");
        var actualizar = new Button { Text = "Actualizar accesos" };
        actualizar.Clicked += async (_, _) => await CargarAsync();
        var salir = new Button { Text = "Cerrar sesión" };
        salir.Clicked += (_, _) =>
        {
            if (Window is not null) Window.Page = new NavigationPage(new vLogin());
        };
        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 24, Spacing = 18,
                Children = { estado, opciones, actualizar, salir }
            }
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarAsync();
    }

    private async Task CargarAsync()
    {
        if (ocupado) return;
        ocupado = true;
        opciones.Children.Clear();
        estado.Text = "Consultando accesos…";
        try
        {
            await permisos.ActualizarAsync();
            estado.Text = "Rol: " + permisos.Rol;
            foreach (var pantalla in Pantallas.Where(p => permisos.PuedeAcceder(p.Id)))
            {
                var boton = new Button { Text = pantalla.Nombre };
                boton.SetDynamicResource(StyleProperty, "BotonPrincipalFloral");
                boton.Clicked += async (_, _) => await AbrirAsync(pantalla.Id, pantalla.Nombre);
                opciones.Children.Add(boton);
            }
            if (opciones.Children.Count == 0)
                estado.Text += "\nNo tienes pantallas habilitadas.";
        }
        catch (Exception error)
        {
            estado.Text = "No se pudieron cargar los accesos.";
            await DisplayAlertAsync("Accesos", ValidacionSeguridad.MensajeError(error), "Aceptar");
        }
        finally { ocupado = false; }
    }

    private async Task AbrirAsync(string id, string nombre)
    {
        if (ocupado) return;
        ocupado = true;
        try
        {
            // Volver a consultar impide abrir opciones revocadas desde que se dibujó el menú.
            await permisos.ActualizarAsync();
            if (!permisos.PuedeAcceder(id))
            {
                opciones.Children.Clear();
                estado.Text = "Tus accesos cambiaron. Pulsa Actualizar accesos.";
                await DisplayAlertAsync("Acceso restringido", "Tu rol no tiene acceso a esta pantalla.", "Aceptar");
                return;
            }
            // Sustituir por la página real cuando esté construida.
            await Navigation.PushAsync(new ContentPage
            {
                Title = nombre,
                Content = new VerticalStackLayout
                {
                    Padding = 24, Spacing = 16,
                    Children =
                    {
                        new Label { Text = nombre, FontSize = 24 },
                        new Label { Text = "Acceso habilitado para el rol " + permisos.Rol + ". Esta pantalla es un ejemplo; aquí construiremos la funcionalidad." }
                    }
                }
            });
        }
        catch (Exception error)
        {
            opciones.Children.Clear();
            estado.Text = "Actualiza los accesos o vuelve a iniciar sesión.";
            await DisplayAlertAsync("Accesos", ValidacionSeguridad.MensajeError(error), "Aceptar");
        }
        finally { ocupado = false; }
    }
}
