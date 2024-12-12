namespace backend_3_module.Services;



public class APIResponse<T>
{
    public bool IsSuccess { get; set; }
    public T? Result { get; set; }
    public string? ErrorMessage { get; set; }
    public int StatusCode { get; set; }
}