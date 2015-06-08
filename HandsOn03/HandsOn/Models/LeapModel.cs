using Leap;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Linq;

namespace HandsOn.Models
{
    public class TItem
    {
        public long ID { get; set; }
        public Point[] Hand { get; set; }
        public Point[] Finger { get; set; }
        public int[] Count { get; set; }
    }

    public class LeapModel : INotifyPropertyChanged
    {
        private string _Message;
        public string Message
        {
            get { return this._Message; }
            set
            {
                this._Message = value;
                OnPropertyChanged();
            }
        }

        public enum ResultState
        {
            Unknown = 0,
            Lock = 1,
            Paper = 2,
            Scissors = 3
        }

        private ResultState _Result = ResultState.Unknown;
        public ResultState Result
        {
            get { return this._Result; }
            set
            {
                if (this._Result != value)
                {
                    this._Result = value;
                    OnPropertyChanged();
                }
            }
        }

        public LeapModel()
        {
        }

        private SampleListener LeapListener;
        private Controller LeapController;

        public void LeapStart()
        {
            this.LeapListener = new SampleListener();
            this.LeapController = new Controller();
            this.LeapController.AddListener(this.LeapListener);
            this.LeapListener.MessageChanged += LeapListener_MessageChanged;
            this.LeapListener.DataChanged += LeapListener_DataChanged;
        }

        void LeapListener_DataChanged(object sender, SampleListener.TPos e)
        {
        }

        public void LeapStop()
        {
            this.LeapController.RemoveListener(this.LeapListener);
            this.LeapController.Dispose();
        }

        private void LeapListener_MessageChanged(object sender, string e)
        {
            this.Message = e;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] String propertyName = "")
        {
            var handler = this.PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SampleListener : Listener
    {
        public delegate void MessageEventHandler(object sender, string e);
        public event MessageEventHandler MessageChanged;
        private void SafeWriteLine(string line)
        {
            var handler = this.MessageChanged;
            if (handler != null)
                handler(this, line);
        }

        public delegate void DataEventHandler(object sender, TPos e);
        public event DataEventHandler DataChanged;
        private void OnData(TPos line)
        {
            var handler = this.DataChanged;
            if (handler != null)
                handler(this, line);
        }

        public class TLeap
        {
            public DateTime ID { get; set; }
            public string Text { get; set; }
        }

        public class TPos
        {
            public long ID { get; set; }
            public Leap.Vector[] Hand { get; set; }
            public Leap.Vector[] Finger { get; set; }
            public int[] Count { get; set; }
        }

        private TPos Pos = new TPos();

        public override void OnFrame(Controller cont)
        {
            var ib = cont.Frame().InteractionBox;
            var leapFrame = cont.Frame();

            this.Pos.Hand = new Leap.Vector[] {Leap.Vector.Zero, Leap.Vector.Zero};
            this.Pos.Finger = new Leap.Vector[] {Leap.Vector.Zero, Leap.Vector.Zero, Leap.Vector.Zero, Leap.Vector.Zero, Leap.Vector.Zero, Leap.Vector.Zero, Leap.Vector.Zero, Leap.Vector.Zero, Leap.Vector.Zero, Leap.Vector.Zero};
            this.Pos.Count = new int[] {0,0};
            if ( leapFrame != null)
            {
                var rightHand = leapFrame.Hands.Rightmost;
                var leftHand  = leapFrame.Hands.Leftmost;
                var rightFingers = rightHand.Fingers;
                var leftFingers = leftHand.Fingers;

                this.Pos.ID =leapFrame.Id;
                this.Pos.Hand[0] = ib.NormalizePoint(leftHand.PalmPosition);
                this.Pos.Hand[1] = ib.NormalizePoint(rightHand.PalmPosition);

                var message = "";
                if (!rightFingers.IsEmpty)
                {
                    message += "    Hand id: " + rightHand.Id
                                + ", Direction:" + rightHand.Direction
                                + System.Environment.NewLine;
                }
                foreach (Finger finger in rightHand.Fingers)
                {
                    message += "    Finger id: " + finger.Id
                                + ", " + finger.Type.ToString()
                                + ", Direction:" + finger.Direction
                                + System.Environment.NewLine;
                }
                SafeWriteLine(message);
                OnData(this.Pos);
            }
        }
    }
}
