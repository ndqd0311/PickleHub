using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PickleHub.Catalog.Application.Features.Brands.DTOs;
using PickleHub.Catalog.Application.Features.Categories.DTOs;
using PickleHub.Catalog.Application.Features.Products.DTOs;
using PickleHub.Catalog.Domain.Enums;
using PickleHub.Catalog.Domain.Repositories;
using PickleHub.Common.DTOs;
using System.Threading;

namespace PickleHub.Catalog.Application.Features.Products.GetProducts
{
    public record GetProductsQuery(
        string? Keyword,
        Guid? CategoryId,
        Guid? BrandId,
        decimal? MinPrice,
        decimal? MaxPrice,
        SortBy SortBy = SortBy.Newest,
        int Page = 1,
        int PageSize = 20
    ) : IRequest<PagedResult<ProductListDto>>;

    public class GetProductsHandler : IRequestHandler<GetProductsQuery, PagedResult<ProductListDto>>
    {
        private readonly IProductRepository _productRepository;
        public GetProductsHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }
        public async Task<PagedResult<ProductListDto>> Handle(GetProductsQuery request, CancellationToken ct)
        {
            var (items, totalCount) = await _productRepository.GetPagedAsync(
                request.Keyword, request.CategoryId, request.BrandId,
                request.MinPrice, request.MaxPrice,
                request.SortBy, request.Page, request.PageSize,
                ct);

            return new PagedResult<ProductListDto>
            {
                Items = items.Select(p => new ProductListDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Slug = p.Slug.Value,
                    BasePrice = p.BasePrice,
                    SoldCount = p.SoldCount,
                    ThumbnailUrl = p.Images
                        .Where(i => !i.IsSizeChart)
                        .OrderBy(i => i.SortOrder)
                        .Select(i => i.Url)
                        .FirstOrDefault(),
                    Brand = p.Brand is null ? null : new BrandDto
                    {
                        Id = p.Brand.Id,
                        Name = p.Brand.Name
                    },
                    Category = p.Category is null ? null : new CategorySummaryDto
                    {
                        Id = p.Category.Id,
                        Name = p.Category.Name,
                        Slug = p.Category.Slug.Value
                    }
                }).ToList(),
                Page = request.Page,
                PageSize = request.PageSize,
                TotalItems = totalCount,

            };
        }
    }
}
