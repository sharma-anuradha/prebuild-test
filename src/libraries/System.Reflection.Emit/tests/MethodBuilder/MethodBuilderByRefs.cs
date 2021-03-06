// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using Xunit;

namespace System.Reflection.Emit.Tests
{
    public class MethodBuilderByRefs
    {
        [Fact]
        public void ByRef_Ldtoken()
        {
            TypeBuilder type = Helpers.DynamicType(TypeAttributes.Public);
            MethodBuilder method = type.DefineMethod("TestMethod", MethodAttributes.Public, typeof(Type), Type.EmptyTypes);
            ILGenerator ilg = method.GetILGenerator();
            ilg.Emit(OpCodes.Ldtoken, typeof(int).MakeByRefType());
            ilg.Emit(OpCodes.Ret);
        }
    }
}