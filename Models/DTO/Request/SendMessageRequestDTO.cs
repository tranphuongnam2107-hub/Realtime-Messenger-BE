﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Models.DTO.Request
{
    public class SendMessageRequestDTO
    {
        public string? ChatId { get; set; }
        public string? Type { get; set; }
        public string? TextMessage { get; set; }
        public List<IFormFile>? Images { get; set; }
        public List<IFormFile>? Files { get; set; }
    }
}
