var updated;
var dataTables;
var editTable;

function applySelect2() {
    if ($('.js-select2').length > 0) {
        $('.js-select2').select2();
    }
    $('.js-select2').on('select2:select', function (e) {
        $('form').validate().element('#' + $(this).attr('id'))
    });

}
function renderDataTables() {
    if ($('.js-data-table').length > 0) {
        var lastIndex = $("#myTable tr td:last-child").index();
        dataTables = $('.js-data-table').DataTable(
            {
                columnDefs: [
                    { orderable: false, targets: lastIndex }
                ]
            });

    }
}
function showErrorMessage(Message) {
    Swal.fire({
        title: "Oops...",
        text: Message.responseText != undefined ? Message.responseText : 'Something went wrong !',
        icon: "error"
    });
}
function showSuccessMessage(Message = 'Good Job') {
    Swal.fire({
        title: "Successfully",
        text: Message,
        icon: "success"
    });
}
function OnModelBegin() {
    DisableButtonSubmit();
}
function OnModelComplete() {
    EnableButtonSubmit();
}
function DisableButtonSubmit() {
    $(':submit').find('span').removeClass('d-none');
    $(':submit').attr('disabled', 'disabled');
}
function EnableButtonSubmit() {
    $(':submit').find('span').addClass('d-none');
    $(':submit').removeAttr('disabled');
}
function OnModelSuccess(data) {
    $("#Modal").modal('hide');
    showSuccessMessage();

    if (editTable === undefined) {
        var row = $(data)

        if (updated != undefined) {
            $('tbody tr').each(function () {
                if ($(this).index() == updated) {

                    dataTables.row($(this)).remove().draw();
                    updated = undefined;
                }
            });
        }

        dataTables.row.add(row).draw();
    }

}
function OnModelFailure(data) {

    $("#Modal").modal('hide');
    showErrorMessage(data);
}
function fireSuccessMessage() {
    var message = $("#SuccessMessage").text().trim();

    if (message != '') {
        showSuccessMessage(message);
    }
}
function ToggleStatus() {

    $("body").delegate(".js-toggle-status", "click", function () {
        var btn = $(this);

        bootbox.confirm({
            message: 'Are you Sure to toggle status ?',
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
                            var row = btn.parents("tr");
                            if (data.user !== undefined) {
                                row.find(".js-update-on").text(data.date);
                                row.find(".js-update-by").text(data.user);
                            }
                            else {
                                row.find(".js-update-on").text(data)
                            }
                            var status = row.find(".js-status");
                            status.text().trim() === 'Deleted' ? status.text('Available') : status.text('Deleted');
                            status.toggleClass("bg-label-danger bg-label-success");

                            row.addClass("animate__animated animate__flash");

                            showSuccessMessage();
                        },
                        error: function () {
                            showErrorMessage();
                        }
                    });
                }
            }
        });
    });
}

function Confirm() {

    $("body").delegate(".js-confirm", "click", function () {
        var btn = $(this);

        bootbox.confirm({
            message: btn.data("message"),
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
}

$(document).ready(function () {

    applySelect2();
    if ($(".js-datepicker").length > 0) {
        $(".js-datepicker").datepicker({
            format: 'd , MM yyyy',
            endDate: new Date(),
            orientation: 'top'
        });
    }

    if ($('.js-textarea').length > 0) {
        tinymce.init({
            selector: '.js-textarea',
            height: 547,
            plugins: [
                'advlist', 'autolink', 'lists', 'link', 'image', 'charmap', 'preview',
                'anchor', 'searchreplace', 'visualblocks', 'code', 'fullscreen',
                'insertdatetime', 'media', 'table', 'help', 'wordcount'
            ],
            toolbar: 'undo redo | blocks | ' +
                'bold italic backcolor | alignleft aligncenter ' +
                'alignright alignjustify | bullist numlist outdent indent | ' +
                'removeformat | help',
            content_style: 'body { font-family:Helvetica,Arial,sans-serif; font-size:16px }'
        });
    }
    
    $('form').on('submit', function () {
        
        var isValid = $(this).valid();

        if (isValid) {
            DisableButtonSubmit()
        };
    });

    fireSuccessMessage();

    ToggleStatus();
    Confirm();
    $('body').delegate(".js-render-modal", "click", function () {

        var btn = $(this);
        var myModal = $("#Modal");

        myModal.find("#ModalLabel").text(btn.data("title"))

        $.get({
            url: btn.data('url'),
            success: function (form) {
                myModal.find(".modal-body").html(form);
                $.validator.unobtrusive.parse(myModal);

                applySelect2();

                if (btn.data("updated")) {
                    updated = btn.parents('tr').index();
                }


                editTable = btn.data("table");

            },
            error: function () {
                showErrorMessage();
            }
        });
        myModal.modal('show');
    });

    renderDataTables();
});