namespace Crucible.Models
{
    public class Website
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public int ServiceId { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
