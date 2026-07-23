using MediatR;
using PickleHub.Catalog.Application.Features.Categories.DTOs;
using PickleHub.Catalog.Domain.Repositories;
using PickleHub.Common.Exceptions;
using PickleHub.Common.ValueObjects;

namespace PickleHub.Catalog.Application.Features.Categories.UpdateCategory
{
    public record UpdateCategoryCommand(Guid Id, string Name, Guid? ParentId) : IRequest<CategoryTreeDto>;

    public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryCommand, CategoryTreeDto>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateCategoryHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<CategoryTreeDto> Handle(UpdateCategoryCommand request, CancellationToken ct)
        {
            var category = await _categoryRepository.GetByIdAsync(request.Id, ct)
                ?? throw new NotFoundException("Danh mục không tồn tại.");

            var slug = await GenerateUniqueSlugAsync(request.Name, request.Id, ct);
            category.Update(request.Name, slug, request.ParentId);

            _categoryRepository.Update(category);
            await _unitOfWork.SaveChangesAsync(ct);

            return new CategoryTreeDto
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug.Value,
                ParentId = category.ParentId
            };
        }

        private async Task<Slug> GenerateUniqueSlugAsync(string name, Guid? excludeId, CancellationToken ct)
        {
            var baseSlug = Slug.Create(name);
            var candidate = baseSlug;
            var counter = 1;

            while (await _categoryRepository.ExistsBySlugAsync(candidate.Value, excludeId, ct))
                candidate = baseSlug.AppendSuffix(counter++);

            return candidate;
        }
    }
}
