using Microsoft.Kinect;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LightBuzz.Kinect2CSV.Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensor _sensor = null;
        KinectCSVManager _recorder = null;
        BodyFrameReader _reader = null;
        IList<Body> _bodies = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _sensor = KinectSensor.GetDefault();

            if (_sensor != null)
            {
                _sensor.Open();

                _bodies = new Body[_sensor.BodyFrameSource.BodyCount];

                _reader = _sensor.BodyFrameSource.OpenReader();
                _reader.FrameArrived += BodyReader_FrameArrived;

                _recorder = new KinectCSVManager();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (_reader != null)
            {
                _reader.Dispose();
                _reader = null;
            }

            if (_sensor != null)
            {
                _sensor.Close();
                _sensor = null;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (_recorder.IsRecording)
            {
                _recorder.Stop();

                button.Content = "Start";

                SaveFileDialog dialog = new SaveFileDialog
                {
                    Filter = "Excel files|*.csv"
                };

                dialog.ShowDialog();

                if (!string.IsNullOrWhiteSpace(dialog.FileName))
                {
                    System.IO.File.Copy(_recorder.Result, dialog.FileName);
                }
            }
            else
            {
                _recorder.Start();

                button.Content = "Stop";
            }
        }

        void BodyReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    frame.GetAndRefreshBodyData(_bodies);

                    Body body = _bodies.Where(b => b != null && b.IsTracked).FirstOrDefault();

                    if (body != null)
                    {
                        _recorder.Update(body);
                    }
                }
            }
        }
    }
}
