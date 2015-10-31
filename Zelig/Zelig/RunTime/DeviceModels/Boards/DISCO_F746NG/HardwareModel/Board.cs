﻿//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.DISCO_F746NG
{
    using System;
    
    using Chipset           = Microsoft.CortexM4OnMBED;
    using ChipsetAbstration = Microsoft.DeviceModels.Chipset.CortexM4;


    public sealed class Board : Chipset.Board
    {
        internal const int GPIO_PORT_SHIFT = 12;
        
        //
        // Serial Ports
        //
        private static readonly string[] m_serialPorts = { "UART0", "UART1" };

        public static readonly ChipsetAbstration.Board.SerialPortInfo UART0 = new ChipsetAbstration.Board.SerialPortInfo() 
        {
            TxPin = (int)PinName.SERIAL_TX,
            RxPin = (int)PinName.SERIAL_RX,
            RtsPin = unchecked((int)PinName.NC),
            CtsPin = unchecked((int)PinName.NC)
        };

        public static readonly ChipsetAbstration.Board.SerialPortInfo UART1 = new ChipsetAbstration.Board.SerialPortInfo() 
        {
            TxPin = (int)PinName.USBTX,
            RxPin = (int)PinName.USBRX,
            RtsPin = unchecked((int)PinName.NC),
            CtsPin = unchecked ((int)PinName.NC)
        };
        
        //
        // Gpio discovery
        //

        public override int PinCount
        {
            get
            {
                return 160;
            }
        }

        public override int NCPin
        {
            get
            {
                return -1;
            }
        }
        
        public override int PinToIndex( int pin )
        {
            int port = pin >> Board.GPIO_PORT_SHIFT;
            int portIndex = pin & 0x000000FF;

            return ( port * 32 ) + portIndex;
        }

        //
        // Serial Port
        //
        public override string[] GetSerialPorts()
        {
            return m_serialPorts;
        }

        public override ChipsetAbstration.Board.SerialPortInfo GetSerialPortInfo(string portName)
        {
            switch (portName)
            {
                case "UART0":
                    return UART0;
                case "UART1":
                    return UART1;
                default:
                    return null;
            }
        }

        //
        // System timer
        //
        public override int GetSystemTimerIRQNumber( )
        {
            return (int)IRQn.TIM5_IRQn;
        }
    }
}
