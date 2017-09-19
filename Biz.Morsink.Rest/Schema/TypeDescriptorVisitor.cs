﻿using System.Linq;

namespace Biz.Morsink.Rest.Schema
{
    /// <summary>
    /// This abstract base class implements the visitor pattern for TypeDescriptors.
    /// </summary>
    /// <typeparam name="R">The type of result the visitor should return.</typeparam>
    public abstract class TypeDescriptorVisitor<R>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        protected TypeDescriptorVisitor() { }
        /// <summary>
        /// Runs the visitor on a TypeDescriptor.
        /// </summary>
        /// <param name="t">A TypeDescriptor to 'visit'.</param>
        /// <returns>An object of type R.</returns>
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
                return VisitFloat(f);
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
        /// <summary>
        /// Previsit function for Union types. 
        /// Override if recursive processing is not needed.
        /// </summary>
        /// <param name="u">A Union TypeDescriptor.</param>
        /// <returns>An object of type R.</returns>
        protected virtual R PrevisitUnion(TypeDescriptor.Union u)
        {
            var options = u.Options.Select(Visit).ToArray();
            return VisitUnion(u, options);
        }
        /// <summary>
        /// Previsit function for Record types. 
        /// Override if recursive processing is not needed.
        /// </summary>
        /// <param name="r">A Record TypeDescriptor.</param>
        /// <returns>An object of type R.</returns>
        protected virtual R PrevisitRecord(TypeDescriptor.Record r)
        {
            var props = r.Properties.Select(p => p.Value.Select(Visit)).ToArray();
            return VisitRecord(r, props);
        }
        /// <summary>
        /// Previsit function for Array types. 
        /// Override if recursive processing is not needed.
        /// </summary>
        /// <param name="a">An Array TypeDescriptor.</param>
        /// <returns>An object of type R.</returns>
        protected virtual R PrevisitArray(TypeDescriptor.Array a)
        {
            var inner = Visit(a.ElementType);
            return VisitArray(a, inner);
        }
        /// <summary>
        /// Previsit function for Value types.. 
        /// Override if recursive processing is not needed.
        /// </summary>
        /// <param name="u">A Value TypeDescriptor.</param>
        /// <returns>An object of type R.</returns>
        protected virtual R PrevisitValue(TypeDescriptor.Value v)
        {
            var inner = Visit(v.BaseType);
            return VisitValue(v, inner);
        }

        /// <summary>
        /// Visits Null TypeDescriptors.
        /// </summary>
        /// <param name="n">A Null TypeDescriptor</param>
        /// <returns>An object of type R.</returns>
        protected abstract R VisitNull(TypeDescriptor.Null n);
        /// <summary>
        /// Visits Value TypeDescriptors.
        /// </summary>
        /// <param name="v">A Value TypeDescriptor.</param>
        /// <param name="inner">An already visited value for the inner type.</param>
        /// <returns>An object of type R.</returns>
        protected abstract R VisitValue(TypeDescriptor.Value v, R inner);
        /// <summary>
        /// Visits String TypeDescriptors.
        /// </summary>
        /// <param name="s">A String TypeDescriptor.</param>
        /// <returns>An object of type R.</returns>
        protected abstract R VisitString(TypeDescriptor.Primitive.String s);
        /// <summary>
        /// Visits DateTime TypeDescriptors.
        /// </summary>
        /// <param name="dt">A DateTimne TypeDescriptor.</param>
        /// <returns>An object of type R.</returns>
        protected abstract R VisitDateTime(TypeDescriptor.Primitive.DateTime dt);
        /// <summary>
        /// Visits Float TypeDescriptors.
        /// </summary>
        /// <param name="f">A Float TypeDescriptor.</param>
        /// <returns>An object of type R.</returns>
        protected abstract R VisitFloat(TypeDescriptor.Primitive.Numeric.Float f);
        /// <summary>
        /// Visits Integral TypeDescriptors.
        /// </summary>
        /// <param name="i">An Integral TypeDescriptor.</param>
        /// <returns>An object of type R.</returns>
        protected abstract R VisitIntegral(TypeDescriptor.Primitive.Numeric.Integral i);
        /// <summary>
        /// Visits Array TypeDescriptors.
        /// </summary>
        /// <param name="a">An Array TypeDescriptor.</param>
        /// <param name="inner">An already visited value for the element type of the array.</param>
        /// <returns>An object of type R.</returns>
        protected abstract R VisitArray(TypeDescriptor.Array a, R inner);
        /// <summary>
        /// Visits Record TypeDescriptors.
        /// </summary>
        /// <param name="r">A Record TypeDescriptor.</param>
        /// <param name="props">An array of already visited values for the property descriptors.</param>
        /// <returns>An object of type R.</returns>
        protected abstract R VisitRecord(TypeDescriptor.Record r, PropertyDescriptor<R>[] props);
        /// <summary>
        /// Visits Union TypeDescriptors.
        /// </summary>
        /// <param name="u">A Union TypeDescriptor.</param>
        /// <param name="options">An array of already visited values for the option descriptors.</param>
        /// <returns>An object of type R.</returns>
        protected abstract R VisitUnion(TypeDescriptor.Union u, R[] options);
    }
}