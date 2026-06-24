using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Legend2Toolbox.Shared.Helpers;

public static class PathHelper
{
    public static string GetExeCurrentPath()
    {
        string? exePath = Environment.ProcessPath;
        return !string.IsNullOrEmpty(exePath)
            ? Path.GetDirectoryName(exePath) ?? AppContext.BaseDirectory
            : AppContext.BaseDirectory;
    }
    public static bool IsValidFilePath(string filePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath)) return false;
            string fullPath = Path.GetFullPath(filePath);
            string appDirectory = GetExeCurrentPath();
            if (fullPath.StartsWith(appDirectory, StringComparison.OrdinalIgnoreCase)) return true;
            if (Path.IsPathRooted(fullPath)
                && fullPath.Length >= 2
                && char.IsLetter(fullPath[0])
                && fullPath[1] == ':') return true;
            if (filePath.Contains("..")) return false;
            var invalidChars = Path.GetInvalidPathChars();
            if (filePath.Any(c => invalidChars.Contains(c))) return false;
            return true;
        }
        catch
        {
            return false;
        }
    }
}
