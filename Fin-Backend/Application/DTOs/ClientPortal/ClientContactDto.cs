namespace FinTech.Application.DTOs.ClientPortal
{
    public class ClientContactDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Relationship { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
    }

    public class ClientContactCreateDto
    {
        public string Name { get; set; }
        public string Relationship { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
    }

    public class ClientContactUpdateDto
    {
        public string Name { get; set; }
        public string Relationship { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
    }
}
