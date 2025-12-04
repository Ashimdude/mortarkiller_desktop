using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.PeerToPeer.Collaboration;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;

namespace mortarkiller
{

    public partial class Form1 : Form
    {
        int OGwidth = 0;
        int OGheight = 0;
        double dpinow = 1.0;
        public int CAPTURE_WIDTH = 20;
        public int CAPTURE_HEIGHT = 40;
        private System.Timers.Timer preciseTimer;
        private DateTime startTime;
        private const string MotdUrl = "http://5.61.47.45:9000/motd.txt";
        private const string PatchUrl = "http://5.61.47.45:9000/patchnotes.txt";
        private static readonly HttpClient client = new HttpClient();
        //system wide hotkey code I stole
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        enum KeyModifier
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            WinKey = 8
        }

        //global important variables
        TcpDoubleClient client1 = new TcpDoubleClient();
        double c1x = 0;
        double c2x = 0;
        double c1y = 0;
        double c2y = 0;
        double hndr = 0;
        double offset = 0;
        int crate_presses = 0;
        int ball_presses = 0;
        bool forgot_seta = false;
        double bridge_angle = 0;
        double carangle1 = 0;
        int getbearing_recall = 0;
        double crate_distance;
        TimeSpan whenshoot;
        double sx = 0;
        double sy = 0;
        double tx = 0;
        int latency = 0;
        double elevation = 0;
        Dictionary<string, string> slotValues = new Dictionary<string, string>();
        Dictionary<string, string> settings = new Dictionary<string, string>()
        {
            { "fov", "default"},
            { "os", "default" },
            { "fullscreen", "default"},
            { "width", "default" },
            { "height", "default" }
        };
        double car_start_x;
        double car_start_y;
        double car_end_x;
        double car_end_y;

        double ty = 0;
        double pixels = 0;
        double crate_angle = 0;
        double mdistance = 0;
        double waterdistance = 0;
        int waterpresses = 0;
        bool setq = false;
        bool setw = false;
        bool seta = false;
        bool sets = false;
        bool setx = false;
        bool setc = false;
        bool pro = false;
        bool beep1Done = false;
        bool beep2Done = false;
        bool beep3Done = false;
        int waitbridge = 0;
        double x_x = 0;
        double x_y = 0;
        double c_x = 0;
        double c_y = 0;
        double finalElapsed;
        TimeSpan beep1Time = TimeSpan.FromSeconds(120);
        TimeSpan beep2Time = TimeSpan.FromSeconds(120.5);
        TimeSpan beep3Time = TimeSpan.FromSeconds(121);
        bool recently_tracked = false;
        double g;
        double tune;
        double v0;
        int width1;
        int height1;
        double ratio1;
        double ratio2;
        double fov = 103;
        double vfov;
        double alpha;
        double beta;
        double crate_direction;
        Dictionary<string, double> real = new Dictionary<string, double>()
        {

        };
        Dictionary<double, string> solutions = new Dictionary<double, string>();
        //convert what you see on the pubg mortar into an angle (degrees * 10)
        public dynamic angles = new Dictionary<int, string>()
        {
            { 855, "121"},
            { 850, "133"},
            { 845, "145"},
            { 840, "157"},
            { 835, "169"},
            { 830, "181"},
            { 825, "193"},
            { 820, "204"},
            { 815, "216"},
            { 810, "228"},
            { 805, "239"},
            { 800, "250"},
            { 795, "262"},
            { 790, "273"},
            { 785, "284"},
            { 780, "295"},
            { 775, "307"},
            { 770, "317"},
            { 765, "328"},
            { 760, "339"},
            { 755, "350"},
            { 750, "360"},
            { 745, "371"},
            { 740, "381"},
            { 735, "391"},
            { 730, "401"},
            { 725, "411"},
            { 720, "421"},
            { 715, "431"},
            { 710, "440"},
            { 705, "450"},
            { 700, "459"},
            { 695, "468"},
            { 690, "477"},
            { 685, "486"},
            { 680, "495"},
            { 675, "503"},
            { 670, "512"},
            { 665, "520"},
            { 660, "528"},
            { 655, "536"},
            { 650, "544"},
            { 645, "551"},
            { 640, "559"},
            { 635, "566"},
            { 630, "573"},
            { 625, "580"},
            { 620, "587"},
            { 615, "593"},
            { 610, "600"},
            { 605, "606"},
            { 600, "612"},
            { 595, "618"},
            { 590, "624"},
            { 585, "629"},
            { 580, "634"},
            { 575, "639"},
            { 570, "644"},
            { 565, "649"},
            { 560, "653"},
            { 555, "658"},
            { 550, "662"},
            { 545, "666"},
            { 540, "669"},
            { 535, "673"},
            { 530, "676"},
            { 525, "679"},
            { 520, "682"},
            { 515, "685"},
            { 510, "687"},
            { 505, "689"},
            { 500, "691"},
            { 495, "693"},
            { 490, "695"},
            { 485, "696"},
            { 480, "697"},
            { 475, "698"},
            { 470, "699"},
            { 465, "699"},
            { 460, "700"},
            { 455, "700"},
        };
        Dictionary<string, int> compassToBearing = new Dictionary<string, int>
        {
            {"N", 0},
            {"NE", 45},
            {"E", 90},
            {"SE", 135},
            {"S", 180},
            {"SW", 225},
            {"W", 270},
            {"NW", 315},
            {"Т", 0},
            {"ТУ", 45},
            {"У", 90},
            {"ЫУ", 135},
            {"Ы", 180},
            {"ЫЦ", 225},
            {"Ц", 270},
            {"ТЦ", 315}
        };
        Dictionary<string, double> speeds = new Dictionary<string, double>
        {
            {"Red", 5.2656},
            {"Yellow", 7.349},
            {"BRDM", 14.2}
        };

        public Form1()
        {
            //the hotkey to open the program and also to calculate elevation
            InitializeComponent();
            LoadMotdAsync();
            this.KeyPreview = true;
            RegisterHotKey(this.Handle, 5, (int)KeyModifier.Alt, Keys.F.GetHashCode());
            SetupPreciseTimer();

        }
        private void SetupPreciseTimer()
        {
            // Create timer with 1ms interval for high precision
            preciseTimer = new System.Timers.Timer(1);

            // Ensure the event is raised on the UI thread
            preciseTimer.SynchronizingObject = this;

            // Auto-reset (true by default)
            preciseTimer.AutoReset = true;

            // Hook up the Elapsed event
            preciseTimer.Elapsed += (sender, e) =>
            {
                // Calculate elapsed time since start
                TimeSpan elapsed = DateTime.Now - startTime;

                if (!beep1Done && elapsed >= beep1Time)
                {
                    beep1Done = true;
                    beep3Done = false;
                    Console.Beep(2000, 30);
                }

                if (!beep2Done && elapsed >= beep2Time)
                {
                    beep2Done = true;
                    Console.Beep(2000, 30);
                }

                if (!beep3Done && elapsed >= beep3Time)
                {
                    beep3Done = true;
                    Console.Beep(2000, 30);
                    preciseTimer.Stop();
                    beep1Done = false;
                    beep2Done = false;
                }
            };
        }

        //become the active window
        bool pop()
        {
            var windowInApplicationIsFocused = Form.ActiveForm != null;
            if (!windowInApplicationIsFocused)
            {
                this.WindowState = FormWindowState.Minimized;
                this.WindowState = FormWindowState.Normal;
                return true;
            }
            else
            {
                return false;
            }
        }
        private async Task LoadMotdAsync()
        {
            try
            {
                // Use await to get the actual string
                string motdContent = await client.GetStringAsync(MotdUrl);
                var motdsplit = Regex.Split(motdContent, "\r\n|\r|\n");
                string version = motdsplit[motdsplit.Length - 1];
                motdsplit = motdsplit.Take(motdsplit.Length - 1).ToArray();
                motdContent = string.Join("\n", motdsplit);
                if ("1.4" != version)
                {
                    listView1.Clear();
                    var patchnotes = Regex.Split(await client.GetStringAsync(PatchUrl), "\r\n|\r|\n");
                    for (int i = 0; i < patchnotes.Length; i++)
                    {
                        listView1.Items.Add(patchnotes[i]);
                    }
                    if (listView1.Items[listView1.Items.Count - 1].Text == "ban")
                    {
                        MessageBox.Show("ПРОГРАММА В БАНЕ. PUBG BANNED, get update");
                        this.Close();
                    }

                }
                //pluck the version at the end of the motd and compare with local version
                //if old ask for an update
                // Update UI safely
                if (motdTextBox.InvokeRequired)
                {
                    motdTextBox.Invoke((MethodInvoker)(() => motdTextBox.Text = motdContent));
                }
                else
                {
                    motdTextBox.Text = motdContent;
                }
            }
            catch (HttpRequestException ex)
            {
                string errorMessage = $"Server error: {ex.Message}";
                motdTextBox.Invoke((MethodInvoker)(() => motdTextBox.Text = errorMessage));
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error: {ex.Message}";
                motdTextBox.Invoke((MethodInvoker)(() => motdTextBox.Text = errorMessage));
            }
        }
        static int PingServer(string host)
        {
            using (Ping ping = new Ping())
            {
                int pingCount = 3;
                int totalTime = 0;
                int successCount = 0;

                for (int i = 0; i < pingCount; i++)
                {
                    try
                    {
                        PingReply reply = ping.Send(host, 1000); // 1-second timeout
                        if (reply.Status == IPStatus.Success)
                        {
                            totalTime += (int)reply.RoundtripTime;
                            successCount++;
                        }
                        else
                        {
                            return 50;
                        }
                    }
                    catch (Exception)
                    {
                        return 50;
                    }
                }

                if (successCount > 0)
                    return totalTime / successCount; // average latency in ms
                else
                    return 50; // all failed
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            RegisterHotKey(this.Handle, 1, (int)KeyModifier.Alt, Keys.Q.GetHashCode());
            RegisterHotKey(this.Handle, 3, (int)KeyModifier.Alt, Keys.A.GetHashCode());
            RegisterHotKey(this.Handle, 4, (int)KeyModifier.Alt, Keys.S.GetHashCode());
            //listView is the output, contains both firing solutions and help cues for user
            listView1.Clear();
            listView1.Items.Add("SET MAP SCALE! Alt+Q, Alt+W");
            listView1.Items[0] = new ListViewItem(listView1.Items[0].Text)
            {
                ForeColor = Color.DarkRed
            };
            //PUBG specific physics constants
            g = 32.16;
            //tune explained later
            tune = -17.8;
            //-17.45
            //starting speed
            v0 = 151;
            //get screen res and ratio
            width1 = Screen.PrimaryScreen.Bounds.Width;
            OGwidth = width1;
            height1 = Screen.PrimaryScreen.Bounds.Height;
            OGheight = height1;
            comboBox3.SelectedItem = "🚫trackpad";
            ratio1 = 16;
            ratio2 = height1 / (width1 / ratio1);
            if (File.Exists("config.txt"))
            {
                using (StreamReader reader = new StreamReader("config.txt"))
                {
                    //loading scale config
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var parts = line.Split(':');
                        if (parts.Length == 2)
                        {
                            settings[parts[0]] = parts[1];
                        }
                    }
                }
                if (settings["fov"] != "default")
                {
                    trackBar1.Value = Convert.ToInt32(settings["fov"]);
                    fov = trackBar1.Value;
                    label1.Text = settings["fov"];
                }
                if (settings["width"] != "default")
                {
                    width1 = Convert.ToInt32(settings["width"]);
                    height1 = Convert.ToInt32(settings["height"]);
                    if (checkBox2.Checked)
                    {
                        button6.Hide();
                    }
                    ratio2 = height1 / (width1 / ratio1);
                }
                if (settings["os"] != default)
                {
                    comboBox3.SelectedItem = settings["os"];
                }
                else
                {
                    comboBox3.SelectedItem = "🚫trackpad";
                }
            }
            if (comboBox3.Text == "🚫trackpad")
            {
                offset = width1 / 240;
            }
            else
            {
                offset = width1 / 240;
            }
        }


        //I dont think I actually use this function here,
        //but you can feed distance and elevation into it and get pubg mortar output
        public int smallcalc(double dist, double elev)
        {
            double minErr = 10;
            string aim = "000";
            for (double i = 85.5; i >= 45.5; i -= 0.5)
            {
                double v0x = v0 * (Math.Cos(i / 180.0 * 3.14));
                double hmax = ((Math.Pow(v0, 2) * Math.Pow((Math.Sin(i / 180.0 * 3.14)), 2)) / (2.0 * g));
                hmax += tune;
                hmax += elev;
                double x = Convert.ToInt32(angles[Convert.ToInt32(i * 10)]) / 2.0;
                double t = 0.0;
                while (hmax >= 0)
                {
                    double vy = g * t;
                    t += 0.01;
                    x += (v0x * 0.01);
                    hmax -= (vy * 0.01);
                }
                if (Math.Abs(x - dist) < minErr)
                {
                    minErr = Math.Abs(x - dist);
                    aim = angles[Convert.ToInt32(i * 10)];
                }
            }
            return Convert.ToInt32(aim);
        }
        public void precisecalc(double dist, double elev, double bestangle, double besterror)
        {

            if (besterror == 0 || bestangle < 49 || listView1.Items.Count < 1 || (bestangle == 85.5 && besterror > 0))
            {
                return;
            }
            double secondangle = bestangle + ((Math.Abs(besterror) / besterror) * 0.5);
            int uno = Convert.ToInt32(angles[(int)(bestangle * 10)]);
            int dos = Convert.ToInt32(angles[(int)(secondangle * 10)]);
            double minErr = 6;
            double newangle = 0;
            int angle_to_meters(double angle)
            {
                return (int)Math.Round(uno + (angle - bestangle) * (dos - uno) / (secondangle - bestangle));
            }
            for (double i = bestangle; Math.Abs(i - bestangle) < 0.5; i += (Math.Abs(besterror) / besterror) * 0.1)
            {
                double v0x = v0 * (Math.Cos(i / 180.0 * 3.14));
                double hmax = ((Math.Pow(v0, 2) * Math.Pow((Math.Sin(i / 180.0 * 3.14)), 2)) / (2.0 * g));
                hmax += tune;
                hmax += elev;
                double x = angle_to_meters(i) / 2.0;
                double t = 0.0;
                while (hmax >= 0)
                {
                    double vy = g * t;
                    t += 0.01;
                    x += (v0x * 0.01);
                    hmax -= (vy * 0.01);
                }
                if (Math.Abs(x - dist) < Math.Abs(minErr))
                {
                    minErr = x - dist;
                    newangle = i;
                }
            }

            listView1.Items[0].Text = listView1.Items[0].Text + " ---> " + angle_to_meters(newangle).ToString();

        }
        public void getBearing(int bearing_input)
        {
            getbearing_recall = bearing_input;
            crate_direction = bearing_input - crate_angle;
            //listView1.Items.Add(crate_direction.ToString());
            //listView1.Items.Add(bearing_input.ToString());
            //listView1.Items.Add(crate_distance.ToString());
            crate_distance = (crate_distance / 100.0) * hndr;
            //listView1.Items.Add(crate_angle.ToString());
            //listView1.Items.Add(finalElapsed.ToString());
            UnregisterHotKey(this.Handle, 6);
            //DEBUG BRO!
            if (seta)
            {
                double angleRadians = crate_direction * (Math.PI / 180.0);
                int x = (int)(sx - offset + (crate_distance * Math.Cos(angleRadians)));
                int y = (int)(sy - (crate_distance * Math.Sin(angleRadians)));
                if (x >= 0 && x <= width1 + 1 && y >= 0 && y <= height1)
                {
                    this.Location = new Point(
                        x,
                        y
                    );
                    ty = y;
                    tx = x + offset;
                    sets = true;
                    mdistance = Math.Round(Math.Sqrt(Convert.ToDouble(((tx - sx) * (tx - sx)) + ((ty - sy) * (ty - sy)))) / hndr * 100, 2);
                    label4.Text = mdistance.ToString("#.##");
                    if (forgot_seta)
                    {
                        forgot_seta = false;
                        pop();
                    }
                    crate_presses = 0;
                    button7.Show();
                    button8.Show();
                    comboBox2.Hide();
                    checkBox4.Show();
                    button9.Show();
                    button11.Show();
                    recently_tracked = true;
                }
                else
                {
                    Console.Beep(500, 100);
                    Console.Beep(350, 230);
                    listView1.Items.Add("Give new Alt+A, doesnt fit");

                    forgot_seta = true;
                }
            }
            else
            {
                listView1.Items.Add("Forgot Alt+A, do it NOW");
                forgot_seta = true;
            }
        }
        public void joinBearing(double ownbearing, double friendbearing)
        {
            ownbearing = -1 * ownbearing * Math.PI / 180.0;
            friendbearing = -1 * friendbearing * Math.PI / 180.0;
            if (seta && sets)
            {
                double dx = tx - sx;
                double dy = ty - sy;
                double baseAngle = Math.Atan2(dy, dx); // Angle from (sx, sy) to (tx, ty)

                // Direction vectors for the two lines
                double dir1X = Math.Cos(ownbearing); // Direct use of adjusted angle
                double dir1Y = Math.Sin(ownbearing);

                double dir2X = Math.Cos(Math.PI + friendbearing); // Opposite direction from (tx, ty)
                double dir2Y = Math.Sin(Math.PI + friendbearing);

                // Determinant for solving intersection
                double det = dir1X * dir2Y - dir1Y * dir2X;

                // Check for parallel lines

                // Solve for t1 (parametric intersection)
                double t1 = ((tx - sx) * dir2Y - (ty - sy) * dir2X) / det;
                double x3 = sx + t1 * dir1X;
                double y3 = sy + t1 * dir1Y;

                if (x3 >= 0 && x3 <= width1 - 1 && y3 >= 0 && y3 <= height1 - 1)
                {
                    this.Location = new Point(
                        (int)(x3 - offset),
                        (int)(y3)
                    );
                    ty = y3;
                    tx = x3 + offset;
                    mdistance = Math.Round(Math.Sqrt(Convert.ToDouble(((tx - sx) * (tx - sx)) + ((ty - sy) * (ty - sy)))) / hndr * 100, 2);
                    label4.Text = mdistance.ToString("#.##");
                }
                else
                {
                    Console.Beep(500, 100);
                    Console.Beep(350, 230);
                }
                recently_tracked = true;

            }
        }
        void calc()
        {
            double minerr = 20;
            double bestangl = 90;
            real.Clear();
            solutions.Clear();
            listView1.Items.Clear();
            int ctr = 0;
            double best_t = 0;
            for (double i = 85.5; i >= 45.5; i -= 0.5)
            {
                //iterate through EVERY possible firing angle in PUBG mortar
                double v0x = v0 * (Math.Cos(i / 180.0 * 3.14));
                double hmax = ((Math.Pow(v0, 2) * Math.Pow((Math.Sin(i / 180.0 * 3.14)), 2)) / (2.0 * g));
                hmax += tune;
                //so this part is weird. Before I did this it overshot when shooting close and overshot when shooting far, or vice versa, idk.
                //and adjusting elevation is a way to affect both of those. Its pretty dead on now. My best guess is this has to do with the fact that
                //mortar projectile does not spawn right in the tube, spawns a bit higher I guess
                hmax += elevation;
                //and the hmax is weird. Its called hmax because I start simulating the projectile from the peak height
                //but it is really just the Y of the shell. 

                //I take the distance pubg gives you, halve it. Calculate the hmax by formula and THAT is my starting point.
                //I guess less simulation time is a way to accumulate less error. I think it works.
                //DRAWBACK: Cant target anything before the peak of the parabola, cant use the mortar as direct fire cannon for attacking skyscraper
                //not that bad

                double x = Convert.ToInt32(angles[Convert.ToInt32(i * 10)]) / 2.0;
                double t = 0.0;
                //physics sim
                while (hmax >= 0)
                {
                    double vy = g * t;
                    t += 0.01;
                    x += (v0x * 0.01);
                    hmax -= (vy * 0.01);
                }
                //IF HIT IS SOMEWHAT CLOSE
                if (Math.Abs(x - mdistance) < 10)
                {
                    if (Math.Abs(x - mdistance) < Math.Abs(minerr))
                    {
                        minerr = x - mdistance;
                        bestangl = i;
                        best_t = t;
                    }

                    //this part has to do with the fact that pubg has two different angles labeled as 699m
                    //and 700m same thing
                    if (i * 10 == 455)
                    {
                        real.Add("MAXIMUM 700", x);
                        solutions.Add(Math.Abs(x - mdistance), "MAXIMUM 700");
                    }
                    else if (i * 10 == 460)
                    {
                        real.Add("smaller 700", x);
                        solutions.Add(Math.Abs(x - mdistance), "smaller 700");
                    }
                    else if (i * 10 == 465)
                    {
                        real.Add("BIGGER 699", x);
                        solutions.Add(Math.Abs(x - mdistance), "BIGGER 699");
                    }
                    else if (i * 10 == 470)
                    {
                        real.Add("smaller 699", x);
                        solutions.Add(Math.Abs(x - mdistance), "smaller 699");
                    }
                    else
                    {
                        //any other angle with unique pubg display distance
                        real.Add(angles[Convert.ToInt32(i * 10)], x);
                        solutions.Add(Math.Abs(x - mdistance), angles[Convert.ToInt32(i * 10)]);
                    }
                    //sort by how good the hit is
                    solutions = solutions.OrderBy(obj => obj.Key).ToDictionary(obj => obj.Key, obj => obj.Value);
                    ctr++;
                }
            }
            //time between click and impact (time elapsed * 2 + length of the anim)
            //label8.Text = ((best_t + 2 + (mdistance / (2 * v0x))).ToString("#.###"));
            label8.Text = (2.100 + (mdistance / (146 * Math.Cos(bestangl / 180.0 * 3.14)))).ToString("#.###");
            //565.05
            //very accurate
            foreach (var item in solutions)
            {
                //sort formatting
                real[item.Value] = Math.Round(real[item.Value], 2);
                mdistance = Math.Round(mdistance, 2);
                if (real[item.Value] > mdistance)
                {
                    listView1.Items.Add(Math.Round(Math.Abs(real[item.Value] - mdistance), 2).ToString() + "m Overshoot.  Aim: " + item.Value);
                }
                else if (real[item.Value] < mdistance)
                {
                    listView1.Items.Add(Math.Round(Math.Abs(real[item.Value] - mdistance), 2).ToString() + "m Short.  Aim: " + item.Value);
                }
                else
                {
                    //if error is 0.000
                    //never happens lol
                    listView1.Items.Add("Precise Hit. Aim: " + item.Value);
                }
            }
            if (listView1.Items.Count != 0)
            {

                if (comboBox3.Text == "✅trackpad" && listView1.Items.Count >= 1)
                {
                    precisecalc(mdistance, elevation, bestangl, minerr);
                }
                //make best firing solution listed as first GREEN and sexy
                listView1.Items[0] = new ListViewItem(listView1.Items[0].Text)
                {
                    ForeColor = Color.Green
                };

            }
            else
            {
                listView1.Items.Add("NO FIRING SOLUTION! CANT HIT");
            }
        }
        //da fov slider
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label1.Text = trackBar1.Value.ToString();
            settings["fov"] = label1.Text;
        }
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if I remove this it doesnt compile
            //lol
        }
        double getElevation(double dist)
        {
            //this gets elevation from distance and angle
            //angle is calculated based on camera fov and resolution
            //you know how many pixels per degree, so you know how many degrees you got
            //angle and distance gets you the other side of the triangle
            vfov = Math.Atan(Math.Tan((fov / 2.0) / 180.0 * 3.14) * ratio2 / ratio1) * 2.0;
            pixels = (height1 / 2) - System.Windows.Forms.Control.MousePosition.Y / dpinow;
            double angle = Math.Atan(Math.Tan(vfov / 2.0) / (height1 / 2.0) * pixels);
            double el = Math.Tan(angle) * dist;
            el = el * -1;
            return el;
        }
        double getDistance(double el)
        {
            //get distance if elevation same as before (locked)
            //visual distance acquisition, without using map
            double fov = trackBar1.Value;
            double vfov = Math.Atan(Math.Tan((fov / 2.0) / 180.0 * 3.14) * ratio2 / ratio1) * 2.0;
            pixels = (height1 / 2) - System.Windows.Forms.Control.MousePosition.Y / dpinow;
            double angle = Math.Atan(Math.Tan(vfov / 2.0) / (height1 / 2.0) * pixels);
            //double elevation = Math.Tan(angle) * dist * -1;
            double dist = (el * -1) / Math.Tan(angle);

            return dist;
        }


        //OLD YOUTUBE TUTORIAL
        //NEEDS UPDATE
        //private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        //{
        //    System.Diagnostics.Process.Start("https://youtu.be/8PT1eohjcSA");
        //}

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == 0x0312)
            {
                /* Note that the three lines below are not needed if you only want to register one hotkey.
                 * The below lines are useful in case you want to register multiple keys, which you can use a switch with the id as argument, or if you want to know which key/modifier was pressed for some particular reason. */

                Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);                  // The key of the hotkey that was pressed.
                KeyModifier modifier = (KeyModifier)((int)m.LParam & 0xFFFF);       // The modifier of the hotkey that was pressed.
                int id = m.WParam.ToInt32();

                if (id == 1)
                {
                    //first key to set scale (alt q)
                    if (checkBox2.Checked)
                    {
                        if (height1 != OGheight)
                        {
                            dpinow = Graphics.FromHwnd(IntPtr.Zero).DpiX / 24.0 / 4.0;
                        }
                    }
                    c1x = System.Windows.Forms.Control.MousePosition.X / dpinow;
                    c1y = System.Windows.Forms.Control.MousePosition.Y / dpinow;
                    RegisterHotKey(this.Handle, 2, (int)KeyModifier.Alt, Keys.W.GetHashCode());
                    //unlocks the other key
                    setq = true;
                }
                if (id == 2)
                {
                    c2x = System.Windows.Forms.Control.MousePosition.X / dpinow;
                    c2y = System.Windows.Forms.Control.MousePosition.Y / dpinow;
                    if (setq)
                    {
                        //scale is not set!
                        setw = true;
                        mdistance = 0;
                        seta = false;
                        sets = false;
                        label4.Text = "";
                        listView1.Items.Clear();
                        UnregisterHotKey(this.Handle, 2);
                        listView1.Items.Add("Set distance now Alt+A, Alt+S");
                        setq = false;
                        if (c1x != 0 && c1y != 0)
                        {
                            hndr = Math.Max(Math.Abs(c2y - c1y), Math.Abs(c2x - c1x));
                            //how many pixels per 100m on screen
                        }
                    }
                }
                if (id == 3)
                {
                    //set mortar (player) position
                    sx = System.Windows.Forms.Control.MousePosition.X / dpinow;
                    sy = System.Windows.Forms.Control.MousePosition.Y / dpinow;
                    seta = true;
                    using (StreamWriter writer = new StreamWriter("config.txt"))
                    {
                        foreach (var pair in settings)
                        {
                            writer.WriteLine($"{pair.Key}:{pair.Value}");
                        }
                    }
                    if (setw && sets)
                    {
                        listView1.Items.Clear();
                        listView1.Items.Add("Now use cursor and Alt+F to set height");
                        mdistance = Math.Round(Math.Sqrt(Convert.ToDouble(((tx - sx) * (tx - sx)) + ((ty - sy) * (ty - sy)))) / hndr * 100, 2);
                        label4.Text = mdistance.ToString("#.##");
                        if (checkBox3.Checked)
                        {
                            calc();
                        }
                    }
                    if (forgot_seta)
                    {
                        getBearing(getbearing_recall);
                    }
                }
                if (id == 4)
                {
                    //set target position
                    //if target updates, it is assumed mortar pos is the same as before. Vice versa too
                    tx = System.Windows.Forms.Control.MousePosition.X / dpinow;
                    ty = System.Windows.Forms.Control.MousePosition.Y / dpinow;
                    sets = true;
                    if (setw && seta)
                    {
                        listView1.Items.Clear();
                        listView1.Items.Add("Now use cursor and Alt+F to set height");
                        mdistance = Math.Sqrt(Convert.ToDouble(((tx - sx) * (tx - sx)) + ((ty - sy) * (ty - sy)))) / hndr * 100;
                        label4.Text = mdistance.ToString("#.##");
                        //System.Windows.Forms.Clipboard.SetText(label4.Text);
                        if (checkBox3.Checked)
                        {
                            calc();
                        }
                    }
                    if (waitbridge == 1)
                    {
                        car_start_x = tx;
                        car_start_y = ty;
                        tx = car_end_x;
                        ty = car_end_y;
                        waitbridge = 2;
                    }
                }
                //this thing is for sorting the firing solutions by error. I forgot how it really works
                if (id == 5)
                {
                    //checkBox1.Text = (System.Windows.Forms.Control.MousePosition.X).ToString() + " " + System.Windows.Forms.Control.MousePosition.Y.ToString() + " " + dpinow.ToString();
                    //the altf hotkey
                    //if window not active, becomes active. If window active - gives you the elevation calculation for your cursor.
                    if ((checkBox2.Checked || !pop()) == false)
                    {
                        //if borderless mode only calculate when mortar window is open
                        //if not open, open it.
                        //if not borderless use the TTS and dont popup the window 
                        return;
                    }
                    if (seta && sets && setw)
                    {
                        using (StreamWriter writer = new StreamWriter("config.txt"))
                        {
                            foreach (var pair in settings)
                            {
                                writer.WriteLine($"{pair.Key}:{pair.Value}");
                            }
                        }
                        if (recently_tracked)
                        {
                            this.Location = new Point(
                                (int)(10),
                                (int)(10) // Flip sign for screen y
                            );
                            recently_tracked = false;
                        }
                        //calculation of firing solution begins
                        if (!checkBox3.Checked)
                        {
                            elevation = getElevation(mdistance);
                            //it only takes distance arg because it reads your cursor pos inside the function
                            label6.Text = elevation.ToString("#.##");
                        }
                        else
                        {
                            //locked elevation (gets distance from elevation and angle instead, rather than get elevation
                            mdistance = getDistance(elevation);
                            label4.Text = mdistance.ToString("#.##");
                        }
                        if (waitbridge == 2)
                        {
                            mdistance = Math.Sqrt(Convert.ToDouble(((tx - sx) * (tx - sx)) + ((ty - sy) * (ty - sy)))) / hndr * 100;
                            double hpixels = System.Windows.Forms.Control.MousePosition.X / dpinow - (width1 / 2.0);
                            bridge_angle = (Math.Atan(Math.Tan(fov / 2.0 * Math.PI / 180.0) / (width1 / 2.0) * hpixels) * 180.0 / Math.PI);
                            double angle1Degrees = 180 - Math.Atan2(sy - car_start_y, sx - car_start_x) * (180.0 / Math.PI);
                            bridge_angle = angle1Degrees + bridge_angle;
                        }
                        //where the sim and output is
                        calc();
                        //post sim

                        if (waitbridge == 2)
                        {
                            RegisterHotKey(this.Handle, 11, (int)KeyModifier.Alt, Keys.C.GetHashCode());
                            listView1.Items.Add("Now track car Alt+C");
                            waitbridge = 3;
                        }
                    }
                    else
                    {
                        //dynamic tips at different aiming stages
                        if (!setw)
                        {
                            listView1.Clear();
                            listView1.Items.Add("SET MAP SCALE! Alt+Q, Alt+W");
                            listView1.Items[0] = new ListViewItem(listView1.Items[0].Text)
                            {
                                ForeColor = Color.DarkRed
                            };
                        }
                        else
                        {
                            listView1.Clear();
                            listView1.Items.Add("DISTANCE NOT SET!");
                            listView1.Items[0] = new ListViewItem(listView1.Items[0].Text)
                            {
                                ForeColor = Color.DarkRed
                            };
                        }
                    }
                    if (checkBox2.Checked)
                    {
                        SpeechSynthesizer synth = new SpeechSynthesizer();

                        // Configure the audio output.
                        synth.SetOutputToDefaultAudioDevice();
                        // Speak a string
                        string fortts = listView1.Items[0].Text.Substring(listView1.Items[0].Text.Length - 3);
                        if (fortts != "t+W")
                        {
                            if (fortts != "HIT")
                            {
                                synth.Speak(fortts);
                            }
                            else
                            {
                                synth.Speak("cant shoot");
                            }
                        }
                    }
                    if (pro)
                    {
                        button5.Show();
                        checkBox3.Show();
                    }
                }
                if (id == 6)
                {
                    if (!seta)
                    {
                        listView1.Items.Add("FORGOT Alt+A ЗАБЫЛ Alt+A");
                    }
                    //SEE HOW FAST THE CRATE MOVED ACROSS YOUR SCREEN AND USE KNOWN CRATE CONSTANT SPEED TO FIND OUT THE DISTANCE
                    //THEN INPUT AZIMUTH AND USE DISTANCE + AZIMUTH TO SHOW WHERE IT IS ON THE MAP (WINDOW GOES THERE WITH THE CORNER)
                    if (crate_presses == 0)
                    {
                        vfov = Math.Atan(Math.Tan((fov / 2.0) / 180.0 * 3.14) * ratio2 / ratio1) * 2.0;
                        pixels = (height1 / 2) - System.Windows.Forms.Control.MousePosition.Y / dpinow;
                        double hpixels = System.Windows.Forms.Control.MousePosition.X / dpinow - (width1 / 2.0);
                        crate_angle = Math.Atan(Math.Tan(fov / 2.0 * Math.PI / 180.0) / (width1 / 2.0) * hpixels) * 180.0 / Math.PI;
                        startTime = DateTime.Now;
                        preciseTimer.Start();
                        beta = Math.Atan(Math.Tan(vfov / 2.0) / (height1 / 2.0) * pixels);
                        crate_presses = 1;
                        return;
                    }
                    if (crate_presses == 1)
                    {

                        preciseTimer.Stop();
                        pixels = (height1 / 2) - System.Windows.Forms.Control.MousePosition.Y / dpinow;
                        finalElapsed = (DateTime.Now - startTime).TotalSeconds;
                        alpha = Math.Atan(Math.Tan(vfov / 2.0) / (height1 / 2.0) * pixels);
                        crate_distance = -1 * (speeds[comboBox2.Text] * finalElapsed) / ((Math.Tan(alpha) - Math.Tan(beta)));
                        crate_presses = 2;
                        pop();
                        textBox3.Clear();
                        textBox3.Show();
                        textBox3.Focus();
                        return;
                    }
                }
                if (id == 7)
                {
                    if (waterpresses == 0)
                    {
                        double wx = System.Windows.Forms.Control.MousePosition.X / dpinow;
                        double wy = System.Windows.Forms.Control.MousePosition.Y / dpinow;
                        listView1.Items.Clear();
                        waterdistance = Math.Sqrt(Convert.ToDouble(((wx - sx) * (wx - sx)) + ((wy - sy) * (wy - sy)))) / hndr * 100;
                        waterpresses = 1;
                    }
                    else
                    {
                        elevation = getElevation(waterdistance);
                        label6.Text = elevation.ToString("#.##");
                        calc();
                        this.Location = new Point(
                            (int)(tx - offset),
                            (int)(ty)
                        );
                        waterpresses = 0;
                        recently_tracked = true;

                        UnregisterHotKey(this.Handle, 7);
                    }
                }
                if (id == 8)
                {
                    double hpixels = System.Windows.Forms.Control.MousePosition.X / dpinow - (width1 / 2.0);
                    crate_angle = Math.Atan(Math.Tan(fov / 2.0 * Math.PI / 180.0) / (width1 / 2.0) * hpixels) * 180.0 / Math.PI;
                    pop();
                    //listView1.Items.Add(crate_angle.ToString());
                    textBox3.Clear();
                    textBox3.Show();
                    textBox3.Focus();
                }
                if (id == 9)
                {
                    setx = true;
                    x_x = System.Windows.Forms.Control.MousePosition.X / dpinow;
                    x_y = System.Windows.Forms.Control.MousePosition.Y / dpinow;
                    if (seta && sets && setc)
                    {
                        pop();
                        double deltaX = x_x - sx;
                        double deltaY = x_y - sy;
                        double angleRadians = Math.Atan2(deltaY, deltaX);
                        double angle1Degrees = angleRadians * (180.0 / Math.PI);
                        if (angle1Degrees < 0)
                        {
                            angle1Degrees += 360;
                        }
                        deltaX = c_x - tx;
                        deltaY = c_y - ty;
                        angleRadians = Math.Atan2(deltaY, deltaX);
                        double angle2Degrees = angleRadians * (180.0 / Math.PI);
                        if (angle1Degrees < 0)
                        {
                            angle1Degrees += 360;
                        }
                        joinBearing(-1 * angle1Degrees, -1 * angle2Degrees);
                        UnregisterHotKey(this.Handle, 9);
                        UnregisterHotKey(this.Handle, 10);
                        setx = false;
                        setc = false;
                        button7.Show();
                        button2.Show();
                        button9.Show();
                        button11.Show();
                        checkBox4.Show();
                    }
                }
                if (id == 10)
                {
                    setc = true;
                    c_x = System.Windows.Forms.Control.MousePosition.X / dpinow;
                    c_y = System.Windows.Forms.Control.MousePosition.Y / dpinow;
                    if (seta && sets && setx)
                    {
                        pop();
                        double deltaX = x_x - sx;
                        double deltaY = x_y - sy;
                        double angleRadians = Math.Atan2(deltaY, deltaX);
                        double angle1Degrees = angleRadians * (180.0 / Math.PI);
                        if (angle1Degrees < 0)
                        {
                            angle1Degrees += 360;
                        }
                        deltaX = c_x - tx;
                        deltaY = c_y - ty;
                        angleRadians = Math.Atan2(deltaY, deltaX);
                        double angle2Degrees = angleRadians * (180.0 / Math.PI);
                        if (angle1Degrees < 0)
                        {
                            angle1Degrees += 360;
                        }
                        joinBearing(-1 * angle1Degrees, -1 * angle2Degrees);
                        UnregisterHotKey(this.Handle, 9);
                        UnregisterHotKey(this.Handle, 10);
                        button7.Show();
                        setx = false;
                        setc = false;
                        button2.Show();
                        button9.Show();
                        button11.Show();
                        checkBox4.Show();
                    }
                }
                if (id == 11)
                {
                    double hpixels = System.Windows.Forms.Control.MousePosition.X / dpinow - (width1 / 2.0);
                    double track_angle = Math.Atan(Math.Tan(fov / 2.0 * Math.PI / 180.0) / (width1 / 2.0) * hpixels) * 180.0 / Math.PI;
                    if (waitbridge == 3)
                    {
                        waitbridge++;
                        carangle1 = track_angle;
                        startTime = DateTime.Now;
                        preciseTimer.Start();

                    }
                    else if (waitbridge == 4)
                    {
                        double finalElapsed1 = (DateTime.Now - startTime).TotalSeconds;
                        double carpos1 = Geometry2D.GetRoadDistanceAtRayIntersection(
                            car_start_x, car_start_y,
                            car_end_x, car_end_y,
                            sx, sy,
                            bridge_angle, carangle1) / hndr * 100;
                        double carpos2 = Geometry2D.GetRoadDistanceAtRayIntersection(
                            car_start_x, car_start_y,
                            car_end_x, car_end_y,
                            sx, sy,
                            bridge_angle, track_angle) / hndr * 100;
                        double bridge_length = Math.Round(Math.Sqrt(Convert.ToDouble(Math.Pow(car_end_x - car_start_x, 2) + Math.Pow(car_end_y - car_start_y, 2))) / hndr * 100, 3);
                        double speed = (carpos2 - carpos1) / finalElapsed1;
                        double graphtime = Cars.TimeAtSpeed(speed);
                        double when = Cars.TimeAtDistance(Cars.DistanceAtTime(Cars.TimeAtSpeed(speed)) + (bridge_length - carpos2));
                        if (checkBox4.Checked)
                        {
                            client1.SendDouble(when - graphtime - (latency * 0.001) - 0.005);
                        }
                        double shoot = when - Convert.ToDouble(label8.Text) - graphtime + finalElapsed1;
                        whenshoot = TimeSpan.FromSeconds(shoot);
                        if (shoot < 0)
                        {
                            preciseTimer.Stop();
                            listView1.Items.Add("No intercept");
                            Console.Beep(500, 100);
                            Console.Beep(350, 230);

                        }
                        beep1Time = whenshoot - TimeSpan.FromSeconds(1.0);   // 1 s before
                        beep2Time = whenshoot - TimeSpan.FromSeconds(0.5);   // 0.5 s before
                        beep3Time = whenshoot;
                        waitbridge = 0;
                        button9.Show();
                        button2.Show();
                        button7.Show();
                        button8.Show();
                        button11.Show();
                        comboBox4.Hide();
                        UnregisterHotKey(this.Handle, 11);
                    }
                }
                if (id == 12)
                {
                    double shoot;
                    client1.ReceiveDouble(out shoot);
                    startTime = DateTime.Now;
                    preciseTimer.Start();
                    whenshoot = TimeSpan.FromSeconds(shoot - Convert.ToDouble(label8.Text) - (latency * 0.001));
                    beep1Time = whenshoot - TimeSpan.FromSeconds(1.0);   // 1 s before
                    beep2Time = whenshoot - TimeSpan.FromSeconds(0.5);   // 0.5 s before
                    beep3Time = whenshoot;
                    startTime = DateTime.Now;
                    preciseTimer.Start();
                }
            }
        }
        private void Form1_Closing(object sender, FormClosingEventArgs e)
        {
            UnregisterHotKey(this.Handle, 1);
            //UnregisterHotKey(this.Handle, 2);
            UnregisterHotKey(this.Handle, 3);
            UnregisterHotKey(this.Handle, 4);
            UnregisterHotKey(this.Handle, 5);

        }


        private void button1_Click(object sender, EventArgs e)
        {
            //custom res/fullscreen setup
            //not neccesary if you are using same res as in window and borderless
            if (label9.Visible)
            {
                if (textBox1.Text.Length > 0 && textBox2.Text.Length > 0)
                {
                    width1 = Convert.ToInt32(textBox1.Text);
                    height1 = Convert.ToInt32(textBox2.Text);
                    settings["width"] = textBox1.Text;
                    settings["height"] = textBox2.Text;
                    ratio1 = 16;
                    ratio2 = height1 / (width1 / ratio1);
                    offset = width1 / 240;
                }
                settings["fullscreen"] = checkBox2.Checked.ToString();
                using (StreamWriter writer = new StreamWriter("config.txt"))
                {
                    foreach (var pair in settings)
                    {
                        writer.WriteLine($"{pair.Key}:{pair.Value}");
                    }
                }
                textBox1.Hide();
                textBox2.Hide();
                label9.Hide();
                checkBox2.Hide();
                if (checkBox2.Checked)
                {
                    //checkbox2 is the actual fulscreen TTS mode
                    button6.Hide();
                    settings["fullscreen"] = "true";
                }
                else
                {
                    settings["fullscreen"] = "false";
                    button6.Show();
                }
            }
            else
            {
                button6.Hide();
                if (settings["fullscreen"] != "default")
                {
                    checkBox2.Checked = Convert.ToBoolean(settings["fullscreen"]);
                }
                textBox1.Show();
                textBox2.Show();
                label9.Show();
                checkBox2.Show();
                if (settings["width"] != "default")
                {
                    textBox1.Text = settings["width"];
                    textBox2.Text = settings["height"];
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string selectedSlot = comboBox1.Text;
            string filePath = "slots.txt";

            // Load existing slot values to avoid overwriting with an empty dictionary
            if (File.Exists(filePath))
            {
                slotValues.Clear(); // Clear current dictionary
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var parts = line.Split(':');
                        if (parts.Length == 2)
                        {
                            slotValues[parts[0]] = parts[1];
                        }
                    }
                }
            }

            // Update the selected slot with the new value
            slotValues[selectedSlot] = hndr.ToString();

            // Save all slot values back to the file
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (var pair in slotValues)
                {
                    writer.WriteLine($"{pair.Key}:{pair.Value}");
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string selectedSlot = comboBox1.Text;
            string filePath = "slots.txt";
            if (!File.Exists(filePath))
            {
                return;
            }
            slotValues.Clear();
            using (StreamReader reader = new StreamReader(filePath))
            {
                //loading scale config
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var parts = line.Split(':');
                    if (parts.Length == 2)
                    {
                        slotValues[parts[0]] = parts[1];
                    }
                }
            }
            if (slotValues.ContainsKey(selectedSlot))
            {
                setq = true;
                setw = true;
                double tmp = Convert.ToDouble(slotValues[selectedSlot]);
                if (tmp != 0)
                {
                    hndr = tmp;
                    if (listView1.Items.Count > 0)
                    {
                        if (listView1.Items[0].Text == ("SET MAP SCALE! Alt+Q, Alt+W"))
                        {
                            listView1.Items.RemoveAt(0);
                            UnregisterHotKey(this.Handle, 2);
                            listView1.Items.Add("Set distance now Alt+A, Alt+S");
                        }
                    }
                    listView1.Items.Add("Loaded✅");
                    //loading scale;
                }
                else
                {
                    listView1.Items.Add("🚫Nothing saved");
                }
            }
            else
            {
                listView1.Items.Add("🚫Nothing saved");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //start drop tracking 

            if (crate_presses == 0 && setw)
            {
                checkBox4.Hide();
                button7.Hide();
                button8.Hide();
                button9.Hide();
                button11.Hide();
                if (!seta)
                {
                    listView1.Items.Add("YOU NEED TO ALT+A");
                }
                //unlocks the hotkey and gives choice what kind of crate to track
                RegisterHotKey(this.Handle, 6, (int)KeyModifier.Alt, Keys.E.GetHashCode());
                comboBox2.Show();
                comboBox2.SelectedItem = "Red";
                listView1.Items.Add("Track the crate (Alt+E)");
            }
            else if (crate_presses == 2 && textBox3.Text != "")
            {
                //drop tracked, now accept compass input and do the math
                //then turn off the thingy
                getBearing(90 - compassToBearing[textBox3.Text.ToUpper()]);
                //where the character was looking on the compass
                UnregisterHotKey(this.Handle, 6);
                textBox3.Hide();
                textBox3.Clear();
            }
        }

        private void textBox3_KeyDown(object sender, KeyEventArgs e)
        {
            //shortcut to not press the button :D
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                if (!button7.Visible)
                {
                    button2.PerformClick();
                }
                else
                {
                    if (ball_presses == 1)
                    {
                        crate_direction = 90 - compassToBearing[textBox3.Text.ToUpper()] - crate_angle;
                        listView1.Items.Add("Bearing is " + crate_direction.ToString("#.#"));
                        UnregisterHotKey(this.Handle, 8);
                        textBox3.Clear();
                        textBox3.Show();
                        textBox3.Focus();
                        ball_presses = 2;
                    }
                    else
                    {
                        if (textBox3.Text != " " && textBox3.Text != "")
                        {
                            var fmt = new NumberFormatInfo();
                            fmt.NegativeSign = "-";
                            joinBearing(crate_direction, double.Parse(textBox3.Text, fmt));
                        }
                        ball_presses = 0;
                        button8.Show();
                        button2.Show();
                        textBox3.Hide();
                        textBox3.Clear();
                        checkBox4.Show();
                        button9.Show();
                        button11.Show();
                    }
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //elevation override
            //when the distance is good but you know that the elevation should be something else
            //e.g. marker from the map placed onto bridges/ocean
            RegisterHotKey(this.Handle, 7, (int)KeyModifier.Alt, Keys.T.GetHashCode());
            button5.Hide();
            listView1.Items.Add("Alt+T Alt+T override elevation");

        }

        private void button6_Click(object sender, EventArgs e)
        {
            //PRO MODE (lock elevation, override elevation, drop tracking)
            string path = Path.Combine("Uaz.csv");
            if (!File.Exists(path))
            {
                listView1.Items.Add("Cars DLC not installed!");
            }
            else
            {
                button9.Show();
                string csv = File.ReadAllText(path);
                Cars.LoadFromCsv(csv);
            }
            comboBox3.Show();
            checkBox4.Show();
            button1.Hide();
            textBox1.Hide();
            label9.Hide();
            textBox2.Hide();
            checkBox2.Hide();
            checkBox2.Checked = false;
            pro = true;
            button6.Hide();
            button2.Show();
            button7.Show();
            button8.Show();
            button10.Show();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (ball_presses == 0)
            {
                checkBox4.Hide();
                button9.Hide();
                button11.Hide();
                button8.Hide();
                RegisterHotKey(this.Handle, 8, (int)KeyModifier.Alt, Keys.E.GetHashCode());
                listView1.Items.Add("Triangulation srarted");
                ball_presses = 1;
                button2.Hide();
            }
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            settings["os"] = comboBox3.Text;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            checkBox4.Hide();
            checkBox4.Hide();
            button9.Hide();
            button11.Hide();
            button2.Hide();
            if (button7.Visible)
            {
                RegisterHotKey(this.Handle, 9, (int)KeyModifier.Shift, Keys.X.GetHashCode());
                RegisterHotKey(this.Handle, 10, (int)KeyModifier.Shift, Keys.C.GetHashCode());
            }
            button7.Hide();
            listView1.Items.Add("triangulate Shift+x shit+C");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (sets)
            {
                car_end_x = tx;
                car_end_y = ty;
                button9.Hide();
                waitbridge = 1;
                comboBox4.Show();
                button11.Hide();
                button2.Hide();
                button7.Hide();
                button8.Hide();
            }
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            string path = Path.Combine(comboBox4.Text + ".csv");
            if (!File.Exists(path))
            {
                MessageBox.Show("No .csv");
                return;
            }

            string csv = File.ReadAllText(path);
            Cars.LoadFromCsv(csv);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            UnregisterHotKey(this.Handle, 6);
            UnregisterHotKey(this.Handle, 7);
            UnregisterHotKey(this.Handle, 8);
            UnregisterHotKey(this.Handle, 9);
            UnregisterHotKey(this.Handle, 10);
            UnregisterHotKey(this.Handle, 11);
            forgot_seta = false;
            recently_tracked = false;
            listView1.Clear();
            comboBox2.Hide();
            button9.Show();
            button2.Show();
            button7.Show();
            button8.Show();
            waitbridge = 0;
            ball_presses = 0;
            crate_presses = 0;
            comboBox4.Hide();
            button11.Hide();
            preciseTimer.Stop();
            textBox3.Clear();
            textBox3.Hide();

            startTime = DateTime.Now;
            beep1Time = TimeSpan.FromSeconds(120);
            beep2Time = TimeSpan.FromSeconds(120.5);
            beep3Time = TimeSpan.FromSeconds(121);
        }
        private void button11_Click(object sender, EventArgs e)
        {
            RegisterHotKey(this.Handle, 11, (int)KeyModifier.Alt, Keys.C.GetHashCode());
            listView1.Clear();
            listView1.Items.Add("Now track car Alt+C");
            button9.Hide();
            button2.Hide();
            button7.Hide();
            button8.Hide();
            waitbridge = 3;
            comboBox4.Show();
            button11.Hide();
            preciseTimer.Stop();
            startTime = DateTime.Now;
            beep1Time = TimeSpan.FromSeconds(120);
            beep2Time = TimeSpan.FromSeconds(120.5);
            beep3Time = TimeSpan.FromSeconds(121);
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked)
            {
                latency = PingServer("5.61.47.45");
                listView1.Items.Add("Exchange ping: " + latency.ToString() + " ms");
                RegisterHotKey(this.Handle, 12, (int)KeyModifier.Shift, Keys.T.GetHashCode());
            }
            else
            {
                UnregisterHotKey(this.Handle, 12);
            }
        }

    }
}


