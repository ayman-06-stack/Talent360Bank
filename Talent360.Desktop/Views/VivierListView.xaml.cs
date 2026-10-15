using System.Windows.Controls;

namespace Talent360.Desktop.Views;

public partial class VivierListView : UserControl
{
    public VivierListView()
    {
        InitializeComponent();
        DataContext = new ViewModels.VivierListViewModel();
    }
}
