using System;

namespace J2i.Net.XInputWrapper
{
    public static class XInput
    {
        private static bool? _isWin8OrNewer;

        private static bool IsWin8OrNewer
        {
            get
            {
                if (_isWin8OrNewer == null)
                {
                    var os = Environment.OSVersion;
                    _isWin8OrNewer = os.Platform == PlatformID.Win32NT && (os.Version.Major > 6 || (os.Version.Major == 6 && os.Version.Minor >= 2));
                }
                return _isWin8OrNewer.Value;
            }
        }

        public static int XInputGetState
            (
            int dwUserIndex, // [in] Index of the gamer associated with the device
            ref XInputState pState // [out] Receives the current state
            )
        {
            return IsWin8OrNewer ? XInput14.XInputGetState(dwUserIndex, ref pState) : XInput910.XInputGetState(dwUserIndex, ref pState);
        }

        public static int XInputSetState
            (
            int dwUserIndex, // [in] Index of the gamer associated with the device
            ref XInputVibration pVibration // [in, out] The vibration information to send to the controller
            )
        {
            return IsWin8OrNewer ? XInput14.XInputSetState(dwUserIndex, ref pVibration) : XInput910.XInputSetState(dwUserIndex, ref pVibration);
        }

        public static int XInputGetCapabilities
            (
            int dwUserIndex, // [in] Index of the gamer associated with the device
            int dwFlags, // [in] Input flags that identify the device type
            ref XInputCapabilities pCapabilities // [out] Receives the capabilities
            )
        {
            return IsWin8OrNewer ? XInput14.XInputGetCapabilities(dwUserIndex, dwFlags, ref pCapabilities) : XInput910.XInputGetCapabilities(dwUserIndex, dwFlags, ref pCapabilities);
        }


        //this function is not available prior to Windows 8
        public static int XInputGetBatteryInformation
            (
            int dwUserIndex, // Index of the gamer associated with the device
            byte devType, // Which device on this user index
            ref XInputBatteryInformation pBatteryInformation // Contains the level and types of batteries
            )
        {
            return IsWin8OrNewer ? XInput14.XInputGetBatteryInformation(dwUserIndex, devType, ref pBatteryInformation) : XInput910.XInputGetBatteryInformation(dwUserIndex, devType, ref pBatteryInformation);
        }

        //this function is not available prior to Windows 8
        public static int XInputGetKeystroke
            (
            int dwUserIndex, // Index of the gamer associated with the device
            int dwReserved, // Reserved for future use
            ref XInputKeystroke pKeystroke // Pointer to an XINPUT_KEYSTROKE structure that receives an input event.
            )
        {
            return IsWin8OrNewer ? XInput14.XInputGetKeystroke(dwUserIndex, dwReserved, ref pKeystroke) : XInput910.XInputGetKeystroke(dwUserIndex, dwReserved, ref pKeystroke);
        }
    }
}
