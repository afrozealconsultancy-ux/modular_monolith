namespace ModularMonolith.Modules.Storage.Domain.Exceptions;

public abstract class StorageException : Exception
{
    protected StorageException(string message) : base(message)
    {
    }
}

public sealed class FileNotFoundException : StorageException
{
    public FileNotFoundException(Guid fileId)
        : base($"File with ID '{fileId}' was not found")
    {
        FileId = fileId;
    }

    public Guid FileId { get; }
}

public sealed class FileAlreadyDeletedException : StorageException
{
    public FileAlreadyDeletedException(Guid fileId)
        : base($"File with ID '{fileId}' has already been deleted")
    {
        FileId = fileId;
    }

    public Guid FileId { get; }
}

public sealed class InfectedFileException : StorageException
{
    public InfectedFileException(Guid fileId, string scanResult)
        : base($"File with ID '{fileId}' is infected: {scanResult}")
    {
        FileId = fileId;
        ScanResult = scanResult;
    }

    public Guid FileId { get; }
    public string ScanResult { get; }
}
