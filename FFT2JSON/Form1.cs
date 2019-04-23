using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Y_FFTLib;

namespace FFT2JSON
{
    public partial class Form1 : Form
    {
		//bool _isOpenFile = false;

		AudioFFTOperator fftOperator;

        private string _savePath;

        private int FFTLength = 64; //Magic number for performing FFT.
        private int _FFTChannel = 0;

        private int _FFTFPS = 60;

        AutoResetEvent saveThreadEvent = new AutoResetEvent(false);

        public Form1()
        {
            InitializeComponent();
        }

        private void BtnOpen_Click(object sender, EventArgs e)
        {
            string filename;
            using (OpenFileDialog openDialog = new OpenFileDialog())
            {
                openDialog.Filter = "*.mp3|*.mp3";
                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    filename = openDialog.FileName;
					//_isOpenFile = true;

					fftOperator = new AudioFFTOperator(filename, _FFTFPS, FFTLength, _FFTChannel);

                    //Change window title
                    Text = "FFT2JSON - " + Path.GetFileNameWithoutExtension(filename);

                    GC.Collect();

					//Get FFT Data
					fftOperator.GetSpectrum();

					//Save the Data
					using (SaveFileDialog save = new SaveFileDialog())
                    {
                        save.Filter = "*.json|*.json";
                        save.FileName = "Unititled.json";
                        if (save.ShowDialog() == DialogResult.OK)
                        {
                            _savePath = save.FileName;
                            Thread workThread = new Thread(new ThreadStart(SaveThread));
                            Thread monitorThread = new Thread(new ThreadStart(MonitorThread));
                            workThread.Name = "Save";
                            monitorThread.Name = "monitor";
                            workThread.Start();
                            monitorThread.Start();
                        }
                    }
                }
            }
        }

        private void SaveThread()
        {
            DataSaver.SaveDataAtPath(fftOperator.resultList, Path.GetDirectoryName(_savePath), Path.GetFileName(_savePath));

            saveThreadEvent.Set();
        }

        private void MonitorThread()
        {
            saveThreadEvent.WaitOne();
			GC.Collect();
            MessageBox.Show("Finished");
        }
    }
}
