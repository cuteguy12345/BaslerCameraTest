using Basler.Pylon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BaslerCameraTest
{
    public class CameraHelper
    {
        public static CameraHelper Instance { get; } = new CameraHelper();

        public BitmapImage GrabImage(Camera camera, CameraSettings cameraSettings, IGrabResult grabResult)
        {
            try
            {
                SetChangeableCameraSettings(camera, cameraSettings);
                Bitmap? bitmap = GrabResultConverterHelper.Instance.GrabResultToBitmap(grabResult);

                if (grabResult.IsValid)
                {

                    BitmapImage? bitmapImage = null;
                    if (bitmapImage is not null)
                    {
                        Bitmap? image = bitmap;
                        image!.Dispose();
                    }
                    bitmapImage = GrabResultConverterHelper.Instance.BitmapToImageSource(bitmap!);
                    return bitmapImage!;
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"An error occured: {ex.Message}_{MethodBase.GetCurrentMethod()!.Name}");
                return null;
            }
        }
        //public BitmapImage GrabImage(Camera camera, CameraSettings settings, IGrabResult grabResult)
        //{
        //    using (var bitmap = new System.Drawing.Bitmap(grabResult.Width, grabResult.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb))
        //    {
        //        var bmpData = bitmap.LockBits(
        //            new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
        //            System.Drawing.Imaging.ImageLockMode.WriteOnly,
        //            bitmap.PixelFormat);

        //        var converter = new PixelDataConverter();
        //        converter.OutputPixelFormat = PixelType.BGRA8packed;
        //        converter.Convert(bmpData.Scan0, bmpData.Stride * bitmap.Height, grabResult);
        //        bitmap.UnlockBits(bmpData);

        //        using (var stream = new System.IO.MemoryStream())
        //        {
        //            bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
        //            stream.Position = 0;

        //            var image = new BitmapImage();
        //            image.BeginInit();
        //            image.CacheOption = BitmapCacheOption.OnLoad;
        //            image.StreamSource = stream;
        //            image.EndInit();
        //            image.Freeze();
        //            return image;
        //        }
        //    }
        //}



        //public void OneShot(Camera camera, CameraSettings settings)
        //{
        //    if (camera == null || !camera.IsOpen)
        //        return;
        //    camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.SingleFrame);
        //    camera.Parameters[PLCamera.TriggerMode].SetValue(PLCamera.TriggerMode.Off);

        //    camera.Parameters[PLCamera.Width].SetValue(settings.Width);
        //    camera.Parameters[PLCamera.Height].SetValue(settings.Height);
        //    if (camera.Parameters.Contains(PLCamera.ExposureTime))
        //    {
        //        camera.Parameters[PLCamera.ExposureTime].SetValue(settings.ExposureTime);
        //    }

        //    if (camera.Parameters.Contains(PLCamera.Gain))
        //    {
        //        camera.Parameters[PLCamera.Gain].SetValue(settings.Gain);
        //    }
        //    if (camera.Parameters.Contains(PLCamera.Gamma))
        //    {
        //        camera.Parameters[PLCamera.Gamma].SetValue(settings.Gamma);
        //    }
        //    camera.StreamGrabber.Start(1);

        //    using (IGrabResult result = camera.StreamGrabber.RetrieveResult(5000, TimeoutHandling.ThrowException))
        //    {
        //        if (result.GrabSucceeded)
        //        {
        //            MessageBox.Show("불러오기 성공");
        //            var image = GrabImage(camera, settings, result);
        //            Application.Current.Dispatcher.Invoke(() =>
        //            {
        //                MainViewModel vm = (MainViewModel)Application.Current.MainWindow.DataContext;
        //                vm.CapturedImage = image;
        //            });
        //        }
        //        else
        //        {
        //            MessageBox.Show("실패");
        //        }
        //    }
        //}
        public void OneShot(Camera? camera, CameraSettings cameraSettings)
        {

            try
            {
                SetCameraStartupSettings(camera, cameraSettings);

                Configuration.AcquireSingleFrame(camera, null);
                camera?.StreamGrabber.Start(1, GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
                MessageBox.Show("1불러오기 성공");
            }
            catch (Exception ex)
            {

            }
        }


        public void ContinuousShot(Camera? camera)
        {

            try
            {
                Configuration.AcquireContinuous(camera, null);
                camera?.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void SetCameraStartupSettings(Camera camera, CameraSettings cameraSettings)
        {


            camera?.Parameters[PLCamera.Gain].TrySetValue(cameraSettings.Gain);
            camera?.Parameters[PLCamera.ExposureTime].TrySetValue(cameraSettings.ExposureTime);
            camera?.Parameters[PLCamera.Gamma].TrySetValue(cameraSettings.Gamma);
            camera?.Parameters[PLCamera.Width].SetValue(cameraSettings.Width, IntegerValueCorrection.Nearest);
            camera?.Parameters[PLCamera.Height].TrySetValue(cameraSettings.Height, IntegerValueCorrection.Nearest);
        }

        public void SetChangebleCameraSettings(Camera camera, CameraSettings cameraSettings)
        {
            camera.Parameters[PLCamera.Gain].TrySetValue(cameraSettings.Gain);
            camera.Parameters[PLCamera.ExposureTime].TrySetValue(cameraSettings.ExposureTime);
            camera.Parameters[PLCamera.Gamma].TrySetValue(cameraSettings.Gamma);
        }

        public void SetChangeableCameraSettings(Camera? camera, CameraSettings cameraSettings)
        {
            camera?.Parameters[PLCamera.Gain].TrySetValue(cameraSettings.Gain);
            camera?.Parameters[PLCamera.ExposureTime].TrySetValue(cameraSettings.ExposureTime);
            camera?.Parameters[PLCamera.Gamma].TrySetValue(cameraSettings.Gamma);
        }
    }
}
