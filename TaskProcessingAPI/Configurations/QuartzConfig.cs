namespace TaskProcessingAPI.Settings
{
    public class QuartzConfig
    {
        public int Frequency { get; set; }
        public string JobName { get; set; }
        public string JobGroup { get; set; }
    }
}
