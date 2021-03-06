// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using Microsoft.Internal;

namespace System.ComponentModel.Composition.ReflectionModel
{
    internal sealed class ReflectionField : ReflectionWritableMember
    {
        private readonly FieldInfo _field;

        public ReflectionField(FieldInfo field)
        {
            ArgumentNullException.ThrowIfNull(field);

            _field = field;
        }

        public FieldInfo UndelyingField
        {
            get { return _field; }
        }

        public override MemberInfo UnderlyingMember
        {
            get { return UndelyingField; }
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return !UndelyingField.IsInitOnly; }
        }

        public override bool RequiresInstance
        {
            get { return !UndelyingField.IsStatic; }
        }

        public override Type ReturnType
        {
            get { return UndelyingField.FieldType; }
        }

        public override ReflectionItemType ItemType
        {
            get { return ReflectionItemType.Field; }
        }

        public override object? GetValue(object? instance)
        {
            return UndelyingField.SafeGetValue(instance);
        }

        public override void SetValue(object? instance, object? value)
        {
            UndelyingField.SafeSetValue(instance, value);
        }
    }
}
