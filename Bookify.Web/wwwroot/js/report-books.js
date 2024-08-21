$(document).ready(function () {
    $(".page-index").click(function () {
        var btn = $(this);
        var index = btn.data('page-index');

        if (btn.parents('li').hasClass('active')) return;

        $("#PageIndex").val(index);
        $("#myReportForm").submit();
    });
    $(".page-previous").click(function () {
        var previousIndex = $('ul').find('.active').find('.page-index').data('page-index');

        $("#PageIndex").val(previousIndex - 1);
        $("#myReportForm").submit();
    });
    $(".page-next").click(function () {
        var previousIndex = $('ul').find('.active').find('.page-index').data('page-index');

        $("#PageIndex").val(previousIndex + 1);
        $("#myReportForm").submit();
    });
});