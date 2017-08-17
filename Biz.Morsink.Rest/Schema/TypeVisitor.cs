using System.Linq;

namespace Biz.Morsink.Rest.Schema
{
    public abstract class TypeVisitor<R>
    {
        protected TypeVisitor() { }
        public R Visit(TypeDescriptor t)
        {
            if (t is TypeDescriptor.Primitive p)
            {
                if (p is TypeDescriptor.Primitive.String s)
                    return VisitString(s);
                if (p is TypeDescriptor.Primitive.DateTime dt)
                    return VisitDateTime(dt);

                var n = (TypeDescriptor.Primitive.Numeric)p;
                if (n is TypeDescriptor.Primitive.Numeric.Integral i)
                    return VisitIntegral(i);

                var f = (TypeDescriptor.Primitive.Numeric.Float)n;
                return VisitFloat(t);
            }
            if (t is TypeDescriptor.Array a)
                return PrevisitArray(a);
            if (t is TypeDescriptor.Record r)
                return PrevisitRecord(r);
            if (t is TypeDescriptor.Null nl)
                return VisitNull(nl);
            if (t is TypeDescriptor.Value v )
                return PrevisitValue(v);
            var u = (TypeDescriptor.Union)t;
            return PrevisitUnion(u);
        }
        protected virtual R PrevisitUnion(TypeDescriptor.Union u)
        {
            var options = u.Options.Select(Visit).ToArray();
            return VisitUnion(u, options);
        }
        protected virtual R PrevisitRecord(TypeDescriptor.Record r)
        {
            var props = r.Properties.Select(p => p.Value.Select(Visit)).ToArray();
            return VisitRecord(r, props);
        }
        protected virtual R PrevisitArray(TypeDescriptor.Array a)
        {
            var inner = Visit(a.BaseType);
            return VisitArray(a, inner);
        }
        protected virtual R PrevisitValue(TypeDescriptor.Value v)
        {
            var inner = Visit(v.BaseType);
            return VisitValue(v, inner);
        }

        protected abstract R VisitNull(TypeDescriptor.Null n);
        protected abstract R VisitValue(TypeDescriptor.Value v, R inner);
        protected abstract R VisitString(TypeDescriptor.Primitive.String s);
        protected abstract R VisitDateTime(TypeDescriptor.Primitive.DateTime dt);
        protected abstract R VisitFloat(TypeDescriptor t);
        protected abstract R VisitIntegral(TypeDescriptor.Primitive.Numeric.Integral i);
        protected abstract R VisitArray(TypeDescriptor.Array a, R inner);
        protected abstract R VisitRecord(TypeDescriptor.Record r, PropertyDescriptor<R>[] props);
        protected abstract R VisitUnion(TypeDescriptor.Union u, R[] options);
    }
}
