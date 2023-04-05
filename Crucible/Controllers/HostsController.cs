using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Crucible.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HostsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public HostsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("upload-output-file")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadOutputFile(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("File not selected.");

                string filename = Path.GetFileName(file.FileName);
                string extension = Path.GetExtension(filename);

                if (extension != ".nmap" && extension != ".nessus" && extension != ".burpsuite")
                    return BadRequest("Invalid file format.");

                var lines = new List<string>();
                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    while (reader.Peek() >= 0)
                    {
                        lines.Add(await reader.ReadLineAsync());
                    }
                }

                var hosts = new List<Crucible.Models.Host>();
                var networks = new List<Network>();
                var interfaces = new List<Interface>();
                var transportProtocols = new List<TransportProtocol>();
                var applicationProtocols = new List<ApplicationProtocol>();
                var protocols = new List<Protocol>();
                var services = new List<Service>();
                var vulnerabilities = new List<Vulnerability>();

                if (extension == ".nmap")
                {
                    (hosts, networks, interfaces, transportProtocols, applicationProtocols, protocols, services) = ParseNmapOutput(lines);
                    vulnerabilities = await ScanVulnerabilities(hosts.Select(h => h.Fqdn).Distinct());
                }
                else if (extension == ".nessus")
                {
                    (hosts, networks, interfaces, transportProtocols, applicationProtocols, protocols, services) = ParseNessusOutput(lines);
                    vulnerabilities = ParseNessusVulnerabilities(lines);
                }
                else if (extension == ".burpsuite")
                {
                    (hosts, networks, interfaces, transportProtocols, applicationProtocols, protocols, services) = ParseBurpSuiteOutput(lines);
                    vulnerabilities = ParseBurpSuiteVulnerabilities(lines);
                }

                await SaveOutputDataToDatabase(hosts, networks, interfaces, transportProtocols, applicationProtocols, protocols, services, vulnerabilities);

                return Ok("Output file uploaded and parsed successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        private async Task SaveOutputDataToDatabase(List<Crucible.Models.Host> hosts, List<Network> networks, List<Interface> interfaces, List<TransportProtocol> transportProtocols, List<ApplicationProtocol> applicationProtocols, List<Protocol> protocols, List<Service> services, List<Vulnerability> vulnerabilities)
        {
            _context.AddRange(hosts);
            _context.AddRange(networks);
            _context.AddRange(interfaces);
            _context.AddRange(transportProtocols);
            _context.AddRange(applicationProtocols);
            _context.AddRange(protocols);
            _context.AddRange(services);
            _context.AddRange(vulnerabilities);

            await _context.SaveChangesAsync();
        }

        private async Task<List<Vulnerability>> ScanVulnerabilities(IEnumerable<string> hostNames)
        {
            var vulnerabilities = new List<Vulnerability>();

            foreach (var hostName in hostNames)
            {
                // Scan for vulnerabilities using Nessus or other tools here
                var response = await new HttpClient().GetAsync($"https://vulnscan.example.com/api/vulnerabilities?host={hostName}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var jObject = JObject.Parse(jsonResponse);
                    var results = jObject["results"];

                    foreach (var result in results)
                    {
                        var vulnerability = new Vulnerability
                        {
                            Name = result["name"].ToString(),
                            Summary = result["summary"].ToString(),
                            Description = result["description"].ToString(),
                            Solution = result["solution"].ToString(),
                            PluginId = int.Parse(result["plugin_id"].ToString()),
                            PluginOutput = result["plugin_output"].ToString(),
                            Severity = int.Parse(result["severity"].ToString()),
                            Reference = result["reference"].ToString(),
                        };
                        vulnerabilities.Add(vulnerability);
                    }
                }
            }

            return vulnerabilities;
        }

        private (List<Host>, List<Network>, List<Interface>, List<TransportProtocol>, List<ApplicationProtocol>, List<Protocol>, List<Service>) ParseNmapOutput(List<string> lines)
        {
            // Parse nmap output and return lists of hosts, networks, interfaces, protocols, and services
            // Implementation details left as exercise for the reader
        }

        private (List<Host>, List<Network>, List<Interface>, List<TransportProtocol>, List<ApplicationProtocol>, List<Protocol>, List<Service>) ParseNessusOutput(List<string> lines)
        {
            // Parse Nessus output and return lists of hosts, networks, interfaces, protocols, and services
            // Implementation details left as exercise for the reader
        }

        private (List<Host>, List<Network>, List<Interface>, List<TransportProtocol>, List<ApplicationProtocol>, List<Protocol>, List<Service>) ParseBurpSuiteOutput(List<string> lines)
        {
            // Parse Burp Suite output and return lists of hosts, networks, interfaces, protocols, and services
            // Implementation details left as exercise for the reader
        }

        private List<Vulnerability> ParseNessusVulnerabilities(List<string> lines)
        {
            // Parse Nessus output and return a list of vulnerabilities
            // Implementation details left as exercise for the reader
            return new List<Vulnerability>();
        }

        private List<Vulnerability> ParseBurpSuiteVulnerabilities(List<string> lines)
        {
            // Parse Burp Suite output and return a list of vulnerabilities
            // Implementation details left as exercise for the reader
            return new List<Vulnerability>();
        }

        [HttpGet]
        public async Task<IActionResult> GetHosts()
        {
            var hosts = await _context.Hosts.ToListAsync();
            return Ok(hosts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetHost(int id)
        {
            var host = await _context.Hosts.FindAsync(id);

            if (host == null)
            {
                return NotFound();
            }

            return Ok(host);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateHost(int id, Host host)
        {
            if (id != host.Id)
            {
                return BadRequest();
            }

            _context.Entry(host).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                if (!HostExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddHosts(List<IFormFile> files)
        {
            var vulnerabilities = new List<Vulnerability>();

            foreach (var file in files)
            {
                using var streamReader = new StreamReader(file.OpenReadStream());
                var lines = await streamReader.ReadToEndAsync();
                List<Host> hosts;
                List<Network> networks;
                List<Interface> interfaces;
                List<TransportProtocol> transportProtocols;
                List<ApplicationProtocol> applicationProtocols;
                List<Protocol> protocols;
                List<Service> services;

                if (file.FileName.EndsWith(".nmap"))
                {
                    (hosts, networks, interfaces, transportProtocols, applicationProtocols, protocols, services) = ParseNmapOutput(lines.Split(Environment.NewLine));
                    vulnerabilities.AddRange(await ScanVulnerabilities(hosts.Select(h => h.Fqdn)));
                }
                else if (file.FileName.EndsWith(".nessus"))
                {
                    (hosts, networks, interfaces, transportProtocols, applicationProtocols, protocols, services) = ParseNessusOutput(lines.Split(Environment.NewLine));
                    vulnerabilities.AddRange(ParseNessusVulnerabilities(lines.Split(Environment.NewLine)));
                }
                else if (file.FileName.EndsWith(".xml"))
                {
                    (hosts, networks, interfaces, transportProtocols, applicationProtocols, protocols, services) = ParseBurpSuiteOutput(lines.Split(Environment.NewLine));
                    vulnerabilities.AddRange(ParseBurpSuiteVulnerabilities(lines.Split(Environment.NewLine)));
                }
                else
                {
                    return BadRequest();
                }

                foreach (var network in networks)
                {
                    if (!_context.Networks.Any(n => n.Network == network.Network))
                    {
                        _context.Networks.Add(network);
                    }
                }
                await _context.SaveChangesAsync();

                foreach (var host in hosts)
                {
                    if (!_context.Hosts.Any(h => h.Fqdn == host.Fqdn))
                    {
                        _context.Hosts.Add(host);
                    }

                    var hostId = _context.Hosts.FirstOrDefault(h => h.Fqdn == host.Fqdn)?.Id;

                    foreach (var intf in interfaces.Where(i => i.HostId == hostId))
                    {
                        if (!_context.Interfaces.Any(i => i.HostId == hostId && i.NetworkId == intf.NetworkId && i.IpAddress == intf.IpAddress))
                        {
                            _context.Interfaces.Add(intf);
                        }
                    }

                    await _context.SaveChangesAsync();

                    foreach (var service in services.Where(s => s.HostId == hostId))
                    {
                        var transportProtocol = transportProtocols.FirstOrDefault(tp => tp.Name == service.Protocol.TransportProtocol.Name);
                        if (transportProtocol == null)
                        {
                            transportProtocol = service.Protocol.TransportProtocol;
                            _context.TransportProtocols.Add(transportProtocol);
                        }

                        var applicationProtocol = applicationProtocols.FirstOrDefault(ap => ap.Name == service.Protocol.ApplicationProtocol.Name);
                        if (applicationProtocol == null)
                        {
                            applicationProtocol = service.Protocol.ApplicationProtocol;
                            _context.ApplicationProtocols.Add(applicationProtocol);
                        }

                        var protocol = protocols.FirstOrDefault(p => p.Name == service.Protocol.Name);
                        if (protocol == null)
                        {
                            protocol = service.Protocol;
                            protocol.TransportProtocol = transportProtocol;
                            protocol.ApplicationProtocol = applicationProtocol;
                            _context.Protocols.Add(protocol);
                        }

                        if (!_context.Services.Any(s => s.HostId == hostId && s.Port == service.Port && s.ProtocolId == protocol.Id))
                        {
                            _context.Services {
                                if (!HostExists(id))
                                {
                                    return NotFound();
                                }
                                else
                                {
                                    throw;
                                }
                            }

                            return NoContent();
                        }

                        [HttpPost]
                        [Authorize(Roles = "Admin")]
                        public async Task<IActionResult> AddHosts(List<IFormFile> files)
                        {
                            var vulnerabilities = new List<Vulnerability>();

                            foreach (var file in files)
                            {
                                using var streamReader = new StreamReader(file.OpenReadStream());
                                var lines = await streamReader.ReadToEndAsync();
                                List<Host> hosts;
                                List<Network> networks;
                                List<Interface> interfaces;
                                List<TransportProtocol> transportProtocols;
                                List<ApplicationProtocol> applicationProtocols;
                                List<Protocol> protocols;
                                List<Service> services;

                                if (file.FileName.EndsWith(".nmap"))
                                {
                                    (hosts, networks, interfaces, transportProtocols, applicationProtocols, protocols, services) = ParseNmapOutput(lines.Split(Environment.NewLine));
                                    vulnerabilities.AddRange(await ScanVulnerabilities(hosts.Select(h => h.Fqdn)));
                                }
                                else if (file.FileName.EndsWith(".nessus"))
                                {
                                    (hosts, networks, interfaces, transportProtocols, applicationProtocols, protocols, services) = ParseNessusOutput(lines.Split(Environment.NewLine));
                                    vulnerabilities.AddRange(ParseNessusVulnerabilities(lines.Split(Environment.NewLine)));
                                }
                                else if (file.FileName.EndsWith(".xml"))
                                {
                                    (hosts, networks, interfaces, transportProtocols, applicationProtocols, protocols, services) = ParseBurpSuiteOutput(lines.Split(Environment.NewLine));
                                    vulnerabilities.AddRange(ParseBurpSuiteVulnerabilities(lines.Split(Environment.NewLine)));
                                }
                                else
                                {
                                    return BadRequest();
                                }

                                foreach (var network in networks)
                                {
                                    if (!_context.Networks.Any(n => n.Network == network.Network))
                                    {
                                        _context.Networks.Add(network);
                                    }
                                }
                                await _context.SaveChangesAsync();

                                foreach (var host in hosts)
                                {
                                    if (!_context.Hosts.Any(h => h.Fqdn == host.Fqdn))
                                    {
                                        _context.Hosts.Add(host);
                                    }

                                    var hostId = _context.Hosts.FirstOrDefault(h => h.Fqdn == host.Fqdn)?.Id;

                                    foreach (var intf in interfaces.Where(i => i.HostId == hostId))
                                    {
                                        if (!_context.Interfaces.Any(i => i.HostId == hostId && i.NetworkId == intf.NetworkId && i.IpAddress == intf.IpAddress))
                                        {
                                            _context.Interfaces.Add(intf);
                                        }
                                    }

                                    await _context.SaveChangesAsync();

                                    foreach (var service in services.Where(s => s.HostId == hostId))
                                    {
                                        var transportProtocol = transportProtocols.FirstOrDefault(tp => tp.Name == service.Protocol.TransportProtocol.Name);
                                        if (transportProtocol == null)
                                        {
                                            transportProtocol = service.Protocol.TransportProtocol;
                                            _context.TransportProtocols.Add(transportProtocol);
                                        }

                                        var applicationProtocol = applicationProtocols.FirstOrDefault(ap => ap.Name == service.Protocol.ApplicationProtocol.Name);
                                        if (applicationProtocol == null)
                                        {
                                            applicationProtocol = service.Protocol.ApplicationProtocol;
                                            _context.ApplicationProtocols.Add(applicationProtocol);
                                        }

                                        var protocol = protocols.FirstOrDefault(p => p.Name == service.Protocol.Name);
                                        if (protocol == null)
                                        {
                                            protocol = service.Protocol;
                                            protocol.TransportProtocol = transportProtocol;
                                            protocol.ApplicationProtocol = applicationProtocol;
                                            _context.Protocols.Add(protocol);
                                        }

                                        if (!_context.Services.Any(s => s.HostId == hostId && s.Port == service.Port && s.ProtocolId == protocol.Id))
                                        {
                                            _context.Services
                                                                .Add(new Service
                                                                {
                                                                    HostId = hostId.Value,
                                                                    Port = service.Port,
                                                                    State = service.State,
                                                                    Name = service.Name,
                                                                    Product = service.Product,
                                                                    Version = service.Version,
                                                                    ExtraInfo = service.ExtraInfo,
                                                                    Description = service.Description,
                                                                    Protocol = protocol
                                                                });
                                        }
                                    }
                                    await _context.SaveChangesAsync();
                                }

                                var vulnerabilityIds = vulnerabilities.Select(v => v.PluginId).Distinct();
                                foreach (var vulnerabilityId in vulnerabilityIds)
                                {
                                    var vulnerability = vulnerabilities.First(v => v.PluginId == vulnerabilityId);
                                    if (!_context.Vulnerabilities.Any(v => v.PluginId == vulnerability.PluginId))
                                    {
                                        _context.Vulnerabilities.Add(vulnerability);
                                    }

                                    var vulnerabilityIdSaved = _context.Vulnerabilities.FirstOrDefault(v => v.PluginId == vulnerability.PluginId)?.Id;

                                    foreach (var writeup in vulnerability.Writeups)
                                    {
                                        if (!_context.Writeups.Any(w => w.Name == writeup.Name))
                                        {
                                            _context.Writeups.Add(writeup);
                                        }

                                        var writeupId = _context.Writeups.FirstOrDefault(w => w.Name == writeup.Name)?.Id;

                                        if (!_context.WriteupVulnerabilityMappings.Any(wvm => wvm.WriteupId == writeupId && wvm.VulnerabilityId == vulnerabilityIdSaved))
                                        {
                                            _context.WriteupVulnerabilityMappings.Add(new WriteupVulnerabilityMapping { WriteupId = writeupId.Value, VulnerabilityId = vulnerabilityIdSaved.Value });
                                        }
                                    }

                                    await _context.SaveChangesAsync();
                                }

                                return Ok();
                            }

                            private bool HostExists(int id)
                            {
                                return _context.Hosts.Any(e => e.Id == id);
                            }
                        }
                    }
                }
            }
        }
    }
}
