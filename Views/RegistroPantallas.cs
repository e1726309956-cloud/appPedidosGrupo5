namespace appPedidosGrupo5.Views;

public sealed record PantallaMenu(string Id, string Nombre, string Descripcion, Func<ContentPage> CrearPagina);

public static class RegistroPantallas
{
    public static IReadOnlyList<PantallaMenu> Todas { get; } = Array.AsReadOnly(new[]
    {
        new PantallaMenu(Constantes.PantallaCatalogo, "Catálogo", "Flores y detalles para cada ocasión.",
            () => new vSeccionEjemplo("Catálogo", "Aquí podrás explorar las flores y los arreglos disponibles.")),
        new PantallaMenu(Constantes.PantallaPedidos, "Pedidos", "Un espacio para consultar y dar seguimiento a los pedidos.",
            () => new vSeccionEjemplo("Pedidos", "Aquí construiremos la consulta y el seguimiento de pedidos según tu rol.")),
        new PantallaMenu(Constantes.PantallaAdministracion, "Administración", "Las herramientas para organizar tu florería.",
            () => new vSeccionEjemplo("Administración", "Aquí construiremos las herramientas de administración de la florería."))
    });
}
