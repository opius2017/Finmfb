using System;

public class CreateLoanCollectionActionDto
{
    public string? CollectionId { get; set; }
    public string? ActionType { get; set; }
    public string? Description { get; set; }
    public string? Result { get; set; }
    public DateTime? FollowUpDate { get; set; }
    public bool RequiresFollowUp { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactDetails { get; set; }
}
