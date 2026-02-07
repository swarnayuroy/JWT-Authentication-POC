namespace API_Service.AppData
{
    public interface IService<T> where T : class
    {
        Task<IEnumerable<T>> Get();
        Task<bool> Save(T entity);
        Task<bool> Update(T entity);
        Task<bool> Delete(string id);
    }
}
