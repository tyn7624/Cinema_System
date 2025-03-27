document.addEventListener("DOMContentLoaded", function () {
    let bookingData = JSON.parse(localStorage.getItem("bookingData"));
    console.log("Dữ liệu bookingData từ localStorage:", bookingData);

    if (!bookingData) {
        console.warn("Không có dữ liệu bookingData.");
        return;
    }

    let movieTitle = document.getElementById("movie-title");
    let cinemaName = document.getElementById("cinema-name");
    let showTime = document.getElementById("show-time");   
    let selectedSeats = document.getElementById("selected-seats");
    let ticketPrice = document.getElementById("ticket-price");
    let foodList = document.getElementById("food-list");
    let totalPriceElement = document.getElementById("total-price");

    console.log("Các phần tử DOM:", { movieTitle, cinemaName, showTime, selectedSeats, ticketPrice, foodList, totalPriceElement });

    if (!movieTitle || !cinemaName || !showTime || !selectedSeats || !ticketPrice || !foodList || !totalPriceElement) {
        console.error("Lỗi: Một hoặc nhiều phần tử không tồn tại trong DOM.");
        return;
    }

    movieTitle.innerText = bookingData.Movie;
    cinemaName.innerText = bookingData.Cinema;
    showTime.innerText = bookingData.ShowTime;
    selectedSeats.innerText = bookingData.Seats.join(", ");
    ticketPrice.innerText = bookingData.TicketPrice.toLocaleString() + " VNĐ/vé";

    let totalPrice = bookingData.Seats.length * bookingData.TicketPrice;

    foodList.innerHTML = "";
    bookingData.Foods.forEach(food => {
        let item = document.createElement("li");
        item.classList.add("list-group-item");
        let totalFoodPrice = food.quantity * parseInt(food.price);
        totalPrice += totalFoodPrice;
        item.innerText = `${food.name} x${food.quantity} - ${totalFoodPrice.toLocaleString()} VNĐ`;
        foodList.appendChild(item);
    });

    totalPriceElement.innerText = totalPrice.toLocaleString() + " VNĐ";
});