using Basler.Pylon;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace BaslerCameraTest
{
    public class GrabResultConverterHelper
    {
        private readonly PixelDataConverter converter = new();
        public static GrabResultConverterHelper Instance { get; } = new GrabResultConverterHelper();

        public BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            try
            {
                using (MemoryStream memory = new MemoryStream())
                {
                    bitmap.Save(memory, ImageFormat.Bmp);
                    memory.Position = 0;

                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();

                    return bitmapImage;

                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }


        public Bitmap GrabResultToBitmap(IGrabResult grabResult)
        {

            try
            {
                Bitmap bitmap = new Bitmap(grabResult.Width, grabResult.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
                converter.OutputPixelFormat = PixelType.BGR8packed;
                IntPtr ptrBmp = bitmapData.Scan0;
                converter.Convert(ptrBmp, bitmapData.Stride * bitmapData.Height, grabResult);
                bitmap.UnlockBits(bitmapData);

                return bitmap;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }
    }
}
