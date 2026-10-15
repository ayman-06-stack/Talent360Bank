using System.Windows.Controls;

namespace Talent360.Desktop.Views;

public partial class CampagneView : UserControl
{
    public CampagneView()
    {
        InitializeComponent();
        DataContext = new ViewModels.CampagneViewModel();
    }
}
