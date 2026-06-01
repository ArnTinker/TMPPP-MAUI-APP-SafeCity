namespace SafeCity.Services;

public class MediaService : IMediaService
{
    public async Task<string?> PickPhotoAsync()
    {
        try
        {
            var results = await MediaPicker.Default.PickPhotosAsync();
            var result  = results?.FirstOrDefault();
            return result?.FullPath;
        }
        catch { return null; }
    }

    public async Task<string?> CapturePhotoAsync()
    {
        try
        {
            var result = await MediaPicker.Default.CapturePhotoAsync();
            return result?.FullPath;
        }
        catch { return null; }
    }
}
