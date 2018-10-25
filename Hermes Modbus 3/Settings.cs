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

namespace Hermes_Modbus_3
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        Machine[] Machines;

        private void btnSave_Click(object sender, EventArgs e)
        {
            TextBox[] txtNames = { machineName1, machineName2, machineName3, machineName4, machineName5, machineName6, machineName7, machineName8, machineName9 };
            DateTimePicker[] txtSStart = { shiftStart1, shiftStart2, shiftStart3, shiftStart4, shiftStart5, shiftStart6, shiftStart7, shiftStart8, shiftStart9 };
            DateTimePicker[] txtSEnd = { shiftEnd1, shiftEnd2, shiftEnd3, shiftEnd4, shiftEnd5, shiftEnd6, shiftEnd7, shiftEnd8, shiftEnd9 };
            TextBox[] txtDTarget = { dayTarget1, dayTarget2, dayTarget3, dayTarget4, dayTarget5, dayTarget6, dayTarget7, dayTarget8, dayTarget9 };
            TextBox[] txtPDowntime = { plannedDowntime1, plannedDowntime2, plannedDowntime3, plannedDowntime4, plannedDowntime5, plannedDowntime6, plannedDowntime7, plannedDowntime8, plannedDowntime9 };
            TextBox[] txtIPAddresses = { machineIP1, machineIP2, machineIP3, machineIP4, machineIP5, machineIP6, machineIP7, machineIP8, machineIP9 };

            //int machineInteger = (int) machineNumeric.Value;
            //Int32.TryParse(machineNumber.Text, out int machineInteger);

            int machineNumber = (int)machineNumeric.Value;

            if (machineNumber < 1 || machineNumber > 9)
            {
                throwError("Invalid data");
                return;
            }

            for (int m = 0; m < machineNumber; m++)
            {
                if (txtSStart[m].Value == null)
                {
                    throwError("Invalid Date in Shift Start: " + m.ToString());
                    return;
                }
                if (txtSEnd[m].Value == null)
                {
                    throwError("Invalid Date in Shift End " + m.ToString());
                    return;
                }
                if (txtSStart[m].Value >= txtSEnd[m].Value)
                {
                    throwError("Start Date must be before End Date " + m.ToString());
                    return;
                }
                string[] splitValues = txtIPAddresses[m].Text.Split('.');

                if (string.IsNullOrEmpty(txtIPAddresses[m].Text))
                {
                    throwError("IP Address is invalid");
                    return;
                }
                if (!splitValues.All(r => byte.TryParse(r, out byte tempForParsing)))
                {
                    throwError("IP Address is invalid: 1");
                    return;
                }
                // Console.WriteLine(ts.ToString());
                //txtSStart[m].Text = TimeSpan.Parse(txtSStart[m].Text).ToString("h:mm");
                //string[] split = txtSStart[m].Text.Split(':');

                //txtSStart[m].Text)
            }


            //string[] splitValues = ipAddress.Text.Split('.');
            //if (string.IsNullOrEmpty(ipAddress.Text) || splitValues.Length != 4)
            //{
            //    throwError("IP Address is invalid");
            //    return;
            //}


            if (string.IsNullOrEmpty(backupFolder.Text))
            {
                // Sanitize this string to avoid pointing in a wrong filepath
                throwError("Backup Folder must not be empty");
                return;
            }
            if (string.IsNullOrEmpty(backupFreq.Text))
            {
                throwError("Backup Frequency missing");
                return;
            }
            if (string.IsNullOrEmpty(readingFreq.Text))
            {
                throwError("Reading Frequency missing");
                return;
            }
            if (string.IsNullOrEmpty(redLimit.Text) || string.IsNullOrEmpty(yellowLimit.Text) || string.IsNullOrEmpty(greenLimit.Text))
            {
                throwError("You must set red, green and yellow limits");
                return;
            }
            //if (string.IsNullOrEmpty(iddleWorker.Text) || string.IsNullOrEmpty(SamplingMachinePanel.Text))
            //{
            //    throwError("Debug is necessary");
            //    return;
            //}

            if (Int32.Parse(redLimit.Text) >= Int32.Parse(yellowLimit.Text) ||
                Int32.Parse(yellowLimit.Text) >= Int32.Parse(greenLimit.Text) ||
                Int32.Parse(greenLimit.Text) > 100 ||
                Int32.Parse(redLimit.Text) < 0)
            {
                throwError("Limits are wrong");
                return;
            }

            Properties.Settings.Default.ReadingFrequency = Int32.Parse(readingFreq.Text);
            Properties.Settings.Default.BackupFolder = backupFolder.Text;
            Properties.Settings.Default.BackupFrequency = Int32.Parse(backupFreq.Text);
            //Properties.Settings.Default.IpAddress = ipAddress.Text;
            Properties.Settings.Default.RedLimit = Int32.Parse(redLimit.Text);
            Properties.Settings.Default.YellowLimit = Int32.Parse(yellowLimit.Text);
            Properties.Settings.Default.GreenLimit = Int32.Parse(greenLimit.Text);
            Properties.Settings.Default.MachineNumber = machineNumber;

            //Properties.Settings.Default.IddleWorker = Int32.Parse(iddleWorker.Text);
            //Properties.Settings.Default.SamplingMachines = Int32.Parse(SamplingMachinePanel.Text);

            Properties.Settings.Default.Save();

            //Properties.Settings.Default.MachineNames = new System.Collections.Specialized.StringCollection();
            //System.Collections.Specialized.StringCollection names = new System.Collections.Specialized.StringCollection();
            //System.Collections.Specialized.StringCollection shiftS = new System.Collections.Specialized.StringCollection();
            //System.Collections.Specialized.StringCollection shiftE = new System.Collections.Specialized.StringCollection();
            //int[] cycleT = new int[machineNumber];
            //int[] plannedDt = new int[machineNumber];
            // Set machine properties to Machines[] object

            // Console.WriteLine("Machine number: " + machineNumber.ToString());
            Machines = new Machine[machineNumber];

            for (int m = 0; m < machineNumber; m++)
            {
                // Default values
                if (string.IsNullOrEmpty(txtNames[m].Text))
                {
                    txtNames[m].Text = "Machine " + m.ToString();

                }
                if (string.IsNullOrEmpty(txtDTarget[m].Text))
                {
                    txtDTarget[m].Text = "0";
                }
                if (string.IsNullOrEmpty(txtPDowntime[m].Text))
                {
                    txtPDowntime[m].Text = "0";
                }

                Machines[m] = new Machine
                {
                    Availability = 0,
                    OEE = 0,
                    Performance = 0,

                    Name = txtNames[m].Text,
                    ExpectedParts = Int32.Parse(txtDTarget[m].Text),
                    ShiftStart = txtSStart[m].Value.ToString("H:mm"),// TimeSpan.Parse( txtSStart[m].Text).ToString("h:mm"),
                    ShiftEnd = txtSEnd[m].Value.ToString("H:mm"),
                    PlannedDownTime = Int32.Parse(txtPDowntime[m].Text),
                    IPAddress = txtIPAddresses[m].Text
                };
                TimeSpan.TryParse(Machines[m].ShiftStart, out TimeSpan shiftSSpan);
                TimeSpan.TryParse(Machines[m].ShiftEnd, out TimeSpan shiftESpan);

                int shiftMinutes = (int)(shiftESpan - shiftSSpan).TotalMinutes;
                //Console.WriteLine(Machines[m].Name + " is " + shiftMinutes + " - " + Machines[m].PlannedDownTime);
                // Console.WriteLine("Machine Cycle Time" + Machines[m].ExpectedParts);

                // int availability = (int)((shiftMinutes - Machines[m].PlannedDownTime)*100/shiftMinutes);

                // Machines[m].Availability = availability;

                // Operating time is (PlannedProductionTime - UnplannedDowntime) / ExpectedParts

                int operatingTime = (shiftMinutes - Machines[m].PlannedDownTime) * 60;

                //Console.WriteLine("OP Time / ExpectedParts" + operatingTime + " / " + Machines[m].ExpectedParts);
                Machines[m].PlannedProductionTime = shiftMinutes - Machines[m].PlannedDownTime;
                // Machines[m].CycleTime = (float)operatingTime / (float)Machines[m].ExpectedParts;

                //Console.WriteLine(Machines[m].CycleTime);
                // Console.WriteLine();

                // Console.WriteLine("Cycle time: " + Machines[m].CycleTime);
                // Console.WriteLine("CT = {0} - {1} / {2}", shiftMinutes, Machines[m].PlannedDownTime, Machines[m].ExpectedParts);

            }

            SerializeMachines(Machines);

            Close();
        }

        public void SerializeMachines(Machine[] machines)
        {

            System.Xml.Serialization.XmlSerializer writer =
                new System.Xml.Serialization.XmlSerializer(typeof(Machine));
            //Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)

            int i = 0;
            foreach (Machine machine in machines)
            {
                try
                {
                    // Create directory if doesn't exist
                    Directory.CreateDirectory(Properties.Settings.Default.BackupFolder);

                    var path = Properties.Settings.Default.BackupFolder + "//m" + i.ToString() + ".xml";
                    FileStream file = System.IO.File.Create(path);

                    writer.Serialize(file, machine);
                    file.Close();
                }
                catch (System.IO.IOException ee)
                {

                    throwError("Exception: " + ee.Message);
                }

                i++;
            }

        }

        public Machine[] DeSerializeMachines(Machine[] machines)
        {
            // First write something so that there is something to read ...  
            //var b = new Book { title = "Serialization Overview" };
            //var writer = new System.Xml.Serialization.XmlSerializer(typeof(Machine));
            //var wfile = new System.IO.StreamWriter(@"c:\temp\SerializationOverview.xml");
            //writer.Serialize(wfile, machines[0]);
            //wfile.Close();
            if (machines == null || string.IsNullOrEmpty(Properties.Settings.Default.BackupFolder)) { return new Machine[1]; }
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
                    // Machine is not backed up.
                    // Return default value
                }
                catch (DirectoryNotFoundException de)
                {
                    Console.WriteLine("DE: " + de.Message);
                    Console.WriteLine("Machine:" + machine.Name);
                    Console.WriteLine("Settings: " + Properties.Settings.Default.BackupFolder);
                }

                i++;
            }
            return newMachines;


        }


        public void throwError(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                var errorBox = MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }

        }

        public void throwError(string message, string errorType)
        {
            // Errortype:
            // retry

            if (errorType == "retry")
            {
                var errorBox = MessageBox.Show(message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
            }
            // Retry connection

        }

        private void Settings_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.MachineNumber == 0)
            {
                Properties.Settings.Default.MachineNumber++;
            }
            machineNumeric.Value = Properties.Settings.Default.MachineNumber;

            readingFreq.Text = Properties.Settings.Default.ReadingFrequency.ToString(); ;
            backupFolder.Text = Properties.Settings.Default.BackupFolder;
            backupFreq.Text = Properties.Settings.Default.BackupFrequency.ToString();
            //ipAddress.Text = Properties.Settings.Default.IpAddress;
            redLimit.Text = Properties.Settings.Default.RedLimit.ToString();
            yellowLimit.Text = Properties.Settings.Default.YellowLimit.ToString();
            greenLimit.Text = Properties.Settings.Default.GreenLimit.ToString();

            //iddleWorker.Text = Properties.Settings.Default.IddleWorker.ToString();
            //SamplingMachinePanel.Text = Properties.Settings.Default.SamplingMachines.ToString();


            TextBox[] txtNames = { machineName1, machineName2, machineName3, machineName4, machineName5, machineName6, machineName7, machineName8, machineName9 };
            DateTimePicker[] txtSStart = { shiftStart1, shiftStart2, shiftStart3, shiftStart4, shiftStart5, shiftStart6, shiftStart7, shiftStart8, shiftStart9 };
            DateTimePicker[] txtSEnd = { shiftEnd1, shiftEnd2, shiftEnd3, shiftEnd4, shiftEnd5, shiftEnd6, shiftEnd7, shiftEnd8, shiftEnd9 };
            TextBox[] txtDTarget = { dayTarget1, dayTarget2, dayTarget3, dayTarget4, dayTarget5, dayTarget6, dayTarget7, dayTarget8, dayTarget9 };
            TextBox[] txtPDowntime = { plannedDowntime1, plannedDowntime2, plannedDowntime3, plannedDowntime4, plannedDowntime5, plannedDowntime6, plannedDowntime7, plannedDowntime8, plannedDowntime9 };
            TextBox[] txtIPAddresses = { machineIP1, machineIP2, machineIP3, machineIP4, machineIP5, machineIP6, machineIP7, machineIP8, machineIP9 };

            try
            {

                Machines = new Machine[Properties.Settings.Default.MachineNumber];
                //for (int m = 0; m < Machines.Length; m++)
                //{
                //    Machines[m] = new Machine
                //    {
                //        Performance = 0,
                //        Availability = 0,
                //        Capacity = 0,
                //        OEE = 0,
                //    };
                //}
                // Path only.
                Machines = DeSerializeMachines(Machines);
                int i = 0;
                //Console.WriteLine("Machines: " + Machines.Count());

                foreach (Machine machine in Machines)
                {
                    if (machine == null) { continue; }
                    txtNames[i].Text = machine.Name;
                    txtSStart[i].Value = DateTime.Parse(machine.ShiftStart);
                    txtSEnd[i].Value = DateTime.Parse(machine.ShiftEnd);

                    txtDTarget[i].Text = machine.ExpectedParts.ToString();
                    txtPDowntime[i].Text = machine.PlannedDownTime.ToString();
                    txtIPAddresses[i].Text = machine.IPAddress;
                    i++;
                }

                //int i = 0;
                //foreach (object n in Properties.Settings.Default.MachineNames)
                //{
                //    if (n != null)
                //    {
                //        txtNames[i].Text = n.ToString();
                //        Machines[i].Name = n.ToString();
                //    }
                //    i++;
                //}
                //i = 0;
                //foreach (object ss in Properties.Settings.Default.ShiftStarts)
                //{
                //    if (ss != null)
                //    {
                //        txtSStart[i].Text = ss.ToString();
                //        Machines[i].ShiftStart = ss.ToString();
                //    }
                //    i++;
                //}
                //i = 0;
                //foreach (object se in Properties.Settings.Default.ShiftEnds)
                //{
                //    if (se != null)
                //    {
                //        txtSEnd[i].Text = se.ToString();
                //        Machines[i].ShiftEnd = se.ToString();

                //    }
                //    i++;
                //}
                //i = 0;
                //foreach (object ct in Properties.Settings.Default.CycleTimes)
                //{
                //    if (ct != null)
                //    {
                //        txtCTime[i].Text = ct.ToString();
                //        Machines[i].CycleTime = Int32.Parse(ct.ToString());
                //    }
                //    i++;
                //}
                //i = 0;
                //foreach (object pd in Properties.Settings.Default.PlannedDowntimes)
                //{
                //    if (pd != null)
                //    {
                //        txtPDowntime[i].Text = pd.ToString();
                //        Machines[i].PlannedDownTime = Int32.Parse(pd.ToString());
                //    }
                //    i++;
                //}


            }
            catch (NullReferenceException ne)
            {
                Console.WriteLine("" + ne.Message);
                // 
                //throw;
            }
            catch (IndexOutOfRangeException ev)
            {
                Console.WriteLine("" + ev.Message);

            }
        }

        private void btnExamine_Click(object sender, EventArgs e)
        {
            // Prepare a dummy string, thos would appear in the dialog
            string dummyFileName = "Save Here";

            SaveFileDialog sf = new SaveFileDialog
            {
                FileName = dummyFileName
            };

            if (sf.ShowDialog() == DialogResult.OK)
            {
                backupFolder.Text = Path.GetDirectoryName(sf.FileName) + @"\";
            }
        }
    }
}
