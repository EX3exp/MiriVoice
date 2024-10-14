using Mirivoice.Mirivoice.Core.Utils;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Serilog;
using Avalonia.Media;
using System.Drawing;
using Avalonia.Platform;
using Avalonia.Media.Imaging;

namespace Mirivoice.Mirivoice.Core.Managers
{
    public class IconManager
    {
        public ImageBrush BosIcon;
        public ImageBrush NonIcon;
        public ImageBrush SpaceIcon;
        public ImageBrush EosIcon;
        public IconManager()
        {
            LoadIcon(new Uri("avares://Mirivoice.Main/Assets/UI/inton-bos.png"), out BosIcon);
            LoadIcon(new Uri("avares://Mirivoice.Main/Assets/UI/inton-no.png"), out NonIcon);
            LoadIcon(new Uri("avares://Mirivoice.Main/Assets/UI/inton-space.png"), out SpaceIcon);
            LoadIcon(new Uri("avares://Mirivoice.Main/Assets/UI/inton-eos.png"), out EosIcon);

        }

        public void LoadIcon(Uri uri, out ImageBrush icon)
        {
            var assets = AssetLoader.Open(uri);

            using (var stream = assets)
            {
                var bitmap = new Avalonia.Media.Imaging.Bitmap(stream);
                icon = new ImageBrush(bitmap)
                {
                    Stretch = Stretch.UniformToFill
                };
            }
        }
    }
}
