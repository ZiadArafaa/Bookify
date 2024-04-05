$(document).ready(function () {
    $("#onSelect").change(function () {
        var GovernrateId = $(this).val();

        $.ajax({
            method: "get",
            url: "/Subscripers/GetAreas?GovernrateId=" + GovernrateId,
            success: function (data) {

                var areaList = $("#areaId");

                areaList.empty();
                areaList.append('<option value=""></option>');

                $.each(data, function (index, area) {
                    areaList.append($('<option></option>').text(area.name).val(area.id))
                });
            },
            error: function () {
                showErrorMessage();
            }
        });

    });
})