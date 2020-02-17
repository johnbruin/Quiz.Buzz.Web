using System;
using System.Threading;
using HidLibrary;

namespace Quizz.Buzz
{
    public class Buzzer
    {
        private readonly bool[] ActiveBuzzers = new bool[4];
        private HidDevice _device;
        private byte[] Lights;
        private readonly int _playerOffset;

        public delegate void AnswerHandler(object sender, AnswerClickEventArgs ace);

        public event AnswerHandler OnAnswerClick;
        public bool IsInitialized { get; private set; } = false;

        public Buzzer(HidDevice device, int playerOffset)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            _playerOffset = playerOffset;
            Init();
        }

        public void Flash(int BuzzerID)
        {
            var thread = new Thread(() =>
            {
                for (int i = 0; i < 5; i++)
                {
                    LightOn(BuzzerID);
                    LightOff(BuzzerID);
                }
            });
            thread.Start();
        }

        public void Active(int BuzzerID)
        {
            ActiveBuzzers[BuzzerID - 1] = true;
        }

        public void Inactive(int BuzzerID)
        {
            ActiveBuzzers[BuzzerID - 1] = false;
        }

        public bool IsActive(int BuzzerID)
        {
            return ActiveBuzzers[BuzzerID - 1];
        }

        public void LightOff(int BuzzerID)
        {
            Lights[BuzzerID] = 0;
            this.SendData(Lights);
        }

        public bool IsLightOn(int BuzzerID)
        {
            return (Lights[BuzzerID] == 255);
        }

        public void LightOn(int BuzzerID)
        {
            Lights[BuzzerID] = 255;
            this.SendData(Lights);
        }

        public void AllLightsOn()
        {
            Lights = new byte[9];
            for (int i = 0; i < Lights.Length - 1; i++)
            {
                Lights[i] = 255;
            }
            SendData(Lights);
        }

        public void AllLightsOff()
        {
            Lights = new byte[9];
            for (int i = 0; i < Lights.Length - 1; i++)
            {
                Lights[i] = 0;
            }
            SendData(Lights);
        }

        private void Init()
        {
            ActiveBuzzers[0] = true;
            ActiveBuzzers[1] = true;
            ActiveBuzzers[2] = true;
            ActiveBuzzers[3] = true;

            _device.MonitorDeviceEvents = true;
            _device.OpenDevice();
            _device.MonitorDeviceEvents = true;
            
            AllLightsOff();

            IsInitialized = true;

            var thread = new Thread(ReadReport);
            thread.Start();
        }

        private void OnReport(HidReport report)
        {
            var output = report.Data;

            //Buzzer 1
            if (IsBitSet(output[2], 5)) OnAnswerClick(this, new AnswerClickEventArgs(1 + _playerOffset, 1, AnswerColor.Blue, this));
            if (IsBitSet(output[2], 4)) OnAnswerClick(this, new AnswerClickEventArgs(1 + _playerOffset, 1, AnswerColor.Orange, this));
            if (IsBitSet(output[2], 3)) OnAnswerClick(this, new AnswerClickEventArgs(1 + _playerOffset, 1, AnswerColor.Green, this));
            if (IsBitSet(output[2], 2)) OnAnswerClick(this, new AnswerClickEventArgs(1 + _playerOffset, 1, AnswerColor.Yellow, this));
            if (IsBitSet(output[2], 1)) OnAnswerClick(this, new AnswerClickEventArgs(1 + _playerOffset, 1, AnswerColor.Red, this));

            //Buzzer 2
            if (IsBitSet(output[3], 2)) OnAnswerClick(this, new AnswerClickEventArgs(2 + _playerOffset, 2, AnswerColor.Blue, this));
            if (IsBitSet(output[3], 1)) OnAnswerClick(this, new AnswerClickEventArgs(2 + _playerOffset, 2, AnswerColor.Orange, this));
            if (IsBitSet(output[2], 8)) OnAnswerClick(this, new AnswerClickEventArgs(2 + _playerOffset, 2, AnswerColor.Green, this));
            if (IsBitSet(output[2], 7)) OnAnswerClick(this, new AnswerClickEventArgs(2 + _playerOffset, 2, AnswerColor.Yellow, this));
            if (IsBitSet(output[2], 6)) OnAnswerClick(this, new AnswerClickEventArgs(2 + _playerOffset, 2, AnswerColor.Red, this));

            //Buzzer 3
            if (IsBitSet(output[3], 7)) OnAnswerClick(this, new AnswerClickEventArgs(3 + _playerOffset, 3, AnswerColor.Blue, this));
            if (IsBitSet(output[3], 6)) OnAnswerClick(this, new AnswerClickEventArgs(3 + _playerOffset, 3, AnswerColor.Orange, this));
            if (IsBitSet(output[3], 5)) OnAnswerClick(this, new AnswerClickEventArgs(3 + _playerOffset, 3, AnswerColor.Green, this));
            if (IsBitSet(output[3], 4)) OnAnswerClick(this, new AnswerClickEventArgs(3 + _playerOffset, 3, AnswerColor.Yellow, this));
            if (IsBitSet(output[3], 3)) OnAnswerClick(this, new AnswerClickEventArgs(3 + _playerOffset, 3, AnswerColor.Red, this));

            //Buzzer 4
            if (IsBitSet(output[4], 4)) OnAnswerClick(this, new AnswerClickEventArgs(4 + _playerOffset, 4, AnswerColor.Blue, this));
            if (IsBitSet(output[4], 3)) OnAnswerClick(this, new AnswerClickEventArgs(4 + _playerOffset, 4, AnswerColor.Orange, this));
            if (IsBitSet(output[4], 2)) OnAnswerClick(this, new AnswerClickEventArgs(4 + _playerOffset, 4, AnswerColor.Green, this));
            if (IsBitSet(output[4], 1)) OnAnswerClick(this, new AnswerClickEventArgs(4 + _playerOffset, 4, AnswerColor.Yellow, this));
            if (IsBitSet(output[3], 8)) OnAnswerClick(this, new AnswerClickEventArgs(4 + _playerOffset, 4, AnswerColor.Red, this));

            var thread = new Thread(ReadReport);
            thread.Start();
        }

        private void ReadReport()
        {
            _device.ReadReport(OnReport);
        }

        private void SendData(byte[] data)
        {
            var report = new HidReport(data.Length)
            {
                Data = data
            };
            _device.WriteReport(report);
        }

        private string ShowBits(byte val)
        {
            string strResult = "";
            for (int t = 128; t > 0; t = t / 2)
            {
                if ((val & t) != 0) strResult = strResult + "1";
                if ((val & t) == 0) strResult = strResult + "0";
            }
            return strResult;
        }

        private bool IsBitSet(byte val, int position)
        {
            string strResult = "";
            for (int t = 128; t > 0; t = t / 2)
            {
                if ((val & t) != 0) strResult = strResult + "1";
                if ((val & t) == 0) strResult = strResult + "0";
            }
            return (strResult.Substring(strResult.Length - position, 1) == "1");
        }
    }
}