using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.IO;
using System.Web.Helpers;
using System.Windows.Threading;
using System.Windows.Forms;
using System.Diagnostics;

namespace CoolCats
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window, IDisposable
    {
        const string CONFIG_NAME = ".cats-config.txt";

        private DispatcherTimer timer;
        private NotifyIcon notifyIcon;

        private Image currentCat;

        public MainWindow()
        {
            Hide();

            InitializeComponent();

            Init();

            Closing += MainWindow_Closing;

            timer = new DispatcherTimer
            {
                Interval = new TimeSpan(3, 0, 0)
            };

            timer.Tick += Timer_Tick;

            timer.Start();

            notifyIcon = new NotifyIcon
            {
                Icon = Properties.Resources.cat_icon,
                Text = "Cool Cats!",
                ContextMenuStrip = new ContextMenuStrip()
            };

            notifyIcon.ContextMenuStrip.Items.Add("Exit!", null, this.ExitClick);
            notifyIcon.ContextMenuStrip.Items.Add("Save Cat!", null, this.SaveCatClick);
            notifyIcon.ContextMenuStrip.Items.Add("Next Cat!", null, this.NextCatClick);

            notifyIcon.Visible = true;
        }
        
        private void ExitClick(object sender, EventArgs e)
        {
            if (MessageBox.Show("┬┴┬┴┤･ω･)ﾉ├┬┴┬┴", "Exit?", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                == System.Windows.Forms.DialogResult.Yes)
            {
                Close();
            }
        }

        private void SaveCatClick(object sender, EventArgs e)
        {
            if (currentCat != null)
            {
                string fname = "\\Pictures\\cat_" + DateTime.Now.Ticks + ".png";
                string path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + fname;
                currentCat.Save(path);
                
                // Opens up the photo
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    Arguments = path,
                    FileName = "explorer.exe"
                };

                Process.Start(startInfo);
            }
        }

        private void NextCatClick(object sender, EventArgs e)
        {
            ChangeBackground();
            timer.Stop();
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            ChangeBackground();
        }

        public static void ResetBackground()
        {
            string wp = GetSavedWallpaper();
            if (wp != null)
                using (var img = Image.FromFile(wp))
                    SetDesktopImage(img);
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ResetBackground();
        }

        class ImageResult
        {
            public string Id { get; set; }
            public string Url { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
        }

        public static async Task<Image> GenerateCatImage()
        {
            // https://api.thecatapi.com/v1/images/search
            using (var client = new HttpClient())
            {
                var stream = await client.GetStreamAsync(new Uri(
                    "https://api.thecatapi.com/v1/images/search")).ConfigureAwait(true);

                using (var sr = new StreamReader(stream))
                {
                    string strang = sr.ReadToEnd();
                    ImageResult[] results = Json.Decode<ImageResult[]>(strang);
                    var res = results[0];

                    var imageStream = await client.GetStreamAsync(new Uri(
                        res.Url)).ConfigureAwait(false);

                    return Image.FromStream(imageStream);
                }
            }
        }

        // https://stackoverflow.com/questions/1940581/c-sharp-image-resizing-to-different-size-while-preserving-aspect-ratio
        public static Size ResizeKeepAspect(Size src, int maxWidth, int maxHeight, bool enlarge = false)
        {
            maxWidth = enlarge ? maxWidth : Math.Min(maxWidth, src.Width);
            maxHeight = enlarge ? maxHeight : Math.Min(maxHeight, src.Height);

            decimal rnd = Math.Min(maxWidth / (decimal)src.Width, maxHeight / (decimal)src.Height);
            return new Size((int)Math.Round(src.Width * rnd), (int)Math.Round(src.Height * rnd));
        }

        public static string GetSavedWallpaper()
        {
            try
            {
                return File.ReadAllText(CONFIG_NAME);
            }
            catch (IOException)
            {
                return null;
            }
        }

        public static void SetDesktopImage(Image img)
        {
            if (img == null)
            {
                return;
            }

            Directory.CreateDirectory("images");

            var path = Directory.GetCurrentDirectory() + "/images/wallpaper.png";
            img.Save(path);
            string oldPath = Wallpaper.ChangeWallpaper(path);

            if (!oldPath.StartsWith(Directory.GetCurrentDirectory()))
            {
                File.WriteAllText(CONFIG_NAME, oldPath);
            }
        }

        public async Task<Bitmap> CreateNewDesktopImage()
        {
            var img = await GenerateCatImage().ConfigureAwait(false);

            currentCat = img;

            var path = Wallpaper.GetWallpaperPath();

            var desktopImage = Image.FromFile(path);

            using (var bmp = new Bitmap(img))
            {
                int newWidth = desktopImage.Width / 5;
                int newHeight = desktopImage.Height / 3;

                using (var resized = new Bitmap(bmp, ResizeKeepAspect(bmp.Size, newWidth, newHeight)))
                {
                    var deskbmp = new Bitmap(desktopImage);

                    int startX = deskbmp.Width - resized.Width;
                    int startY = deskbmp.Height - resized.Height - 48; // taskbar height is 48px

                    for (int y = 0; y < resized.Height; y++)
                    {
                        for (int x = 0; x < resized.Width; x++)
                        {
                            deskbmp.SetPixel(startX + x, startY + y, resized.GetPixel(x, y));
                        }
                    }

                    deskbmp.Save("wallpaper.png");

                    return deskbmp;
                }
            }
        }

        async void ChangeBackground()
        {
            using (var image = await CreateNewDesktopImage().ConfigureAwait(false))
            {
                SetDesktopImage(image);
            }
        }

        void Init()
        {
            ChangeBackground();
        }

        public void Dispose()
        {
            notifyIcon.Dispose();
            currentCat.Dispose();
        }
    }
}
