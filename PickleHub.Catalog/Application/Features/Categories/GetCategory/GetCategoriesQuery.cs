using MediatR;
using Microsoft.EntityFrameworkCore;
using PickleHub.Catalog.Application.Features.Categories.DTOs;
using PickleHub.Catalog.Infrastructure.Persistence;

namespace PickleHub.Catalog.Application.Features.Categories.GetCategory
{
    public record GetCategoriesQuery(bool Flat = false) : IRequest<List<CategoryTreeDto>>;

    public class GetCategoriesHandler : IRequestHandler<GetCategoriesQuery, List<CategoryTreeDto>>
    {
        private readonly CatalogDbContext _db;

        public GetCategoriesHandler(CatalogDbContext db)
        {
            _db = db;
        }

        public async Task<List<CategoryTreeDto>> Handle(GetCategoriesQuery request, CancellationToken ct)
        {
            var categories = await _db.Categories.AsNoTracking().ToListAsync(ct);

            var dtos = categories.Select(c => new CategoryTreeDto
            {
                Id = c.Id,
                Name = c.Name,
                ParentId = c.ParentId
            }).ToList();

            if (request.Flat)
            {
                return dtos;
            }

            var lookup = dtos.ToDictionary(d => d.Id);
            var roots = new List<CategoryTreeDto>();

            foreach (var dto in dtos)
            {
                if (dto.ParentId.HasValue && lookup.TryGetValue(dto.ParentId.Value, out var parent))
                {
                    parent.Children.Add(dto);
                }
                else
                {
                    roots.Add(dto);
                }
            }

            return roots;

        }
    }
}
