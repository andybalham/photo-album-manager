namespace PhotoManager.Models;

public enum SortField { Name, DateCreated }
public enum SortDirection { Ascending, Descending }

public record SortOptions(SortField Field, SortDirection Direction)
{
    public static SortOptions Default => new(SortField.Name, SortDirection.Ascending);

    public SortOptions Toggle() => Direction == SortDirection.Ascending
        ? this with { Direction = SortDirection.Descending }
        : this with { Direction = SortDirection.Ascending };

    public SortOptions WithField(SortField field) =>
        Field == field ? Toggle() : new(field, SortDirection.Ascending);
}
