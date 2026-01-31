function hideToast() {
    const toastElement = document.getElementsByClassName("page_toast_notification")[0];
    if (toastElement) {
        toastElement.style.display = "none";
        toastElement.remove();
    }
    return;
}
function showLoader() {
    const loaderElement = document.getElementsByClassName("page_loader")[0];
    if (loaderElement) {
        loaderElement.style.display = "flex";
    }
}
function hideLoader() {
    const loaderElement = document.getElementsByClassName("page_loader")[0];
    if (loaderElement) {
        loaderElement.style.display = "none";
    }
}


$(document).ready(function () {
    hideLoader();

    $(".form_signIn").on("submit", function (e) {
        var form = $(this);
        if (form.valid()) {
            showLoader();
        }
    });

    $(".form_registration").on("submit", function (e) {
        var form = $(this);
        if (form.valid()) {
            showLoader();
        }
    });
});