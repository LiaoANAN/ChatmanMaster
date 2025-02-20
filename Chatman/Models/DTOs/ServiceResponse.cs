using System.Text.Json.Serialization;

namespace Chatman.Models.DTOs
{
    public class ServiceResponse<T>
    {
        public T Data { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public Dictionary<string, string[]> Errors { get; set; }

        [JsonIgnore]
        public int StatusCode { get; set; }

        // 成功響應 - 帶數據
        public static ServiceResponse<T> ExcuteSuccess(T data, string message = "操作成功")
        {
            return new ServiceResponse<T>
            {
                Data = data,
                Success = true,
                Message = message,
                StatusCode = 200
            };
        }

        // 成功響應 - 不帶數據
        public static ServiceResponse<T> ExcuteSuccess(string message = "操作成功")
        {
            return new ServiceResponse<T>
            {
                Success = true,
                Message = message,
                StatusCode = 200
            };
        }

        // 錯誤響應
        public static ServiceResponse<T> ExcuteError(string message = "操作失敗", int statusCode = 400)
        {
            return new ServiceResponse<T>
            {
                Success = false,
                Message = message,
                StatusCode = statusCode
            };
        }

        // 錯誤響應 - 帶詳細錯誤信息
        public static ServiceResponse<T> ExcuteError(Dictionary<string, string[]> errors, string message = "操作失敗", int statusCode = 400)
        {
            return new ServiceResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors,
                StatusCode = statusCode
            };
        }

        // 驗證錯誤響應
        public static ServiceResponse<T> ValidationError(Dictionary<string, string[]> errors)
        {
            return new ServiceResponse<T>
            {
                Success = false,
                Message = "驗證失敗",
                Errors = errors,
                StatusCode = 422
            };
        }

        // 未找到響應
        public static ServiceResponse<T> NotFound(string message = "未找到資源")
        {
            return new ServiceResponse<T>
            {
                Success = false,
                Message = message,
                StatusCode = 404
            };
        }

        // 未授權響應
        public static ServiceResponse<T> Unauthorized(string message = "未授權的訪問")
        {
            return new ServiceResponse<T>
            {
                Success = false,
                Message = message,
                StatusCode = 401
            };
        }

        // 禁止訪問響應
        public static ServiceResponse<T> Forbidden(string message = "禁止訪問")
        {
            return new ServiceResponse<T>
            {
                Success = false,
                Message = message,
                StatusCode = 403
            };
        }

        // 伺服器錯誤響應
        public static ServiceResponse<T> ServerError(string message = "伺服器內部錯誤")
        {
            return new ServiceResponse<T>
            {
                Success = false,
                Message = message,
                StatusCode = 500
            };
        }
    }
}
