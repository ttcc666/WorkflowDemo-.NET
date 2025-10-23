namespace WorkFlowDemo.Models.Dtos
{
    /// <summary>
    /// 创建订单DTO
    /// </summary>
    public class CreateOrderDto
    {
        /// <summary>
        /// 产品ID
        /// </summary>
        public string ProductId { get; set; } = string.Empty;

        /// <summary>
        /// 购买数量
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 优惠券代码
        /// </summary>
        public string? CouponCode { get; set; }

        /// <summary>
        /// 订单金额
        /// </summary>
        public decimal OrderAmount { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public string UserId { get; set; } = string.Empty;
    }
}