using System.Globalization;

namespace Server.Models;

/// <summary>
/// Данные, наполняемые LDAP-запросом, сохраняемые в cookie авторизации.
/// </summary>
public class User
{
    public string IdentityName { get; set; } = "anonymous";
    public string? DisplayName { get; set; }
    public string? Base { get; set; }
    public List<string> Groups { get; set; } = new List<string>();
    public string? LdapErrorResponseMessage { get; set; }
    public int AltCodePage { get; set; } = System.Text.Encoding.ASCII.CodePage;

    public bool HasAnyGroup(IEnumerable<string> groups)
    {
        if (!groups.Any())
        {
            return true;
        }

        if (!this.Groups.Any())
        {
            return false;
        }
        
        return Groups.Intersect(groups, StringComparer.Create(CultureInfo.InvariantCulture, true)).Any();
    }

    public bool HasGroup(string group)
    {
        return this.Groups.Contains(group, StringComparer.Create(CultureInfo.InvariantCulture, true));
    }
}