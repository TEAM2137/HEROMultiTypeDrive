using CTRE.Phoenix.Controller;
using CTRE.Phoenix.MotorControl;
using Microsoft.SPOT;
using System.Text;
using System.Threading;


namespace HeroArcadeDriveEx
{
    public class Program
    {
        /* create a PWM Speed controller */
        static PWMSpeedController BackRight = new PWMSpeedController(CTRE.HERO.IO.Port3.PWM_Pin8);
        static PWMSpeedController FrontRight = new PWMSpeedController(CTRE.HERO.IO.Port3.PWM_Pin7);
        static PWMSpeedController FrontLeft = new PWMSpeedController(CTRE.HERO.IO.Port3.PWM_Pin6);
        static PWMSpeedController BackLeft = new PWMSpeedController(CTRE.HERO.IO.Port3.PWM_Pin4);

        static StringBuilder stringBuilder = new StringBuilder();

        static CTRE.Phoenix.Controller.GameController _gamepad = null;

        public static float x = 0.0F;
        public static float y = 0.0F;
        public static bool Mode = false;

        public static void Main()
        {
            /* loop forever */
            while (true)
            {
                if (null == _gamepad)
                    _gamepad = new GameController(new CTRE.Phoenix.UsbHostDevice(0));
                // This is the code that keeps the robot safe, if we loose the controller, "DON'T FEED THE DOG" 
                if (_gamepad.GetConnectionStatus() == CTRE.Phoenix.UsbDeviceConnection.Connected)
                {
                    /* feed watchdog to keep Talon's enabled */
                    CTRE.Phoenix.Watchdog.Feed();
                    /* drive robot using gamepad */
                    Drive();
                }
                /* print whatever is in our string builder */
                Debug.Print(stringBuilder.ToString());
                stringBuilder.Clear();
                /* run this task every 20ms */
                Thread.Sleep(20);
            }
        }
        /**
         * If value is within 10% of center, clear it.
         * @param value [out] floating point value to deadband.
         */
        static void Deadband(ref float value)
        {
            if (value < -0.05)
            {
                /* outside of deadband */
            }
            else if (value > +0.05)
            {
                /* outside of deadband */
            }
            else
            {
                /* within 10% so zero it */
                value = 0;
            }
        }

        static void Drive()
        {

            // left and right "stick" values are raw data, "x" and "y" are Deadband values 
            float leftstick = 0.0F;
            float rightstick = 0.0F;
            // These are variables for the calcualted power levels sent to MotorControllers.
            float leftThrottle = 0.0F;
            float rightThrottle = 0.0F;
            float wheelAdjust = 0.0F;
            // These are the Halo Drive Tuning Constants, values need to be tuned to bot, wheelbase and gear ratios determine tunings.
            float WheelGain = 0.7F;
            float quickTurnGain = 0.5F;
            float HaloThreshold = 0.15F;

            bool ControlTypeTank = _gamepad.GetButton(1);
            bool ControlTypeHalo = _gamepad.GetButton(3);
            string Type = "";

            if (ControlTypeTank || Mode)
            {
                leftstick = -1 * _gamepad.GetAxis(1);
                rightstick = -1 * _gamepad.GetAxis(5);
                x = leftstick;
                y = rightstick;
                Deadband(ref x);
                Deadband(ref y);
                Mode = true;
                Type = "Tank";
                leftThrottle = x;
                rightThrottle = y;

            }
            if (ControlTypeHalo || Mode == false)
            {
                leftstick = -1 * _gamepad.GetAxis(1);
                rightstick = -1 * _gamepad.GetAxis(2);
                x = leftstick;
                y = rightstick;
                Deadband(ref x);
                Deadband(ref y);
                Mode = false;
                Type = "Halo";
                // If Throttle is below threshold, then do quickturn calcuation 
                if (System.Math.Abs(x) < HaloThreshold)
                {
                    wheelAdjust = quickTurnGain * y;
                    leftThrottle = -wheelAdjust;
                    rightThrottle = wheelAdjust;
                }
                // Not Quickturn, so do the typical Halo calculation 
                else
                {
                    wheelAdjust = WheelGain * y;
                    leftThrottle = x - wheelAdjust;
                    rightThrottle = x + wheelAdjust;
                }
            }

            // Forcing throttle values between -1 and 1 "There is probably a better function to do this, but I am ignorant"
            if (leftThrottle > 1) leftThrottle = 1;
            if (leftThrottle < -1) leftThrottle = -1;
            if (rightThrottle > 1) rightThrottle = 1;
            if (rightThrottle < -1) rightThrottle = -1;

            // Write Motor Control Values to the Motor Controllers ONCE!!!!! 
            FrontLeft.Set(leftThrottle);
            BackLeft.Set(leftThrottle);
            FrontRight.Set(rightThrottle);
            BackRight.Set(rightThrottle);


            stringBuilder.Append(Type);
            stringBuilder.Append("\t");
            stringBuilder.Append(leftstick);
            stringBuilder.Append("\t");
            stringBuilder.Append(rightstick);
            stringBuilder.Append("\t");
            stringBuilder.Append(x);
            stringBuilder.Append("\t");
            stringBuilder.Append(y);
            stringBuilder.Append("\t");
            stringBuilder.Append(leftThrottle);
            stringBuilder.Append("\t");
            stringBuilder.Append(rightThrottle);
            stringBuilder.Append("\t");
            stringBuilder.Append(wheelAdjust);
            stringBuilder.Append("\t");
        }
    }
}