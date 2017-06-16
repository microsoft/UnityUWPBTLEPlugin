
namespace LegoSDK
{
    public enum LEMessageType
    {
        /** Unknown */
        LEMessageTypeHubUnknown = 0xFF,
        /** Properties */
        LEMessageTypeHubProperties = 0x01,
        /** Hub Actions */
        LEMessageTypeHubActions = 0x02,
        /** Hub Alerts */
        LEMessageTypeHubAlerts = 0x03,
        /** Attached I/O */
        LEMessageTypeHubAttachedIO = 0x04,
        /** Errors */
        LEMessageTypeError = 0x05,
        /** Restart in boot mode */
        LEMessageTypeFirmwareUpdateBootMode = 0x10,
        /** Port Information Request */
        LEMessageTypePortInformationRequest = 0x21,
        /** Mode Information Request */
        LEMessageTypePortModeInformationRequest = 0x22,
        /** Input Format (Single) */
        LEMessageTypePortInputFormatSetupSingle = 0x41,
        /** Input Format (Combined) */
        LEMessageTypePortInputFormatSetupCombined = 0x42,
        /** Port Information */
        LEMessageTypePortInformation = 0x43,
        /** Port Mode Information */
        LEMessageTypePortModeInformation = 0x44,
        /** Port Value (Single) */
        LEMessageTypePortValueSingle = 0x45,
        /** Port Value (Combined) */
        LEMessageTypePortValueCombined = 0x46,
        /** Port Input Format (Single) */
        LEMessageTypePortInputFormatSingle = 0x47,
        /** Port Input Format (Combined) */
        LEMessageTypePortInputFormatCombined = 0x48,
        /** Virtual Port Setup */
        LEMessageTypeVirtualPortSetup = 0x61,
        /** Port Output Command */
        LEMessageTypePortOutputCommand = 0x81,
        /** Port Output Command Feedback */
        LEMessageTypePortOutputCommandFeedback = 0x82,
    }

    public enum HubProperty
    {
        NAME = 0x1,
        BUTTON = 0x2,
        FIRMWARE_VERSION = 0x3,
        HARDWARE_VERSION = 0x4,
        RSSI = 0x5,
        BATTERY_VOLTAGE = 0x6,
        BATTERY_TYPE = 0x7,
        MANUFACTURER_NAME = 0x8,
        RADIO_FIRMWARE_VERSION = 0x9,
        WIRELESS_PROTOCOL_VERSION = 0xa,
        HARDWARE_SYSTEM_TYPE = 0xb,
        HARDWARE_NETWORK_ID = 0xc,
        PRIMARY_MAC_ADDRESS = 0xd,
        SECONDARY_MAC_ADDRESS = 0xe
    }

    public enum HubPropertyOperation
    {
        SET = 0x1,
        ENABLE_UPDATES = 0x2,
        DISABLE_UPDATES = 0x3,
        RESET = 0x4,
        REQUEST_UPDATE = 0x5,
        UPDATE = 0x6
    }

    enum LEMessagePortInformationType
    {
        Value = 0x00,
        Mode = 0x01,
        ModeCombinations = 0x02
    }

    enum LEMessagePortModeInformationRequestType
    {
        /** Name */
        LEMessagePortModeInformationRequestTypeName = 0x00,
        /** Raw */
        LEMessagePortModeInformationRequestTypeRaw = 0x01,
        /** Pct */
        LEMessagePortModeInformationRequestTypePct = 0x02,
        /** SI */
        LEMessagePortModeInformationRequestTypeSI = 0x03,
        /** Symbol */
        LEMessagePortModeInformationRequestTypeSymbol = 0x04,
        /** Mapping */
        LEMessagePortModeInformationRequestTypeMapping = 0x05,
        /** Value Format */
        LEMessagePortModeInformationRequestTypeValueFormat = 0x80, //128
    }

    enum LEVisionSensorColorIndex
    {
        LEVisionSensorColorIndexOff = -1,
        LEVisionSensorColorIndexBlue = 3,
        LEVisionSensorColorIndexGreen = 5,
        LEVisionSensorColorIndexRed = 9,
        LEVisionSensorColorIndexWhite = 10,
    }


    enum LEMessageErrorType
    {
        /** ACK */
        LEMessageErrorTypeACK = 0x01,
        /** NACK */
        LEMessageErrorTypeNACK = 0x02,
        /** Buffer Overflow */
        LEMessageErrorTypeBufferOverFlow = 0x03,
        /** Timeout */
        LEMessageErrorTypeTimeout = 0x04,
        /** Command not recognized */
        LEMessageErrorTypeCommandNotRecognized = 0x05,
        /** Invalid use (e.g. parameter errors) */
        LEMessageErrorTypeInvalidUse = 0x06
    }

    enum BrickPorts
    {
        PortC = 1,
        PortD = 2,
        ColorLight = 51,
        Power1 = 55,
        Power2 = 56,
        Power3 = 57,    // brick motors
        Power4 = 58,
        PWRL1 = 59,
        PWRL2 = 60
    }

    enum AttachedIoEvent
    {
        DetachedIo = 0x00,
        AttachedIo = 0x01,
        AttachedVirtualIo = 0x02
    }

    public enum StartupInformation
    {
        BufferIfNecessary = 0b00000000,
        ExecuteImmediately = 0b00010000,
        Other = 0b00100000
    }

    public enum CompletionInformation
    {
        NoAction = 0b0000,
        CommandFeedback = 0b0001,
        Other1 = 0b0010,
        Other2 = 0b0011
    }

    public static class Helper
    {
        public static byte BuildStartupCompletionInformation(StartupInformation si, CompletionInformation ci)
        {
            return (byte)((byte)si | (byte)ci);
        }
    }

    public enum PortOutputCommandSubCommandTypeEnum
    {
        NO_OPERATION = 0x0,
        START_POWER_MOTOR_ONE_AND_TWO = 0x2,
        SET_ACC_TIME = 0x5,
        SET_DEC_TIME = 0x6,
        START_SPEED = 0x7,
        START_SPEED_MOTOR_ONE_AND_TWO = 0x8,
        SPEED_FOR_TIME = 0x9,
        SPEED_FOR_TIME_MOTOR_ONE_AND_TWO = 0xa,
        START_SPEED_FOR_DEGREES = 0xb,
        START_SPEED_FOR_DEGREES_MOTOR_ONE_AND_TWO = 0xc,
        START_SPEED_GOTO_ABSOLUTE_POSITION = 0xd,
        START_SPEED_GOTO_ABSOLUTE_POSITION_MOTOR_ONE_AND_TWO = 0xe,
        PRESET_ENCODER_MOTOR_ONE_AND_TWO = 0x14,
        SET_RGB_COLOR_NO = 0x40,
        SET_RGB_RAW = 0x41,
        ACTIVATE_BEHAVIOR = 0x4a,
        DEACTIVATE_BEHAVIOR = 0x4b,
        DIRECT_WRITE = 0x50,
        DIRECT_MODE_WRITE = 0x51
    }

    public enum RGBLightMode
    {
        /*
         * Discrete mode allows selecting a color index from a set of predefined colors
         */
        RGB_LIGHT_MODE_DISCRETE = 0,
        /*
         * Absolute mode allows selecting any color by specifying its RGB component values
         */
        RGB_LIGHT_MODE_ABSOLUTE = 1,
        /*
         * Unknown
         */
        RGB_LIGHT_MODE_UNKNOWN = 2
    }

    public class PortOutputCommandFeedbackType
    {
        /** Buffer Empty + Command In Progress */
        static byte BIT_BUSY_EMPTY = 0x01;

        /** Buffer Empty + Command Completed */
        static byte BIT_BUSY_COMPLETED = 0x02;

        /** Current Command(s) Discarded */
        static byte BIT_DISCARDED = 0x04;

        /** Idle */
        static byte BIT_IDLE = 0x08;

        /** Busy/Full */
        static byte BIT_BUSY_FULL = 0x10;

        byte _data;
        public PortOutputCommandFeedbackType()
        {
            _data = 0x00;
        }

        public PortOutputCommandFeedbackType(byte data)
        {
            _data = data;
        }

        public byte Data
        {
            get { return _data; }
            set { _data = value; }
        }

        public bool BusyEmpty
        {
            get { return (byte)(_data & BIT_BUSY_EMPTY) > 0 ? true : false; }
        }
        public bool BusyCompleted
        {
            get { return (byte)(_data & BIT_BUSY_COMPLETED) > 0 ? true : false; }
        }
        public bool Discarded
        {
            get { return (byte)(_data & BIT_DISCARDED) > 0 ? true : false; }
        }
        public bool Idle
        {
            get { return (byte)(_data & BIT_IDLE) > 0 ? true : false; }
        }
        public bool BusyFull
        {
            get { return (byte)(_data & BIT_BUSY_FULL) > 0 ? true : false; }
        }

        public override string ToString()
        {
            string msg = "";
            if (BusyEmpty)
                msg += "BIT_BUSY_EMPTY";

            if (BusyCompleted)
                msg += " " + "BIT_BUSY_COMPLETED";

            if (Discarded)
                msg += " " + "BIT_DISCARDED";

            if (Idle)
                msg += " " + "BIT_IDLE";

            if (BusyFull)
                msg += " " + "BIT_BUSY_FULL";

            return msg;

        }
    }

    public enum MotorWithTachoMode
    {
        /** Power (Out) */
        POWER = 0,
        /** Speed */
        SPEED = 1,
        /** Position */
        POSITION = 2,
        /** Unknown */
        UNKNOWN = 3
    }

    public enum VisionSensorColorIndex
    {
        /** Off */
        OFF = -1,
        /** Blue */
        BLUE = 3,
        /** Green */
        GREEN = 5,
        /** Red */
        RED = 9,
        /** White */
        WHITE = 10
    }

    public enum MotorWithTachoEndState
    {
        /** Drifting =Floating */
        DRIFTING = 0,
        /** Holding */
        HOLDING = 126,
        /** Braking */
        BRAKING = 127,
        /** Unknown */
        UNKNOWN = -1
    }

    public enum MotorWithTachoAccDecProfileConfiguration
    {
        /** Do not make use of acceleration and deceleration profiles */
        NONE = 0,
        /** Use acceleration profile when command starts */
        START = 1,
        /** Use deceleration profile when command ends */
        END = 2,
        /** Use both acceleration and deceleration profiles */
        BOTH = 3
    }

}