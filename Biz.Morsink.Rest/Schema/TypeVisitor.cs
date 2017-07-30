using System.Linq;

namespace Biz.Morsink.Rest.Schema
{
    public abstract class TypeVisitor<R>
    {
        protected TypeVisitor() { }
        public R Visit(TypeDescriptor t)
        {
            var p = t as TypeDescriptor.Primitive;
            if (p != null)
            {
                var s = p as TypeDescriptor.Primitive.String;
                if (s != null)
                    return VisitString(s);
                var n = (TypeDescriptor.Primitive.Numeric)p;

                var dt = p as TypeDescriptor.Primitive.DateTime;
                if (dt != null)
                    return VisitDateTime(dt);

                var i = n as TypeDescriptor.Primitive.Numeric.Integral;
                if (i != null)
                    return VisitIntegral(i);
                var f = (TypeDescriptor.Primitive.Numeric.Float)n;
                return VisitFloat(t);
            }
            var a = t as TypeDescriptor.Array;
            if (a != null)
                return PrevisitArray(a);
            var r = t as TypeDescriptor.Record;
            if (r != null)
                return PrevisitRecord(r);
            var nl = t as TypeDescriptor.Null;
            if (nl != null)
                return VisitNull(nl);
            var v = t as TypeDescriptor.Value;
            if (v != null)
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
