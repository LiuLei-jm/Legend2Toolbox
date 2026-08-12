using System.ComponentModel;
using System.Windows.Input;

namespace Legend2Toolbox.WpfClient;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly NotifyIcon _notifyIcon;

    public MainWindow(MainViewModel mainViewModel)
    {
        InitializeComponent();
        DataContext = mainViewModel;
        _notifyIcon = new NotifyIcon
        {
            Icon = new Icon("Resources/favicon.ico"),
            Visible = true,
            Text = "功能网关"
        };
        _notifyIcon.DoubleClick += (s, e) =>
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        };
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        _notifyIcon.Dispose();
    }

    private void Window_StateChanged(object sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized) Hide();
    }

    private void TitleBar_MouseLiftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left) DragMove();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void MinimumButton_Click(object sender, RoutedEventArgs e)
    {
        Hide();
    }
}