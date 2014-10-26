using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightBuzz.Kinect2CSV
{
    public class KinectCSVManager
    {
        int _current = 0;

        bool _hasEnumeratedJoints = false;

        public bool IsRecording { get; protected set; }

        public string Folder { get; protected set; }

        public string Result { get; protected set; }

        public void Start()
        {
            IsRecording = true;
            Folder = DateTime.Now.ToString("yyy_MM_dd_HH_mm_ss");

            Directory.CreateDirectory(Folder);
        }

        public void Update(Body body)
        {
            if (!IsRecording) return;
            if (body == null || !body.IsTracked) return;

            string path = Path.Combine(Folder, _current.ToString() + ".line");

            using (StreamWriter writer = new StreamWriter(path))
            {
                StringBuilder line = new StringBuilder();
                
                if (!_hasEnumeratedJoints)
                {
                    foreach (var joint in body.Joints.Values)
                    {
                        line.Append(string.Format("{0};;;", joint.JointType.ToString()));
                    }
                    line.AppendLine();

                    foreach (var joint in body.Joints.Values)
                    {
                        line.Append("X;Y;Z;");
                    }
                    line.AppendLine();

                    _hasEnumeratedJoints = true;
                }

                foreach (var joint in body.Joints.Values)
                {
                    line.Append(string.Format("{0};{1};{2};", joint.Position.X, joint.Position.Y, joint.Position.Z));
                }

                writer.Write(line);

                _current++;
            }
        }

        public void Stop()
        {
            IsRecording = false;

            Result = DateTime.Now.ToString("yyy_MM_dd_HH_mm_ss") + ".csv";

            using (StreamWriter writer = new StreamWriter(Result))
            {
                for (int index = 0; index < _current; index++)
                {
                    string path = Path.Combine(Folder, index.ToString() + ".line");

                    if (File.Exists(path))
                    {
                        string line = string.Empty;

                        using (StreamReader reader = new StreamReader(path))
                        {
                            line = reader.ReadToEnd();
                        }

                        writer.WriteLine(line);
                    }
                }
            }

            Directory.Delete(Folder, true);
        }
    }
}