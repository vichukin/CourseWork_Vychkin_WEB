namespace CourseWork_Vychkin_WEB.Data
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string ImagePath { get; set; } = default!;
        public List<House>? houses { get; set; }
    }
}
