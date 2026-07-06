using MediatR;
using Microsoft.EntityFrameworkCore;
using PickleHub.Catalog.Application.Features.Categories.DTOs;
using PickleHub.Catalog.Infrastructure.Persistence;
using PickleHub.Common.Exceptions;

namespace PickleHub.Catalog.Application.Features.Categories.UpdateCategory
{
    public record UpdateCategoryCommand(Guid Id, string Name, Guid? ParentId) : IRequest<CategoryTreeDto>;

    public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryCommand, CategoryTreeDto>
    {
        private readonly CatalogDbContext _db;

        public UpdateCategoryHandler(CatalogDbContext db)
        {
            _db = db;
        }

        public async Task<CategoryTreeDto> Handle(UpdateCategoryCommand request, CancellationToken ct)
        {
            var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == request.Id, ct);

            if (category == null)
            {
                throw new NotFoundException( "Danh mục không tồn tại.");
            }

            category.Name = request.Name;
            category.ParentId = request.ParentId;
            category.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);

            return new CategoryTreeDto { Id = category.Id, Name = category.Name, ParentId = category.ParentId };
        }
    }
}
