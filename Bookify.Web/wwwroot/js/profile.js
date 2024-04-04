$(document).ready(function () {
    $('#btnDismiss').click(function () {
        $('.js-dismiss').attr('src', '/assets/images/avatar.png');
        $("#removeBtn").val(true);
        $("#FileImage").val('')
    });
    $('.click-img').click(function () {
        $("#FileImage").click()
    })
});
