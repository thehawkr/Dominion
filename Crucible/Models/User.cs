namespace Crucible.Models
{
    public class User
    {
        public int Id { get; set; }
        public int HostId { get; set; }
        public string Username { get; set; }
        public string Domain { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
