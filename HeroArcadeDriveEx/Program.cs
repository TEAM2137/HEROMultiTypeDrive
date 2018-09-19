using System;
using System.Threading;
using Microsoft.SPOT;
using System.Text;


using CTRE.Phoenix;
using CTRE.Phoenix.Controller;
using CTRE.Phoenix.MotorControl;
using CTRE.Phoenix.MotorControl.CAN;

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

        static CTRE.Phoenix.Controller.GameControllerValues gv = new CTRE.Phoenix.Controller.GameControllerValues();


        public static float WheelGain = 0.7F;
        public static float rightstick = 0.0F;
        public static float wheelAdjust = 0.0F;
        public static bool Mode = false;

        public static void Main()
        {
            /* loop forever */
            while (true)
            {
                /* drive robot using gamepad */
                Drive();
                /* print whatever is in our string builder */
                Debug.Print(stringBuilder.ToString());
                stringBuilder.Clear();
                /* feed watchdog to keep Talon's enabled */
                CTRE.Phoenix.Watchdog.Feed();
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
            if (value < -0.10)
            {
                /* outside of deadband */
            }
            else if (value > +0.10)
            {
                /* outside of deadband */
            }
            else
            {
                /* within 10% so zero it */
                value = 0;
            }
        }
        static void Quickturn()
        {
            wheelAdjust = WheelGain * rightstick;
            FrontLeft.Set(wheelAdjust);
            BackLeft.Set(wheelAdjust);
            FrontRight.Set(-wheelAdjust);
            BackRight.Set(-wheelAdjust);
        }
        static void Drive()
        {
            if (null == _gamepad)
                _gamepad = new GameController(new CTRE.Phoenix.UsbHostDevice(0));
            //_gamepad.GetAllValues(ref gv);
            float leftstick = 0.0F;
            float rightstick = 0.0F;
            float HaloThreshold = 0.25F;
            double wheelAdjust = 0.0F;
            float WheelGain = 0.7F;
            bool ControlTypeTank = _gamepad.GetButton(1);
            bool ControlTypeHalo = _gamepad.GetButton(3);
            //bool Default = true;
            string Type = "";
            if (ControlTypeTank || Mode)
            {
                leftstick = _gamepad.GetAxis(1);
                rightstick = _gamepad.GetAxis(5);
                Mode = true;
                Type = "Tank";
            } 
            if (ControlTypeHalo || Mode == false)
            {
                leftstick = _gamepad.GetAxis(1);
                rightstick = _gamepad.GetAxis(2);
                Mode = false;
                Type = "Halo";
            }

            float x;
            x = leftstick;
            float y;
            y = rightstick;
            Deadband(ref x);
            Deadband(ref y);

            float leftThrot = x; //+ twist;
            float rightThrot = y; //- twist;

            if (ControlTypeHalo)
            {
                if (System.Math.Abs(x) > HaloThreshold)
                {
                    Quickturn();
                }
                else
                {
                    WheelGain = WheelGain * rightstick;
                    FrontLeft.Set((float)leftThrot - (float)wheelAdjust);
                    BackLeft.Set((float)leftThrot - (float)wheelAdjust);
                    FrontRight.Set((float)leftThrot + (float)wheelAdjust);
                    BackRight.Set((float)leftThrot + (float)wheelAdjust);
                }
            }
            else
            {
                FrontLeft.Set((float)leftThrot);
                BackLeft.Set((float)leftThrot);
                FrontRight.Set((float)rightThrot);
                BackRight.Set((float)rightThrot);
            }

            stringBuilder.Append(Type);
            stringBuilder.Append("\t");
            stringBuilder.Append(x);
            stringBuilder.Append("\t");
            stringBuilder.Append(y);
            stringBuilder.Append("\t");
        }
    }
}