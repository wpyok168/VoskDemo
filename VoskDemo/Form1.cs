using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using Vosk;
using static System.Net.Mime.MediaTypeNames;

namespace VoskDemo
{
    public partial class Form1 : Form
    {
        //本实例只能在64位平台 any cpu 不行
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            VoskTest();
        }
        
        //WasapiCapture wasapi = new WasapiCapture();//麦克风
        //WaveInEvent wasapi = new WaveInEvent();//麦克风
        
        WasapiLoopbackCapture wasapi = new WasapiLoopbackCapture();  //扬声器

        private void VoskTest()
        {
            //==
            IEnumerable<MMDevice> speakDevices = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).ToArray(); //获取所有扬声器设备
            //IEnumerable<MMDevice> speakDevices = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active).ToArray(); //获取所有扬声器、麦克风设备
            MMDevice device = speakDevices.ToList()[0];
            //==
            //MMDevice device = WasapiLoopbackCapture.GetDefaultLoopbackCaptureDevice();
            //WasapiLoopbackCapture wasapi1 = new WasapiLoopbackCapture(device);

            Task.Run(() => {
                
                wasapi.WaveFormat = new NAudio.Wave.WaveFormat(48000, 16, 1);
                //Vosk.Vosk.SetLogLevel(0);
                Vosk.Model model = new Vosk.Model("models");
                VoskRecognizer rec = new VoskRecognizer(model, wasapi.WaveFormat.SampleRate);
                rec.SetMaxAlternatives(0);
                rec.SetWords(false);
                wasapi.DataAvailable += (_, a) =>
                {
                    if (rec.AcceptWaveform(a.Buffer, a.BytesRecorded))
                    {
                        if (this.textBox1.InvokeRequired)
                        {
                            this.textBox1.Invoke(new Action(() =>
                            {
                                this.textBox1.AppendText(rec.FinalResult());
                                
                            }));

                        }
                        
                    }
                    else if(this.checkBox1.Checked)
                    {
                        if (this.textBox1.InvokeRequired)
                        {
                            this.textBox1.Invoke(new Action(() =>
                            {
                                this.textBox1.AppendText(rec.PartialResult());
                            }));

                        }
                    }
                };
               
            });
            
        }
        private delegate void ShowTextBoxtext(string txt);
        public void showtxt(string txt) 
        {
            this.textBox1.AppendText(txt);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            wasapi.StartRecording();
        }
    }
}
