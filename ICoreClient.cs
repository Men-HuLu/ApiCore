using System;

namespace ApiCore
{
    public interface ICoreClient
    {
        Response Execute(IRequest req);
        Response<T> Execute<T>(IRequest<T> req);
        IAsyncResult BeginExecute(IRequest req, AsyncCallback callback);
    }
}