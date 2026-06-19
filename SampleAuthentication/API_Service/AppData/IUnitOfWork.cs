namespace API_Service.AppData
{
    public interface IUnitOfWork
    {
        public Task<bool> ExecuteAndCommit(params Func<Task>[] operations);
    }
}
