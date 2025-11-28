using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
    public class RelationshipMapDto
    {
        public Guid CustomerId { get; set; }
        public List<RelationshipNodeDto> Nodes { get; set; } = new();
        public List<RelationshipEdgeDto> Edges { get; set; } = new();
    }

    public class RelationshipNodeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }

    public class RelationshipEdgeDto
    {
        public Guid SourceId { get; set; }
        public Guid TargetId { get; set; }
        public string RelationshipType { get; set; } = string.Empty;
    }
}
