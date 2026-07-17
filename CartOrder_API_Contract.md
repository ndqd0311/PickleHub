# Cart & Order Service — API Contract

**Phiên bản:** 1.1  
**Ngày:** 17/07/2026  
**Dành cho:** Frontend và API Gateway  

---

## 1. Tổng quan

**Cart & Order Service** chịu trách nhiệm quản lý giỏ hàng tạm thời của khách hàng, thực hiện quy trình đặt hàng (Checkout), hủy đơn, và quản lý các trạng thái đơn hàng. Service này **tự xác thực JWT Bearer Token** thông qua cơ chế giải mã Token trực tiếp ở tầng Microservice (thay vì phụ thuộc vào Gateway đính kèm Headers).

*   **Base URL (qua API Gateway):** `http://localhost:5000/orders` và `http://localhost:5000/cart`
*   **Format dữ liệu:** JSON

---

## 2. Xác thực & Phân quyền (JWT Bearer Token)

Mọi HTTP Request từ ngoài vào hệ thống cần đính kèm JWT Token vào Header **`Authorization`** theo định dạng Bearer:

`Authorization: Bearer <mã_token_jwt>`

Trình kiểm duyệt xác thực của Cart & Order Service sẽ giải mã Token này để lấy ra các Claims:
*   **`sub` (hoặc `NameIdentifier`):** Chứa ID định danh của người dùng (UUID).
*   **`role` (hoặc `ClaimTypes.Role`):** Chứa vai trò của người dùng (`Customer` hoặc `Admin`).

*   **Customer APIs:** Yêu cầu Token hợp lệ. Hệ thống tự trích xuất ID người dùng từ Token để truy xuất dữ liệu cá nhân.
*   **Admin APIs:** Yêu cầu Token hợp lệ và chứa Claim `role` bằng `"Admin"`.

---

## 3. Cấu trúc Lỗi Chuẩn

Khi xảy ra lỗi (Validation, Not Found, Forbidden,...), hệ thống sẽ trả về HTTP Status code tương ứng kèm theo cấu trúc JSON sau:

```json
{
  "error": {
    "message": "Mô tả nguyên nhân lỗi cụ thể",
    "errors": {               // Chỉ xuất hiện khi lỗi Validation nhiều trường dữ liệu
      "fieldName": ["Chi tiết lỗi 1", "Chi tiết lỗi 2"]
    }
  }
}
```

| HTTP Status | Trường hợp xảy ra |
| :--- | :--- |
| **400 Bad Request** | Lỗi định dạng dữ liệu đầu vào (ví dụ: số lượng <= 0, thiếu thông tin giao hàng). |
| **401 Unauthorized** | Token không hợp lệ hoặc thiếu Token trong Header `Authorization`. |
| **403 Forbidden** | Người dùng cố tình truy cập các API của Admin khi không có quyền Admin. |
| **404 Not Found** | Không tìm thấy đơn hàng hoặc sản phẩm trong giỏ hàng. |
| **500 Internal Error** | Lỗi máy chủ hệ thống hoặc mất kết nối database. |

---

## 4. Các mô hình Dữ liệu (Schemas)

### CartDto
```json
{
  "cartId": "uuid",
  "items": [
    {
      "cartItemId": "uuid",
      "productId": "uuid",
      "productName": "string",
      "imageUrl": "string",
      "unitPrice": 1850000.00,
      "quantity": 2,
      "subTotal": 3700000.00
    }
  ],
  "grandTotal": 3700000.00
}
```

### OrderDto *(Chi tiết đơn hàng)*
```json
{
  "orderId": "uuid",
  "totalPrice": 1850000.00,
  "status": "Pending",        // Các trạng thái: Pending, Confirmed, Shipping, Completed, Cancelled
  "createdAt": "2026-07-13T09:00:00Z",
  "shippingFullName": "Nguyễn Văn A",
  "shippingPhone": "0912345678",
  "shippingAddress": "123 Đường Láng",
  "shippingCity": "Hà Nội",
  "items": [
    {
      "productId": "uuid",
      "productName": "Vợt Selkirk Power Air (Snapshot)",
      "unitPrice": 1850000.00,
      "quantity": 1,
      "subTotal": 1850000.00
    }
  ]
}
```

### OrderSummaryDto *(Dòng tóm tắt đơn hàng)*
```json
{
  "orderId": "uuid",
  "totalPrice": 1850000.00,
  "status": "Pending",
  "createdAt": "2026-07-13T09:00:00Z",
  "itemCount": 1
}
```

---

## 5. Danh sách các API Endpoint

### 5.1. Nhóm API Giỏ hàng (Cart)

#### Lấy chi tiết giỏ hàng hiện tại
*   **Method / Route:** `GET /cart`
*   **Auth:** Customer (Yêu cầu JWT Bearer Token)
*   **Response 200:** `CartDto` (Nếu chưa có giỏ hàng, hệ thống trả về danh sách items rỗng).

#### Thêm sản phẩm vào giỏ hàng
*   **Method / Route:** `POST /cart/items`
*   **Auth:** Customer (Yêu cầu JWT Bearer Token)
*   **Request Body:**
    ```json
    {
      "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "quantity": 2
    }
    ```
*   **Response 200:** `uuid` (ID của dòng CartItem vừa được tạo hoặc cộng dồn).

#### Cập nhật số lượng sản phẩm trong giỏ hàng
*   **Method / Route:** `PUT /cart/items`
*   **Auth:** Customer (Yêu cầu JWT Bearer Token)
*   **Request Body:**
    ```json
    {
      "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "quantity": 5                         // Số lượng mới thay thế hoàn toàn số lượng cũ
    }
    ```
*   **Response 200:** `uuid` (ID của sản phẩm vừa được cập nhật).

#### Xóa sản phẩm ra khỏi giỏ hàng
*   **Method / Route:** `DELETE /cart/items/{productId}`
*   **Auth:** Customer (Yêu cầu JWT Bearer Token)
*   **Response 200:** `uuid` (ID của sản phẩm vừa được xóa).

---

### 5.2. Nhóm API Đơn hàng dành cho Khách hàng (Customer Orders)

#### Tiến hành Đặt hàng (Checkout)
*   **Method / Route:** `POST /orders`
*   **Auth:** Customer (Yêu cầu JWT Bearer Token)
*   **Request Body:**
    ```json
    {
      "shippingFullName": "Nguyễn Văn A",
      "shippingPhone": "0912345678",
      "shippingAddress": "123 Đường Láng",
      "shippingCity": "Hà Nội"
    }
    ```
*   **Response 200:** `uuid` (ID đơn hàng `OrderId` vừa được tạo).
*   **Logic ngầm:** Clear giỏ hàng nội bộ và phát sự kiện `OrderCreatedEvent` để trừ kho.

#### Xem lịch sử đơn hàng cá nhân
*   **Method / Route:** `GET /orders`
*   **Auth:** Customer (Yêu cầu JWT Bearer Token)
*   **Response 200:** Mảng các `OrderSummaryDto` (Sắp xếp đơn mới nhất lên đầu).

#### Xem chi tiết một đơn hàng cá nhân
*   **Method / Route:** `GET /orders/{id}`
*   **Auth:** Customer (Yêu cầu JWT Bearer Token - phải đúng chủ đơn hàng)
*   **Response 200:** `OrderDto`
*   **Response 403:** Nếu truy cập đơn hàng của tài khoản khác.

#### Yêu cầu hủy đơn hàng
*   **Method / Route:** `PUT /orders/{id}/cancel`
*   **Auth:** Customer (Yêu cầu JWT Bearer Token - phải đúng chủ đơn hàng)
*   **Request Body:**
    ```json
    {
      "reason": "Tôi không muốn mua nữa"
    }
    ```
*   **Response 200:** `true` (Hủy thành công).
*   **Lưu ý:** Chỉ cho phép hủy khi trạng thái đơn hàng là `Pending` hoặc `Confirmed`. Phát sự kiện `OrderCancelledEvent` để hoàn kho.

---

### 5.3. Nhóm API Quản trị dành cho Admin (Admin Orders)

#### Lấy danh sách toàn bộ đơn hàng của hệ thống (Phân trang + Lọc)
*   **Method / Route:** `GET /orders/admin`
*   **Auth:** Admin (Yêu cầu JWT Bearer Token chứa `role: Admin`)
*   **Query Parameters:**
    *   `status` (String, Optional): Lọc theo trạng thái đơn hàng (`Pending`, `Confirmed`, `Shipping`, `Completed`, `Cancelled`).
    *   `searchKeyword` (String, Optional): Lọc theo tên người nhận hoặc số điện thoại.
    *   `page` (Int, Mặc định = 1)
    *   `pageSize` (Int, Mặc định = 20)
*   **Response 200:**
    ```json
    {
      "items": [ "OrderSummaryDto" ],
      "page": 1,
      "pageSize": 20,
      "totalItems": 150,
      "totalPages": 8,
      "hasNextPage": true,
      "hasPreviousPage": false
    }
    ```

#### Xem chi tiết đơn hàng bất kỳ
*   **Method / Route:** `GET /orders/admin/{id}`
*   **Auth:** Admin (Yêu cầu JWT Bearer Token chứa `role: Admin`)
*   **Response 200:** `OrderDto` (Không lọc theo chủ sở hữu).

#### Cập nhật trạng thái xử lý đơn hàng
*   **Method / Route:** `PUT /orders/admin/{id}/status`
*   **Auth:** Admin (Yêu cầu JWT Bearer Token chứa `role: Admin`)
*   **Request Body:**
    ```json
    {
      "status": "Shipping"                 // Trạng thái mới muốn đổi sang
    }
    ```
*   **Response 200:** `string` (Trạng thái đơn hàng sau khi đổi, ví dụ: `"Shipping"`).

#### Xem báo cáo tóm tắt tình hình kinh doanh (Dashboard Summary)
*   **Method / Route:** `GET /orders/admin/dashboard-summary`
*   **Auth:** Admin (Yêu cầu JWT Bearer Token chứa `role: Admin`)
*   **Response 200:**
    ```json
    {
      "totalRevenue": 154800000.00,        // Chỉ tính từ đơn Confirmed & Completed
      "totalOrders": 120,
      "pendingOrders": 15,
      "completedOrders": 95,
      "cancelledOrders": 10
    }
    ```
