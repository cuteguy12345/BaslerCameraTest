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
        private Stopwatch stopwatch = new();

        public MainViewModel()
        {

            OneShotCommand = new RelayCommand(OneShot);
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


        private double _gain;
        public double Gain
        {
            get => _gain;
            set 
            { 
                _gain = value;
                OnPropertyChanged(); 
            }
        }

        private double _exposureTime;
        public double ExposureTime
        {
            get => _exposureTime;
            set 
            { 
                _exposureTime = value;
                OnPropertyChanged(); 
            }
        }

        private double _gamma;
        public double Gamma
        {
            get => _gamma;
            set 
            {   
                _gamma = value;
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
                      
            Application.Current.Dispatcher.Invoke(() =>
            {
                var image = CameraHelper.Instance.GrabImage(camera, GetCurrentCameraSettings(), e.GrabResult);

                CapturedImage = image;
                OnPropertyChanged(nameof(CapturedImage));

            });

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

                if (camera.Parameters.Contains(PLCamera.ExposureTime))
                    ExposureTime = camera.Parameters[PLCamera.ExposureTime].GetValue();

                if (camera.Parameters.Contains(PLCamera.Gain))
                {
                    Gain = camera.Parameters[PLCamera.Gain].GetValue();
                }

                if (camera.Parameters.Contains(PLCamera.Gamma))
                    Gamma = camera.Parameters[PLCamera.Gamma].GetValue();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"카메라 초기화 실패: {ex.Message}");
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

        public CameraSettings GetCurrentCameraSettings() => new CameraSettings()
        {
            Width = this.Width,
            Height = this.Height,
            Gain = this.Gain,
            ExposureTime = this.ExposureTime,
            Gamma = this.Gamma
        };
    }
}
