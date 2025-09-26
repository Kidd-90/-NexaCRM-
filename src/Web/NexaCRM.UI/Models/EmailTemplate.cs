using System;
using System.Collections.Generic;

namespace NexaCRM.WebClient.Models
{
    public class EmailTemplate
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Subject { get; set; }
        public List<EmailBlock> Blocks { get; set; } = new();
    }

    public class EmailBlock
    {
        public string Type { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
