namespace PickleHub.Catalog.Application.Features.Categories.DTOs
{
    // dùng khi feature khác cần lồng thông tin Category.
    public class CategorySummaryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
