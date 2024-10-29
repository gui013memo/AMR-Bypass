using System;
using System.IO;
using System.Xml;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using Modbus.Device; // For ModbusSlave
using Modbus.Data;  // For DataStore and DataStoreFactory

namespace AMR_Bypass
{
    public partial class Form1 : Form
    {
        // Add a lock object for thread safety
        private static readonly object coilsLock = new object();

        // Config file paths
        private string configFilePath = @"C:\SW\SQS Cilient\ACA\App\ProuductSignInUI.exe.config";
        private string backupConfigFilePath = @"C:\SW\SQS Cilient\ACA\App\ProuductSignInUI.exe.config.bak";

        // ProductSignIn application path
        private string productSignInAppPath = @"C:\SW\SQS Cilient\ACA\App\ProuductSignInUI.exe";

        // Original Controller_IP value
        private string originalControllerIP = "192.168.1.45";
        private string ambControllerIP = "192.168.1.46";

        // Coils array
        private bool[] coils = new bool[8]; // Bits 0 to 7, initialized to false

        // Modbus slave and threads
        private ModbusSlave slave;
        private Thread modbusThread;
        private Thread dataUpdaterThread;

        public Form1()
        {
            InitializeComponent();

            // Handle unexpected closures
            Application.ApplicationExit += new EventHandler(OnApplicationExit);
            this.FormClosing += new FormClosingEventHandler(OnFormClosing);

            // Initialize UI
            lblLeftStatus.Text = "OFF";
            lblLeftStatus.ForeColor = System.Drawing.Color.Red;
            lblRightStatus.Text = "OFF";
            lblRightStatus.ForeColor = System.Drawing.Color.Red;

            // Start initialization
            InitializeAMB();
        }

        private void InitializeAMB()
        {
            // Restore original config if backup exists (in case of previous unexpected closure)
            if (File.Exists(backupConfigFilePath))
            {
                RestoreOriginalConfig();
                RestartProductSignIn();
            }

            // Modify the config file
            ModifyProductSignInConfig();

            // Restart the ProductSignIn application
            RestartProductSignIn();

            // Start the Modbus TCP server in a separate thread
            modbusThread = new Thread(() => StartModbusTcpServer(coils));
            modbusThread.IsBackground = true;
            modbusThread.Start();

            LogStatus("AMB Modbus TCP Server is running on IP 192.168.1.46...");
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            CleanupAMB();
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            CleanupAMB();
        }

        private void CleanupAMB()
        {
            // Stop Modbus server and threads
            if (slave != null)
            {
                slave.Dispose();
            }

            if (modbusThread != null && modbusThread.IsAlive)
            {
                modbusThread.Abort();
            }

            if (dataUpdaterThread != null && dataUpdaterThread.IsAlive)
            {
                dataUpdaterThread.Abort();
            }

            // Restore original config and restart ProductSignIn
            RestoreOriginalConfig();
            RestartProductSignIn();
        }

        private void ModifyProductSignInConfig()
        {
            try
            {
                // Backup the original config file
                if (!File.Exists(backupConfigFilePath))
                {
                    File.Copy(configFilePath, backupConfigFilePath);
                }

                XmlDocument configDoc = new XmlDocument();
                configDoc.Load(configFilePath);

                XmlNode controllerIPNode = configDoc.SelectSingleNode("//setting[@name='Controller_IP']/value");
                if (controllerIPNode != null)
                {
                    controllerIPNode.InnerText = ambControllerIP;
                    configDoc.Save(configFilePath);
                    LogStatus("Modified ProductSignIn config to use AMB Controller_IP.");
                }
                else
                {
                    LogStatus("Controller_IP setting not found in config file.");
                }
            }
            catch (Exception ex)
            {
                LogStatus($"Error modifying ProductSignIn config: {ex.Message}");
            }
        }

        private void RestoreOriginalConfig()
        {
            try
            {
                if (File.Exists(backupConfigFilePath))
                {
                    File.Copy(backupConfigFilePath, configFilePath, true);
                    File.Delete(backupConfigFilePath);
                    LogStatus("Restored original ProductSignIn config.");
                }
            }
            catch (Exception ex)
            {
                LogStatus($"Error restoring ProductSignIn config: {ex.Message}");
            }
        }

        private void RestartProductSignIn()
        {
            try
            {
                // Kill existing ProductSignIn processes
                foreach (var process in Process.GetProcessesByName("ProuductSignInUI"))
                {
                    process.Kill();
                    process.WaitForExit();
                    LogStatus("Killed existing ProductSignIn process.");
                }

                // Start the ProductSignIn application
                Process.Start(productSignInAppPath);
                LogStatus("Started ProductSignIn application.");
            }
            catch (Exception ex)
            {
                LogStatus($"Error restarting ProductSignIn: {ex.Message}");
            }
        }

        private void StartModbusTcpServer(bool[] coils)
        {
            try
            {
                // Set the server IP address
                IPAddress serverIPAddress = IPAddress.Parse("192.168.1.46");

                // Create a TCP listener on IP 192.168.1.46 and port 502
                TcpListener tcpListener = new TcpListener(serverIPAddress, 502);
                tcpListener.Start();

                byte slaveId = 1;

                // Create Modbus slave
                slave = ModbusTcpSlave.CreateTcp(slaveId, tcpListener);

                // Create a default data store
                DataStore dataStore = DataStoreFactory.CreateDefaultDataStore();

                // Assign the data store to the slave
                slave.DataStore = dataStore;

                // Start a thread to update the coils in the data store
                dataUpdaterThread = new Thread(() => DataUpdater(dataStore, coils));
                dataUpdaterThread.IsBackground = true;
                dataUpdaterThread.Start();

                // Start listening for requests
                slave.Listen();
            }
            catch (Exception ex)
            {
                LogStatus($"Modbus TCP Server exception: {ex.Message}");
            }
        }

        private void DataUpdater(DataStore dataStore, bool[] coils)
        {
            while (true)
            {
                lock (coilsLock)
                {
                    // Update the coils in the data store
                    // Modbus coil addresses start at 1
                    for (ushort i = 0; i < coils.Length; i++)
                    {
                        dataStore.CoilDiscretes[i + 1] = coils[i];
                    }
                }

                Thread.Sleep(100); // Update every 100 ms
            }
        }

        private void btnLeft_Click(object sender, EventArgs e)
        {
            lock (coilsLock)
            {
                coils[0] = !coils[0];
                UpdateLeftStatus();
            }
        }

        private void btnRight_Click(object sender, EventArgs e)
        {
            lock (coilsLock)
            {
                coils[1] = !coils[1];
                UpdateRightStatus();
            }
        }

        private void UpdateLeftStatus()
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action(UpdateLeftStatus));
            }
            else
            {
                lblLeftStatus.Text = coils[0] ? "ON" : "OFF";
                lblLeftStatus.ForeColor = coils[0] ? System.Drawing.Color.Green : System.Drawing.Color.Red;

                btnLeft.ForeColor = coils[0] ? System.Drawing.Color.White : System.Drawing.Color.Black;
                btnLeft.BackColor = coils[0] ? System.Drawing.Color.Green : System.Drawing.Color.Red;

                LogStatus($"LEFT bit is now {(coils[0] ? "ON" : "OFF")}");
            }
        }

        private void UpdateRightStatus()
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action(UpdateRightStatus));
            }
            else
            {
                lblRightStatus.Text = coils[1] ? "ON" : "OFF";
                lblRightStatus.ForeColor = coils[1] ? System.Drawing.Color.Green : System.Drawing.Color.Red;

                btnRight.ForeColor = coils[0] ? System.Drawing.Color.White : System.Drawing.Color.Black;
                btnRight.BackColor = coils[0] ? System.Drawing.Color.Green : System.Drawing.Color.Red;

                LogStatus($"RIGHT bit is now {(coils[1] ? "ON" : "OFF")}");
            }
        }

        private void LogStatus(string message)
        {
            if (txtStatusLog.InvokeRequired)
            {
                txtStatusLog.Invoke(new Action<string>(LogStatus), message);
            }
            else
            {
                txtStatusLog.AppendText($"{DateTime.Now}: {message}{Environment.NewLine}");
                txtStatusLog.SelectionStart = txtStatusLog.Text.Length;
                txtStatusLog.ScrollToCaret();
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
