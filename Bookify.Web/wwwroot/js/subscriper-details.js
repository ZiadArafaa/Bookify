$("body").delegate(".js-renew", "click", function () {
    var btn = $(this);
    bootbox.confirm({
        message: /* btn.data("message") */ "Are You Sure To Renew Subscribtion.",
        buttons: {
            confirm: {
                label: 'Yes',
                className: 'btn-success'
            },
            cancel: {
                label: 'No',
                className: 'btn-secondary'
            }
        },
        callback: function (result) {
            if (result) {
                $.post({
                    url: btn.data("url"),
                    data: {
                        "__RequestVerificationToken": $("input[name = '__RequestVerificationToken']").val()
                    },
                    success: function () {
                        showSuccessMessage();
                    },
                    error: function (data) {
                        showErrorMessage(data);
                    }
                });
            }
        }
    });
});

$("body").delegate(".js-rental-cancel", "click", function () {
    var btn = $(this);
    bootbox.confirm({
        message: /* btn.data("message") */ "Are You Sure To Renew Subscribtion.",
        buttons: {
            confirm: {
                label: 'Yes',
                className: 'btn-danger'
            },
            cancel: {
                label: 'No',
                className: 'btn-secondary'
            }
        },
        callback: function (result) {
            if (result) {
                $.post({
                    url: btn.data("url"),
                    data: {
                        "__RequestVerificationToken": $("input[name = '__RequestVerificationToken']").val()
                    },
                    success: function (data) {
                        btn.parents('tr').remove();
                        var rentalCount = parseInt($('#numOfCopies').text());

                        $('#numOfCopies').text(rentalCount - data.copiesCount)

                        if ($('#rentalsTable tbody tr').length === 0) {
                            $('#rentalsTable').fadeOut(function () {
                                $('#alert').fadeIn()
                            });
                        }
                    },
                    error: function (data) {
                        showErrorMessage(data);
                    }
                });
            }
        }
    });
});