using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaContacts.ViewModels;

namespace AvaloniaContacts.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

    }

    private async void RemoveButton_Click(object sender, RoutedEventArgs e)
    {
        var vm = DataContext as MainWindowViewModel;
        if (vm?.SelectedContact == null)
            return;

        var dialog = new ConfirmDialog();
        var result = await dialog.ShowDialog<bool>(this);
        if (result)
        {
            vm.RemoveSelectedContact();
        }
    }
}