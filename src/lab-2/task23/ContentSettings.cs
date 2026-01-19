namespace Task23;

public class ContentSettings
{
    public string ContentType { get; set; } = "FigletText";

    public string FigletText { get; set; } = "Hello";

    public string FigletColor { get; set; } = "White";

    public string ImageBase64 { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public int MaxImageWidth { get; set; } = 50;
}