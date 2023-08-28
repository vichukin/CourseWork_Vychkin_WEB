namespace CourseWork_Vychkin_WEB.Data
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public List<House>? Houses { get; set; }

    }
}
