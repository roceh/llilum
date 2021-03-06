﻿//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.CortexM3OnMBED.HardwareModel
{
    using GPIO_FRAMEWORK = Windows.Devices.Gpio.Provider;
    using Chipset = Microsoft.CortexM3OnCMSISCore;
    using ChipsetAbstration = Microsoft.DeviceModels.Chipset.CortexM3;


    public class HardwareProvider : Chipset.HardwareProvider
    {
        //
        // Gpio Discovery 
        //
        public override int PinCount => Board.Instance.PinCount;

        public override int PinToIndex(int pin)
        {
            return Board.Instance.PinToIndex(pin);
        }

        public override int InvalidPin
        {
            get
            {
                return Board.Instance.NCPin;
            }
        }

        //
        // Serial Discovery
        //

        public override string[] GetSerialPorts()
        {
            return Board.Instance.GetSerialPorts();
        }

        public override bool GetSerialPinsFromPortName(string portName, out int txPin, out int rxPin, out int rtsPin, out int ctsPin)
        {
            ChipsetAbstration.Board.SerialPortInfo portInfo = Board.Instance.GetSerialPortInfo(portName);
            if (portInfo == null)
            {
                int invalidPin = this.InvalidPin;
                txPin = invalidPin;
                rxPin = invalidPin;
                ctsPin = invalidPin;
                rtsPin = invalidPin;
                return false;
            }

            txPin =  portInfo.TxPin;
            rxPin =  portInfo.RxPin;
            ctsPin = portInfo.CtsPin;
            rtsPin = portInfo.RtsPin;
            return true;
        }
    }
}
