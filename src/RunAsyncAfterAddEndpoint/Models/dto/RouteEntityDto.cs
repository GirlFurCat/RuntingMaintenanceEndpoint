namespace RunAsyncAfterAddEndpoint.Models.dto
{
    public class RouteEntityDto
    {
        public int? id { get; set; }
        public string? path { get; set; }

        public string? method { get; set; }

        public string? sql { get; set; }

        public Dictionary<string, string>? parameter { get; set; }

        public string? response { get; set; }

        public bool? authorization { get; set; }

        public string? version { get; set; }

        public string? introduction { get; set; }

        public string? createdBy { get; set; }

        public DateTime? createdAt { get; set; }
    }
}
