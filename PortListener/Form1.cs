using System.IO.Ports;

namespace PortListener
{
    public partial class Form1 : Form, IDisposable
    {
        private Task _task;
        private CancellationTokenSource _cts;
        private LogManager _logManager;

        public Form1()
        {
            this.StartPosition = FormStartPosition.CenterScreen;

            InitializeComponent();
            InitializeComboBox();

            _logManager = new LogManager(textBox1);
        }

        ~Form1()
        {
            Dispose(false);
        }

        private async Task ListenPortsAsync(string port, CancellationToken cancellationToken)
        {
            Random rnd = new Random();

            if (string.IsNullOrWhiteSpace(port))
                throw new ArgumentNullException(nameof(port));

            using (SerialPort serialPort = StartListeningPort(port, 9600))
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    SendWeightData(serialPort, $"Data:{rnd.Next(1, 1000)}M {rnd.Next(1, 1000)}KG");
                    await Task.Delay(100);
                }

                StopListeningPort(serialPort);
            }
        }

        private SerialPort StartListeningPort(string portName, int baudRate)
        {
            if (portName == null)
                throw new ArgumentNullException(nameof(portName));

            SerialPort serialPort = new SerialPort(portName, baudRate);
            serialPort.Open();

            // Event handler for receiving data
            serialPort.DataReceived += SerialPortDataReceived;

            _logManager.Log("Listening to the port. Press any key to exit.");

            return serialPort;
        }

        private void StopListeningPort(SerialPort serialPort)
        {
            if (serialPort.IsOpen)
            {
                serialPort.DataReceived -= SerialPortDataReceived;
                serialPort.Close();
            }
        }

        private void InitializeComboBox()
        {
            comboBox1.Items.AddRange(SerialPort.GetPortNames());
            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;
        }

        private void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var serialPort = (SerialPort)sender;

            // Read the incoming data
            string data = serialPort.ReadExisting().Trim();

            // Display the received weight input
            _logManager.Log($"Weight: {data}");
        }

        private void SendWeightData(SerialPort serialPort, string weight)
        {
            if (serialPort.IsOpen)
            {
                serialPort.WriteLine(weight);
                _logManager.Log($"Sent weight: {weight}");
            }
            else
            {
                _logManager.Log("The port is not open.");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string port = comboBox1.SelectedItem?.ToString();
            if (port != null)
            {
                _cts = new CancellationTokenSource();
                _task = Task.Factory.StartNew(async () => await ListenPortsAsync(port, _cts.Token));
                button1.Enabled = false;
                button2.Enabled = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _cts?.Cancel();

            Thread.Sleep(500);

            _logManager.Log("Stopped listening to the port.");

            button2.Enabled = false;
            button1.Enabled = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _logManager.ClearLog();
        }


    }
}