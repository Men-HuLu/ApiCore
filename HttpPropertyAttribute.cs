using System;

namespace ApiCore
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class HttpPropertyAttribute : Attribute
    {
        public HttpMember Member { get; }
        public HttpPropertyAttribute(HttpMember member)
        {
            Member = member;
        }
    }
}
