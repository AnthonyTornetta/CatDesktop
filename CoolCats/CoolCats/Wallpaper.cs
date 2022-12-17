using System;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace CoolCats
{
    // https://social.msdn.microsoft.com/Forums/en-US/ab83d0c3-0b82-4353-b447-38ad297dfece/how-to-change-the-wallpaper-programmatically?forum=csharpgeneral

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1060:Move pinvokes to native methods class", Justification = "<Pending>")]
    public static class Wallpaper
    {
        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;
        //const int SPI_GETDESKWALLPAPER = 0x0073;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int SystemParametersInfo(int uAction, int uParam, StringBuilder lpvParam, int fuWinIni);

        public static string GetWallpaperPath()
        {
            //StringBuilder s = new StringBuilder(1000);

            //_ = SystemParametersInfo(SPI_GETDESKWALLPAPER, 1000, s, 0);

            //string path = s.ToString();
            //if (System.IO.File.Exists(path))
            //{
            //    return path;
            //}
            //else
            //{
                string p = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Microsoft/Windows/Themes/CachedFiles/";
                return System.IO.Directory.GetFiles(p)[0];
            //}
        }

        public static string ChangeWallpaper(string filename)
        {
            string oldPath = GetWallpaperPath();
            _ = SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, new StringBuilder(filename), SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
            return oldPath;
        }
    }
}