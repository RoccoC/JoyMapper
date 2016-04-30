using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.DirectInput;
using vJoyInterfaceWrap;

namespace JoyMapper
{
    /**
     * A wrapper around the vJoy Joystick class.
     **/
    class VirtualController : IController
    {
        public delegate void FFBDataReceiveEventHandler(object sender, EventArgs e);

        public enum JoystickCapabilities
        {
            AXIS_X,
            AXIS_Y,
            AXIS_Z,
            AXIS_RX,
            AXIS_RY,
            AXIS_RZ,
            POV,
            SLIDER_0,
            SLIDER_1,
            WHEEL
        }

        public uint ID { get; private set; }
        public bool Connected { get; private set; } = false;
        public List<JoystickCapabilities> Capabilities { get; private set; }
        public int ButtonCount { get; private set; }
        public int ContinuousPOVCount { get; private set; }
        public int DirectionalPOVCount { get; private set; }
        public IList<Guid> SupportedFFBEffects { get; private set; }

        public event FFBDataReceiveEventHandler FFBDataReceived;

        private vJoy joystick;
        private Dictionary<uint, Guid> virtualEffectGuidMap = new Dictionary<uint, Guid>
        {
            { 0x26, EffectGuid.ConstantForce },
            { 0x27, EffectGuid.RampForce },
            { 0x30, EffectGuid.Square },
            { 0x31, EffectGuid.Sine },
            { 0x32, EffectGuid.Triangle },
            { 0x33, EffectGuid.SawtoothUp },
            { 0x34, EffectGuid.SawtoothDown },
            { 0x40, EffectGuid.Spring },
            { 0x41, EffectGuid.Damper },
            { 0x42, EffectGuid.Inertia },
            { 0x43, EffectGuid.Friction }
        };

        public VirtualController(uint ID)
        {
            this.ID = ID;
            this.joystick = new vJoy();
        }

        public void Connect()
        {
            if(!this.Connected)
            {
                // ensure device is available
                VjdStat status = this.joystick.GetVJDStatus(this.ID);
                switch (status)
                {
                    case VjdStat.VJD_STAT_FREE:
                        break;
                    case VjdStat.VJD_STAT_OWN:
                    case VjdStat.VJD_STAT_BUSY:
                    case VjdStat.VJD_STAT_MISS:
                    default:
                        throw new Exception(String.Format("vJoy Device {0} is missing or already in use!\n", this.ID));
                };

                // check driver version against local DLL version
                uint DllVer = 0, DrvVer = 0;
                if (!this.joystick.DriverMatch(ref DllVer, ref DrvVer))
                {
                    throw new Exception(String.Format("Version of vJoy Driver ({0:X}) does not match vJoy DLL Version ({1:X})!\n", DrvVer, DllVer));
                }

                this.loadCapabilities();

                // now aquire the vJoy device
                if (!joystick.AcquireVJD(this.ID))
                {
                    throw new Exception(String.Format("Failed to acquire vJoy device number {0}!\n", this.ID));
                }

                if (this.SupportedFFBEffects.Count > 0)
                {
                    this.joystick.FfbRegisterGenCB(this.OnVirtualFFBDataReceived, null);
                }

                this.joystick.ResetVJD(this.ID);

                this.Connected = true;
            }
        }

        public void Disconnect()
        {
            if (this.Connected)
            {
                if (this.SupportedFFBEffects.Count > 0)
                {
                    this.joystick.FfbRegisterGenCB(null, null);
                }
                this.joystick.RelinquishVJD(this.ID);
                this.Connected = false;
            }
        }

        private void loadCapabilities()
        {
            this.Capabilities = new List<JoystickCapabilities>();

            // Check which axes are supported
            if (this.joystick.GetVJDAxisExist(this.ID, HID_USAGES.HID_USAGE_X))
            {
                this.Capabilities.Add(JoystickCapabilities.AXIS_X);
            }
            if (this.joystick.GetVJDAxisExist(this.ID, HID_USAGES.HID_USAGE_Y))
            {
                this.Capabilities.Add(JoystickCapabilities.AXIS_Y);
            }
            if (this.joystick.GetVJDAxisExist(this.ID, HID_USAGES.HID_USAGE_Z))
            {
                this.Capabilities.Add(JoystickCapabilities.AXIS_Z);
            }
            if (this.joystick.GetVJDAxisExist(this.ID, HID_USAGES.HID_USAGE_RX))
            {
                this.Capabilities.Add(JoystickCapabilities.AXIS_RX);
            }
            if (this.joystick.GetVJDAxisExist(this.ID, HID_USAGES.HID_USAGE_RY))
            {
                this.Capabilities.Add(JoystickCapabilities.AXIS_RY);
            }
            if (this.joystick.GetVJDAxisExist(this.ID, HID_USAGES.HID_USAGE_RZ))
            {
                this.Capabilities.Add(JoystickCapabilities.AXIS_RZ);
            }
            if (this.joystick.GetVJDAxisExist(this.ID, HID_USAGES.HID_USAGE_POV))
            {
                this.Capabilities.Add(JoystickCapabilities.POV);
            }
            if (this.joystick.GetVJDAxisExist(this.ID, HID_USAGES.HID_USAGE_SL0))
            {
                this.Capabilities.Add(JoystickCapabilities.SLIDER_0);
            }
            if (this.joystick.GetVJDAxisExist(this.ID, HID_USAGES.HID_USAGE_SL1))
            {
                this.Capabilities.Add(JoystickCapabilities.SLIDER_1);
            }
            if (this.joystick.GetVJDAxisExist(this.ID, HID_USAGES.HID_USAGE_WHL))
            {
                this.Capabilities.Add(JoystickCapabilities.WHEEL);
            }

            // get supported FFB effects
            if (this.joystick.IsDeviceFfb(this.ID))
            {
                foreach (KeyValuePair<uint, Guid> entry in this.virtualEffectGuidMap)
                {
                    if (this.joystick.IsDeviceFfbEffect(this.ID, entry.Key)) {
                        this.SupportedFFBEffects.Add(entry.Value);
                    }
                }
            }

            // Get the number of buttons and POV Hat switches supported by this vJoy device
            this.ButtonCount = this.joystick.GetVJDButtonNumber(this.ID);
            this.ContinuousPOVCount = this.joystick.GetVJDContPovNumber(this.ID);
            this.DirectionalPOVCount = this.joystick.GetVJDDiscPovNumber(this.ID);
        }

        private void OnVirtualFFBDataReceived(IntPtr data, object userData)
        {
            // TODO: parse data and convert to DI types in EventArgs subclass
            this.OnFFBDataReceived(EventArgs.Empty);
        }

        private void OnFFBDataReceived(EventArgs e)
        {
            if (this.FFBDataReceived != null)
            {
                this.FFBDataReceived(this, e);
            }
        }
    }
}
