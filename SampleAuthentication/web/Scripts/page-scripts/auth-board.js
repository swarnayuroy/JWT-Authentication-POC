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

    $(".verification_close_button").on("click", function (e) {
        e.preventDefault();  // Prevent default link behavior

        showLoader();

        // Small delay before navigation to allow loader to display
        setTimeout(function () {
            window.location.href = `${window.location.origin}/Home/Logout`;
        }, 1000);
    });

    $(".verify_link").on("click", function (e) {
        e.preventDefault();  // Prevent default link behavior

        showLoader();
        const verifyUser = "verify"

        // Small delay before navigation to allow loader to display
        setTimeout(function () {
            window.location.href = `${window.location.origin}/Home/VerifyAccount?value=${verifyUser}&haveOtpValue=${false}`;
        }, 1000);
    });

    $(".submit_link").on("click", function (e) {
        e.preventDefault();  // Prevent default link behavior

        showLoader();
        const value = document.getElementById('otp_input').value.toString();

        // Small delay before navigation to allow loader to display
        setTimeout(function () {
            window.location.href = `${window.location.origin}/Home/VerifyAccount?value=${value}&haveOtpValue=${true}`;
        }, 1000);
    });

    const otpInput = $(".form_otp_input input");
    if (otpInput) {
        // Allow only digits on keypress
        otpInput.on("keypress", function (e) {
            const charCode = e.which || e.keyCode;

            // Allow: backspace, delete, arrow keys
            if (charCode === 8 || charCode === 46 || charCode === 37 || charCode === 39) {
                return;
            }

            // Block non-numeric
            if (charCode < 48 || charCode > 57) {
                e.preventDefault();
            }
        });

        // Handle paste / fallback
        otpInput.on("input", function () {
            this.value = this.value.replace(/\D/g, '').slice(0, 6);
        });
    }

    $(".resend_otp_link").on("click", function (e) {
        e.preventDefault();  // Prevent default link behavior

        showLoader();

        // Store the href to avoid context issues
        var resendOTP_Url = $(this).attr('href');

        // Small delay before navigation to allow loader to display
        setTimeout(function () {
            window.location.href = resendOTP_Url;
        }, 1000);
    });

    // Pagination handler (delegated to support AJAX-injected pagination controls)
    $(document).on("click", ".pagination-btn:not(.disabled)", function (e) {
        e.preventDefault();

        var pageUrl = $(this).attr('href');
        var currentSearch = $(".search-textbox").val().trim();

        if (currentSearch.length >= 3) {
            var urlParams = new URLSearchParams(pageUrl.split('?')[1] || '');
            var page = urlParams.get('page') || 1;
            fetchSearchResults(currentSearch, parseInt(page));
        } else {
            showLoader();
            setTimeout(function () {
                window.location.href = pageUrl;
            }, 500);
        }
    });

    // Search handler with 3-second debounce
    var searchDebounceTimer = null;

    $(".search-textbox").on("input", function () {
        var searchText = $(this).val().trim();

        clearTimeout(searchDebounceTimer);

        if (searchText.length === 0) {
            searchDebounceTimer = setTimeout(function () {
                fetchSearchResults("", 1);
            }, 2000);
        } else if (searchText.length >= 3) {
            searchDebounceTimer = setTimeout(function () {
                fetchSearchResults(searchText, 1);
            }, 2000);
        }
    });

    $(document).on("click", ".view-user-detail", function (e) {

        e.preventDefault();

        var detailUrl = $(this).attr("href");

        showLoader();

        $.ajax({
            url: detailUrl,
            type: "GET",
            success: function (partialHtml) {
                renderUserDetailModal(partialHtml);
                hideLoader();
            },
            error: function (xhr) {
                if (xhr && xhr.responseText) {
                    renderUserDetailModal(xhr.responseText);
                }
                hideLoader();
            }
        });
    });

    function renderUserDetailModal(modalHtml) {
        $("#user-modal-root").html(modalHtml);
        $(".user-detail-layout").addClass("is-visible");
    }

    function fetchSearchResults(searchText, page) {
        showLoader();

        var url = '/Home/PaginateOperation?page=' + page;
        if (searchText.length > 0) {
            url += '&searchText=' + encodeURIComponent(searchText);
        }

        $.ajax({
            url: url,
            type: 'GET',
            success: function (responseHtml) {
                var parsedHtml = $(responseHtml);

                var newSearchResults = parsedHtml.find('#fetched-results');
                if (newSearchResults.length === 0) {
                    newSearchResults = parsedHtml.filter('#fetched-results');
                }

                if (newSearchResults.length > 0) {
                    $('#fetched-results').html(newSearchResults.html());
                }

                hideLoader();
            },
            error: function () {
                hideLoader();
            }
        });
    }
});

$(document).on("click", ".user-detail-close-button, .user-detail-layout", function (e) {
    if (
        $(e.target).hasClass("user-detail-layout") ||
        $(e.target).hasClass("user-detail-close-button") ||
        $(e.target).closest(".user-detail-close-button").length > 0
    ) {
        e.preventDefault();
        $("#user-modal-root").empty();
    }
});

$(document).on("click", ".user-detail-container", function (e) {
    e.stopPropagation();
});