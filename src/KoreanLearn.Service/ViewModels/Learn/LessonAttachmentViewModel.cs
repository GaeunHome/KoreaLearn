namespace KoreanLearn.Service.ViewModels.Learn;

public class LessonAttachmentViewModel
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string FileSizeDisplay { get; set; } = string.Empty;
}
