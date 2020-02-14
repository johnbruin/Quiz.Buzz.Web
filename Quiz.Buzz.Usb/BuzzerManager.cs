using HidLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Quizz.Buzz
{
    public class BuzzerManager
    {
        private const int VendorId = 0x054C;
        private const int ProductId = 0x1000;

        public event Buzzer.AnswerHandler OnAnswerClick;

        public List<Buzzer> Buzzers { get; private set; }

        public BuzzerManager()
        {
            var devices = HidDevices.Enumerate(VendorId, ProductId).ToList();
            Buzzers = new List<Buzzer>();
            for (var index = 0; index < devices.Count; index++)
            {
                var hidDevice = devices[index];
                Buzzers.Add(new Buzzer(hidDevice, index * 4));
            }

            foreach (var buzzer in Buzzers)
            {
                buzzer.OnAnswerClick += (sender, ace) => { OnAnswerClick?.Invoke(this, ace); };
            }
        }        

        public bool IsInitialized
        {
            get { return Buzzers.TrueForAll(e => e.IsInitialized); }
        }

        public void AllLightOn()
        {
            foreach (var buzzer in Buzzers)
            {
                buzzer.AllLightsOn();
            }
        }

        public void AllLightOff()
        {
            foreach (var buzzer in Buzzers)
            {
                buzzer.AllLightsOff();
            }
        }

        public void FlashLight(short playerId, short times)
        {
            for (int i = 0; i < times; i++)
            {
                SetLight(playerId, true);
                SetLight(playerId, false);
            }
        }

        public void SetLightOn(short playerId)
        {
            SetLight(playerId, true);
        }

        public void SetLightOff(short playerId)
        {
            SetLight(playerId, false);
        }

        public void SetLight(short playerId, bool value)
        {
            var deviceId = (playerId - 1) / 4;
            var localDeviceId = (playerId - 1) % 4 + 1;

            if (deviceId > Buzzers.Count - 1)
            {
                throw new ArgumentOutOfRangeException(nameof(playerId));
            }

            if (value)
            {
                Buzzers[deviceId].LightOn(localDeviceId);
            }
            else
            {
                Buzzers[deviceId].LightOff(localDeviceId);
            }
        }

        public bool IsLightOn(short playerId)
        {
            var deviceId = (playerId - 1) / 4;
            var localDeviceId = (playerId - 1) % 4 + 1;

            if (deviceId > Buzzers.Count - 1)
            {
                throw new ArgumentOutOfRangeException(nameof(playerId));
            } 
            
            return Buzzers[deviceId].IsLightOn(localDeviceId);
        }
        
        public void WaitForInitialization()
        {
            while (!IsInitialized)
            {
                Thread.Sleep(100);
            }
        }
    }
}