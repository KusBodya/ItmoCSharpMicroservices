namespace Task33.Options;

public class GrpcServiceOptions
{
    public static string SectionName => "GrpcService";

    public string Url { get; set; } = "http://localhost:5001";
}
