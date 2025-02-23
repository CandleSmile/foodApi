﻿namespace BusinessLayer.Services.Implementation
{
    using AutoMapper;
    using BusinessLayer.Dto;
    using BusinessLayer.Services.Interfaces;
    using BusinessLayer.Validators;
    using DataLayer.Items;
    using DataLayer.Repositories.Interfaces;
    using Microsoft.AspNetCore.Http;
    using System.Threading.Tasks;
    using Utilities.ErrorHandle;

    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly InputValidator _inputValidator = new InputValidator();

        /// <summary>
        /// Initializes a new instance of the <see cref="MealService"/> class.
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="mapper"></param>
        public OrderService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<OrderDto> MakeOrderAsync(CartDto cart)
        {
            var validator = new CartDtoValidator(cart);
            validator.Valid();
            var deliveryDateTime = await _unitOfWork.DeliveryDateTimeSlots.GetDeliveryDateTimeSlotByIdAsync(cart.DeliveryDateTimeSlotId);

            if (deliveryDateTime.IsAvailable)
            {
                var ctx = _httpContextAccessor.HttpContext;
                var user = ctx.User;

                Order order = new Order();
                decimal total = 0;

                foreach (var item in cart.CartItems)
                {
                    OrderItem orderItem = new OrderItem();
                    orderItem.MealId = item.MealId;
                    orderItem.Title = item.Title;
                    orderItem.Quantity = item.Quantity;
                    orderItem.Price = item.Price;
                    total += orderItem.Price * orderItem.Quantity;
                    order.OrderItems.Add(orderItem);
                }
                order.TotalPrice = total;
                order.DateCreated = DateTime.Now;
                order.IsSuccessful = true;
                order.IsPaid = false;
                order.User = await _unitOfWork.Users.GetUserByNameAsync(user.Identity.Name);
                order.DeliveryDate = deliveryDateTime.DeliveryDate.Date;
                order.TimeSlot = deliveryDateTime.TimeSlot.Time;
                deliveryDateTime.MadeOrders--;
                await _unitOfWork.Orders.AddAsync(order);
                await _unitOfWork.SaveAsync();
                var result = _mapper.Map<OrderDto>(order);
                return result;
            }

            throw new BadRequestExeption((int)ErrorCodes.NoValidData, "Soory, this time slot was closed. Please, choose another.");
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByUserAsync()
        {
            var ctx = _httpContextAccessor.HttpContext;
            var user = await _unitOfWork.Users.GetUserByNameAsync(ctx.User.Identity.Name);

            var res = await _unitOfWork.Orders.GetOrdersByUserIdAsync(user.Id);
            return _mapper.Map<IEnumerable<OrderDto>>(res);
        }
    }
}
