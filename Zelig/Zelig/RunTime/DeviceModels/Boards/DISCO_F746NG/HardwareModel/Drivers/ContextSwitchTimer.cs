//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define TIMERS_SELF_TEST

namespace Microsoft.Zelig.DISCO_F746NG.Drivers
{
    using System.Runtime.CompilerServices;

    using RT      = Microsoft.Zelig.Runtime;
    using Chipset = Microsoft.DeviceModels.Chipset.CortexM3.Drivers; 

    public sealed class ContextSwitchTimer : Chipset.ContextSwitchTimer
    {
        protected override uint GetTicksForQuantumValue( uint ms )
        {
            // DISCO_F746NG uses the Core clock (216Mhz) for SysTick 
            return (uint)( RT.Configuration.CoreClockFrequency / 1000 ) * ms; 
        }
    }
}
