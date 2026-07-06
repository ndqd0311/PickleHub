using MediatR;
using PickleHub.Catalog.Application.Features.Categories.DTOs;
using PickleHub.Catalog.Domain.Entities;
using PickleHub.Catalog.Infrastructure.Persistence;

namespace PickleHub.Catalog.Application.Features.Categories.CreateCategory
{
    public record CreateCategoryCommand(string Name, Guid? ParentId) : IRequest<CategoryTreeDto>;

    public class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, CategoryTreeDto>
    {
        private readonly CatalogDbContext _db;

        public CreateCategoryHandler(CatalogDbContext db)
        {
            _db = db;
        }

        public async Task<CategoryTreeDto> Handle(CreateCategoryCommand request, CancellationToken ct)
        {
            var category = new Category
            {
                Name = request.Name,
                ParentId = request.ParentId
            };

            _db.Categories.Add(category);
            await _db.SaveChangesAsync(ct);

            return new CategoryTreeDto { Id = category.Id, Name = category.Name, ParentId = category.ParentId };
        }
    }
}
