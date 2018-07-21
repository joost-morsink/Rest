using Biz.Morsink.Rest.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Representation class for exceptions.
    /// </summary>
    public class ExceptionRepresentation : ITypeRepresentation
    {
        /// <summary>
        /// Representation of Exceptions works one way only.
        /// This method throws a NotSupportedException.
        /// </summary>
        public object GetRepresentable(object rep)
        {
            throw new NotSupportedException();
        }
        public Type GetRepresentableType(Type type)
            => IsRepresentation(type) ? typeof(Exception) : null;

        public object GetRepresentation(object obj)
            => ExceptionInfo.Create((Exception)obj);

        public Type GetRepresentationType(Type type)
            => IsRepresentable(type) ? typeof(ExceptionInfo) : null;

        public bool IsRepresentable(Type type)
            => typeof(Exception).IsAssignableFrom(type);

        public bool IsRepresentation(Type type)
            => typeof(ExceptionInfo).IsAssignableFrom(type);
    }
}
