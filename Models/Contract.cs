using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace Chinhsachso.Models
{
    public class Contract
    {
        public int Id { get; set; }

        [Required]
        public string? FileName { get; set; }

        [Required]
        public string? FilePath { get; set; }

        public string? OCRText { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public string? CreatedBy { get; set; }

        public int? MetadataId { get; set; }
        public Metadata? Metadata { get; set; }

        public string? ContractName { get; set; }

        [NotMapped]
        public IFormFile? File { get; set; }
    }
}
