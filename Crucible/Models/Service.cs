namespace Crucible.Models
{
    public class Service
{
    public int Id { get; set; }
    public int HostId { get; set; }
    public int Port { get; set; }
    public int ProtocolId { get; set; }
    public string State { get; set; }
    public string Name { get; set; }
    public string Product { get; set; }
    public string Version { get; set; }
    public string ExtraInfo { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    }
}
