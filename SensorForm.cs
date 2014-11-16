using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace ArduinoSocketClient
{
    public partial class SensorForm : Form
    {
        SensorClient _sensorClient = null;

        public SensorForm()
        {
            InitializeComponent();
            _sensorClient = new SensorClient(this);
        }

        public delegate void UpdateTextCallback(string text);
        public void printOutput(String data) {
            data = data + ",";
            if (sensorOutput.InvokeRequired)
            {
                sensorOutput.Invoke(new UpdateTextCallback(this.printOutput), new object[] { data });
            }
            else
            {
                sensorOutput.AppendText(data + ",\n");
            }
        }

        public delegate void UpdateProgressCallback(string text);
        public void updateProgress(String data)
        {
            if (progressBar1.InvokeRequired)
            {
                progressBar1.Invoke(new UpdateProgressCallback(this.updateProgress), new object[] { data });
            }
            else
            {
                progressBar1.Value = Convert.ToInt32(data);
            }
        }

        private void initSensor()
        {
            _sensorClient.PollForData();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            Thread t = new Thread(initSensor);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }


    }
}
