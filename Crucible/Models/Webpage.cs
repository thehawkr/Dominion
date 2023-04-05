namespace Crucible.Models
{
    public class Webpage
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public int WebsiteId { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
