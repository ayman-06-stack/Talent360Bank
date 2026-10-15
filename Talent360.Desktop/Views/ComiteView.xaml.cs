using System.Windows.Controls;

namespace Talent360.Desktop.Views;

public partial class ComiteView : UserControl
{
    public ComiteView()
    {
        InitializeComponent();
        DataContext = new ViewModels.ComiteViewModel();
    }
}
