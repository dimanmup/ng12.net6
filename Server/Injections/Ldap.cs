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

    /// <summary>
    /// Будет использоваться для конвертации байт значения атрибута,<br/>
    /// 1) если оно имеет тип byte[],
    /// 2) если оно имеет тип string, но идентифицировать локальные символы не удалось, т.е. вместо них \uFFFD.
    /// </summary>
    public int AltCodePage { get; set; }

    public LdapConnection GetLdapConnection() => new LdapConnection(
        new LdapDirectoryIdentifier(Domain, Port),
        new NetworkCredential(AccountName, AccountPassword),
        AuthType.Basic
        );
}