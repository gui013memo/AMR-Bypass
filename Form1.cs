using System;
using System.Drawing;
using System.IO;
using System.Xml;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using Modbus.Device; // For ModbusSlave
using Modbus.Data;   // For DataStore and DataStoreFactory
using myLogger;

namespace AMR_Bypass
{
    public partial class Form1 : Form
    {
        // Add a lock object for thread safety
        private static readonly object coilsLock = new object();

        // Config file paths
        private static string configFilePath = @"C:\SW\SQS Client\ACA\App\ProuductSignInUI.exe.config";
        private static string backupConfigFilePath = @"C:\SW\SQS Client\ACA\App\ProuductSignInUI.exe.config.bak";

        // ProductSignIn application path
        private static string productSignInAppPath = @"C:\SW\SQS Client\ACA\App\ProuductSignInUI.exe";

        // Original Controller_IP value
        private static string originalControllerIP = "192.168.1.45";
        private static string ambControllerIP = "192.168.1.46";

        // Initialize coil statuses (8 bits)
        private bool[] coils = new bool[8]; // Bits 0 to 7, initialized to false

        private Thread modbusThread;
        private Thread dataUpdaterThread;
        private TcpListener tcpListener;

        Logger logger;

        public Form1()
        {
            InitializeComponent();
            this.Load += new EventHandler(Form1_Load);
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);

            logger = new Logger();

            // Handle unexpected closures
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            //AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        private void Form1_Load(object sender, EventArgs e)
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

            // Update UI labels
            UpdateLeftStatus();
            UpdateRightStatus();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Restore original config and restart ProductSignIn
            RestoreOriginalConfig();
            RestartProductSignIn();

            // Stop the Modbus TCP Server
            if (tcpListener != null)
            {
                tcpListener.Stop();
            }
        }

        private void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            RestoreOriginalConfig();
            RestartProductSignIn();
            logger.Log("Application_ThreadException happened");
        }

        //private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        //{
        //    RestoreOriginalConfig();
        //    RestartProductSignIn();
        //    logger.Log("CurrentDomain_UnhandledException happened");
        //}

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
                    logger.Log("Modified ProductSignIn config to use AMB Controller_IP.");
                }
                else
                {
                    logger.Log("Controller_IP setting not found in config file.");
                }
            }
            catch (Exception ex)
            {
                logger.Log($"Error modifying ProductSignIn config: {ex.Message}");
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
                    logger.Log("Restored original ProductSignIn config.");
                }
            }
            catch (Exception ex)
            {
                logger.Log($"Error restoring ProductSignIn config: {ex.Message}");
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
                    logger.Log("Killed existing ProductSignIn process.");
                }

                // Start the ProductSignIn application
                Process.Start(productSignInAppPath);
                logger.Log("Started ProductSignIn application.");
            }
            catch (Exception ex)
            {
                logger.Log($"Error restarting ProductSignIn: {ex.Message}");
            }
        }

        private void StartModbusTcpServer(bool[] coils)
        {
            try
            {
                // Set the server IP address
                IPAddress serverIPAddress = IPAddress.Parse("192.168.1.46");

                // Create a TCP listener on IP 192.168.1.46 and port 502
                tcpListener = new TcpListener(serverIPAddress, 502);
                tcpListener.Start();

                byte slaveId = 1;

                // Create Modbus slave
                ModbusSlave slave = ModbusTcpSlave.CreateTcp(slaveId, tcpListener);

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
                logger.Log($"Modbus TCP Server exception: {ex.Message}");
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

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void UpdateLeftStatus()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(UpdateLeftStatus));
            }
            else
            {
                lblLeftStatus.Text = coils[0] ? "ON" : "OFF";
                lblLeftStatus.ForeColor = coils[0] ? Color.Green : Color.Red;
            }
        }

        private void UpdateRightStatus()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(UpdateRightStatus));
            }
            else
            {
                lblRightStatus.Text = coils[1] ? "ON" : "OFF";
                lblRightStatus.ForeColor = coils[1] ? Color.Green : Color.Red;
            }
        }
    }
}
