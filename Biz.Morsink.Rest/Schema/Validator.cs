using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.Schema
{
    public struct Validator
    {
        private readonly TypeDescriptor typeDescriptor;
        private readonly Validators validators;

        public Validator(TypeDescriptor typeDescriptor, TypeDescriptorValidatorCreator validatorCreator)
        {
            this.typeDescriptor = typeDescriptor;
            this.validators = new Validators(validatorCreator);
        }
        private Validator(TypeDescriptor typeDescriptor, Validators validators)
        {
            this.typeDescriptor = typeDescriptor;
            this.validators = validators;
        }
        public bool IsValid()
            => typeDescriptor != null && validators.ValidatorVisitor.Visit(typeDescriptor);
        public Validator ConsumeNull()
            => typeDescriptor == null ? this
            : new Validator(validators.ConsumeNullVisitor.Visit(typeDescriptor), validators);
        public Validator ConsumeObject(object o)
        {
            o = validators.GetRepresentation(o);
            var type = o?.GetType();
            if (o == null)
                return ConsumeNull();
            else if (type.IsPrimitive || type == typeof(string) || type == typeof(DateTime))
                return ConsumeValue(o);
            else if (o is IDictionary dict)
            {
                var res = this;
                foreach (string key in dict.Keys)
                    res = res.ConsumeProperty(key, dict[key]);
                return res;
            }
            else
                throw new NotImplementedException();
        }
        public Validator ConsumeValue(object o)
            => new Validator(validators.ConsumeValueVisitor.Visit(typeDescriptor, o), validators);
        public Validator ConsumeProperty(string prop, object value)
            => throw new NotImplementedException();
        private class Validators
        {
            private readonly TypeDescriptorValidatorCreator creator;
            public Validators(TypeDescriptorValidatorCreator creator)
            {
                this.creator = creator;
                ValidatorVisitor = ValidatorVisitor.Instance;
                ConsumeNullVisitor = new ConsumeNullVisitor(creator.TypeDescriptorCreator);
                ConsumeValueVisitor = new ConsumeValueVisitor(creator.TypeDescriptorCreator);
                ConsumePropertyVisitor = new ConsumePropertyVisitor(creator.TypeDescriptorCreator);
            }
            public Type GetRepresentationType(Type type)
                => creator.GetRepresentationType(type);
            public object GetRepresentation(object item)
                => creator.GetRepresentation(item);
            public ValidatorVisitor ValidatorVisitor { get; }
            public ConsumeNullVisitor ConsumeNullVisitor { get; }
            public ConsumeValueVisitor ConsumeValueVisitor { get; }
            public ConsumePropertyVisitor ConsumePropertyVisitor { get; }

            public Validators Copy()
                => new Validators(creator);
        }
        private class ValidatorVisitor : TypeDescriptorVisitor<bool>
        {
            public static ValidatorVisitor Instance { get; } = new ValidatorVisitor();
            private ValidatorVisitor() { }
            protected override bool VisitAny(TypeDescriptor.Any a)
                => true;

            protected override bool VisitArray(TypeDescriptor.Array a, bool inner)
                => inner;

            protected override bool VisitBoolean(TypeDescriptor.Primitive.Boolean b)
                => false;

            protected override bool VisitDateTime(TypeDescriptor.Primitive.DateTime dt)
                => false;

            protected override bool VisitDictionary(TypeDescriptor.Dictionary d, bool valueType)
                => true;

            protected override bool VisitFloat(TypeDescriptor.Primitive.Numeric.Float f)
                => false;

            protected override bool VisitIntegral(TypeDescriptor.Primitive.Numeric.Integral i)
                => false;

            protected override bool VisitIntersection(TypeDescriptor.Intersection i, bool[] parts)
                => parts.All(x => x);

            protected override bool VisitNull(TypeDescriptor.Null n)
                => false;

            protected override bool VisitRecord(TypeDescriptor.Record r, PropertyDescriptor<bool>[] props)
                => props.All(p => !p.Required);

            protected override bool VisitReferable(TypeDescriptor.Referable r, bool expandedDescriptor)
                => expandedDescriptor;

            protected override bool VisitReference(TypeDescriptor.Reference r)
                => false;

            protected override bool VisitString(TypeDescriptor.Primitive.String s)
                => false;

            protected override bool VisitUnion(TypeDescriptor.Union u, bool[] options)
                => options.Any(x => x);

            protected override bool VisitValue(TypeDescriptor.Value v, bool inner)
                => false;
        }
        private class ConsumeNullVisitor : TypeDescriptorVisitor<TypeDescriptor>
        {
            private readonly TypeDescriptorCreator typeDescriptorCreator;

            public ConsumeNullVisitor(TypeDescriptorCreator typeDescriptorCreator)
            {
                this.typeDescriptorCreator = typeDescriptorCreator;
            }

            protected override TypeDescriptor VisitAny(TypeDescriptor.Any a)
                => a;

            protected override TypeDescriptor VisitArray(TypeDescriptor.Array a, TypeDescriptor inner)
                => null;

            protected override TypeDescriptor VisitBoolean(TypeDescriptor.Primitive.Boolean b)
                => null;

            protected override TypeDescriptor VisitDateTime(TypeDescriptor.Primitive.DateTime dt)
                => null;

            protected override TypeDescriptor VisitDictionary(TypeDescriptor.Dictionary d, TypeDescriptor valueType)
                => null;

            protected override TypeDescriptor VisitFloat(TypeDescriptor.Primitive.Numeric.Float f)
                => null;

            protected override TypeDescriptor VisitIntegral(TypeDescriptor.Primitive.Numeric.Integral i)
                => null;

            protected override TypeDescriptor VisitIntersection(TypeDescriptor.Intersection i, TypeDescriptor[] parts)
            {
                if (parts.Any(p => p == null))
                    return null;
                else
                    return TypeDescriptor.MakeIntersection(i.Name, parts, null);
            }

            protected override TypeDescriptor VisitNull(TypeDescriptor.Null n)
            {
                return TypeDescriptor.Any.Instance;
            }

            protected override TypeDescriptor VisitRecord(TypeDescriptor.Record r, PropertyDescriptor<TypeDescriptor>[] props)
                => null;

            protected override TypeDescriptor VisitReferable(TypeDescriptor.Referable r, TypeDescriptor expandedDescriptor)
                => expandedDescriptor;

            protected override TypeDescriptor VisitReference(TypeDescriptor.Reference r)
            {
                var td = typeDescriptorCreator.GetDescriptorByName(r.RefName);
                return td == null ? null : Visit(td);
            }

            protected override TypeDescriptor VisitString(TypeDescriptor.Primitive.String s)
                => null;

            protected override TypeDescriptor VisitUnion(TypeDescriptor.Union u, TypeDescriptor[] options)
                => TypeDescriptor.MakeUnion(u.Name, options.Where(o => o != null), null);

            protected override TypeDescriptor VisitValue(TypeDescriptor.Value v, TypeDescriptor inner)
                => null;
        }
        private class ConsumeValueVisitor : TypeDescriptorVisitor<TypeDescriptor>
        {
            private readonly TypeDescriptorCreator typeDescriptorCreator;

            public ConsumeValueVisitor(TypeDescriptorCreator typeDescriptorCreator)
            {
                this.typeDescriptorCreator = typeDescriptorCreator;
            }
            public object Value { get; private set; }
            public TypeDescriptor Visit(TypeDescriptor typeDescriptor, object value)
            {
                Value = value;
                return Visit(typeDescriptor);
            }

            protected override TypeDescriptor VisitAny(TypeDescriptor.Any a)
                => a;

            protected override TypeDescriptor VisitArray(TypeDescriptor.Array a, TypeDescriptor inner)
                => null;

            protected override TypeDescriptor VisitBoolean(TypeDescriptor.Primitive.Boolean b)
                => Value is bool ? TypeDescriptor.Any.Instance : null;

            protected override TypeDescriptor VisitDateTime(TypeDescriptor.Primitive.DateTime dt)
                => Value is DateTime ? TypeDescriptor.Any.Instance : null;

            protected override TypeDescriptor VisitDictionary(TypeDescriptor.Dictionary d, TypeDescriptor valueType)
                => null;

            protected override TypeDescriptor VisitFloat(TypeDescriptor.Primitive.Numeric.Float f)
                => Value is float || Value is double || Value is decimal ? TypeDescriptor.Any.Instance : null;

            protected override TypeDescriptor VisitIntegral(TypeDescriptor.Primitive.Numeric.Integral i)
                => Value is long || Value is int || Value is short || Value is byte
                || Value is ulong || Value is uint || Value is ushort || Value is sbyte
                ? TypeDescriptor.Any.Instance : null;

            protected override TypeDescriptor VisitIntersection(TypeDescriptor.Intersection i, TypeDescriptor[] parts)
                => parts.All(x => x != null) ? TypeDescriptor.MakeIntersection(i.Name, parts, null) : null;

            protected override TypeDescriptor VisitNull(TypeDescriptor.Null n)
                => null;

            protected override TypeDescriptor VisitRecord(TypeDescriptor.Record r, PropertyDescriptor<TypeDescriptor>[] props)
                => null;

            protected override TypeDescriptor VisitReferable(TypeDescriptor.Referable r, TypeDescriptor expandedDescriptor)
                => expandedDescriptor;

            protected override TypeDescriptor VisitReference(TypeDescriptor.Reference r)
            {
                var td = typeDescriptorCreator.GetDescriptorByName(r.RefName);
                return td == null ? null : Visit(td);
            }

            protected override TypeDescriptor VisitString(TypeDescriptor.Primitive.String s)
                => Value is string ? TypeDescriptor.Any.Instance : null;

            protected override TypeDescriptor VisitUnion(TypeDescriptor.Union u, TypeDescriptor[] options)
                => options.Any(o => o != null) ? TypeDescriptor.MakeUnion(u.Name, options.Where(o => o != null), null);

            protected override TypeDescriptor VisitValue(TypeDescriptor.Value v, TypeDescriptor inner)
                => Value.Equals(v.InnerValue) ? TypeDescriptor.Any.Instance : null;
        }
        private class ConsumePropertyVisitor : TypeDescriptorVisitor<TypeDescriptor>
        {
            private readonly TypeDescriptorCreator creator;

            public ConsumePropertyVisitor(TypeDescriptorCreator creator)
            {
                this.creator = creator;
            }

            public string Key { get; private set; }
            public object Value { get; private set; }
            public Validators Validators { get; private set; }

            public TypeDescriptor Visit(TypeDescriptor typeDescriptor, Validators validators, string key, object value)
            {
                Key = key;
                Value = value;
                Validators = validators;
                return Visit(typeDescriptor);
            }

            protected override TypeDescriptor VisitAny(TypeDescriptor.Any a)
                => a;

            protected override TypeDescriptor VisitArray(TypeDescriptor.Array a, TypeDescriptor inner)
                => null;

            protected override TypeDescriptor VisitBoolean(TypeDescriptor.Primitive.Boolean b)
                => null;

            protected override TypeDescriptor VisitDateTime(TypeDescriptor.Primitive.DateTime dt)
                => null;

            protected override TypeDescriptor VisitDictionary(TypeDescriptor.Dictionary d, TypeDescriptor valueType)
                => new Validator(valueType, Validators.Copy()).ConsumeObject(Value).IsValid() ? d : null;

            protected override TypeDescriptor VisitFloat(TypeDescriptor.Primitive.Numeric.Float f)
                => null;

            protected override TypeDescriptor VisitIntegral(TypeDescriptor.Primitive.Numeric.Integral i)
                => null;
            protected override TypeDescriptor VisitIntersection(TypeDescriptor.Intersection i, TypeDescriptor[] parts)
                => parts.All(p => p != null) ? TypeDescriptor.MakeIntersection(i.Name, parts, null);

            protected override TypeDescriptor VisitNull(TypeDescriptor.Null n)
                => null;

            protected override TypeDescriptor PrevisitRecord(TypeDescriptor.Record r)
            {
                var prop = r.Properties.Where(p => p.Key == Key).FirstOrDefault();
                if (prop.Key == null)
                    return r;
                var isValid = new Validator(prop.Value.Type, Validators.Copy()).ConsumeObject(Value).IsValid();
                return isValid ? TypeDescriptor.MakeRecord(r.Name, r.Properties.Where(p => p.Key != Key).Select(p => p.Value), null) : null;
            }
            protected override TypeDescriptor VisitRecord(TypeDescriptor.Record r, PropertyDescriptor<TypeDescriptor>[] props)
            {
                return null;
            }

            protected override TypeDescriptor VisitReferable(TypeDescriptor.Referable r, TypeDescriptor expandedDescriptor)
                => expandedDescriptor;

            protected override TypeDescriptor VisitReference(TypeDescriptor.Reference r)
            {
                var td = creator.GetDescriptorByName(r.RefName);
                return td == null ? null : Visit(td);
            }

            protected override TypeDescriptor VisitString(TypeDescriptor.Primitive.String s)
                => null;

            protected override TypeDescriptor VisitUnion(TypeDescriptor.Union u, TypeDescriptor[] options)
                => options.All(o => o == null) ? null : TypeDescriptor.MakeUnion(u.Name, options.Where(o => o != null), null);

            protected override TypeDescriptor VisitValue(TypeDescriptor.Value v, TypeDescriptor inner)
                => null;

        }
    }
}
