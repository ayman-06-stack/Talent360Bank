using System.Collections.Generic;
using System.Windows;
using Talent360.Domain.Entities;

namespace Talent360.Desktop.Views;

public partial class CollaborateurListFilteredView : Window
{
    public CollaborateurListFilteredView(IEnumerable<Collaborateur> collaborateurs)
    {
        InitializeComponent();
        Grid.ItemsSource = collaborateurs;
    }
}
