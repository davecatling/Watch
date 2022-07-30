using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatchWpfClient.ViewModels
{
    public class MainWatchVm
    {
        private double _hourStartAngle;
        private double _minuteStartAngle;
        private double _secondStartAngle;
        private string _secondStartDuration;
        private string _minuteStartDuration;
        private string _hourStartDuration;

        public double HourStartAngle
        {
            get => _hourStartAngle;
            private set => _hourStartAngle = value;
        }

        public double MinuteStartAngle
        {
            get => _minuteStartAngle;
            private set => _minuteStartAngle = value;
        }

        public double SecondStartAngle
        {
            get => _secondStartAngle;
            private set => _secondStartAngle = value;
        }

        public string SecondStartDuration
        {
            get => _secondStartDuration;
            private set => _secondStartDuration = value;
        }

        public string MinuteStartDuration
        {
            get => _minuteStartDuration;
            private set => _minuteStartDuration = value;
        }

        public string HourStartDuration
        {
            get => _hourStartDuration;
            private set => _hourStartDuration = value;
        }

        public MainWatchVm()
        {
            var time = DateTime.Now;
            HourStartAngle = (time.Hour / 12F) * 360 ;
            MinuteStartAngle = (time.Minute / 60F) * 360;
            SecondStartAngle = (time.Second / 60F) * 360;
            var millisecondsTillNextSecond = time.Millisecond == 0 ? 999 : 1000 - time.Millisecond;
            var secondsTillNextMinute = time.Second == 0 ? 60 : 59 - time.Second;
            var minutesTillNextHour = time.Minute == 0 ? 59 : 59 - time.Minute;
            var hoursTillNext12Hours = time.Hour == 0 ? 11 : 11 - (time.Hour % 12);
            SecondStartDuration = time.Second == 0 ? "0:1:0" : $"0:0:{secondsTillNextMinute}.{millisecondsTillNextSecond}";
            MinuteStartDuration = $"0:{minutesTillNextHour}:{secondsTillNextMinute}.{millisecondsTillNextSecond}";
            HourStartDuration = $"{hoursTillNext12Hours}:{minutesTillNextHour}:{secondsTillNextMinute}.{millisecondsTillNextSecond}";
        }
    }
}
