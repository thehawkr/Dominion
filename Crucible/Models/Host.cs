namespace Crucible.Models
{
    public class Host
    {
        public int Id { get; set; }
        public string Fqdn { get; set; }
        public string OperatingSystem { get; set; }
        public string Version { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
