using MediatR;
using Microsoft.EntityFrameworkCore;
using PickleHub.Catalog.Infrastructure.Persistence;
using PickleHub.Common.Exceptions;

namespace PickleHub.Catalog.Application.Features.Categories.DeleteCategory
{
    public record DeleteCategoryCommand(Guid Id) : IRequest;

    public class DeleteCategoryHandler : IRequestHandler<DeleteCategoryCommand>
    {
        private readonly CatalogDbContext _db;

        public DeleteCategoryHandler(CatalogDbContext db)
        {
            _db = db;
        }

        public async Task Handle(DeleteCategoryCommand request, CancellationToken ct)
        {
            var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == request.Id, ct);

            if (category == null)
            {
                throw new NotFoundException( "Danh mục không tồn tại.");
            }

            var hasChildren = await _db.Categories.AnyAsync(c => c.ParentId == request.Id, ct);
            var hasProducts = await _db.Products.AnyAsync(p => p.CategoryId == request.Id, ct);

            if (hasChildren || hasProducts)
            {
                throw new ConflictException( "Danh mục vẫn còn sản phẩm hoặc danh mục con. Không thể xóa");
            }

            _db.Categories.Remove(category);
            await _db.SaveChangesAsync(ct);
        }
    }
}
