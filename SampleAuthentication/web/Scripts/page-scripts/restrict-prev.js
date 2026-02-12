// Prevent back navigation after logout
function preventBackNavigation() {
    window.history.forward();
}

// Disable back button
function disableBackButton() {
    window.history.pushState(null, "", window.location.href);
    window.onpopstate = function () {
        window.history.pushState(null, "", window.location.href);
    };
}

// Execute on page load
window.onload = function () {
    preventBackNavigation();
    disableBackButton();
};

// Additional protection using pageshow event
window.addEventListener('pageshow', function (event) {
    if (event.persisted) {
        // Page was loaded from cache (user pressed back button)
        window.location.href = 'Account/Login';
    }
});