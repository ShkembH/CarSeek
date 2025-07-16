namespace CarSeek.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(string fileName, byte[] fileData, string folderPath, CancellationToken cancellationToken = default);
    Task<bool> DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default);
}
