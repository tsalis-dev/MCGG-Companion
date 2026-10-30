using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Windows.Interop;
using McggCompanion.App.ViewModels;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace McggCompanion.App;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainWindowViewModel viewModel;
    private bool cleanupStarted;
    private bool cleanupFinished;

    public MainWindow()
    {
        InitializeComponent();
        viewModel = new MainWindowViewModel(new WindowInteropHelper(this).EnsureHandle());
        DataContext = viewModel;
        Closing += OnClosing;
    }

    private async void OnClosing(object? sender, CancelEventArgs args)
    {
        if (cleanupFinished)
        {
            return;
        }

        args.Cancel = true;
        if (cleanupStarted)
        {
            return;
        }

        cleanupStarted = true;
        await viewModel.DisposeAsync();
        cleanupFinished = true;
        _ = Dispatcher.BeginInvoke(new Action(Close));
    }
}
