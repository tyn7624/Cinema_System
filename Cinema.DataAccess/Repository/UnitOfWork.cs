﻿using System;
using System.Threading.Tasks;
using Cinema.DataAccess.Data;
using Cinema.DataAccess.Repository.IRepository;
using Cinema.Models;

namespace Cinema.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            Product = new ProductRepository(_db);
            Movie = new MovieRepository(_db);
            Coupon = new CouponRepository(_db);
            ApplicationUser = new ApplicationUserRepository(_db);
            showTime = new ShowTimeRepository(_db);
            OrderTable = new OrderRepository(_db);
            OrderDetail = new OrderDetailRepository(_db);
            ShowTimeSeat = new ShowTimeSeatRepository(_db);
            Cinema = new CinemaRepository(_db);

            Room = new RoomRepository(_db);
            Seat = new SeatRepository(_db);
        }

        public IMovieRepository Movie { get; private set; }
        public IProductRepository Product { get; private set; }
        public ICouponRepository Coupon { get; private set; }
        public IApplicationUserRepository ApplicationUser { get; private set; }
        public IShowTimeRepository showTime { get; private set; }
        public IOrderRepository OrderTable { get; private set; }
        public IOrderDetailRepository OrderDetail { get; private set; }
        public IShowTimeSeatRepository ShowTimeSeat { get; private set; }
        public ICinemaRepository Cinema { get; private set; }

        public IRoomRepository Room { get; private set; }
        public ISeatRepository Seat { get; private set; }
        //public IScheduleRepository Schedule { get; private set; }
        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
