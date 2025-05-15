using Basler.Pylon;
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
using Basler.Pylon;

namespace BaslerCameraTest
{
    public class MainViewModel : ViewModelBase
    {
        private Basler.Pylon.Camera? camera = null;

        public MainViewModel()
        {
            ShotCommand = new RelayCommand(ContinuousShot);
            OneShotCommand = new RelayCommand(OneShot);
            StopCommand = new RelayCommand(Stop);
            RefreshDeviceList();
        }
        public ObservableCollection<ICameraInfo> AvailableCameras { get; } = new();

        public void RefreshDeviceList()
        {
            AvailableCameras.Clear();
            foreach (var cam in CameraFinder.Enumerate())
            {
                AvailableCameras.Add(cam);
            }
        }

        public ICommand OneShotCommand { get ; set; }
        public ICommand ShotCommand { get; set; }
        public ICommand StopCommand { get; set; }

        private int _width;
        public int Width
        {
            get => _width;
            set 
            {
                _width = value;
                OnPropertyChanged(); 
            }
        }

        private int _height;
        public int Height
        {
            get => _height;
            set 
            {
                _height = value;
                OnPropertyChanged(); 
            }
        }


        private double _gainRaw;
        public double GainRaw
        {
            get => _gainRaw;
            set 
            {
                _gainRaw = value;
                OnPropertyChanged(); 
            }
        }

        private long _exposureTime;
        public long ExposureTime
        {
            get => _exposureTime;
            set 
            { 
                _exposureTime = value;
                OnPropertyChanged(); 
            }
        }

        private BitmapImage _capturedImage;
        public BitmapImage CapturedImage
        {
            get => _capturedImage;
            set 
            {
                _capturedImage = value; 
                OnPropertyChanged(nameof(CapturedImage));
            }
        }

        private void OnImageGrabbed(object sender, ImageGrabbedEventArgs e)
        {
            try
            {
                if (camera == null || e.GrabResult == null || !e.GrabResult.GrabSucceeded)
                    return;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    var image = CameraHelper.Instance.GrabImage(camera, GetCurrentCameraSettings(), e.GrabResult);
                    if (image != null)
                    {
                        CapturedImage = image;
                        OnPropertyChanged(nameof(CapturedImage));
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void InitializeCamera(ICameraInfo cameraInfo)
        {
            try
            {
                if (camera != null && camera.IsOpen)
                {
                    camera.Close();
                }

                camera = new Basler.Pylon.Camera(cameraInfo);
                camera.Open();

                camera.StreamGrabber.ImageGrabbed += OnImageGrabbed;
                if (camera.Parameters.Contains(PLCamera.Width))
                    Width = (int)camera.Parameters[PLCamera.Width].GetValue();

                if (camera.Parameters.Contains(PLCamera.Height))
                    Height = (int)camera.Parameters[PLCamera.Height].GetValue();

                if (camera.Parameters.Contains(PLCamera.Gain))
                {
                    GainRaw = camera.Parameters[PLCamera.Gain].GetValue();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private ICameraInfo _selectedCamera;
        public ICameraInfo SelectedCamera
        {
            get => _selectedCamera;
            set
            {
                if (_selectedCamera != value)
                {
                    _selectedCamera = value;
                    OnPropertyChanged();
                    InitializeCamera(_selectedCamera);
                }
            }
        }

        private void OneShot()
        {
            CameraHelper.Instance.OneShot(camera, GetCurrentCameraSettings());

        }
        private void ContinuousShot()
        {
            CameraHelper.Instance.ContinuousShot(camera, GetCurrentCameraSettings());

        }

        private void Stop()
        {
            CameraHelper.Instance.Stop(camera);
        }
        public CameraSettings GetCurrentCameraSettings() => new CameraSettings()
        {
            Width = this.Width,
            Height = this.Height,
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

        private void ApplyAndSaveSettings()
        {
            try
            {
                if (camera == null || !camera.IsOpen)
                {
                    MessageBox.Show("카메라가 열려 있지 않습니다.");
                    return;
                }


                if (camera.Parameters.Contains(PLCamera.GainRaw))
                    camera.Parameters[PLCamera.GainRaw].SetValue((long)GainRaw);

                if (camera.Parameters.Contains(PLCamera.ExposureAuto))
                    camera.Parameters[PLCamera.ExposureAuto].SetValue("Off");

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
            }
            catch (Exception ex)
            {
                MessageBox.Show("설정 저장 실패: " + ex.Message);
            }
        }
    }
}
