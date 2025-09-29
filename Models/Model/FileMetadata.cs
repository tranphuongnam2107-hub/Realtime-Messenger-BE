using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Model
{
    public class FileMetadata
    {
        public string? FileName { get; set; }     
        public string? FileUrl { get; set; }     
        public long Size { get; set; }         
        public string? ContentType { get; set; }
    }
}
