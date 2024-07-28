var bookcopies = [];
var currentBookcopies = [];
var books = [];

function OnSearchSuccess(data) {

    const parser = new DOMParser();
    const domData = parser.parseFromString(data, "text/html");
    var bookId = $(domData).find("#myBtn").data('book-id');
    var copyId = $(domData).find("#myBtn").data('copy-id');

    if (books.filter((item) => item == bookId).length > 0) {
        showErrorMessage({ 'responseText': "Books Rental Can't be Dublicated" });
    }
    else if (countSelected === maxCopies) {
        showErrorMessage({ 'responseText': `Total Max Of Books Rental ${maxCopies}.` });
    }
    else {
        showSuccessMessage();

        $('#myTable').prepend($(data));
        books.push(bookId);
        bookcopies.push(copyId);
        countSelected++;

        if (JSON.stringify(bookcopies) != JSON.stringify(currentBookcopies)) {
            $('#mySubmit').removeAttr('hidden');
        }

        $("input[type='search']").val('');
    }

}
function OnSearchFailure(data) {
    data.responseText != "" ? showErrorMessage({ "responseText": data.responseText }) :
        showErrorMessage({ "responseText": "Bad Rquest!" });
}

function PrepareInputs() {
    $('#myForm .select-input').each((index, element) => {
        element.setAttribute('name', `SelectedCopies[${index}]`);
        element.setAttribute("id", `SelectedCopies_${index}_`);
    });
}

$(document).ready(() => {

    if ($('#myTable tr').length != 1 && JSON.stringify(bookcopies) != JSON.stringify(currentBookcopies)) {
        $('#mySubmit').attr('hidden', false);
    }

    if (isUpdated) {

        PrepareInputs();
        $('#myForm .select-input').each((index, element) => {
            bookcopies.push(parseInt($(element).attr('value')));
            currentBookcopies.push(parseInt($(element).attr('value')));
            books.push($(element).data('book-id'));
            countSelected++;
        });

    }

    $('#myForm').submit(function () {

        PrepareInputs();
    })

    $("#myTable").delegate("#myBtn", "click", function () {
        var btn = $(this);

        var removedBookId = btn.data('book-id');
        var removedCopyId = btn.data('copy-id');

        if (currentBookcopies.filter((item) => item == removedCopyId).length > 0) {
            $.post({
                url: '/Rentals/RemoveRentalCopy?bookCopyId=' + removedCopyId,
                data: {
                    "__RequestVerificationToken": $("input[name='__RequestVerificationToken']").val()
                },
                success: function () {
                    console.log("Success Removed");
                },
                error: function () {
                    console.log("Error Removed");
                }
            });
        }

        $(`#myForm input[value=${removedCopyId}]`).remove();
        $(btn.parents("tr")).remove();

        countSelected--;

        books = books.filter((item) => item != removedBookId);
        bookcopies = bookcopies.filter((item) => item != removedCopyId);

        PrepareInputs();

        if ($('#myTable tr').length === 1 || JSON.stringify(bookcopies) == JSON.stringify(currentBookcopies)) {
            $('#mySubmit').attr('hidden', true);
        }
        else {
            $('#mySubmit').attr('hidden', false);
        }

    })


})