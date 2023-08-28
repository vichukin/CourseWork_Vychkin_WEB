namespace CourseWork_Vychkin_WEB.Models.ViewModels.TagsViewModels
{
    public class CreateTagVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public IFormFile? Image { get; set; } = default!;
    }
}
