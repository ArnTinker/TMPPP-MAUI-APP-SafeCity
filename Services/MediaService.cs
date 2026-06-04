namespace SafeCity.Services;

public class MediaService : IMediaService
{
    public async Task<string?> PickPhotoAsync()
    {
        try
        {
            // PickPhotosAsync is the recommended API; take first selection for single-pick UX
            var results = await MediaPicker.Default.PickPhotosAsync();
            var result  = results?.FirstOrDefault();
            return result is null ? null : await CopyToAppStorageAsync(result.FullPath);
        }
        catch { return null; }
    }

    public async Task<string?> CapturePhotoAsync()
    {
        try
        {
            var result = await MediaPicker.Default.CapturePhotoAsync();
            return result is null ? null : await CopyToAppStorageAsync(result.FullPath);
        }
        catch { return null; }
    }

    private static async Task<string> CopyToAppStorageAsync(string sourcePath)
    {
        var dest = Path.Combine(FileSystem.AppDataDirectory, $"photo_{Guid.NewGuid():N}.jpg");
        using var src  = File.OpenRead(sourcePath);
        using var dst  = File.Create(dest);
        await src.CopyToAsync(dst);
        return dest;
    }
}
