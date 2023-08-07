using Newtonsoft.Json;

namespace CrudStudent
{
    public class student
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Age { get; set; }
    }
    public class Createstudent
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Age { get; set; }
    }
    public class Updatestudent
    {
        public string id { get; set; }
        public string Name { get; set; }
        public string Age { get; set; }
    }
}