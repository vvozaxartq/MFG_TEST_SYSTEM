namespace ATS.Core.Models;

public interface IStructuredLogSink
{
    void Append(string path, StructuredLogEntry entry);
}
