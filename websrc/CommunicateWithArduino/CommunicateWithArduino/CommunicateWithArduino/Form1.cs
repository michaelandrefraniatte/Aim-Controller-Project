using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;

namespace CommunicateWithArduino
{
	public partial class Form1 : Form
	{
		public static System.IO.Ports.SerialPort port;
		delegate void SetTextCallback(string text);

		// This BackgroundWorker is used to demonstrate the 
		// preferred way of performing asynchronous operations.
		private BackgroundWorker hardWorker;

		private Thread readThread = null;

		public Form1()
		{
			InitializeComponent();

			hardWorker = new BackgroundWorker();
			sendBtn.Enabled = false;
		}

		private void btnConnect_Click(object sender, EventArgs e)
		{
			System.ComponentModel.IContainer components = 
				new System.ComponentModel.Container();
			port = new System.IO.Ports.SerialPort(components);
			port.PortName = comPort.SelectedItem.ToString();
			port.BaudRate = Int32.Parse(baudRate.SelectedItem.ToString());
			port.DtrEnable = true;
			port.ReadTimeout = 5000;
			port.WriteTimeout = 500;
			port.Open();

			readThread = new Thread(new ThreadStart(this.Read));
			readThread.Start();
			this.hardWorker.RunWorkerAsync();

			btnConnect.Text = "<Connected>";

			btnConnect.Enabled = false;
			comPort.Enabled = false;
			sendBtn.Enabled = true;
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			foreach (string s in SerialPort.GetPortNames())
			{
				comPort.Items.Add(s);
			}
			if (comPort.Items.Count > 0)
				comPort.SelectedIndex = comPort.Items.Count-1;
			else
				comPort.SelectedIndex = 0;

			baudRate.Items.Add("2400");
			baudRate.Items.Add("4800");
			baudRate.Items.Add("9600");
			baudRate.Items.Add("14400");
			baudRate.Items.Add("19200");
			baudRate.Items.Add("28800");
			baudRate.Items.Add("38400");
			baudRate.Items.Add("57600");
			baudRate.Items.Add("115200");

			baudRate.SelectedIndex = 2;
		}

		private void sendBtn_Click(object sender, EventArgs e)
		{
			if (port.IsOpen)
			{
				port.Write(sendText.Text);
			}
		}

		private void SetText(string text)
		{
			// InvokeRequired required compares the thread ID of the
			// calling thread to the thread ID of the creating thread.
			// If these threads are different, it returns true.
			if (this.receiveText.InvokeRequired)
			{
				SetTextCallback d = new SetTextCallback(SetText);
				this.Invoke(d, new object[] { text });
			}
			else
			{
                this.receiveText.Text += "Text: ";
                this.receiveText.Text += text;
                this.receiveText.Text += Environment.NewLine;
            }
		}

		public void Read()
		{
			while (port.IsOpen)
			{
				try
				{
					if (port.BytesToRead > 0)
					{
						string message = port.ReadLine();
						this.SetText(message);
					}
				}
				catch (TimeoutException) { }
			}
		}

		private void Form1_FormClosed(object sender, FormClosedEventArgs e)
		{
			try
			{
				if(!(readThread == null))
					readThread.Abort();
			}
			catch (NullReferenceException)
			{
			}

			try
			{
				port.Close();
			}
			catch (NullReferenceException)
			{
			}
		}
	}
}
