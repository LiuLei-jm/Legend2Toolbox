namespace Legend2Toolbox.Shared.Helpers;

public static class CdkHelper
{
    private const string _chars = "ABCDEFHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    public static string GenerateMembershipCard(int length, double faceValue)
    {
        var randomPart = new StringBuilder(length - 4);
        using var rng = RandomNumberGenerator.Create();
        var buffer = new byte[1];
        for (int i = 0; i < length - 4; i++)
        {
            rng.GetBytes(buffer);
            var idx = buffer[0] % _chars.Length;
            randomPart.Append(_chars[idx]);
        }
        var faceValuePart = ((int)(faceValue)).ToString("D4");
        return $"{randomPart}{faceValuePart}";
    }
}
