$(function () {
    // Client-side filter for any table on the page with a #tableFilter box.
    $('#tableFilter').on('input', function () {
        var value = $(this).val().toLowerCase();
        // Searching should cover rows collapsed behind a "show all" button too.
        if (value) {
            $('.activity-extra').removeClass('d-none activity-extra');
            $('#showAllActivity').remove();
        }
        $('.table tbody tr').filter(function () {
            $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1);
        });
    });

    // Confirmation prompts declared as <form data-confirm="...">. Keeping the
    // (translated) text in an attribute instead of an inline onclick="confirm('…')"
    // means an apostrophe in a translation can never break the script and leave
    // the button dead.
    $(document).on('submit', 'form[data-confirm]', function (e) {
        if (!window.confirm(this.getAttribute('data-confirm'))) {
            e.preventDefault();
        }
    });
});
