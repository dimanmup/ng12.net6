namespace Server.Authorization;

public class Descriptor
{
    public bool UseCookie { get; set; } = false;
    public Role[] Roles { get; set; } = new Role[0];
}
