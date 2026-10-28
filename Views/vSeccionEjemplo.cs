namespace appPedidosGrupo5.Views;

public sealed class vSeccionEjemplo : ContentPage
{
    public vSeccionEjemplo(string nombre, string descripcion)
    {
        Title = nombre;
        this.SetDynamicResource(BackgroundColorProperty, "FloralCream");
        var titulo = new Label { Text = nombre };
        titulo.SetDynamicResource(StyleProperty, "TituloFloral");
        var detalle = new Label { Text = descripcion };
        detalle.SetDynamicResource(StyleProperty, "TextoFloral");
        var volver = new Button { Text = "Volver al inicio" };
        volver.SetDynamicResource(StyleProperty, "BotonPrincipalFloral");
        volver.Clicked += async (_, _) =>
        {
            volver.IsEnabled = false;
            try { await Navigation.PopAsync(); }
            finally { volver.IsEnabled = true; }
        };
        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 24, Spacing = 20, MaximumWidthRequest = 650, HorizontalOptions = LayoutOptions.Fill,
                Children =
                {
                    new Image { Source = "floreria_ramo.png", HeightRequest = 140 },
                    titulo, detalle,
                    new Label { Text = "Próximamente", FontFamily = "OpenSansSemibold", TextColor = Color.FromArgb("#244C3A") },
                    new Label { Text = "Esta sección está en preparación. Por ahora es una vista de ejemplo, sin datos ni operaciones." },
                    volver
                }
            }
        };
    }
}
