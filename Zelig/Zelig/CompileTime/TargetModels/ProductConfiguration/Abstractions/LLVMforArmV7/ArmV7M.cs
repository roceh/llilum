﻿//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment.Abstractions.Architectures
{
    using Microsoft.Zelig.Runtime.TypeSystem;
    using Microsoft.Zelig.TargetModel.ArmProcessor;
    using System;
    using System.Collections.Generic;
    using ZeligIR = Microsoft.Zelig.CodeGeneration.IR;

    public sealed class ArmV7M : ArmPlatform
    {
        private const Capabilities c_ProcessorCapabilities = Capabilities.ARMv7M;
        private const String       c_PlatformName          = InstructionSetVersion.Platform_LLVM;

        //
        // State
        //

        //
        // Constructor Methods
        //

        public ArmV7M( ZeligIR.TypeSystemForCodeTransformation typeSystem, MemoryMapCategory memoryMap )
            : base( typeSystem, memoryMap, c_ProcessorCapabilities )
        {
        }

        public override void RegisterForNotifications( ZeligIR.TypeSystemForCodeTransformation ts,
                                                       ZeligIR.CompilationSteps.DelegationCache cache )
        {
            base.RegisterForNotifications( ts, cache );

            cache.Register( new ZeligIR.CompilationSteps.Handlers.SoftwareFloatingPoint() );

            cache.Register( new ArmV4.Optimizations( this ) );
        }

        public override ZeligIR.ImageBuilders.CompilationState CreateCompilationState( ZeligIR.ImageBuilders.Core core,
                                                                               ZeligIR.ControlFlowGraphStateForCodeTransformation cfg )
        {
            return new ArmV7MCompilationState( core, cfg );
        }

        //--//

        //
        // Access Methods
        //

        public override string PlatformName
        {
            get { return c_PlatformName; }
        }

        public override string PlatformVersion
        {
            get
            {
                return InstructionSetVersion.PlatformVersion_7M;
            }
        }

        public override string PlatformVFP
        {
            get
            {
                return InstructionSetVersion.PlatformVFP_SoftVFP;
            }
        }

        public override bool PlatformBigEndian
        {
            get { return false; }
        }        

        //--//

        public override TypeRepresentation GetMethodWrapperType( )
        {
            return m_typeSystem.GetWellKnownType( "Microsoft_Zelig_ARMv7_MethodWrapper" );
        }

        //
        // Not implememted, and used only during machine code emission
        //

        public override bool HasRegisterContextArgument( MethodRepresentation md )
        {
            //////if(md.ThisPlusArguments.Length > 1)
            //////{
            //////    TypeRepresentation td = md.ThisPlusArguments[1];

            //////    if(td is PointerTypeRepresentation)
            //////    {
            //////        td = td.UnderlyingType;

            //////        if(td == m_typeSystem.GetWellKnownType( "Microsoft_Zelig_ProcessorARMv7M_RegistersOnStack" ))
            //////        {
            //////            return true;
            //////        }
            //////    }
            //////}

            return false;
        }

        public override void ComputeSetOfRegistersToSave( ZeligIR.Abstractions.CallingConvention cc,
                                                                   ZeligIR.ControlFlowGraphStateForCodeTransformation cfg,
                                                                   BitVector modifiedRegisters,
                                                               out BitVector registersToSave,
                                                               out Runtime.HardwareException he )
        {
            throw new Exception( "ComputeSetOfRegistersToSave not implemented" );
        }

        //--//

        protected override void ComputeRegisterFlushFixup( BitVector registersToSave )
        {
            throw new Exception( "ComputeRegisterFlushFixup not implemented" );
        }
    }
}