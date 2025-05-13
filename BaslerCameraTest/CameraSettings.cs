using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaslerCameraTest
{
    public class CameraSettings
    {
        public double ExposureTime { get; set; }
        public long GainRaw {  get; set; }
        public int Width {  get; set; }
        public int Height { get; set; }
    }
}
