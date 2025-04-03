﻿using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Cinema_System.Areas.Request
{
    public class CountdownHub : Hub
    {
        private static int countdownTime = 30;
        private static bool isCounting = false;
        private static readonly object lockObj = new object();
        private static HashSet<string> selectedSeats = new HashSet<string>(); // To store selected seat IDs

        public async Task StartCountdown()
        {
            lock (lockObj)
            {
                if (isCounting)
                    return;
                isCounting = true;
            }

            while (countdownTime > 0)
            {
                await Clients.All.SendAsync("ReceiveCountdown", countdownTime);
                await Task.Delay(1000); // Đếm ngược mỗi giây
                countdownTime--;
            }

            await Clients.All.SendAsync("CountdownFinished", selectedSeats); // Send selected seats to clients
            ResetCountdown();
        }

        public void SelectSeat(string seatId)
        {
            lock (lockObj)
            {
                if (!selectedSeats.Contains(seatId))
                {
                    selectedSeats.Add(seatId);
                }
            }
        }

        public void DeselectSeat(string seatId)
        {
            lock (lockObj)
            {
                selectedSeats.Remove(seatId);
            }
        }

        private void ResetCountdown()
        {
            lock (lockObj)
            {
                countdownTime = 30;
                isCounting = false;
                selectedSeats.Clear(); // Clear the selected seats after countdown
            }
        }
    }
}