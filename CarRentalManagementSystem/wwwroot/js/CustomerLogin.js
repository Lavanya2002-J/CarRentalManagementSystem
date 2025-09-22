// This function gets the current time and formats it
function updateLiveTime() {
    const dateTimeElement = document.getElementById('live-datetime');
    if (dateTimeElement) {
        const now = new Date();
        const options = {
            weekday: 'long', hour: 'numeric', minute: 'numeric', second: 'numeric',
            hour12: true, timeZone: 'Asia/Colombo', timeZoneName: 'short'
        };
        const formattedDateTime = new Intl.DateTimeFormat('en-US', options).format(now);
        dateTimeElement.innerHTML = `Good day! It's currently ${formattedDateTime} in Sri Lanka.`;
    }
}

// Run the function once immediately on page load
updateLiveTime();

// Set the function to run every second (1000 milliseconds)
setInterval(updateLiveTime, 1000);

// --- Password Toggle Script ---
document.getElementById('togglePassword').addEventListener('click', function () {
    const passwordInput = document.getElementById('passwordInput');
    const icon = this.querySelector('i');

    // Toggle the type attribute
    const type = passwordInput.getAttribute('type') === 'password' ? 'text' : 'password';
    passwordInput.setAttribute('type', type);

    // Toggle the icon
    if (type === 'password') {
        icon.classList.remove('fa-eye');
        icon.classList.add('fa-eye-slash');
    } else {
        icon.classList.remove('fa-eye-slash');
        icon.classList.add('fa-eye');
    }
});