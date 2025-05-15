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

        public BitmapImage? GrabImage(Camera camera, CameraSettings settings, IGrabResult grabResult)
        {
            try
            {
                if (!grabResult.IsValid || !grabResult.GrabSucceeded)
                    return null;
                //SetChangebleCameraSettings(camera, settings);

                using (Bitmap bitmap = GrabResultConverterHelper.Instance.GrabResultToBitmap(grabResult))
                {
                    return GrabResultConverterHelper.Instance.BitmapToImageSource(bitmap);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }
        public void OneShot(Camera? camera, CameraSettings cameraSettings)
        {
            try
            {
                //SetChangebleCameraSettings(camera, cameraSettings);

                Configuration.AcquireSingleFrame(camera, null);
                camera?.StreamGrabber.Start(1, GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        public void ContinuousShot(Camera? camera, CameraSettings cameraSettings)
        {

            try
            {
                //SetChangebleCameraSettings(camera, cameraSettings);
                Configuration.AcquireContinuous(camera, null);
                camera?.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void Stop(Camera? camera)
        {
            try
            {
                camera?.StreamGrabber.Stop();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        //public static void SetChangebleCameraSettings(Camera camera, CameraSettings cameraSettings)
        //{
        //    camera.Parameters[PLCamera.Gain].TrySetValue(cameraSettings.GainRaw);
        //    camera.Parameters[PLCamera.ExposureTimeAbs].TrySetValue(cameraSettings.ExposureTime);
        //    camera.Parameters[PLCamera.Width].TrySetValue(cameraSettings.Width);
        //    camera.Parameters[PLCamera.Height].TrySetValue(cameraSettings.Height);   
        //}
    }
}