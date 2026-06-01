namespace SafeCity.Services;

public interface IMediaService
{
    Task<string?> PickPhotoAsync();
    Task<string?> CapturePhotoAsync();
}
