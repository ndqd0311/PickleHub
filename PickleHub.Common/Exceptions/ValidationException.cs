//namespace PickleHub.Common.Exceptions
//{
//    public class ValidationException : Exception
//    {

//        // Lỗi theo từng field, ví dụ: { "email": ["Email không hợp lệ"], "password": ["Mật khẩu quá ngắn"] }
//        // Rỗng nếu chỉ là 1 lỗi tổng quát (dùng constructor đơn giản bên dưới).
//        public Dictionary<string, string[]> Errors { get; }

//        // Constructor cho lỗi đơn giản (giữ tương thích với toàn bộ code đã viết trước đó)
//        public ValidationException( string message) : base(message)
//        {
//            Errors = new Dictionary<string, string[]>();
//        }

//        // Constructor mới cho lỗi nhiều field
//        public ValidationException( Dictionary<string, string[]> errors)
//            : base("Dữ liệu gửi lên không hợp lệ.")
//        {
//            Errors = errors;
//        }
//    }
//}
