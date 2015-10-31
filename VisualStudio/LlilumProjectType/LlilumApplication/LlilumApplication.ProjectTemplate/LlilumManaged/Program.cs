﻿//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define LPC1768
//#define K64F
#define DISCO_F746NG

namespace Managed
{
    using System;
    using Windows.Devices.Gpio;
    using System.Runtime.InteropServices;
    using System.Threading;

#if LPC1768
    using Microsoft.Zelig.LPC1768;
#elif K64F
    using Microsoft.Zelig.K64F;
#elif (DISCO_F746NG)
    using Microsoft.Zelig.DISCO_F746NG;
#endif

    class Program
    {
        // Example of C interop functions
        [DllImport("C")]
        private static extern int AddOneInterop(int input);


        const float period = 0.75f;
        const float timePerMode = 4.0f;

        static int[] pinNumbers =
        {
#if (LPC1768)
            (int)PinName.LED1,
            (int)PinName.LED4,
#elif (K64F)
            (int)PinName.LED_BLUE,
            (int)PinName.LED_RED,
#elif (DISCO_F746NG)
          (int) PinName.LED1,
>>>>>>> Initial tweaks to get DISCO_F746NG working - blinky c# app now runs
#else
            #error No target board defined.
#endif
        };

        static void Main()
        {
            var controller = GpioController.GetDefault();

            var threadToggledPin = controller.OpenPin(pinNumbers[0]);
            var loopToggledPin = controller.OpenPin(pinNumbers[1]);

            threadToggledPin.SetDriveMode(GpioPinDriveMode.Output);
            loopToggledPin.SetDriveMode(GpioPinDriveMode.Output);

            int threadPinState = 1;
            int loopPinState = 1;

            // Toggling a pin with a thread
            var ev = new AutoResetEvent(false);
            var solitaryBlinker = new Thread(delegate ()
            {
                while (true)
                {
                    ev.WaitOne(1000, false);

                    threadToggledPin.Write((GpioPinValue)threadPinState);

                    // Using the C interop here
                    threadPinState = AddOneInterop(threadPinState) % 2;
                }
            });
            solitaryBlinker.Start();

            // Toggling a pin in a loop
            long last = DateTime.Now.Ticks;

            while (true)
            {
                long now = DateTime.Now.Ticks;

                // Toggle every 500ms
                if (now > (last + 5000000))
                {
                    last = now;

                    // Toggle the pin
                    loopToggledPin.Write((GpioPinValue)loopPinState);

                    // Using the C interop here
                    loopPinState = AddOneInterop(loopPinState) % 2;
                }
            }
        }
    }
}
