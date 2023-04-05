using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Crucible.Models;

namespace YourNamespace
{
    public class AppDbContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public AppDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public DbSet<Crucible.Models.Host> Hosts { get; set; }
        public DbSet<Network> Networks { get; set; }
        public DbSet<Interface> Interfaces { get; set; }
        public DbSet<TransportProtocol> TransportProtocols { get; set; }
        public DbSet<ApplicationProtocol> ApplicationProtocols { get; set; }
        public DbSet<Protocol> Protocols { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Vulnerability> Vulnerabilities { get; set; }
        public DbSet<Writeup> Writeups { get; set; }
        public DbSet<WriteupVulnerabilityMapping> WriteupVulnerabilityMappings { get; set; }
        public DbSet<ServiceVulnerabilityMapping> ServiceVulnerabilityMappings { get; set; }
        public DbSet<Website> Websites { get; set; }
        public DbSet<Webpage> Webpages { get; set; }
        public DbSet<WebpageVulnerabilityMapping> WebpageVulnerabilityMappings { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupUserMapping> GroupUserMappings { get; set; }
        public DbSet<Operator> Operators { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_configuration.GetConnectionString("DefaultConnection"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WriteupVulnerabilityMapping>().HasKey(wvm => new { wvm.WriteupId, wvm.VulnerabilityId });
            modelBuilder.Entity<ServiceVulnerabilityMapping>().HasKey(svm => new { svm.ServiceId, svm.VulnerabilityId });
            modelBuilder.Entity<GroupUserMapping>().HasKey(gum => new { gum.GroupId, gum.UserId });
        }
    }
}
