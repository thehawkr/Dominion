namespace Crucible.Models
{
    public class Interface
    {
        public int Id { get; set; }
        public int HostId { get; set; }
        public int NetworkId { get; set; }
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
