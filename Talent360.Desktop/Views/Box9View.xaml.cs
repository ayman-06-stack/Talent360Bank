using System.Windows;
using System.Windows.Controls;
using Talent360.Application.Enums;
using Microsoft.Extensions.DependencyInjection;
using Talent360.Application.Interfaces;

namespace Talent360.Desktop.Views;

public partial class Box9View : UserControl
{
    private readonly ViewModels.Box9ViewModel _vm;

    public Box9View()
    {
        InitializeComponent();
        // Creer un scope explicite pour obtenir un DbContext frais a chaque ouverture
        var app = (App)System.Windows.Application.Current;
        using var scope = app.Services.CreateScope();
        var collabRepo = scope.ServiceProvider.GetRequiredService<ICollaborateurRepository>();
        var calcul = scope.ServiceProvider.GetRequiredService<ICalculEngine>();
        _vm = new ViewModels.Box9ViewModel(collabRepo, calcul);
        DataContext = _vm;
    }

    private void Cell_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is ViewModels.Box9ViewModel.BoxCell cell)
        {
            var members = _vm.GetMembers(cell.Category);
            var wnd = new CollaborateurListFilteredView(members);
            wnd.Owner = System.Windows.Application.Current.MainWindow;
            wnd.ShowDialog();
        }
    }
}
