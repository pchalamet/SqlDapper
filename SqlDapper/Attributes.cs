namespace SqlDapper;


[AttributeUsage(AttributeTargets.Class)]
public class TableAttribute : Attribute {
    public TableAttribute(string name) {
        Name = name;
    }

    public string Name { get; init; }
}

[AttributeUsage(AttributeTargets.Property)]
public class KeyAttribute : Attribute {
}
