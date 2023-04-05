namespace Crucible.Models
{
    public class Protocol
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int TransportProtocolId { get; set; }
        public int ApplicationProtocolId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
