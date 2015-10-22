//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.DISCO_F746NG
{
    using System;
    using Runtime;
    using Chipset = Microsoft.CortexM4OnMBED;
    using ChipsetAbstration = Microsoft.DeviceModels.Chipset.CortexM4;

    public sealed class SpiProvider : Chipset.HardwareModel.SpiProvider
    {
        public static readonly SpiChannelInfo SPI0 = new SpiChannelInfo()
        {
            Mosi = (int)PinName.SPI_MOSI,
            Miso = (int)PinName.SPI_MISO,
            Sclk = (int)PinName.SPI_SCK,
            ChipSelect = (int)PinName.SPI_CS,
            SetupTime = 10,
            HoldTime = 10,
            ReserveMisoPin = true,
            ActiveLow = true,
        };

        public override bool SpiBusySupported
        {
            get
            {
                return false;
            }
        }

        public override SpiChannelInfo GetSpiChannelInfo(int id)
        {
            switch (id)
            {
                case 0:
                    return SPI0;
                default:
                    return null;
            }
        }
    }
}
