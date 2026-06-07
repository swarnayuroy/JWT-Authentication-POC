function hideToast() {
    const toastElement = document.getElementsByClassName("page_toast_notification")[0];
    if (toastElement) {
        toastElement.style.display = "none";
        toastElement.remove();
    }
    return;
}
function hideToastList(elementOrEvent) {
    let notificationContainer = null;

    // Handle both onclick event and timeout calls
    if (elementOrEvent) {
        if (elementOrEvent instanceof Event) {
            // Called from onclick event - traverse from event target
            const closeButton = elementOrEvent.target.closest('.page_toast_notification_close_btn');
            notificationContainer = closeButton ? closeButton.closest('.notification-container') : null;
        } else if (elementOrEvent instanceof Element) {
            // Called from timeout with button element as context
            notificationContainer = elementOrEvent.closest('.notification-container');
        }
    }

    // Fallback: if not found, try to find any notification container
    if (!notificationContainer) {
        notificationContainer = document.querySelector('.notification-container');
    }

    if (notificationContainer) {
        // Add fade-out animation class
        notificationContainer.classList.add('notification-fade-out');

        // Remove the notification after animation completes (300ms)
        setTimeout(() => {
            notificationContainer.remove();

            // Check if there are any notifications left
            const notificationList = document.querySelector('.notification-list-container');
            if (notificationList && notificationList.children.length === 0) {
                // Remove the entire container if no notifications remain
                notificationList.remove();
            }
        }, 300);
    }
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