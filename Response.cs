using System.Net;

namespace ApiCore
{
    public class Validation
    {
        public string Error { get; set; }
        public Validation(string error = null)
        {
            Error = error;
        }
        public bool Success
        {
            get { return Error == null; }
        }
        public bool Fail
        {
            get { return Error != null; }
        }
    }
    public class Validation<T> : Validation
    {
        public T Body { get; }
        public Validation(T body = default(T), string error = null) : base(error)
        {
            Body = body;
        }
    }

    public class Response : Validation
    {
        public HttpStatusCode HttpCode { get; }
        public Response(HttpStatusCode code, string error = null) : base(error)
        {
            HttpCode = code;
        }
    }
    public class Response<T> : Validation<T>
    {
        public HttpStatusCode HttpCode { get; }
        public Response(HttpStatusCode code, T body = default(T), string error = null) : base(body, error)
        {
            HttpCode = code;
        }
    }
}
