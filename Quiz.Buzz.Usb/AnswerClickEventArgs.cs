using System;

namespace Quizz.Buzz
{
    public class AnswerClickEventArgs : EventArgs
    {
        public readonly int PlayerId;
        public readonly int DevicePlayerId;
        public readonly AnswerColor Answer;
        public readonly Buzzer Buzzer;

        public AnswerClickEventArgs(int playerId, int devicePlayerId, AnswerColor answer, Buzzer buzzer)
        {
            this.PlayerId = playerId;
            this.DevicePlayerId = devicePlayerId;
            this.Answer = answer;
            this.Buzzer = buzzer;
        }

        public bool IsLightOn()
        {
            return Buzzer.IsLightOn(DevicePlayerId);
        }

        public void LightOn()
        {
            Buzzer.LightOn(DevicePlayerId);
        }

        public void LightOff()
        {
            Buzzer.LightOff(DevicePlayerId);
        }

        public void SetLight(bool value)
        {
            if (value)
            {
                LightOn();
            }
            else
            {
                LightOff();
            }
        }
    }
}