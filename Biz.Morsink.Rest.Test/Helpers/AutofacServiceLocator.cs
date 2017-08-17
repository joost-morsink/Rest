using Autofac;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Test.Helpers
{
    public class AutofacServiceLocator : IServiceLocator
    {
        private readonly ILifetimeScope scope;

        public AutofacServiceLocator(ILifetimeScope scope)
        {
            this.scope = scope;
        }

        public object ResolveOptional(Type t)
            => scope.ResolveOptional(t);

        public object ResolveRequired(Type t)
            => scope.Resolve(t);

        public IEnumerable<object> ResolveMulti(Type t)
            => scope.Resolve(typeof(IEnumerable<>).MakeGenericType(t)) as IEnumerable<object>;
    }
}
