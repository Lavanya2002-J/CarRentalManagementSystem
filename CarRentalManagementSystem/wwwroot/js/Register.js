// A reusable function to set up a password visibility toggle
function setupPasswordToggle(inputId, buttonId) {
    const passwordInput = document.getElementById(inputId);
    const toggleButton = document.getElementById(buttonId);

    if (passwordInput && toggleButton) {
        toggleButton.addEventListener('click', function () {
            const icon = this.querySelector('i');
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
    }
}

// Set up the toggle for the main password field
setupPasswordToggle('passwordInput', 'togglePassword');

// Set up the toggle for the confirm password field
setupPasswordToggle('confirmPasswordInput', 'toggleConfirmPassword');