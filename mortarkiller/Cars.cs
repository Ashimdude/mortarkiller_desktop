using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace mortarkiller
{
    public static class Cars
    {
        public readonly struct Point
        {
            public double T { get; }
            public double X { get; }
            public double V { get; }
            public Point(double t, double x, double v) => (T, X, V) = (t, x, v);
        }
        

        private static List<Point> _data = new List<Point>();

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public static bool IsTslGameFocused()
        {
            IntPtr foregroundHwnd = GetForegroundWindow();
            if (foregroundHwnd == IntPtr.Zero)
                return false;

            uint pid;
            GetWindowThreadProcessId(foregroundHwnd, out pid);

            if (pid == 0)
                return false;

            try
            {
                Process proc = Process.GetProcessById((int)pid);
                return string.Equals(proc.ProcessName, "TslGame", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                // Process already exited or access denied → definitely not focused
                return false;
            }
        }

        public static void LoadFromCsv(string csvContent)
        {
            _data.Clear();
            var lines = csvContent
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Skip(1);

            foreach (var line in lines)
            {
                var cols = line.Split(';');
                if (cols.Length != 3) continue;

                if (double.TryParse(cols[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var t) &&
                    double.TryParse(cols[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var x) &&
                    double.TryParse(cols[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                {
                    _data.Add(new Point(t, x, v));
                }
            }
            _data.Sort((a, b) => a.T.CompareTo(b.T));
        }

        public static double TimeAtSpeed(double targetSpeed)
        {
            if (_data.Count < 2) throw new InvalidOperationException("Data not loaded.");
            double minV = _data[0].V, maxV = _data[_data.Count - 1].V;
            if (targetSpeed <= minV) return _data[0].T;
            if (targetSpeed >= maxV) return _data[_data.Count - 1].T;

            for (int i = 0; i < _data.Count - 1; i++)
            {
                var p0 = _data[i]; var p1 = _data[i + 1];
                if (p0.V <= targetSpeed && targetSpeed < p1.V)
                {
                    double f = (targetSpeed - p0.V) / (p1.V - p0.V);
                    return p0.T + f * (p1.T - p0.T);
                }
            }
            return _data[_data.Count - 1].T;
        }
        public static double TimeAtDistance(double targetDistance)
        {
            if (_data.Count < 2) throw new InvalidOperationException("Data not loaded.");
            double minX = _data[0].X, maxX = _data[_data.Count - 1].X;
            if (targetDistance <= minX) return Math.Round(_data[0].T, 3);
            if (targetDistance >= maxX) return Math.Round(_data[_data.Count - 1].T, 3);

            for (int i = 0; i < _data.Count - 1; i++)
            {
                var p0 = _data[i];
                var p1 = _data[i + 1];
                if (p0.X <= targetDistance && targetDistance < p1.X)
                {
                    double f = (targetDistance - p0.X) / (p1.X - p0.X);
                    return Math.Round(p0.T + f * (p1.T - p0.T), 3);
                }
            }
            return Math.Round(_data[_data.Count - 1].T, 3);
        }

        public static double DistanceAtTime(double targetTime)
        {
            if (_data.Count < 2) throw new InvalidOperationException("Data not loaded.");
            double minT = _data[0].T, maxT = _data[_data.Count - 1].T;
            if (targetTime <= minT) return _data[0].X;
            if (targetTime >= maxT) return _data[_data.Count - 1].X;

            for (int i = 0; i < _data.Count - 1; i++)
            {
                var p0 = _data[i]; var p1 = _data[i + 1];
                if (p0.T <= targetTime && targetTime < p1.T)
                {
                    double f = (targetTime - p0.T) / (p1.T - p0.T);
                    return p0.X + f * (p1.X - p0.X);
                }
            }
            return _data[_data.Count - 1].X;
        }
    }
    public static class Geometry2D
    {
        public static double GetRoadDistanceAtRayIntersection(
            double roadStartX, double roadStartY,
            double roadEndX, double roadEndY,
            double camX, double camY,
            double camAngleDeg,
            double rayAngleRelativeDeg)
        {
            // Convert degrees to radians
            double toRad = Math.PI / 180.0;
            double totalAngle = (camAngleDeg - rayAngleRelativeDeg) * toRad;

            // Road direction vector
            double rdx = roadEndX - roadStartX;
            double rdy = roadEndY - roadStartY;

            // Ray direction vector (screen coords: y down)
            double raydx = Math.Cos(totalAngle);
            double raydy = -Math.Sin(totalAngle);

            // Solve intersection using line param equations:
            // Road: P = R0 + t*Rdir
            // Ray:  Q = C0 + u*Cdir
            // We solve for t (along road) and u (along ray)
            double denom = rdx * (-raydy) + rdy * raydx;
            if (Math.Abs(denom) < 1e-9)
                throw new Exception("Lines are parallel — no intersection");

            double t = ((camX - roadStartX) * (-raydy) + (camY - roadStartY) * raydx) / denom;
            // double u = ((roadStartX - camX) * rdy - (roadStartY - camY) * rdx) / denom;

            // Clamp intersection to road segment if needed
            // (Assuming always intersects per your description)

            // Distance along the road from its start
            double roadLength = Math.Sqrt(rdx * rdx + rdy * rdy);
            double distanceAlongRoad = t * roadLength;

            return distanceAlongRoad;
        }
    }
    public class TcpDoubleClient
    {
        private const string ServerIp = "5.61.47.45"; // Replace with your server's IP
        private const int Port = 5001; // Replace with your server's port

        public bool SendDouble(double value)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    client.Connect(ServerIp, Port);
                    using (NetworkStream stream = client.GetStream())
                    {
                        // Send command byte 'S'
                        stream.WriteByte((byte)'S');
                        // Send double
                        byte[] buffer = BitConverter.GetBytes(value);
                        stream.Write(buffer, 0, buffer.Length);
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending double: {ex.Message}");
                return false;
            }
        }

        public bool ReceiveDouble(out double value)
        {
            value = 0;
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    client.Connect(ServerIp, Port);
                    using (NetworkStream stream = client.GetStream())
                    {
                        // Send command byte 'R'
                        stream.WriteByte((byte)'R');
                        // Read double
                        byte[] buffer = new byte[8];
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);

                        if (bytesRead == 8)
                        {
                            value = BitConverter.ToDouble(buffer, 0);
                            return true;
                        }
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving double: {ex.Message}");
                return false;
            }
        }

    }



}

