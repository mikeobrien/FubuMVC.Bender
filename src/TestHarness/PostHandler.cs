namespace TestHarness
{
    public class PostHandler
    {
        public class Model
        {
            public string Value { get; set; }
        }

        public Model Execute(Model request)
        {
            return request;    
        }
    }
}