using System.Windows.Controls;

namespace Talent360.Desktop.Views;

public partial class PosteCritiqueListView : UserControl
{
    public PosteCritiqueListView()
    {
        InitializeComponent();
        DataContext = new ViewModels.PosteCritiqueListViewModel();
    }
}
