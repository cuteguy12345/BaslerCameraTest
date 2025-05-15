using Basler.Pylon;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

using System.Windows.Threading;


namespace BaslerCameraTest
{
    public partial class MainViewModelToolKitUse : ObservableObject
    {
        private Camera? camera = null;

        public MainViewModelToolKitUse()
        {
            RefreshDeviceList();
        }

        [ObservableProperty]
        private ObservableCollection<ICameraInfo> _availableCameras = new();
        private bool CanExecuteCameraCommand() => SelectedCamera != null;

        [ObservableProperty]
        private bool _isContinuousShooting;

        [ObservableProperty]
        private ICameraInfo _selectedCamera;

        [ObservableProperty]
        private int _width;

        [ObservableProperty]
        private int _height;

        [ObservableProperty]
        private long _gainraw;

        [ObservableProperty]
        private double _exposureTime;


        [ObservableProperty]
        private BitmapImage _capturedImage;


        [ObservableProperty]
        private string _exposureTimeInput;

        [ObservableProperty]
        private string _widthInput;

        [ObservableProperty]
        private string _heightInput;

        [ObservableProperty]
        private string _gainInput;


        [RelayCommand(CanExecute = nameof(CanExecuteCameraCommand))]
        private void OneShot()
        {
            CameraHelper.Instance.OneShot(camera, GetCurrentCameraSettings());
        }

        [RelayCommand(CanExecute = nameof(CanExecuteCameraCommand))]
        private void ContinuousShot()
        {
            CameraHelper.Instance.ContinuousShot(camera, GetCurrentCameraSettings());
            IsContinuousShooting = true;
        }

        [RelayCommand(CanExecute = nameof(CanExecuteCameraCommand))]
        private void Stop()
        {
            CameraHelper.Instance.Stop(camera);
            IsContinuousShooting = false;
            MessageBox.Show("중지 되었습니다.");
        }

        public void RefreshDeviceList()
        {
            AvailableCameras.Clear();
            foreach (var cam in CameraFinder.Enumerate())
            {
                AvailableCameras.Add(cam);
            }
        }

        partial void OnSelectedCameraChanged(ICameraInfo value)
        {
            InitializeCamera(value);

            OneShotCommand.NotifyCanExecuteChanged();
            ContinuousShotCommand.NotifyCanExecuteChanged();
            StopCommand.NotifyCanExecuteChanged();
        }


        private void InitializeCamera(ICameraInfo cameraInfo)
        {
            try
            {
                camera?.Close();

                camera = new Basler.Pylon.Camera(cameraInfo);
                camera.Open();

                camera.StreamGrabber.ImageGrabbed += OnImageGrabbed;
                
                if (camera.Parameters.Contains(PLCamera.ExposureTimeAbs))
                    ExposureTime = camera.Parameters[PLCamera.ExposureTimeAbs].GetValue();
                  ExposureTimeInput = ExposureTime.ToString();

                if (camera.Parameters.Contains(PLCamera.GainRaw))
                    Gainraw = camera.Parameters[PLCamera.GainRaw].GetValue();
                GainInput = Gainraw.ToString();


                if (camera.Parameters.Contains(PLCamera.Width))
                    Width = (int)camera.Parameters[PLCamera.Width].GetValue();
                WidthInput = Width.ToString();


                if (camera.Parameters.Contains(PLCamera.Height))
                    Height = (int)camera.Parameters[PLCamera.Height].GetValue();
                HeightInput = Height.ToString();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void OnImageGrabbed(object sender, ImageGrabbedEventArgs e)
        {
            try
            {
                if (camera == null || e.GrabResult == null || !e.GrabResult.GrabSucceeded)
                    return;

                var dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;

                dispatcher.Invoke(() =>
                {
                    var image = CameraHelper.Instance.GrabImage(camera, GetCurrentCameraSettings(), e.GrabResult);
                    if (image != null)
                    {
                        CapturedImage = image;
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("이미지 처리 실패: " + ex.Message);
            }
        }

        public CameraSettings GetCurrentCameraSettings() => new()
        {
            Width = this.Width,
            Height = this.Height,
            GainRaw = this.Gainraw,
            ExposureTime = this.ExposureTime,
        };

        partial void OnGainrawChanged(long value)
        {
            if (camera?.IsOpen == true && camera.Parameters.Contains(PLCamera.GainRaw))
            {
                camera.Parameters[PLCamera.GainRaw].SetValue(value);
                GainInput = value.ToString();
            }
        }

        partial void OnExposureTimeChanged(double value)
        {
            if (camera?.IsOpen == true && camera.Parameters.Contains(PLCamera.ExposureTimeAbs))
            {
              camera.Parameters[PLCamera.ExposureTimeAbs].SetValue(value);
                ExposureTimeInput = value.ToString("f0");
            }
        }

        partial void OnWidthChanged(int value)
        {
            if (camera?.IsOpen == true && camera.Parameters.Contains(PLCamera.Width))
            {
                camera.Parameters[PLCamera.Width].SetValue(value);
                WidthInput = value.ToString();
            }
        }

        partial void OnHeightChanged(int value)
        {
            if (camera?.IsOpen == true && camera.Parameters.Contains(PLCamera.Height))
            {
                camera.Parameters[PLCamera.Height].SetValue(value);
                HeightInput = value.ToString();
            }
                
        }

        partial void OnExposureTimeInputChanged(string value)
        {
            if (!double.TryParse(value, out double parsed)) 
                return;

            if (camera?.IsOpen == true && camera.Parameters.Contains(PLCamera.ExposureTimeAbs))
            {
                var param = camera.Parameters[PLCamera.ExposureTimeAbs];

                param.SetValue(parsed);
                ExposureTime = parsed;
            }
        }

    }
}