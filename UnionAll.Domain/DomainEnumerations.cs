namespace UnionAll.Domain
{
    public enum NodeValueTypes
    {
        Default = 0,
        Name = 1,
        Description = 2,
        Amount = 3,
        Percentage = 4,
        Date = 5,
        Money = 6,
        Measure = 7,
        Definition = 8
    }

    public enum NodeTopics
    {
        Default = 0,
        Continent = 1,
        Region = 2,
        Country = 3,
        Institution = 4,
        Organisation = 5,
        Individual = 6,
        Political = 7,
        Financial = 8,
        Economic = 9,
        Industrial = 10,
        Commercial = 11,
        Research = 12
    }

    public enum NodesStatusValues
    {
        Active = 1,
        Deleted = 9
    }

    public enum VectorStatusValues
    {
        Active = 1,
        Deleted = 9
    }

    public enum UrlNavigationType
    {
        None = 0,
        Previous = 1,
        Next = 2
    }
}
