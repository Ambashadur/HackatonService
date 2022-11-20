namespace HackatonService.Domain
{
    public class YaraMatchEntity
    {
        public string PathToFile { get; set; }

        public string Rule { get; set; }

        public string RuleKey { get; set; }

        public string MacAddress { get; set; }
    }
}
