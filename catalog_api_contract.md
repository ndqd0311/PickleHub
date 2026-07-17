# Catalog Service — API Contract

> **Base URL (qua API Gateway):** `http://localhost:5000/api/catalog`
> **Format:** JSON (trừ endpoint upload ảnh dùng `multipart/form-data`)
> **Auth:** Các endpoint đọc (`GET`) không cần token. Các endpoint ghi (`POST`, `PUT`, `PATCH`, `DELETE`) yêu cầu JWT với `role = admin`.

---

## Cấu trúc lỗi chuẩn

Mọi lỗi đều trả về cùng 1 format:

```json
{
  "error": {
    "message": "Mô tả lỗi",
    "errors": {               // chỉ có khi lỗi validation nhiều field
      "fieldName": ["lỗi 1", "lỗi 2"]
    }
  }
}
```

| HTTP Status | Khi nào |
|---|---|
| 400 | Request không hợp lệ (thiếu field, sai định dạng) |
| 401 | Chưa đăng nhập hoặc token hết hạn |
| 403 | Không đủ quyền |
| 404 | Resource không tồn tại |
| 409 | Xung đột (SKU trùng, đang được tham chiếu) |
| 500 | Lỗi hệ thống |

---

## 1. Brands

### `GET /brands`
Lấy danh sách tất cả thương hiệu.

**Response 200:**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Selkirk"
  }
]
```

### `POST /brands` *(Admin)*
```json
// Request body
{ "name": "Selkirk" }

// Response 201
{ "id": "uuid", "name": "Selkirk" }
```

### `PUT /brands/{id}` *(Admin)*
```json
// Request body
{ "name": "Selkirk Sport" }

// Response 200
{ "id": "uuid", "name": "Selkirk Sport" }
```

### `DELETE /brands/{id}` *(Admin)*
- **Response 204**: xóa thành công
- **Response 409**: thương hiệu vẫn còn sản phẩm

---

## 2. Categories

### `GET /categories`
Lấy danh mục dạng cây (mặc định) hoặc danh sách phẳng.

**Query params:**
| Param | Kiểu | Mặc định | Mô tả |
|---|---|---|---|
| `flat` | bool | `false` | `true` = danh sách phẳng, `false` = dạng cây |

**Response 200 (dạng cây):**
```json
[
  {
    "id": "uuid",
    "name": "Vợt Pickleball",
    "slug": "vot-pickleball",
    "parentId": null,
    "children": [
      {
        "id": "uuid",
        "name": "Vợt Carbon",
        "slug": "vot-carbon",
        "parentId": "uuid",
        "children": []
      }
    ]
  }
]
```

### `POST /categories` *(Admin)*
```json
// Request body
{ "name": "Vợt Carbon", "parentId": "uuid | null" }

// Response 201
{ "id": "uuid", "name": "Vợt Carbon", "slug": "vot-carbon", "parentId": "uuid", "children": [] }
```

### `PUT /categories/{id}` *(Admin)*
```json
// Request body
{ "name": "Vợt Carbon Pro", "parentId": "uuid | null" }

// Response 200: object category đã cập nhật
```

### `DELETE /categories/{id}` *(Admin)*
- **Response 204**: xóa thành công
- **Response 409**: vẫn còn sản phẩm hoặc danh mục con

---

## 3. Products

### `GET /products`
Tìm kiếm & lọc sản phẩm — **chỉ trả về sản phẩm có `status = Active`.**

**Query params:**
| Param | Kiểu | Mặc định | Mô tả |
|---|---|---|---|
| `keyword` | string | — | Tìm theo tên |
| `categoryId` | uuid | — | Lọc theo danh mục |
| `brandId` | uuid | — | Lọc theo thương hiệu |
| `minPrice` | decimal | — | Giá tối thiểu |
| `maxPrice` | decimal | — | Giá tối đa |
| `sortBy` | string | `Newest` | `Newest` / `PriceAsc` / `PriceDesc` / `BestSelling` |
| `page` | int | `1` | Trang hiện tại |
| `pageSize` | int | `20` | Số item mỗi trang |

**Response 200:**
```json
{
  "items": [
    {
      "id": "uuid",
      "name": "Vợt Selkirk Power Air",
      "slug": "vot-selkirk-power-air",
      "basePrice": 1850000,
      "thumbnailUrl": "https://res.cloudinary.com/...",
      "soldCount": 42,
      "brand": { "id": "uuid", "name": "Selkirk" },
      "category": { "id": "uuid", "name": "Vợt Carbon", "slug": "vot-carbon" }
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalItems": 57
}
```

---

### `GET /products/{slug-or-id}`
Lấy chi tiết 1 sản phẩm — nhận cả **slug** (string) hoặc **UUID** (guid).

> **Quan trọng với Cart & Order Service:**
> - `variants[].id` là `productVariantId` cần lưu trong `cart_item` và `order_item`
> - `variants[].price` là giá tại thời điểm hiện tại — Cart & Order cần **snapshot** giá này khi thêm vào giỏ/đặt hàng
> - `images` được sắp xếp theo `sortOrder` tăng dần; ảnh có `isSizeChart = false` và `sortOrder` nhỏ nhất là ảnh thumbnail chính

**Response 200:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Vợt Selkirk Power Air",
  "slug": "vot-selkirk-power-air",
  "description": "Mô tả chi tiết sản phẩm...",
  "basePrice": 1850000,
  "status": "Active",
  "specsJson": "{\"weight\": \"215g\", \"surface\": \"Carbon Fiber\"}",
  "soldCount": 42,
  "category": {
    "id": "uuid",
    "name": "Vợt Carbon",
    "slug": "vot-carbon"
  },
  "brand": {
    "id": "uuid",
    "name": "Selkirk"
  },
  "images": [
    {
      "id": "uuid",
      "publicId": "picklehub/products/abc123",
      "url": "https://res.cloudinary.com/your-cloud/image/upload/picklehub/products/abc123.webp",
      "variantId": null,
      "sortOrder": 0,
      "isSizeChart": false
    },
    {
      "id": "uuid",
      "publicId": "picklehub/products/sizechart123",
      "url": "https://res.cloudinary.com/your-cloud/image/upload/picklehub/products/sizechart123.webp",
      "variantId": null,
      "sortOrder": 99,
      "isSizeChart": true
    }
  ],
  "variants": [
    {
      "id": "a1b2c3d4-0000-0000-0000-000000000001",
      "sku": "SELKIRK-PA-RED",
      "attributesJson": "{\"color\": \"Đỏ\"}",
      "price": 1850000
    },
    {
      "id": "a1b2c3d4-0000-0000-0000-000000000002",
      "sku": "SELKIRK-PA-BLUE",
      "attributesJson": "{\"color\": \"Xanh\"}",
      "price": 1850000
    }
  ]
}
```

**Response 404:** sản phẩm không tồn tại hoặc đang ở trạng thái `Draft`/`Hidden`.

---

### `POST /products` *(Admin)*
```json
// Request body
{
  "name": "Vợt Selkirk Power Air",
  "description": "Mô tả...",
  "categoryId": "uuid",
  "brandId": "uuid",
  "basePrice": 1850000,
  "specs": "{\"weight\": \"215g\"}"   // optional, JSON string
}

// Response 201
// Trả về ProductDetailDto (xem schema GET /products/{id})
// Lưu ý: status mặc định = "Draft" — cần gọi PATCH /products/{id}/publish mới hiện ra ở GET /products
```

---

### `PUT /products/{id}` *(Admin)*
Cập nhật thông tin sản phẩm. Slug tự động tạo lại từ `name`.

```json
// Request body
{
  "name": "Vợt Selkirk Power Air v2",
  "description": "Mô tả mới...",
  "categoryId": "uuid",
  "brandId": "uuid",
  "basePrice": 1950000,
  "specs": "{\"weight\": \"210g\"}"
}

// Response 200: ProductDetailDto đã cập nhật
```

---

### `PATCH /products/{id}/publish` *(Admin)*
Chuyển sản phẩm từ `Draft` → `Active`. Yêu cầu sản phẩm phải có **ít nhất 1 ảnh** và **ít nhất 1 biến thể** trước khi publish.

- **Response 204**: publish thành công
- **Response 400**: chưa có ảnh hoặc chưa có biến thể
- **Response 400**: sản phẩm đang ở trạng thái `Hidden` (cần restore trước)

---

### `PATCH /products/{id}/restore` *(Admin)*
Chuyển sản phẩm từ `Hidden` → `Draft`.

- **Response 204**: restore thành công
- **Response 400**: sản phẩm không ở trạng thái `Hidden`

---

### `DELETE /products/{id}` *(Admin)*
**Xóa mềm** — chuyển `status = Hidden`. Không xóa vật lý. Sản phẩm không còn xuất hiện trong `GET /products` nhưng dữ liệu vẫn còn nguyên trong DB (đảm bảo đơn hàng cũ vẫn tham chiếu được).

- **Response 204**: ẩn thành công
- **Response 400**: sản phẩm đang ở trạng thái `Draft` — không cần ẩn, hãy dùng `PATCH /products/{id}/publish` để publish trước hoặc để nguyên Draft
- **Response 404**: sản phẩm không tồn tại

---

## 4. Product Variants

### `POST /products/{productId}/variants` *(Admin)*
```json
// Request body
{
  "sku": "SELKIRK-PA-RED",
  "attributesJson": "{\"color\": \"Đỏ\"}",   // optional
  "price": 1850000
}

// Response 201
{
  "id": "uuid",
  "sku": "SELKIRK-PA-RED",
  "attributesJson": "{\"color\": \"Đỏ\"}",
  "price": 1850000
}
```

- **Response 409**: SKU đã tồn tại trong sản phẩm này

---

### `PUT /products/{productId}/variants/{variantId}` *(Admin)*
```json
// Request body
{
  "sku": "SELKIRK-PA-RED-V2",
  "attributesJson": "{\"color\": \"Đỏ\", \"grip\": \"Small\"}",
  "price": 1900000
}

// Response 200: ProductVariantDto đã cập nhật
```

---

### `DELETE /products/{productId}/variants/{variantId}` *(Admin)*
- **Response 204**: xóa thành công
- **Response 404**: biến thể không tồn tại trong sản phẩm này

---

## 5. Product Images

### `POST /products/{productId}/images` *(Admin)*
Upload ảnh sản phẩm lên Cloudinary.

**Content-Type:** `multipart/form-data`

| Field | Kiểu | Bắt buộc | Mô tả |
|---|---|---|---|
| `file` | File | ✅ | File ảnh (.jpg, .jpeg, .png, .webp), tối đa 5MB |
| `sortOrder` | int | Không | Thứ tự hiển thị (mặc định 0) — ảnh `sortOrder` nhỏ nhất là thumbnail |
| `variantId` | uuid | Không | Gắn ảnh với 1 biến thể cụ thể; null = ảnh chung cho sản phẩm |
| `isSizeChart` | bool | Không | `true` = ảnh bảng size (mặc định `false`) |

**Response 201:**
```json
{
  "id": "uuid",
  "publicId": "picklehub/products/abc123",
  "url": "https://res.cloudinary.com/your-cloud/image/upload/picklehub/products/abc123.webp",
  "variantId": null,
  "sortOrder": 0,
  "isSizeChart": false
}
```

---

### `DELETE /products/{productId}/images/{imageId}` *(Admin)*
Xóa ảnh khỏi DB **và** xóa file trên Cloudinary.

- **Response 204**: xóa thành công
- **Response 404**: ảnh không tồn tại trong sản phẩm này

---

## Ghi chú dành riêng cho Cart & Order Service

### Luồng thêm vào giỏ hàng

```
1. Gọi GET /products/{slug-or-id}
   → Lấy thông tin sản phẩm + danh sách variants

2. Khách chọn variant (màu sắc, size...)
   → Lưu variants[].id làm productVariantId trong cart_item

3. Snapshot các thông tin sau TẠI THỜI ĐIỂM THÊM VÀO GIỎ:
   - productNameSnapshot  ← product.name
   - productImageUrl      ← images[0].url (ảnh isSizeChart=false, sortOrder nhỏ nhất)
   - unitPrice            ← variants[selected].price

   KHÔNG lấy lại giá từ Catalog khi tạo đơn hàng — dùng snapshot đã lưu.
```

### Trường nào Cart & Order cần lưu lại (không query lại Catalog)

| Trường cần snapshot | Lấy từ | Lưu vào |
|---|---|---|
| `productVariantId` | `variants[].id` | `cart_item.product_variant_id`, `order_item.product_variant_id` |
| `productNameSnapshot` | `product.name` | `cart_item.product_name_snapshot`, `order_item.product_name_snapshot` |
| `productImageUrl` | `images[0].url` (thumbnail) | `cart_item.product_image_url`, `order_item.product_image_url` |
| `unitPrice` | `variants[selected].price` | `order_item.unit_price` |

### Kiểm tra tồn kho

Cart & Order **không** gọi Catalog để kiểm tra tồn kho — tồn kho thuộc về **Inventory Service**. Flow đúng:

```
Thêm vào giỏ  → không cần check tồn kho
Đặt hàng      → Cart & Order gọi Inventory Service để check + trừ tồn kho
```

### Sản phẩm bị ẩn sau khi đã thêm vào giỏ

Nếu Admin ẩn sản phẩm (`DELETE /products/{id}`) trong khi khách đã có sản phẩm đó trong giỏ hàng: Cart & Order Service nên tự handle (ví dụ đánh dấu cart_item là `unavailable`, không block luồng xem giỏ hàng). Catalog Service không thông báo chủ động về sự kiện này — Cart & Order tự kiểm tra lại khi cần (khi checkout, gọi `GET /products/{id}` để verify `status = Active`).

---

## Tóm tắt endpoints

| Method | Route | Auth | Mô tả |
|---|---|---|---|
| GET | `/brands` | Không | Danh sách brand |
| POST | `/brands` | Admin | Tạo brand |
| PUT | `/brands/{id}` | Admin | Sửa brand |
| DELETE | `/brands/{id}` | Admin | Xóa brand |
| GET | `/categories` | Không | Danh mục (cây/phẳng) |
| POST | `/categories` | Admin | Tạo danh mục |
| PUT | `/categories/{id}` | Admin | Sửa danh mục |
| DELETE | `/categories/{id}` | Admin | Xóa danh mục |
| GET | `/products` | Không | Tìm kiếm & lọc sản phẩm |
| GET | `/products/{slug-or-id}` | Không | Chi tiết sản phẩm |
| POST | `/products` | Admin | Tạo sản phẩm (Draft) |
| PUT | `/products/{id}` | Admin | Sửa thông tin sản phẩm |
| PATCH | `/products/{id}/publish` | Admin | Publish (Draft → Active) |
| PATCH | `/products/{id}/restore` | Admin | Restore (Hidden → Draft) |
| DELETE | `/products/{id}` | Admin | Ẩn sản phẩm (soft delete) |
| POST | `/products/{id}/variants` | Admin | Thêm biến thể |
| PUT | `/products/{id}/variants/{variantId}` | Admin | Sửa biến thể |
| DELETE | `/products/{id}/variants/{variantId}` | Admin | Xóa biến thể |
| POST | `/products/{id}/images` | Admin | Upload ảnh (Cloudinary) |
| DELETE | `/products/{id}/images/{imageId}` | Admin | Xóa ảnh |
