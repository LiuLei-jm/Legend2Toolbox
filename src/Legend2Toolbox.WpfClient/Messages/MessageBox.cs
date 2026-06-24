namespace Legend2Toolbox.WpfClient.Messages;

public static class Notification
{
    public static void Show(string message)
    {
        MessageBox.Show(message);
    }
}
