﻿using Net.payOS.Types;

namespace Cinema_System.Areas.Request
{
    public class PaymentRequest
    {
        public long OrderCode { get; set; }  // ✅ Mã đơn hàng
        public int TotalAmount { get; set; }  // ✅ Tổng tiền
        public string Description { get; set; } = string.Empty; // ✅ Mô tả thanh toán
        public List<SeatSelectedRequest> Seats { get; set; } = new List<SeatSelectedRequest>(); // ✅ Ghế ngồi
        public List<ItemData> Items { get; set; } = new List<ItemData>(); // ✅ Danh sách item (đã đổi từ FoodItem)
        public string CancelUrl { get; set; } = string.Empty; // ✅ URL khi hủy
        public string ReturnUrl { get; set; } = string.Empty; // ✅ URL khi thanh toán thành công

        public string Coupon { get; set; } = string.Empty;
    }
}
