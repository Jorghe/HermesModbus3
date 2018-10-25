using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Hermes_Modbus_3
{
    public partial class MainPage : Form
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            int machineNumber = Properties.Settings.Default.MachineNumber;
            if (machineNumber < 1 || machineNumber > 9)
            {
                ThrowError("Invalid data");
                return;
            }
            //if (string.IsNullOrEmpty(Properties.Settings.Default.IpAddress))
            //{
            //    ThrowError("IP Address is invalid");
            //    return;
            //}
            if (string.IsNullOrEmpty(Properties.Settings.Default.BackupFolder))
            {
                // Sanitize this string to avoid pointing in a wrong filepath
                ThrowError("Backup Folder must not be empty");
                return;
            }
            if (Properties.Settings.Default.BackupFrequency <= 0)
            {
                ThrowError("Backup Frequency missing");
                return;
            }
            if (Properties.Settings.Default.ReadingFrequency <= 0)
            {
                ThrowError("Reading Frequency missing");
                return;
            }
            //if (string.IsNullOrEmpty(Properties.Settings.Default.RedLimit) || string.IsNullOrEmpty(yellowLimit.Text) || string.IsNullOrEmpty(greenLimit.Text))
            //{
            //    throwError("You must set red, green and yellow limits");
            //    return;
            //}

            this.Hide();
            var app = Application.OpenForms["MachinePanel"];
            if (app == null)
            {
                MachinePanel panel = new MachinePanel();
                panel.Show();
            }
            else { app.Show(); app.BringToFront(); }
        }


        private void ThrowError(string message)
        {
            //var errorBox = ErroBox.Show(message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
            //ModbusTimer.Stop();
            //ModbusTimer.Dispose();
            if (!string.IsNullOrEmpty(message))
            {
                var errorBox = MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            //string result = ErrorBox.ShowBox(message, "Error in Connection");
            //if (result == null || result.Equals("1"))
            //{
            //    // WriteModbusMetrics();
            //    // ModbusTimer.Start();
            //    // MessageBox.Show("OK Button was Clicked");
            //}

            //if (result.Equals("2"))
            //{
            //    // MessageBox.Show("Cancel Button was Clicked");
            //}
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            var app = Application.OpenForms["Settings"];
            if (app == null)
            {
                Settings settings = new Settings();
                settings.Show();
            }
            else { app.Show(); app.BringToFront(); }
        }
    }
}
