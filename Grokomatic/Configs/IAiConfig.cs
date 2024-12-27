namespace Grokomatic.Configs
{
    public interface IAiConfig
    {
        string? ApiKey { get; set; }
        string? Model { get; set; }
        string? ImageModel { get; set; }
        string? Endpoint { get; set; }
    }
}