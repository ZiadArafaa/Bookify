function OnModelSuccess(data) {
    $("#Modal").modal('hide');
    showSuccessMessage();

    if (updated != undefined) {
        $('tbody tr').each(function () {
            if ($(this).index() == updated) {

                $('#myTable tbody tr').eq($(this).index()).replaceWith(data);
                updated = undefined
                return;
            }
        });
    }
    else {
        $('#myTable tbody').append(data);
        $('#myTable').removeClass('d-none');
        $('.js-alert').addClass('d-none');
        var count = $('#myCount');
        count.text(parseInt(count.text()) + 1);
    }
}

$(document).ready(function () {
    var numAnim = new countUp.CountUp('myCount', parseInt($('.js-count').text()))
    numAnim.start()
});