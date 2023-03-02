namespace Model.Account;

public class User : Base
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string ProfileImageUrl { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string[] Roles { get; set; }
    public string Issuer { get; set; }
    public string Website { get; set; }
    public bool IsDeleted { get; set; }
}