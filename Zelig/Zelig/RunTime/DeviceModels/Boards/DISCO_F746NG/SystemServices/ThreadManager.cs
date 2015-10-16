//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.DISCO_F746NG
{
    using RT      = Microsoft.Zelig.Runtime;
    using Chipset = Microsoft.CortexM4OnMBED;


    public sealed class ThreadManager : Chipset.ThreadManager
    {
        private const int DefaultStackSizeDISCO_F746NG = (4 * 1024) / sizeof( uint );

        //--//

        //
        // Helper Methods
        //

        public override int DefaultStackSize
        {
            get
            {
                return DefaultStackSizeDISCO_F746NG;
            }
        }
    }
}

