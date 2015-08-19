﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MVCConfigurator.Domain.Models;

namespace MVCConfigurator.Domain.Repositories
    {
    public interface IOrderRepository
        {
        IList<Order> GetAllOrders();
        Order GetOrderById(int orderId);
        IList<Order> GetOrderByProduct(Product product);
        Order AddOrder(Order order);
        bool UpdateOrder(Order order);
        bool DeleteOrder(Order order);


        }
    }