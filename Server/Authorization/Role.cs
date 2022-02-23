using System;
namespace Server.Authorization;

#nullable disable

public class Role
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string[] Groups { get; set; } = new string[0];
    public string[] HttpPaths { get; set; } = new string[0];
    public string[] GraphqlPaths { get; set; } = new string[0];
}
