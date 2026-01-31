namespace API_Service.Models.ResponseModel
{
    public class ResponseDetail
    {
        public bool Status { get; set; }
        public string Message { get; set; } = string.Empty;
    }
    public class ResponseDataDetail<T> : ResponseDetail
    {
        public required T Data { get; set; }
    }
}
