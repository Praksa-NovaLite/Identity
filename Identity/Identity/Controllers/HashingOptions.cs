namespace Identity.Controllers
{
    public class HashingOptions
    {
        public static string SectionName = "HashingOptions";
        public required string Salt { get; set; }
        public int IterationCount { get; set; }
    }
}
