using System.Text.Json;

namespace Movies.Client.Helpers {

    public class JsonSerializerOptionsWrapper {
        public JsonSerializerOptions Options { get; set; }

        public JsonSerializerOptionsWrapper() {
            Options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        }
    }
}