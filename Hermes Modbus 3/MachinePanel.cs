using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HermesToolBox;

namespace Hermes_Modbus_3
{
    public partial class MachinePanel : Form
    {
        public MachinePanel()
        {
            InitializeComponent();
        }
        Color MachinePanelColor = Color.FromArgb(51, 153, 255);//110, 136, 152
        Color LabelColor = Color.FromArgb(0, 51, 102);
        // Color RedAlertColor = Color.FromArgb(217,4,41);
        Color BackgroundColor = Color.FromArgb(202, 210, 197); // 46, 82, 102, allan (46, 82, 102)
        Color BorderColor = Color.FromArgb(132, 169, 140);//110, 136, 152   
        Color TitleColor = Color.FromArgb(46, 82, 102); // 211, 208, 203
        Color TextColor = Color.FromArgb(211, 208, 203);
        Color CircularProgressBarColor = Color.FromArgb(204, 255, 0);
        Color InnerProgressColor = Color.FromArgb(40, 250, 250, 250);
        Color OfflineColor = Color.Gray;

        Color BorderRedMachine = Color.IndianRed;
        Color BorderYellowMachine = Color.Yellow;
        Color BorderGreenMachine = Color.Green;

        Color RedMachine = Color.IndianRed;
        Color YellowMachine = Color.Yellow;
        Color GreenMachine = Color.Green;

        Color IddleWorkerColor = Color.IndianRed;

        Machine[] Machines;

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
            Console.WriteLine("Close:");
            var app = Application.OpenForms["MainPage"];
            if (app == null)
            {
                MainPage main = new MainPage();
                main.Show();
            }
            else { app.Show(); app.BringToFront(); }
        }

        private void MachinePanel_Load(object sender, EventArgs e)
        {
            BackColor = BackgroundColor;
            Machines = DeSerializeMachines(Machines);
            // Console.WriteLine("Machines: " + Machines.Length);
            buildMachineGrid();
            //BuildMachinePanel();
            //WriteModbusMetrics();

            ModbusTimer.Interval = Properties.Settings.Default.ReadingFrequency;
            SamplingTimer.Interval = 5000;

            Label wtrmrk = new Label()
            {
                Name = "wtrmrk",
                Text = "Demo Version",
                Font = new Font(FontFamily.GenericSerif, 84, FontStyle.Bold),
                ForeColor = Color.FromArgb(150, 255, 255, 255),
                AutoSize = true,
                Location = new Point(Width / 2, Height / 2)
            };
        }

        public Machine[] DeSerializeMachines(Machine[] machines, string tag)
        {
            Console.WriteLine("Update machine: " + tag);
            if (machines == null || string.IsNullOrEmpty(Properties.Settings.Default.BackupFolder))
            {
                machines = new Machine[Properties.Settings.Default.MachineNumber];
                //return machines;
            }
            if (!Int32.TryParse(tag, out int r))
            {
                machines = new Machine[Properties.Settings.Default.MachineNumber];
            }

            int tagNumber = Int32.Parse(tag);

            //Machine[] newMachines = new Machine[machines.Count()];

            try
            {

                // Double check format?
                // Add Version
                System.Xml.Serialization.XmlSerializer reader =
            new System.Xml.Serialization.XmlSerializer(typeof(Machine));
                StreamReader file = new StreamReader(
                    Properties.Settings.Default.BackupFolder + "//m" + tag + ".xml");
                Machine newMachine = (Machine)reader.Deserialize(file);
                //newMachines[tagNumber] = newMachine;
                machines[tagNumber] = newMachine;
                file.Close();
                // Console.WriteLine(machine.Name);
            }
            catch (FileNotFoundException)
            {
                //newMachines[i] = new Machine();
                //Console.WriteLine("Default Machine: " + newMachines[i].Name);
                // Machine is not backed up.
                // Return default value
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Backup folder is empty");
            }
            catch (InvalidOperationException ie)
            {
                Console.WriteLine("Invalid operation: " + ie.Message);
                foreach (var d in ie.Data)
                {
                    Console.WriteLine("Data: " + d);
                }
            }


            return machines;

        }

        public Machine[] DeSerializeMachines(Machine[] machines)
        {
            if (machines == null || string.IsNullOrEmpty(Properties.Settings.Default.BackupFolder))
            {
                machines = new Machine[Properties.Settings.Default.MachineNumber];
                //return machines;
            }
            Console.WriteLine("Deserialize machines: " + machines.Length);

            // Now we can read the serialized book ...  

            int i = 0;
            Machine[] newMachines = new Machine[machines.Count()];
            foreach (Machine machine in machines)
            {
                try
                {

                    // Double check format?
                    // Add Version
                    System.Xml.Serialization.XmlSerializer reader =
                new System.Xml.Serialization.XmlSerializer(typeof(Machine));
                    StreamReader file = new StreamReader(
                        Properties.Settings.Default.BackupFolder + "//m" + i.ToString() + ".xml");
                    Machine newMachine = (Machine)reader.Deserialize(file);
                    newMachines[i] = newMachine;
                    file.Close();
                    // Console.WriteLine(machine.Name);
                }
                catch (FileNotFoundException)
                {
                    newMachines[i] = new Machine();
                    Console.WriteLine("Default Machine: " + newMachines[i].Name);
                    // Machine is not backed up.
                    // Return default value
                }
                catch (ArgumentException)
                {
                    Console.WriteLine("Backup folder is empty");
                }
                catch (InvalidOperationException ie)
                {
                    Console.WriteLine("Invalid operation: " + ie.Message);
                    foreach (var d in ie.Data)
                    {
                        Console.WriteLine("Data: " + d);
                    }
                }

                i++;
            }
            return newMachines;


        }

        private void buildMachineGrid()
        {
            // Create a grid of all machines
            // Each machine is assigned to a GroupBox called "machinePanel+i"
            // Each machine contains:
            // * A panel called "machineColor+i" whose BackColor will change according to the limits
            // * A label called "machineData+i" contains the holdRegister data
            // * The name of the Machine 
            //navPanel.Height = (int)(Height * .05f);

            MachineGrid.Width = (int)(Width * .99f);
            MachineGrid.Height = (int)(Height * .9f);

            // MachineGrid.Location = new Point(0, navPanel.Height);

            //machinePanel1.Height = MachineGrid.Height - mName.Height;

            //MachineGrid.RowStyles = 

            int totalPadding = 10;


            double panelProportion = 0.8;
            int relativeLocationX = 0;
            int relativeLocationY = 0;
            string font = "Arial";
            float fontSize = 18f;


            int machineNumber = Properties.Settings.Default.MachineNumber;

            // Console.WriteLine("Machine names" + machineNumber + " - " + Machines.Length);

            for (var i = 0; i < machineNumber; i++)
            {



                HermesGroupBox machinePanel = new HermesGroupBox()
                {
                    //Width = (this.Width / 3) - (2 * totalPadding),
                    //Height = (this.Height / 3) - (2 * totalPadding),

                    Font = new Font(font, 18f, FontStyle.Regular),
                    Anchor = (AnchorStyles.Left| AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom),

                    //Anchor = (AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Top),
                    GroupBorderColor = BorderColor,

                    GroupPanelColor = MachinePanelColor,
                    GroupPanelShape = HermesGroupBox.PanelType.Rounded,
                    // Text = "Hello",
                    //TextBackColor = TitleColor,
                    // TextBorderColor = TitleBorderColor,
                    // ForeColor = TextColor,
                    Tag = i.ToString(),

                };
                // int totalHeight = MachineGrid.GetRowHeights()[0];
                // int totalWidth = MachineGrid.GetColumnWidths()[0];

                int xUnit = (int)(MachineGrid.GetColumnWidths()[0] / 20);
                int yUnit = (int)(MachineGrid.GetRowHeights()[0] / 12);




                HermesLabel lblName = new HermesLabel()
                {
                    Text = Machines[i].Name,
                    CornerRadius = 15,
                    Anchor = (AnchorStyles.Left | AnchorStyles.Right),
                    Name = "lblMachineName" + i.ToString(),
                    TextAlign = ContentAlignment.MiddleCenter,
                    //Dock = DockStyle.Top,
                    AutoSize = false,
                    //Size = new Size(.GetColumnWidths()[0], yUnit*2),
                    //Location = new Point(0,0 ), //xUnit, yUnit
                    Font = new Font(font, fontSize, FontStyle.Bold),
                    ForeColor = TitleColor,
                    BackColor = MachinePanelColor,
                    BackgroundColor = LabelColor,

                };
                //Panel header = new Panel()
                //{
                //    Dock = DockStyle.Top,
                //    Height = yUnit,
                //    BackColor = Color.Blue,
                //};

                TableLayoutPanel HeaderPanel = new TableLayoutPanel()
                {
                    Height = yUnit * 2,
                    Width = xUnit*20,
                    Anchor = ( AnchorStyles.Top | AnchorStyles.Left),
                    //Size = new Size(machinePanel.Width, yUnit * 2),
                    Location = new Point(0, 0),
                    RowCount = 1,
                    ColumnCount = 2,
                    //BackColor = Color.White,
                };
                HeaderPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
                HeaderPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100));

                #region Metrics Panel
                TableLayoutPanel MetricsPanel = new TableLayoutPanel()
                {
                    Anchor = (AnchorStyles.Left |AnchorStyles.Top | AnchorStyles.Bottom),
                    Width = xUnit*20,
                    //Height = totalHeight - yUnit * 2,
                    Location = new Point(0, yUnit * 2),
                    ColumnCount = 2,
                    RowCount = 1,
                    //Width = totalWidth,
                    //BackColor = Color.Blue

                };
                MetricsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40));
                MetricsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60));

                #endregion


                //MetricsPanel.ColumnStyles.Add()
                //// In order to create rows, i mod 3 sets the horizontal position to 0
                //if (i % 3 == 0)
                //{
                //    relativeLocationX = totalPadding;
                //    if (i == 0) // First row is 0, implement this access
                //    { relativeLocationY = 0; }
                //    else
                //    { relativeLocationY = relativeLocationY + machinePanel.Height + totalPadding; }
                //}
                // Machine properties
                machinePanel.Name = "machinePanel" + i.ToString();
                //machinePanel.Location = new Point(relativeLocationX, relativeLocationY);
                //machinePanel.Click += MachinePanel_Click;

                //double width = machinePanel.Width * panelProportion;
                //double height = machinePanel.Height * panelProportion;

                //int xPadding = (int)(machinePanel.Width / 20);



                #region Availability
                Label lblAv = new Label()
                {
                    //Size = new System.Drawing.Size(100, 20),
                    Text = "Availability",
                    BackColor = Color.FromArgb(40, 250, 250, 250),
                    //AutoSize = true,
                    Anchor = (AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Top),
                    //Dock = DockStyle.Fill,
                    Margin = new Padding(0),

                    //Font = new Font(font, 14f, FontStyle.Regular),
                    TextAlign = ContentAlignment.MiddleLeft,
                    //Height = yUnit * 2
                };
                Label lblAvailability = new Label()
                {
                    //AutoSize = rue,
                    Font = new Font(font, fontSize, FontStyle.Bold),
                    Anchor = (AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Top),
                    BackColor = Color.FromArgb(40, 250, 250, 250),
                    Margin = new Padding(0),
                    Name = "mAvailability" + i.ToString(),
                    //Size = new Size(xUnit * 3, yUnit*2),
                    TextAlign = ContentAlignment.MiddleRight,
                    //Location = new Point(xUnit * 4, 0),
                    //Height = yUnit * 2
                }; // Availability

                //lblAv.Location = new Point(0, 0);
                #endregion

                #region Performance
                Label lblPerf = new Label()
                {
                    Text = "Performance",
                    Margin = new Padding(0),
                    Anchor = (AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Top),
                    //Dock = DockStyle.Fill,
                    AutoSize = true,

                    //Font = new Font(font, yUnit, FontStyle.Regular),
                    TextAlign = ContentAlignment.MiddleLeft,
                    //Height = yUnit * 2



                };
                Label lblPerformance = new Label()
                {
                    // AutoSize = true,
                    // Size = new Size(xUnit * 4, yUnit*2),
                    Font = new Font(font, fontSize, FontStyle.Bold),

                    Margin = new Padding(0),
                    Anchor = (AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Top),
                    //Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleRight,
                    //Font = new Font(font, fontSize, FontStyle.Bold),
                    Name = "mPerformance" + i.ToString(),
                    //Location = new Point(xUnit * 4, 0),
                    //Height = yUnit * 2
                };

                // lblPerf.Location = new Point(0, 0);
                #endregion

                #region OEE
                Label lblOEE = new Label()
                {
                    Text = "OEE",
                    Margin = new Padding(0),
                    Anchor = (AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Top),
                    //Dock = DockStyle.Fill,                    //AutoSize = true,  
                    //Font = new Font(font, 14f, FontStyle.Regular),
                    TextAlign = ContentAlignment.MiddleLeft,
                    //Height = yUnit * 2,
                    BackColor = Color.FromArgb(40, 250, 250, 250),



                };
                Label labelOEE = new Label()
                {
                    //Size = new Size(xUnit * 4, yUnit*2),
                    TextAlign = ContentAlignment.MiddleRight,
                    Margin = new Padding(0),
                    Anchor = (AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Top),
                    //Dock = DockStyle.Fill,
                    //Height = yUnit * 2,
                    Font = new Font(font, fontSize, FontStyle.Bold),
                    BackColor = Color.FromArgb(40, 250, 250, 250),
                    //AutoSize = true,
                    // Font = new Font(font, fontSize, FontStyle.Bold),
                    Name = "mOEE" + i.ToString(),
                    //Location = new Point(xUnit * 4, 0)
                };
                //lblOEE.Location = new Point(0, 0);
                #endregion

                #region Ideal Cycle Time
                Label lblICycleTime = new Label()
                {
                    //Name = "IdealCycleTime" + i.ToString(),
                    //Location = new Point(0,0),
                    Anchor = (AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Top),
                    //Dock = DockStyle.Fill,
                    Text = "Ideal Cycle Time:",
                    Font = new Font(font, 14f, FontStyle.Regular),
                    TextAlign = ContentAlignment.MiddleLeft,
                    //Height = yUnit * 2

                    // AutoSize = true
                };

                Label lblCycleTime = new Label()
                {
                    Name = "IdealCycleTime" + i.ToString(),
                    Anchor = (AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Top),
                    Dock = DockStyle.Fill,
                    // Location = new Point(xUnit * 2, 0),
                    //AutoSize = true,
                    Font = new Font(font, fontSize, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleRight,
                    Text = String.Format("{0:0.00}", Machines[i].CycleTime),
                    //Height = yUnit * 2,

                };
                #endregion

                #region Right Panel
                TableLayoutPanel RightPanel = new TableLayoutPanel()
                {
                    //Height = yUnit * 8,
                    //Width = xUnit * 10,
                    Anchor = (AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top),
                    Height = yUnit*9,
                    //MaximumSize = new Size(xUnit * 10, yUnit * 8),
                    //Name = "MetricsPanel" + i.ToString(),
                    RowCount = 4,
                    ColumnCount = 2,
                    //Location = new Point(xUnit * 10, yUnit * 2),

                    Font = new Font(font, 14f, FontStyle.Regular),
                };

                //MetricsPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, yUnit * 1.8f));
                //MetricsPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                RightPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));

                RightPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100));

                RightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
                RightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
                RightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
                RightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25));

                #endregion

                #region Left Panel
                TableLayoutPanel LeftPanel = new TableLayoutPanel()
                {
                    //Height = yUnit*10,
                    //Width = xUnit*10,
                    Anchor = (AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top),
                    Height = xUnit*9,
                    RowCount = 3,
                    ColumnCount = 1,
                    //Location = new Point(0, yUnit * 2),


                };
                LeftPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                LeftPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                LeftPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

                #endregion



                TableLayoutPanel TargetPanel = new TableLayoutPanel()
                {
                    Anchor = AnchorStyles.Left,// | AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Top),
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 1,
                    Height = (int)(yUnit * 1.8),
                };
                TargetPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                TargetPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                //TargetPanel.ColumnStyles.Add(new RowStyle(SizeType.AutoSize));
                Label lblTarget = new Label()
                {
                    Name = "lblTarget" + i.ToString(),
                    AutoSize = true,
                    Margin = new Padding(0),
                    Anchor = (AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Top),
                    TextAlign = ContentAlignment.MiddleCenter,
                    //Location = new Point(xUnit, yUnit*2), //xUnit*6, yUnit*3)
                    Text = "Target: "
                };

                #region CurrentParts
                Label lblCurrentParts = new Label()
                {
                    Name = "lblCurrentParts" + i.ToString(),
                    Margin = new Padding(0),

                    //Size = new Size(xUnit * 1, (int)(yUnit * 1.8f)), //(xUnit * 3, (int)(yUnit * 1.8f)
                    Anchor = (AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Top),

                    // Font.Size = fontSize,
                    //Font = new Font(font, fontSize, FontStyle.Bold),
                    //AutoSize = true,
                    TextAlign = ContentAlignment.MiddleLeft,
                    //BackColor = Color.Blue,
                    // MaximumSize = new Size(xUnit * 4, (int)(yUnit * 2f)),
                    //Location = new Point((int)(xUnit * 4.5),(int) (yUnit*1.8f)),//(xUnit*6, yUnit*5
                    Text = "0"
                };


                CircularProgressBar.CircularProgressBar targetPBar = new CircularProgressBar.CircularProgressBar()
                {
                    Name = "mTargetBar" + i.ToString(),
                    //Width = xUnit * 10,
                    //Height = (int)(yUnit * 2.5f),
                    Anchor = (AnchorStyles.Bottom | AnchorStyles.Top),
                    Size = new Size(xUnit * 4, xUnit * 4), //(MachineGrid.GetRowHeights()[0] / 12);

                    //Location = new Point(xUnit,(int)(yUnit * 3.5f)), //(xUnit * 0, yUnit * 4)
                    Minimum = 0,
                    Maximum = Machines[i].ExpectedParts,
                    InnerWidth = -1,//0
                    InnerMargin = 0,
                    InnerColor = InnerProgressColor,
                    SuperscriptText = "",
                    SubscriptText = "",
                    SubscriptColor = Color.FromArgb(239, 11, 11),
                    ProgressWidth = 22, //20
                    ProgressColor = Color.FromArgb(33, 255, 2), //(204, 255, 0)
                    Font = new Font(font, fontSize, FontStyle.Bold),
                    Margin = new Padding(0),
                    //OuterWidth = 10,
                    OuterColor = Color.Gray,//FromArgb(204, 255, 0),//Gray
                    TextMargin = new Padding(1, 0, 0, 1),



                };

                #endregion
                //Console.WriteLine("lblCurrentParts" + i.ToString());

                TableLayoutPanel CycleTimePanel = new TableLayoutPanel()
                {
                    Anchor = (AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Top),
                    //Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 1,
                    //Height = (int)(yUnit*1.8f),
                };
                #region Current CycleTime
                Label lblCurrentCycleTime = new Label()
                {
                    Name = "mCycleTime" + i.ToString(),
                    //Font = new Font(font, fontSize, FontStyle.Bold),
                    Anchor = (AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Top),

                    //Location = new Point((int)(xUnit * 4.8f), yUnit*10), //(xUnit * 7, yUnit*10)
                    TextAlign = ContentAlignment.MiddleLeft,
                    Height = yUnit * 2,
                    Text = @"0"
                };
                Label lblcct = new Label()
                {
                    Location = new Point(xUnit * 0, (int)(yUnit * 10.7f)), //yUnit* 10
                    AutoSize = true,
                    Anchor = (AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Top),

                    //Font = new Font(font, 15f, FontStyle.Regular),//18f
                    TextAlign = ContentAlignment.MiddleLeft,
                    Text = "Cycle Time:",
                    Font = new Font(font, 16f, FontStyle.Bold),
                };
                #endregion


                lblAvailability.Show();
                lblPerformance.Show();
                labelOEE.Show();


                lblName.Show();

                lblTarget.Show();
                lblCurrentParts.Show();
                lblICycleTime.Show();
                lblCycleTime.Show();
                targetPBar.Show();

                TargetPanel.Controls.Add(lblTarget);
                TargetPanel.Controls.Add(lblCurrentParts);

                CycleTimePanel.Controls.Add(lblcct);
                CycleTimePanel.Controls.Add(lblCurrentCycleTime);

                HeaderPanel.Controls.Add(lblName, 1, 0);

                LeftPanel.Controls.Add(TargetPanel);
                LeftPanel.Controls.Add(targetPBar);
                LeftPanel.Controls.Add(CycleTimePanel);


                RightPanel.Controls.Add(lblAv, 0, 0);
                RightPanel.Controls.Add(lblAvailability, 1, 0);

                RightPanel.Controls.Add(lblPerf, 0, 1);
                RightPanel.Controls.Add(lblPerformance, 1, 1);

                RightPanel.Controls.Add(lblOEE, 0, 2);
                RightPanel.Controls.Add(labelOEE, 1, 2);


                RightPanel.Controls.Add(lblICycleTime, 0, 3);
                RightPanel.Controls.Add(lblCycleTime, 1, 3);



                machinePanel.Controls.Add(HeaderPanel);

                MetricsPanel.Controls.Add(LeftPanel);
                MetricsPanel.Controls.Add(RightPanel);


                machinePanel.Controls.Add(MetricsPanel);


                machinePanel.Show();
                MachineGrid.Controls.Add(machinePanel);



                //machinePanel
            }
        }

        private void ConnectToModbus()
        {
            int m = 0;
            foreach(Machine machine in Machines)
            {
                string ipAddress = machine.IPAddress;
                if(!machine.Offline)
                {
                    Console.WriteLine("Machine: " + machine.Name );

                    try
                    {
                        EasyModbus.ModbusClient modbus = new EasyModbus.ModbusClient(ipAddress, 502);

                        modbus.Connect();

                        if (modbus.Connected)
                        {
                            bool[] coils = modbus.ReadCoils(0, 1);
                            Console.WriteLine("Coil: " + coils[0]);
                            machine.CoilStatus = coils[0];
                            machine.CalculatePerformance();
                            machine.CalculateAvailability();
                            machine.OEE = machine.Availability * machine.Performance / 100;

                        } else { Console.WriteLine("Modbus not connected"); }

                        modbus.Disconnect();
                    }
                    catch (EasyModbus.Exceptions.ConnectionException ce)
                    {
                        machine.ModbusConnection = false;
                        Console.WriteLine("Modbus exception: " + ce.Message);
                        // throwError("Connection not available, please try again.");
                        return;
                    }
                    catch (System.Net.Sockets.SocketException)
                    {
                        machine.ModbusConnection = false;
                        Console.WriteLine("IP address exception:");
                        return;
                    }
                }
                

                m++;

            }
            

            
            
        }

        private void ChangeLiveData()
        {
            int machineNumber = Properties.Settings.Default.MachineNumber;

            for (int i = 0; i < machineNumber; i++)
            {
                if (!Machines[i].Offline)
                {
                    Control[] gbCurrentParts = this.Controls.Find("lblCurrentParts" + i.ToString(), true);
                    gbCurrentParts[0].Text = Machines[i].ExpectedParts.ToString();

                    Control[] pgbCurrentParts = Controls.Find("mTargetBar" + i.ToString(), true);
                    CircularProgressBar.CircularProgressBar pgb = (CircularProgressBar.CircularProgressBar)pgbCurrentParts[0];
                    if (Machines[i].CurrentParts <= pgb.Maximum)
                    {
                        pgb.Value = Machines[i].CurrentParts;
                    }
                    pgb.Text = Machines[i].CurrentParts.ToString();

                    // Control[] gbmOEE = this.Controls.Find("mOEE" + i.ToString(), true);
                    // gbmOEE[0].Text = Machines[i].OEE.ToString();

                    Control[] lblCycletime = this.Controls.Find("mCycleTime" + i.ToString(), true);
                    lblCycletime[0].Text = String.Format("{0:0.00}", Machines[i].CurrentCycleTime);

                    Control[] lblICycleTime = this.Controls.Find("IdealCycleTime" + i.ToString(), true);
                    if (Machines[i].CycleTime > 100)
                    {
                        lblICycleTime[0].Text = String.Format("{0:0}", Machines[i].CycleTime);

                    }
                    else
                    {
                        lblICycleTime[0].Text = String.Format("{0:0.00}", Machines[i].CycleTime);

                    }

                }
            }

        }

        private void ChangeMetricsData()
        {
            int machineNumber = Properties.Settings.Default.MachineNumber;
            for (int i = 0; i < machineNumber; i++)
            {
                if (Machines[i] == null) { return; }
                // Console.WriteLine("Performance Machine: " + Machines[i].Performance.ToString());

                Control[] mPanel = this.Controls.Find("machinePanel" + i.ToString(), true);
                Control[] mName = this.Controls.Find("lblMachineName" + i.ToString(), true);
                Control[] gbmAvailability = this.Controls.Find("mAvailability" + i.ToString(), true);
                Control[] gbmPerformance = this.Controls.Find("mPerformance" + i.ToString(), true);
                Control[] gbmOEE = this.Controls.Find("mOEE" + i.ToString(), true);

                gbmAvailability[0].Text = Machines[i].Availability.ToString();

                gbmPerformance[0].Text = Machines[i].Performance.ToString();

                if (Machines[i].OEE > 0) { gbmOEE[0].Text = Machines[i].OEE.ToString(); }
                else { gbmOEE[0].Text = " - "; }

                if (Machines[i].IsOnShift)
                {
                    mName[0].Text = Machines[i].Name;
                    // Change color according to OEE
                    int redLimit = Properties.Settings.Default.RedLimit;
                    int yellowLimit = Properties.Settings.Default.YellowLimit;
                    int greenLimit = Properties.Settings.Default.GreenLimit;

                    int machineOEE = Machines[i].OEE;

                    switch (Machines[i].CurrentIddleReason)
                    {
                        case Machine.IddleReason.Unexpected:
                            {
                                

                                //Control[] machineColor = this.Controls.Find("machinePanel" + i.ToString(), true);

                                //(MachineGroupBox)machineColor;

                                
                                Color backColor;
                                Color borderColor;

                                if (machineOEE >= greenLimit) // && maxLimit
                                {
                                    backColor = GreenMachine;
                                    borderColor = BorderGreenMachine;
                                }
                                else if (machineOEE >= yellowLimit)
                                {
                                    backColor = YellowMachine;
                                    borderColor = BorderYellowMachine;
                                }
                                else
                                {
                                    borderColor = BorderRedMachine;
                                    backColor = RedMachine;
                                }

                                // ((HermesGroupBox)mPanel[0]).GroupBorderColor = borderColor;
                                // ((HermesGroupBox)mPanel[0]).GroupPanelColor = backColor;
                                // ((HermesGroupBox)mPanel[0]).GroupPanelColor = IddleWorkerColor;

                                // Console.WriteLine("Not working hours, Show Offline machine");
                                // Control[] machineColor = this.Controls.Find("mPanel" + i.ToString(), true);
                                // ((Panel)machineColor[0]).BackColor = backColor;

                            }
                            break;
                        case Machine.IddleReason.Break:
                            break;
                        case Machine.IddleReason.ChangeOverTime:
                            break;
                        case Machine.IddleReason.Trials:
                            break;
                        case Machine.IddleReason.Spare:
                            break;
                        default:
                            break;
                    }

                    
                    //((MachineGroupBox)machineColor[0]).GroupPanelColor = backColor;
                }
                else
                {
                    mName[0].Text = Machines[i].Name + " - Offline";

                    ((HermesGroupBox)mPanel[0]).GroupPanelColor = OfflineColor;
                    // ((MachineGroupBox)mPanel[0]).N
                }




            }
        }

        private void ModbusTimer_Tick(object sender, EventArgs e)
        {
            // ReadModbusButtons();
            ConnectToModbus();
            ChangeLiveData();
            //WriteCSVFile();
        }


        private void SamplingTimer_Tick(object sender, EventArgs e)
        {
            ChangeMetricsData();

        }
    }
}
