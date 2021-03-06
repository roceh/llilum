﻿//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.K64F
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
        private static readonly string[] m_serialPorts = { "UART0", "UART1", "UART3" };

        public static readonly ChipsetAbstration.Board.SerialPortInfo UART0 = new ChipsetAbstration.Board.SerialPortInfo() 
        {
            TxPin = (int)PinName.PTC14,
            RxPin = (int)PinName.PTC15,
            RtsPin = unchecked((int)PinName.NC),
            CtsPin = unchecked((int)PinName.NC)
        };

        public static readonly ChipsetAbstration.Board.SerialPortInfo UART1 = new ChipsetAbstration.Board.SerialPortInfo() 
        {
            TxPin = (int)PinName.USBTX,
            RxPin = (int)PinName.USBRX,
            RtsPin = unchecked((int)PinName.NC),
            CtsPin = unchecked((int)PinName.NC)
        };

        public static readonly ChipsetAbstration.Board.SerialPortInfo UART3 = new ChipsetAbstration.Board.SerialPortInfo()
        {
            TxPin = (int)PinName.PTC17,
            RxPin = (int)PinName.PTC16,
            RtsPin = unchecked((int)PinName.NC),
            CtsPin = unchecked((int)PinName.NC)
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
                case "UART3":
                    return UART3;
                default:
                    return null;
            }
        }

        //
        // System timer
        //
        public override int GetSystemTimerIRQNumber( )
        {
            return (int)IRQn.PIT3_IRQn;
        }

        public override int GetSerialPortIRQNumber(string portName)
        {
            switch (portName)
            {
                case "UART0":
                    return (int)IRQn.UART0_RX_TX_IRQn;
                case "UART1":
                    return (int)IRQn.UART1_RX_TX_IRQn;
                case "UART3":
                    return (int)IRQn.UART3_RX_TX_IRQn;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
