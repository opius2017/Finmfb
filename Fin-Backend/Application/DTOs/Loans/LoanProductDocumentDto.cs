public class LoanProductDocumentDto
{
    public string? Id { get; set; }
    public string? LoanProductId { get; set; }
    public string? DocumentType { get; set; }
    public string? DocumentName { get; set; }
    public string? Description { get; set; }
    public bool IsRequired { get; set; }
    public string? ApplicableFor { get; set; }
}
