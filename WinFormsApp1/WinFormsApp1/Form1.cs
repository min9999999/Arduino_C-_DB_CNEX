using System;
using System.IO.Ports;
using System.Windows.Forms;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        public class Data
        {
            public string Category;
            public int Value;
            public string Timestamp;
        }

        private const string BasePath = "https://arduinoconnectex-default-rtdb.firebaseio.com/"; //������ Firebase Database URL�� �Է�
        private const string FirebaseSecret = "E09Dva4dBklDbRCYPQi9TGPkvCU8Ut0vn9Iji737"; //������ Firebase Database ��й�ȣ�� �Է�
        private static FirebaseClient _client;

        SerialPort port = new SerialPort();

        public Form1()
        {
            InitializeComponent();
            _client = new FirebaseClient(BasePath, new FirebaseOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult(FirebaseSecret)
            });
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            if (!port.IsOpen) return;

            // �Ƶ��̳뿡 "1" ����
            port.Write("1");

            // ���� ��¥�� �ð��� ������
            string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // Firebase�� ������ ����
            var data = new Data
            {
                Category = "ON",
                Value = 1,
                Timestamp = currentDateTime
            };

            // ������ ����
            await _client
                 .Child("Arduino/Log") // �����Ͱ� ����� ��θ� ����
                 .PostAsync(data); // Firebase�� ������ ����
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            if (!port.IsOpen) return;

            // �Ƶ��̳뿡 "0" ����
            port.Write("0");

            // ���� ��¥�� �ð��� ������
            string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // Firebase�� ������ ����
           var data = new Data
           {
               Category = "OFF",
               Value = 0,
               Timestamp = currentDateTime
           };

            await _client
                 .Child("Arduino/Log") // �����Ͱ� ����� ��θ� ����
                 .PostAsync(data); // Firebase�� ������ ����
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text == "") return;
            try
            {
                if (port.IsOpen)
                {
                    port.Close();
                }
                else
                {
                    port.PortName = comboBox1.SelectedItem.ToString(); // "Com" ����;
                    port.BaudRate = 9600;
                    port.DataBits = 8;
                    port.StopBits = StopBits.One;                      // "StopBits": ������Ʈ�� ���� ��Ÿ���� ������
                    port.Parity = Parity.None;                         // "Parity Bit": ������ ���� �� ������ �����ϴµ� Ȱ��
                    port.Open();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "�˸�", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            button1.Text = port.IsOpen ? "Disconnect" : "Connect";
            comboBox1.Enabled = !port.IsOpen;
        }

        private void comboBox1_Click(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            foreach (var item in SerialPort.GetPortNames())
            {
                comboBox1.Items.Add(item);
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            try
            {
                // Firebase���� ������ ��������
                var data = await _client
                    .Child("Arduino/Log") // �����Ͱ� ����� ��θ� ����
                    .OnceAsync<DataModel>(); // DataModel�� Firebase���� ������ �������� ����

                // ����ȭ�� ��� ����
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                // ���� ��¥�� �ð��� �����Ͽ� ���ϸ� ����
                string fileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_data.csv";
                string filePath = Path.Combine(desktopPath, fileName);

                // CSV ���Ϸ� ����
                using (var writer = new StreamWriter(filePath))
                {
                    // CSV ��� �ۼ�
                    await writer.WriteLineAsync("Timestamp,Category,Value,");

                    // ������ �ۼ�
                    foreach (var item in data)
                    {
                        var Category = item.Object.Category;
                        var value = item.Object.Value;
                        var timestamp = item.Object.Timestamp;
                        await writer.WriteLineAsync($"{timestamp},{Category},{value}");
                    }
                }

                MessageBox.Show("CSV ������ ���������� ����Ǿ����ϴ�.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"���� �߻�: {ex.Message}");
            }
        }
        public class DataModel
        {
            public string Timestamp { get; set; }
            public string Category { get; set; }
            public int Value { get; set; }
        }
    }
}



       