using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fanex.CheckDataConsistency.Business.Services;

public interface DeleteFilesService
{
    Task ExecuteAsync();
}

public class DeleteFilesServiceImp(
    ILogger<DeleteFilesServiceImp> logger
) : DeleteFilesService
{
    private readonly SemaphoreSlim semaphore = new(1, 1);
    private const int retentionDays = 7;
    private const int maxRetryAttempts = 3;
    private readonly string basePath = AppConfigService.Config.Path;

    public async Task ExecuteAsync()
    {
        try
        {
            var directoryInfo = new DirectoryInfo(basePath);
            if (!directoryInfo.Exists)
            {
                logger.LogWarning("Directory {Path} does not exist", directoryInfo.FullName);
                return;
            }

            await semaphore.WaitAsync();
            try
            {
                await ProcessDirectoryAsync(directoryInfo);
            }
            finally
            {
                semaphore.Release();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while executing DeleteFilesService");
            throw new DeleteFilesServiceException("Failed to execute DeleteFilesService", ex);
        }
    }

    private async Task ProcessDirectoryAsync(DirectoryInfo directoryInfo)
    {
        try
        {
            await ProcessFilesAsync(directoryInfo);
            await ProcessSubDirectoriesAsync(directoryInfo);
            await DeleteEmptyDirectoryAsync(directoryInfo);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogError(ex, "Access denied to directory {Directory}", directoryInfo.FullName);
            throw new DeleteFilesServiceException($"Access denied to directory {directoryInfo.FullName}", ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while processing directory {Directory}", directoryInfo.FullName);
            throw new DeleteFilesServiceException($"Error processing directory {directoryInfo.FullName}", ex);
        }
    }

    private async Task ProcessFilesAsync(DirectoryInfo directoryInfo)
    {
        var files = directoryInfo.EnumerateFiles();
        foreach (var file in files)
        {
            await DeleteFileWithRetryAsync(file);
        }
    }

    private async Task ProcessSubDirectoriesAsync(DirectoryInfo directoryInfo)
    {
        var subDirectories = directoryInfo.EnumerateDirectories();
        foreach (var subDirectory in subDirectories)
        {
            await ProcessDirectoryAsync(subDirectory);
        }
    }

    private async Task DeleteFileWithRetryAsync(FileInfo fileInfo)
    {
        for (var attempt = 1; attempt <= maxRetryAttempts; attempt++)
        {
            try
            {
                if (!fileInfo.Exists)
                {
                    return;
                }

                if (!IsFileOld(fileInfo))
                {
                    return;
                }

                if (!await HasDeletePermissionAsync(fileInfo))
                {
                    logger.LogWarning("No permission to delete file {File}", fileInfo.FullName);
                    return;
                }

                await DeleteFileAsync(fileInfo);
                logger.LogInformation("Successfully deleted file {File}", fileInfo.FullName);
                return;
            }
            catch (IOException ex) when (attempt < maxRetryAttempts)
            {
                logger.LogWarning(ex, "Attempt {Attempt} failed to delete file {File}", attempt, fileInfo.FullName);
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to delete file {File} after {Attempt} attempts",
                    fileInfo.FullName, attempt);
                throw new DeleteFilesServiceException(
                    $"Failed to delete file {fileInfo.FullName} after {attempt} attempts", ex);
            }
        }
    }

    private static bool IsFileOld(FileInfo fileInfo)
    {
        var fileAge = DateTime.UtcNow.Subtract(fileInfo.CreationTimeUtc).TotalDays;
        return fileAge > retentionDays;
    }

    private async Task<bool> HasDeletePermissionAsync(FileInfo fileInfo)
    {
        try
        {
            await using var stream = fileInfo.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while checking file permissions for {File}", fileInfo.FullName);
            return false;
        }
    }

    private static Task DeleteFileAsync(FileInfo fileInfo)
    {
        fileInfo.Delete();
        return Task.CompletedTask;
    }

    private Task DeleteEmptyDirectoryAsync(DirectoryInfo directoryInfo)
    {
        try
        {
            if (!IsDirectoryEmpty(directoryInfo) || !IsDirectoryOld(directoryInfo))
            {
                return Task.CompletedTask;
            }

            directoryInfo.Delete();
            logger.LogInformation("Successfully deleted empty directory {Directory}", directoryInfo.FullName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete directory {Directory}", directoryInfo.FullName);
            throw new DeleteFilesServiceException($"Failed to delete directory {directoryInfo.FullName}", ex);
        }

        return Task.CompletedTask;
    }

    private static bool IsDirectoryEmpty(DirectoryInfo directoryInfo)
    {
        var fileSystemInfos = directoryInfo.EnumerateFileSystemInfos();
        return !fileSystemInfos.Any();
    }

    private static bool IsDirectoryOld(DirectoryInfo directoryInfo)
    {
        var lastAccessTime = directoryInfo.LastAccessTimeUtc;
        return DateTime.UtcNow.Subtract(lastAccessTime).TotalDays > retentionDays;
    }

    private static TimeSpan CalculateDelayToNext10Min()
    {
        var now = DateTime.UtcNow;
        var nextRun = new DateTime(now.Year, now.Month, now.Day, now.Hour, (now.Minute / 10 + 1) * 10, 0);
        if (nextRun <= now)
        {
            nextRun = nextRun.AddHours(1);
        }

        return nextRun - now;
    }

    private static TimeSpan CalculateDelayToNext21Hour()
    {
        var now = DateTime.UtcNow;
        var nextRun = new DateTime(now.Year, now.Month, now.Day, 21, 0, 0);
        if (nextRun <= now)
        {
            nextRun = nextRun.AddDays(1);
        }

        return nextRun - now;
    }

    private async Task WaitForNextRunAsync()
    {
        var delay = CalculateDelayToNext21Hour();
        logger.LogInformation("Waiting {Delay} until next run at {NextRun}",
            delay, DateTime.UtcNow.Add(delay));
        await Task.Delay(delay);
    }
}

public class DeleteFilesServiceException : Exception
{
    public DeleteFilesServiceException(string message) : base(message)
    {
    }

    public DeleteFilesServiceException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}