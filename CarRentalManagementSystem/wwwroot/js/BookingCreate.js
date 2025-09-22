document.addEventListener('DOMContentLoaded', function () {
    // Get references to elements
    const pickupDateInput = document.getElementById('pickupDate');
    const returnDateInput = document.getElementById('returnDate');
    const dailyRateElement = document.getElementById('dailyRate');
    const totalCostElement = document.getElementById('totalCost');

    // Get the daily rate from the data attribute
    const dailyRate = parseFloat(dailyRateElement.getAttribute('data-rate'));
    let returnDatePicker = null;

    // Function to calculate and display the total cost
    function calculateTotal() {
        const pickupDate = pickupDateInput._flatpickr.selectedDates[0];
        const returnDate = returnDateInput._flatpickr.selectedDates[0];

        if (pickupDate && returnDate) {
            const timeDiff = returnDate.getTime() - pickupDate.getTime();
            // Ensure we count rental days correctly; picking up today and returning tomorrow is 1 day.
            const dayDiff = Math.max(1, Math.ceil(timeDiff / (1000 * 3600 * 24)));

            if (dayDiff > 0) {
                const total = dayDiff * dailyRate;
                totalCostElement.textContent = 'LKR ' + total.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
            } else {
                totalCostElement.textContent = 'LKR 0.00';
            }
        } else {
            totalCostElement.textContent = 'LKR 0.00';
        }
    }

    // Initialize Flatpickr for the pickup date
    flatpickr(pickupDateInput, {
        altInput: true,
        altFormat: "F j, Y",
        dateFormat: "Y-m-d",
        minDate: "today",
        onChange: function (selectedDates) {
            const pickupDate = selectedDates[0];
            if (pickupDate) {
                // Enable and configure the return date picker
                returnDateInput.disabled = false;

                if (returnDatePicker) {
                    returnDatePicker.destroy(); // Destroy previous instance to reconfigure
                }

                returnDatePicker = flatpickr(returnDateInput, {
                    altInput: true,
                    altFormat: "F j, Y",
                    dateFormat: "Y-m-d",
                    minDate: new Date(pickupDate.fp_incr(1)), // Set min return date to day after pickup
                    onChange: calculateTotal
                });
                // Open the return date picker automatically
                returnDatePicker.open();
            } else {
                // Disable return date if pickup is cleared
                returnDateInput.disabled = true;
                if (returnDatePicker) returnDatePicker.clear();
            }
            calculateTotal();
        }
    });
});