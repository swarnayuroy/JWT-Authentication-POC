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