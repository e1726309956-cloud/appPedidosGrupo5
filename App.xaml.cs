using Microsoft.Extensions.DependencyInjection;

namespace appPedidosGrupo5
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(CrearPaginaInicial());
        }

        public static Page CrearPaginaInicial() => new NavigationPage(
            Constantes.UsarAutenticacion
                ? new Views.vLogin()
                : new Views.vMenu());
    }
}
