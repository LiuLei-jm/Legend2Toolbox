namespace Legend2Toolbox.WpfClient.Messages;

public static class Notification
{
    public static void Show(string message)
    {
        System.Windows.Forms.MessageBox.Show(message);
    }
}
