using System.Windows.Controls;

namespace Talent360.Desktop.Views;

public partial class EngagementView : UserControl
{
    public EngagementView()
    {
        InitializeComponent();
        DataContext = new ViewModels.EngagementViewModel();
    }
}
