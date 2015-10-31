﻿//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

using System.Collections.ObjectModel;
using Microsoft.Zelig.Debugging;

namespace Microsoft.Zelig.LLVM
{
    using System;
    using System.Diagnostics;
    using System.Collections.Generic;
    using IR = Microsoft.Zelig.CodeGeneration.IR;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;
    using System.Linq;

    public partial class LLVMModuleManager
    {
        public void ConvertTypeLayoutsToLLVM( )
        {
            // LT72NOTE: are we thread safe??
            if( !m_typeSystemAlreadyConverted )
            {
                foreach( TS.TypeRepresentation type in m_typeSystem.Types )
                {
                    GetOrInsertType( type );
                }

                m_typeSystemAlreadyConverted = true;
            }
        }

        public LLVM._Type GetOrInsertBasicTypeAsLLVMSingleValueType( TS.TypeRepresentation tr )
        {
            if( tr is TS.EnumerationTypeRepresentation )
            {
                tr = tr.UnderlyingType;
            }

            if( tr is TS.PointerTypeRepresentation )
            {
                tr = m_typeSystem.WellKnownTypes.System_IntPtr;
            }

            if( tr is TS.ValueTypeRepresentation && !( tr is TS.ScalarTypeRepresentation ) )
            {
                tr = m_typeSystem.WellKnownTypes.System_IntPtr;
            }

            bool isValueType = ( tr is TS.ValueTypeRepresentation );
            return m_module.GetOrInsertType( tr.FullName, ( int )tr.Size * 8, isValueType );
        }

        public LLVM._Type GetOrInsertInlineType( TS.TypeRepresentation tr )
        {
            _Type type = GetOrInsertType( tr );
            if( type.IsPointer &&
                ( type.UnderlyingType != null ) &&
                !type.UnderlyingType.IsValueType )
            {
                type = type.UnderlyingType;
            }

            return type;
        }

        public LLVM._Type GetOrInsertType( TS.TypeRepresentation tr )
        {
            TS.WellKnownFields wkf = m_typeSystem.WellKnownFields;
            TS.WellKnownTypes wkt = m_typeSystem.WellKnownTypes;

            //
            // delayed types do not participate in layout
            //
            if( tr is TS.DelayedMethodParameterTypeRepresentation ||
                tr is TS.DelayedTypeParameterTypeRepresentation )
            {
                return null;
            }

            if( tr is TS.PointerTypeRepresentation )
            {
                TS.TypeRepresentation underlying = tr.UnderlyingType;

                if( underlying is TS.DelayedMethodParameterTypeRepresentation ||
                    underlying is TS.DelayedTypeParameterTypeRepresentation )
                {
                    return null;
                }
            }

            if( tr is TS.ArrayReferenceTypeRepresentation )
            {
                TS.TypeRepresentation contained = tr.ContainedType;

                if( contained is TS.DelayedMethodParameterTypeRepresentation ||
                    contained is TS.DelayedTypeParameterTypeRepresentation )
                {
                    return null;
                }
            }

            if( !tr.ValidLayout )
            {
                // only unresolved generics get here
                return null;
            }

            if( tr is TS.PinnedPointerTypeRepresentation || tr is TS.EnumerationTypeRepresentation )
            {
                tr = tr.UnderlyingType;
            }

            // Try to get the cached type before we create a new one.
            _Type cachedType;
            if( m_typeRepresentationsToType.TryGetValue( tr, out cachedType ) )
            {
                return cachedType;
            }

            // Remap scalar basic types to native types.
            if( tr == wkt.System_Boolean ||
                tr == wkt.System_Char ||
                tr == wkt.System_SByte ||
                tr == wkt.System_Byte ||
                tr == wkt.System_Int16 ||
                tr == wkt.System_UInt16 ||
                tr == wkt.System_Int32 ||
                tr == wkt.System_UInt32 ||
                tr == wkt.System_Int64 ||
                tr == wkt.System_UInt64 ||
                tr == wkt.System_Single ||
                tr == wkt.System_Double ||
                tr == wkt.System_IntPtr ||
                tr == wkt.System_UIntPtr )
            {
                m_typeRepresentationsToType[ tr ] = GetOrInsertBasicTypeAsLLVMSingleValueType( tr );
                return m_typeRepresentationsToType[ tr ];
            }

            string typeName = tr.FullName;

            // Pointer and Boxed type representation 
            if( tr is TS.PointerTypeRepresentation )
            {
                if( tr.UnderlyingType == wkt.System_Void )
                {
                    // Special case: we remap void * to an IntPtr to allow LLVM to function.
                    return GetOrInsertType( wkt.System_IntPtr );
                }

                _Type underlyingType = GetOrInsertType( tr.UnderlyingType );

                if (underlyingType == null)
                {
                    return null;
                }

                m_typeRepresentationsToType[ tr ] = m_module.GetOrInsertPointerType( typeName, underlyingType );
                return m_typeRepresentationsToType[ tr ];
            }
            else if( tr is TS.BoxedValueTypeRepresentation )
            {
                _Type objectType = GetOrInsertType( wkt.System_Object ).UnderlyingType;
                _Type underlyingType = GetOrInsertType( tr.UnderlyingType );
                _Type boxedType = m_module.GetOrInsertBoxedType( typeName, objectType, underlyingType );
                m_typeRepresentationsToType[ tr ] = m_module.GetOrInsertPointerType( boxedType );
                return m_typeRepresentationsToType[ tr ];
            }

            // All other types
            bool isValueType = (tr is TS.ValueTypeRepresentation);
            LLVM._Type llvmType = m_module.GetOrInsertType( typeName, ( int )tr.Size * 8, isValueType );

            // Ensure that we always return the correct type for storage. If this is a value type, then we're done.
            // Otherwise, return the type as a pointer.
            if( llvmType.IsValueType )
            {
                m_typeRepresentationsToType[ tr ] = llvmType;
            }
            else
            {
                m_typeRepresentationsToType[ tr ] = m_module.GetOrInsertPointerType( llvmType );
            }

            // Inline the parent class for object types. We will represent unboxed Value types as flat, c++ style, types
            // because they are 'inlined' in the object layout and their fields offset are based on such layout. We also
            // exempt ObjectHeader as a special case, since it can't inherit anythnig.
            if( ( tr.Extends != null ) &&
                ( tr.Extends != wkt.System_ValueType ) &&
                ( tr != wkt.Microsoft_Zelig_Runtime_ObjectHeader ) )
            {
                _Type parentType = GetOrInsertType( tr.Extends );
                llvmType.AddField( 0, parentType.UnderlyingType, ".extends" );
            }

            // Special case: System.Object always gets an ObjectHeader.
            if( tr == wkt.System_Object )
            {
                _Type headerType = GetOrInsertType( m_typeSystem.WellKnownTypes.Microsoft_Zelig_Runtime_ObjectHeader );
                llvmType.AddField( 0, headerType.UnderlyingType, "object_header" );
            }

            foreach( var field in tr.Fields )
            {
                // static fields are not part of object layout
                if( field is TS.StaticFieldRepresentation )
                {
                    continue;
                }

                if( !field.ValidLayout )
                {
                    continue;
                }

                // Strings are implemented inside the type
                if( field == wkf.StringImpl_FirstChar )
                {
                    _Type arrayType = m_module.GetOrInsertZeroSizedArray( GetOrInsertType( wkt.System_Char ) );
                    llvmType.AddField( ( uint )field.Offset, arrayType, field.Name );

                    continue;
                }

                llvmType.AddField( ( uint )field.Offset, GetOrInsertType( field.FieldType ), field.Name );
            }

            if( tr is TS.ArrayReferenceTypeRepresentation )
            {
                _Type arrayType = m_module.GetOrInsertZeroSizedArray( GetOrInsertType( tr.ContainedType ) );
                llvmType.AddField( wkt.System_Array.Size, arrayType, "array_data" );
            }

            llvmType.SetupFields( );

            return m_typeRepresentationsToType[ tr ];
        }

        public _Type GetOrInsertType( TS.MethodRepresentation mr )
        {
            var args = new List<_Type>( );

            foreach( var param in mr.ThisPlusArguments )
            {
                args.Add( GetOrInsertType( param ) );
            }

            if( mr is TS.StaticMethodRepresentation )
                args.RemoveAt( 0 );

            _Type retType = GetOrInsertType( mr.ReturnType );

            if( retType == null )
            {
                retType = m_module.GetVoidType( );
            }

            string sigName;
            if( mr.Flags.HasFlag( TS.MethodRepresentation.Attributes.PinvokeImpl ) )
                sigName = mr.Name;
            else
                sigName = mr.ToShortString( );

            return m_module.GetOrInsertFunctionType( sigName, retType, args );
        }

        public DebugInfo GetDebugInfoFor( TS.MethodRepresentation method )
        {
            if( method.DebugInfo == null )
            {
                var cfg = IR.TypeSystemForCodeTransformation.GetCodeForMethod( method );
                if( cfg == null )
                    return m_dummyDebugInfo;

                method.DebugInfo = ( from op in cfg.DataFlow_SpanningTree_Operators
                                      where op.DebugInfo != null
                                      select op.DebugInfo
                                   ).FirstOrDefault( );

                if( method.DebugInfo == null )
                    method.DebugInfo = m_dummyDebugInfo;
            }

            return method.DebugInfo;
        }
    }
}