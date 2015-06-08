using Leap;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

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
                if (this._Message != value)
                {
                    this._Message = value;
                    OnPropertyChanged();
                }
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

        public delegate void DataEventHandler(object sender, string e);
        public event DataEventHandler DataChanged;
        private void OnData(string line)
        {
            var handler = this.DataChanged;
            if (handler != null)
                handler(this, line);
        }

        private Object thisLock = new Object();

        public override void OnInit(Controller controller)
        {
            SafeWriteLine("Initialized");
        }

        public override void OnConnect(Controller controller)
        {
            SafeWriteLine("Connected");
            controller.EnableGesture(Gesture.GestureType.TYPE_CIRCLE);
            controller.EnableGesture(Gesture.GestureType.TYPE_KEY_TAP);
            controller.EnableGesture(Gesture.GestureType.TYPE_SCREEN_TAP);
            controller.EnableGesture(Gesture.GestureType.TYPE_SWIPE);
        }

        public override void OnDisconnect(Controller controller)
        {
            //Note: not dispatched when running in a debugger.
            SafeWriteLine("Disconnected");
        }

        public override void OnExit(Controller controller)
        {
            SafeWriteLine("Exited");
        }

        public override void OnFrame(Controller controller)
        {
            var frame = controller.Frame();
            var message = "";

            message += "Frame id: " + frame.Id
                        + ", timestamp: " + frame.Timestamp
                        + ", hands: " + frame.Hands.Count
                        + ", fingers: " + frame.Fingers.Count
                        + ", tools: " + frame.Tools.Count
                        + ", gestures: " + frame.Gestures().Count
                        + System.Environment.NewLine;

            foreach (Hand hand in frame.Hands)
            {
                message += "  Hand id: " + hand.Id
                            + ", palm position: " + hand.PalmPosition
                            + System.Environment.NewLine;
                // Get the hand's normal vector and direction
                Leap.Vector normal = hand.PalmNormal;
                Leap.Vector direction = hand.Direction;

                // Calculate the hand's pitch, roll, and yaw angles
                message += "  Hand pitch: " + direction.Pitch * 180.0f / (float)Math.PI + " degrees, "
                            + "roll: " + normal.Roll * 180.0f / (float)Math.PI + " degrees, "
                            + "yaw: " + direction.Yaw * 180.0f / (float)Math.PI + " degrees"
                            + System.Environment.NewLine;

                // Get the Arm bone
                Arm arm = hand.Arm;
                message += "  Arm direction: " + arm.Direction
                            + ", wrist position: " + arm.WristPosition
                            + ", elbow position: " + arm.ElbowPosition
                            + System.Environment.NewLine;

                // Get fingers
                foreach (Finger finger in hand.Fingers)
                {
                    message += "    Finger id: " + finger.Id
                                + ", " + finger.Type.ToString()
                                + ", length: " + finger.Length
                                + "mm, width: " + finger.Width + "mm"
                                + ", TipPosition:" + finger.StabilizedTipPosition
                                + ", Direction"+ finger.Direction
                                + System.Environment.NewLine;

                    // Get finger bones
                    Bone bone;
                    foreach (Bone.BoneType boneType in (Bone.BoneType[])Enum.GetValues(typeof(Bone.BoneType)))
                    {
                        bone = finger.Bone(boneType);
                        message += "      Bone: " + boneType
                                    + ", start: " + bone.PrevJoint
                                    + ", end: " + bone.NextJoint
                                    + ", direction: " + bone.Direction
                                    + System.Environment.NewLine;
                    }
                }

            }

            // Get tools
            foreach (Tool tool in frame.Tools)
            {
                message += "  Tool id: " + tool.Id
                            + ", position: " + tool.TipPosition
                            + ", direction " + tool.Direction
                            + System.Environment.NewLine;
            }

            // Get gestures
            GestureList gestures = frame.Gestures();
            for (int i = 0; i < gestures.Count; i++)
            {
                Gesture gesture = gestures[i];

                switch (gesture.Type)
                {
                    case Gesture.GestureType.TYPE_CIRCLE:
                        CircleGesture circle = new CircleGesture(gesture);

                        // Calculate clock direction using the angle between circle normal and pointable
                        String clockwiseness;
                        if (circle.Pointable.Direction.AngleTo(circle.Normal) <= Math.PI / 2)
                        {
                            //Clockwise if angle is less than 90 degrees
                            clockwiseness = "clockwise";
                        }
                        else
                        {
                            clockwiseness = "counterclockwise";
                        }

                        float sweptAngle = 0;

                        // Calculate angle swept since last frame
                        if (circle.State != Gesture.GestureState.STATE_START)
                        {
                            CircleGesture previousUpdate = new CircleGesture(controller.Frame(1).Gesture(circle.Id));
                            sweptAngle = (circle.Progress - previousUpdate.Progress) * 360;
                        }

                        message += "  Circle id: " + circle.Id
                                       + ", " + circle.State
                                       + ", progress: " + circle.Progress
                                       + ", radius: " + circle.Radius
                                       + ", angle: " + sweptAngle
                                       + ", " + clockwiseness
                                       + System.Environment.NewLine;
                        break;
                    case Gesture.GestureType.TYPE_SWIPE:
                        SwipeGesture swipe = new SwipeGesture(gesture);
                        message += "  Swipe id: " + swipe.Id
                                       + ", " + swipe.State
                                       + ", position: " + swipe.Position
                                       + ", direction: " + swipe.Direction
                                       + ", speed: " + swipe.Speed
                                       + System.Environment.NewLine;
                        break;
                    case Gesture.GestureType.TYPE_KEY_TAP:
                        KeyTapGesture keytap = new KeyTapGesture(gesture);
                        message += "  Tap id: " + keytap.Id
                                       + ", " + keytap.State
                                       + ", position: " + keytap.Position
                                       + ", direction: " + keytap.Direction
                                       + System.Environment.NewLine;
                        break;
                    case Gesture.GestureType.TYPE_SCREEN_TAP:
                        ScreenTapGesture screentap = new ScreenTapGesture(gesture);
                        message += "  Tap id: " + screentap.Id
                                       + ", " + screentap.State
                                       + ", position: " + screentap.Position
                                       + ", direction: " + screentap.Direction
                                       + System.Environment.NewLine;
                        break;
                    default:
                        SafeWriteLine("  Unknown gesture type.");
                        break;
                }
            }

            if (!frame.Hands.IsEmpty || !frame.Gestures().IsEmpty)
            {
                message += System.Environment.NewLine;
            }
            this.SafeWriteLine(message);
        }
    }
}
