using System;

namespace Chinhsachso.Models
{
    public class Metadata
    {
        public int Id { get; set; }
        public string? ContractNumber { get; set; }
        public string? Type { get; set; }
        public DateTime? SignedDate { get; set; }
        public string? PartnerName { get; set; }
    }
}
