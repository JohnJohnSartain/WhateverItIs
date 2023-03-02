namespace Model.Account;

public class CreatorPublic : UserPublic
{
    public CreatorPublic(User user) : base(user) { }

    public int PublishedArticles { get; set; }
    public int Likes { get; set; }
    public int Dislikes { get; set; }
    public int ApprovalRating { get => (Likes == 0 || Dislikes == 0) ? 100 : (int)(((double)Likes / ((double)Likes + (double)Dislikes)) * 100); }
}