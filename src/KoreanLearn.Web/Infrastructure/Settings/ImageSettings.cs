namespace KoreanLearn.Web.Infrastructure.Settings;

public class ImageSettings
{
    public const string Section = "ImageSettings";
    public string UploadPath { get; set; } = "wwwroot/images/products";
    public int MaxFileSizeMb { get; set; } = 5;
    public string[] AllowedExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".webp"];
}
