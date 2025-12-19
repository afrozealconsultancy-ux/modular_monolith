namespace ModularMonolith.Modules.Storage.Domain.ValueObjects;

public sealed class FileMetadata
{
    public string Key { get; private set; }
    public string Value { get; private set; }

    private FileMetadata(string key, string value)
    {
        Key = key;
        Value = value;
    }

    public static FileMetadata Create(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be empty", nameof(key));

        return new FileMetadata(key, value ?? string.Empty);
    }
}
