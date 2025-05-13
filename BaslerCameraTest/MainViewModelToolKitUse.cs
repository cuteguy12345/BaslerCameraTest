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
using System.Windows.Media.Media3D;
using System.Windows.Threading;


namespace BaslerCameraTest
{
    public partial class MainViewModelToolKitUse : ObservableObject
    {
        private Basler.Pylon.Camera? camera = null;

        public MainViewModelToolKitUse()
        {
            RefreshDeviceList();
        }

        [ObservableProperty]
        private ObservableCollection<ICameraInfo> _availableCameras = new();
        private bool CanExecuteCameraCommand() => SelectedCamera != null;


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
        private long _exposureTimeRaw;


        [RelayCommand(CanExecute = nameof(CanExecuteCameraCommand))]
        private void OneShot()
        {
            CameraHelper.Instance.OneShot(camera, GetCurrentCameraSettings());
        }

        [RelayCommand(CanExecute = nameof(CanExecuteCameraCommand))]
        private void ContinuousShot()
        {
            CameraHelper.Instance.ContinuousShot(camera);
        }

        [RelayCommand(CanExecute = nameof(CanExecuteCameraCommand))]
        private void Stop()
        {
            CameraHelper.Instance.Stop(camera);
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
            ApplyAndSaveSettingsCommand.NotifyCanExecuteChanged();
        }


        private void InitializeCamera(ICameraInfo cameraInfo)
        {
            try
            {
                camera?.Close();

                camera = new Basler.Pylon.Camera(cameraInfo);
                camera.Open();

                camera.StreamGrabber.ImageGrabbed += OnImageGrabbed;

                if (camera.Parameters.Contains(PLCamera.UserSetSelector))
                    camera.Parameters[PLCamera.UserSetSelector].SetValue("UserSet1");

                if (camera.Parameters.Contains(PLCamera.UserSetLoad))
                    camera.Parameters[PLCamera.UserSetLoad].Execute();
                
                if (camera.Parameters.Contains(PLCamera.ExposureTimeAbs))
                    ExposureTime = camera.Parameters[PLCamera.ExposureTimeAbs].GetValue();

                if(camera.Parameters.Contains(PLCamera.GainRaw))
                    Gainraw = camera.Parameters[PLCamera.GainRaw].GetValue();


                if (camera.Parameters.Contains(PLCamera.Width))
                    Width = (int)camera.Parameters[PLCamera.Width].GetValue();

                if (camera.Parameters.Contains(PLCamera.Height))
                    Height = (int)camera.Parameters[PLCamera.Height].GetValue();

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

        private void SaveUserSet()
        {
            try
            {
                if (camera.Parameters.Contains(PLCamera.UserSetSelector))
                    camera.Parameters[PLCamera.UserSetSelector].SetValue("UserSet1");

                if (camera.Parameters.Contains(PLCamera.UserSetDefaultSelector))
                    camera.Parameters[PLCamera.UserSetDefaultSelector].SetValue("UserSet1");

                if (camera.Parameters.Contains(PLCamera.UserSetSave))
                    camera.Parameters[PLCamera.UserSetSave].Execute();
            }
            catch (Exception ex)
            {
                MessageBox.Show("설정 저장 실패: " + ex.Message);
            }
        }



        [RelayCommand(CanExecute = nameof(CanExecuteCameraCommand))]

        private void ApplyAndSaveSettings()
        {
            try
            {
                if (camera == null || !camera.IsOpen)
                {
                    MessageBox.Show("카메라가 열려 있지 않습니다.");
                    return;
                }
                //var entries = camera.Parameters[PLCamera.UserSetSelector].GetAllValues();
                //foreach (var entry in entries)
                //{
                //    MessageBox.Show($"지원되는 UserSet: {entry}");
                //}
                if (camera.Parameters.Contains(PLCamera.GainRaw))
                    camera.Parameters[PLCamera.GainRaw].SetValue(Gainraw);

                //if (camera.Parameters.Contains(PLCamera.ExposureAuto))
                //    camera.Parameters[PLCamera.ExposureAuto].SetValue("Off");

                if (camera.Parameters.Contains(PLCamera.ExposureTimeAbs))
                    camera.Parameters[PLCamera.ExposureTimeAbs].SetValue(ExposureTime);

                if (camera.Parameters.Contains(PLCamera.Width))
                    camera.Parameters[PLCamera.Width].SetValue(Width);

                if (camera.Parameters.Contains(PLCamera.Height))
                    camera.Parameters[PLCamera.Height].SetValue(Height);

                if (camera.Parameters.Contains(PLCamera.UserSetSelector))
                    camera.Parameters[PLCamera.UserSetSelector].SetValue("UserSet1");

                if (camera.Parameters.Contains(PLCamera.UserSetDefaultSelector))
                    camera.Parameters[PLCamera.UserSetDefaultSelector].SetValue("UserSet1");

                if (camera.Parameters.Contains(PLCamera.UserSetSave))
                    camera.Parameters[PLCamera.UserSetSave].Execute();


                MessageBox.Show(" 설정 저장 완료");
                //var entries = camera.Parameters[PLCamera.UserSetSelector].GetAllValues();
                //foreach (var entry in entries)
                //{
                //    Console.WriteLine($"지원되는 UserSet: {entry}");
                //}

            }
            catch (Exception ex)
            {
                MessageBox.Show("설정 저장 실패: " + ex.Message);
            }
        }
    }
}