$(document).ready(function () {
    $("#onSelect").change(function () {
        var GovernrateId = $(this).val();
        $.ajax({
            method: "get",
            url: "/Subscribers/GetAreas?GovernrateId=" + GovernrateId,
            success: function (data) {

                var areaList = $("#areaId");

                areaList.empty();
                areaList.append('<option value="" selected disabled>Select Areas</option>');

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