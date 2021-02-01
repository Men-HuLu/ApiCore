namespace ApiCore
{
    public interface IRequest
    {
        string GetPath();
        HttpMethod GetMethod();
    }

    public interface IRequest<T>: IRequest
    {
    }
}