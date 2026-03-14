$(document).ready(function () {
    hideLoader();

    $(".logout-btn").on("click", function (e) {
        e.preventDefault();  // Prevent default link behavior

        showLoader();

        // Store the href to avoid context issues
        var logoutUrl = $(this).attr('href');

        // Small delay before navigation to allow loader to display
        setTimeout(function () {
            window.location.href = logoutUrl;
        }, 1000);
    });

    // Pagination handler
    $(".pagination-btn:not(.disabled)").on("click", function (e) {
        e.preventDefault();

        showLoader();

        var pageUrl = $(this).attr('href');

        setTimeout(function () {
            window.location.href = pageUrl;
        }, 500);
    });
});