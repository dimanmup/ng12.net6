using System.DirectoryServices.Protocols;
using System.Net;

namespace Server.Injections;

#nullable disable

public class Ldap
{
    public string Domain { get; set; }
    public int Port { get; set; }
    public string AccountName { get; set; }
    public string AccountPassword { get; set; }
    public int CodePage { get; set; }
    public LdapConnection GetLdapConnection() => new LdapConnection(
        new LdapDirectoryIdentifier(Domain, Port),
        new NetworkCredential(AccountName, AccountPassword),
        AuthType.Basic
        );
}