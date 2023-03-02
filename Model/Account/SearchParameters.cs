namespace Model.Account;

public record SearchParameters : BaseSearchParameters
{
    public bool? IsCreator { get; set; }
}