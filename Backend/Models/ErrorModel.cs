using BackendAPI.Entities.Enums;

namespace BackendAPI.Models
{
    public class ErrorModel
    {
        public ErrorType ErrorType { get; set; }
        public string Message { get; set; }
    }
}
