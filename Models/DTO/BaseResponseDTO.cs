using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTO
{
    public class BaseResponseDTO<T>
    {
        public int Status { get; set; }
        public string? Message { get; set; }
        public T? DataRes { get; set; }
        public List<string>? Errors { get; set; }

        public BaseResponseDTO(int status, string? message, T? dataRes, List<string>? errors)
        {
            Status = status;
            Message = message;
            DataRes = dataRes;
            Errors = errors;
        }

        public static BaseResponseDTO<T> Success(string message, T? data, int statusCode)
        {
            return new BaseResponseDTO<T>(statusCode, message, data, null);
        }

        public static BaseResponseDTO<T> Fail(string message, T? dataRes , List<string>? errors, int statusCode)
        {
            return new BaseResponseDTO<T>(statusCode, message, dataRes, errors);
        }
    }
}
