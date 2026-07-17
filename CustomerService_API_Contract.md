# Customer Service — API Contract
**Phiên bản:** 1.0  
**Ngày:** 01/07/2026  
**Tác giả:** (Người 1)  
**Dành cho:** Người 2 (Cart & Order, Payment, Notification, Review)

---

## Tổng quan

Customer Service quản lý hồ sơ khách hàng và sổ địa chỉ. Service này **không xử lý authentication** — đó là nhiệm vụ của Authen Service. Customer Service nhận `X-User-Id` và `X-User-Role` từ Gateway (đã được validate JWT trước đó).

**Base URL:** `http://localhost:5003` (internal) / `/customers` (qua Gateway)

---

## Xác thực & Phân quyền

Mọi request đều đi qua **API Gateway**. Gateway validate JWT và forward 2 header:

| Header | Mô tả |
|--------|-------|
| `X-User-Id` | UUID của user đang đăng nhập |
| `X-User-Role` | Role: `Admin` hoặc `Customer` |

- **Public** — không cần header (không có endpoint public trong Customer Service)
- **Authenticated** — cần `X-User-Id` hợp lệ
- **Admin only** — cần `X-User-Role: Admin`

---

## Data Models

### CustomerDto
```json
{
  "id": "uuid",
  "email": "string",
  "fullName": "string",
  "phoneNumber": "string | null",
  "avatarUrl": "string | null",
  "isBlocked": "boolean",
  "createdAt": "datetime"
}
```

### AddressDto
```json
{
  "id": "uuid",
  "fullName": "string",
  "phoneNumber": "string",
  "province": "string",
  "district": "string",
  "ward": "string",
  "streetAddress": "string",
  "isDefault": "boolean"
}
```

### CustomerSummaryDto *(dùng cho Admin Dashboard)*
```json
{
  "id": "uuid",
  "email": "string",
  "fullName": "string",
  "phoneNumber": "string | null",
  "isBlocked": "boolean",
  "createdAt": "datetime"
}
```

---

## Endpoints

---

### 1. Lấy thông tin Customer đang đăng nhập

```
GET /customers/me
```

**Auth:** Authenticated (Customer hoặc Admin)

**Response 200:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "customer@example.com",
  "fullName": "Nguyễn Văn A",
  "phoneNumber": "0912345678",
  "avatarUrl": null,
  "isBlocked": false,
  "createdAt": "2026-01-15T08:00:00Z"
}
```

**Dùng khi:** Hiển thị Customer Panel (tên, avatar trên header).

---

### 2. Cập nhật thông tin Customer đang đăng nhập

```
PUT /customers/me
```

**Auth:** Authenticated

**Request body:**
```json
{
  "fullName": "Nguyễn Văn A",
  "phoneNumber": "0912345678"
}
```

**Response 200:** `CustomerDto`

---

### 3. Lấy sổ địa chỉ của Customer đang đăng nhập

```
GET /customers/me/addresses
```

**Auth:** Authenticated

**Response 200:**
```json
[
  {
    "id": "uuid",
    "fullName": "Nguyễn Văn A",
    "phoneNumber": "0912345678",
    "province": "Hà Nội",
    "district": "Cầu Giấy",
    "ward": "Dịch Vọng",
    "streetAddress": "123 Xuân Thủy",
    "isDefault": true
  }
]
```

**Dùng khi:** Checkout — hiển thị sẵn danh sách địa chỉ để customer chọn.

---

### 4. Thêm địa chỉ mới

```
POST /customers/me/addresses
```

**Auth:** Authenticated

**Request body:**
```json
{
  "fullName": "Nguyễn Văn A",
  "phoneNumber": "0912345678",
  "province": "Hà Nội",
  "district": "Cầu Giấy",
  "ward": "Dịch Vọng",
  "streetAddress": "123 Xuân Thủy",
  "isDefault": false
}
```

**Response 201:** `AddressDto`

**Lưu ý:** Nếu `isDefault: true`, tất cả địa chỉ khác của customer đó sẽ được set `isDefault: false` tự động.

---

### 5. Cập nhật địa chỉ

```
PUT /customers/me/addresses/{addressId}
```

**Auth:** Authenticated  
**Lưu ý:** Customer chỉ được sửa địa chỉ của chính mình.

**Request body:** Giống POST

**Response 200:** `AddressDto`

---

### 6. Xóa địa chỉ

```
DELETE /customers/me/addresses/{addressId}
```

**Auth:** Authenticated

**Response 204:** No content

**Lưu ý:** Không cho xóa địa chỉ mặc định nếu còn địa chỉ khác — phải đặt địa chỉ khác làm mặc định trước.

---

### 7. Đặt địa chỉ làm mặc định

```
PATCH /customers/me/addresses/{addressId}/set-default
```

**Auth:** Authenticated

**Response 200:** `AddressDto`

---

### 8. Lấy thông tin Customer theo ID *(internal — dùng cho service khác)*

```
GET /customers/{id}
```

**Auth:** Admin only hoặc internal service call (qua header `X-Internal-Service: true`)

**Response 200:** `CustomerDto`

**Response 404:**
```json
{
  "error": { "message": "Không tìm thấy khách hàng." }
}
```

**Dùng khi:** Order Service cần lấy email customer để publish event.

---

### 9. Lấy địa chỉ theo ID *(internal — dùng cho Order Service)*

```
GET /customers/addresses/{addressId}
```

**Auth:** Admin only hoặc internal service call

**Response 200:** `AddressDto`

**Dùng khi:** Order Service gọi sang để snapshot địa chỉ giao hàng vào đơn hàng tại thời điểm đặt.

---

### 10. Lấy danh sách Customer *(Admin)*

```
GET /customers
```

**Auth:** Admin only

**Query params:**

| Param | Type | Default | Mô tả |
|-------|------|---------|-------|
| `keyword` | string | null | Tìm theo tên hoặc email |
| `isBlocked` | bool | null | Lọc theo trạng thái |
| `page` | int | 1 | Trang hiện tại |
| `pageSize` | int | 20 | Số bản ghi/trang |

**Response 200:**
```json
{
  "items": [ "CustomerSummaryDto" ],
  "totalCount": 100,
  "page": 1,
  "pageSize": 20
}
```

---

### 11. Lấy chi tiết Customer *(Admin)*

```
GET /customers/{id}/detail
```

**Auth:** Admin only

**Response 200:** `CustomerDto`

---

### 12. Block / Unblock Customer *(Admin)*

```
PATCH /customers/{id}/block
```

**Auth:** Admin only

**Request body:**
```json
{
  "isBlocked": true
}
```

**Response 204:** No content

**Lưu ý:** Khi block customer, Customer Service publish event `CustomerBlockedEvent` → Authen Service subscribe để thu hồi refresh token ngay lập tức.

---

### 13. Dashboard summary *(Admin)*

```
GET /customers/dashboard/summary
```

**Auth:** Admin only

**Response 200:**
```json
{
  "newCustomersThisWeek": 12,
  "totalCustomers": 340,
  "totalBlocked": 3
}
```

**Dùng khi:** Admin Dashboard ghép cùng data từ Order Service và Inventory Service.

---

## Error Response Format

Tất cả error đều trả về format thống nhất:

```json
{
  "error": {
    "message": "Mô tả lỗi",
    "errors": {
      "fieldName": ["Chi tiết lỗi validation"]
    }
  }
}
```

| HTTP Status | Khi nào |
|-------------|---------|
| 400 | Validation failed |
| 401 | Thiếu hoặc sai JWT |
| 403 | Không đủ quyền |
| 404 | Không tìm thấy resource |
| 409 | Conflict (trùng dữ liệu) |
| 500 | Lỗi server |

---

## Ghi chú cho người 2

### Khi implement Cart & Order Service:

1. **Checkout flow** — gọi `GET /customers/me/addresses` để hiển thị danh sách địa chỉ. Customer chọn 1 địa chỉ → gọi `GET /customers/addresses/{addressId}` để lấy thông tin đầy đủ → **snapshot toàn bộ vào bảng Order** (không lưu addressId, lưu thẳng tên/SĐT/địa chỉ). Lý do: sau này customer xóa địa chỉ thì đơn hàng cũ vẫn hiển thị đúng địa chỉ lúc đặt.

2. **Lấy email customer** — gọi `GET /customers/{id}` để lấy email, dùng trong payload của `OrderCreatedEvent` và `OrderStatusUpdatedEvent` để Notification Service gửi email.

3. **Header internal call** — khi service gọi lẫn nhau (không qua Gateway), thêm header `X-Internal-Service: true` để bypass auth check. Gateway chỉ add header này cho internal network, client bên ngoài không thể giả mạo.

### Khi implement Review Service:

Gọi Order Service để verify đơn hàng ở trạng thái `HoanThanh` trước khi cho phép review. API contract của Order Service do người 2 tự định nghĩa — xem file `OrderService_API_Contract.md`.
