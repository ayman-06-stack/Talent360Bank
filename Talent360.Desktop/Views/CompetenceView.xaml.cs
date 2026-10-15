using System.Windows.Controls;

namespace Talent360.Desktop.Views;

public partial class CompetenceView : UserControl
{
    public CompetenceView()
    {
        InitializeComponent();
        // Le ViewModel cree son propre scope dans Charger()
        DataContext = new ViewModels.CompetenceViewModel();
    }
}
