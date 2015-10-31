﻿//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.DISCO_F746NG
{
    using RT            = Microsoft.Zelig.Runtime;
    using ChipsetModel  = Microsoft.CortexM4OnMBED;

    
    public sealed class Device : Microsoft.CortexM4OnMBED.Device
    {
        static readonly uint[] s_bootstrapStackDISCO_F746NG = new uint[ (4 * 1024) / sizeof( uint ) ]; 

        //
        // Access Methods
        //

        public override uint[] BootstrapStack
        {
            [RT.NoInline]
            get
            {
                return s_bootstrapStackDISCO_F746NG;
            }
        }

        public override uint ManagedHeapSize
        {
            get
            { 
                return 0x10000u;
            }
        }
    }
}
