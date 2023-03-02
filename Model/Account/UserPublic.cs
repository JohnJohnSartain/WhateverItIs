namespace Model.Account;

public class UserPublic : Base
{
    public UserPublic(User user)
    {
        Id = user.Id;
        Created = user.Created;
        CreatedBy = user.CreatedBy;
        Modified = user.Modified;
        ModifiedBy = user.ModifiedBy;

        Name = user.FirstName + " " + user.LastName;
        ProfileImageUrl = user.ProfileImageUrl;
        Email = user.Email;
        Website = user.Website;
        Roles = user.Roles;
    }

    public string Name { get; set; }
    public string ProfileImageUrl { get; set; }
    public string Email { get; set; }
    public string Website { get; set; }
    public string[] Roles { get; set; }
}