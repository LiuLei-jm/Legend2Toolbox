using System.Windows.Input;
using Forms = System.Windows.Forms;

namespace Legend2Toolbox.WpfClient;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private Forms.NotifyIcon _notifyIcon;
    public MainWindow(MainViewModel mainViewModel)
    {
        InitializeComponent();
        DataContext = mainViewModel;
        _notifyIcon = new Forms.NotifyIcon
        {
            Icon = new System.Drawing.Icon("Resources/favicon.ico"),
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

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        _notifyIcon.Dispose();
    }

    private void Window_StateChanged(object sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized) Hide();
    }

    private void TitleBar_MouseLiftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
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