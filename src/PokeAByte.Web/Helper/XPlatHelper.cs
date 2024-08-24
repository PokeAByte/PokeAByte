using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PokeAByte.Web.Helper;

/// <summary>
/// Helper functions for dealing with cross plattform OS interactions.
/// </summary>
public static class XPlatHelper
{
    /// <summary>
    /// Opens a directory in the default file manager of the user.
    /// </summary>
    /// <param name="directory"> The directory to open. </param>
    public static void OpenFileManager(string directory)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start("explorer.exe", directory);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process.Start("open", directory);
        }
        else
        {
            Process.Start("xdg-open", directory);
        }
    }
}