$(document).ready(function () {
    $("#myTable").DataTable({
        serverSide: true,
        stateSave: true,
        processing: true,
        language: {
            searchPlaceholder: "by Title and Author..."
        },
        ajax: {
            url: '/Books/GetBooks',
            type: 'Post',
        },
        columns: [
            {
                name: "Title",
                render: function (data, type, row) {
                    var image = row.imageThumbnailUrl == null ? "/images/books/thumbs/no-book.jpg" : row.imageThumbnailUrl;
                    var div = `<div class="d-flex align-items-center">
                                            <div class="me-2">
                                        <a href="/books/details/${row.id}"><img src="${image}" height="60" alt="Book Image"></a>
                                            </div>
                                            <div class="d-flex justify-content-start flex-column">
                                                <a href="/books/details/${row.id}" class="text-gray-900 fw-bold fs-6">${row.title}</a>
                                                <span class="text-muted fw-semibold text-muted d-block fs-7">${row.author}</span>
                                            </div>
                                        </div>`
                    return div;
                }
            },
            {
                data: "hall",
                name: "Hall"
            },
            {
                date: "createOn",
                render: function (data, type, row) {
                    return moment(row.createOn).format('DD, MMM YYYY');
                },
                name: "CreateOn"
            },
            {
                data: "isDeleted",
                render: function (data, type, row) {
                    return `<span class="badge bg-label-${row.isDeleted ? "danger" : "success"} js-status">
                                                         ${row.isDeleted ? "Deleted" : "Available"}
                            </span>`;
                },
                name: "IsDeleted"
            },
            {
                name: "Actions",
                render: function (data, type, row) {
                    return `<div class="dropdown">
                                    <a class="btn p-0 dropdown-toggle hide-arrow" data-bs-toggle="dropdown">
                                        <i class="bx bx-dots-vertical-rounded"></i>
                                    </a>
                                    <div class="dropdown-menu">
                                        <a class="dropdown-item" href="/books/edit/${row.id}"><i class="bx bx-edit-alt me-1"></i>Edit</a>
         <a class="dropdown-item js-toggle-status" href="javascript:;" data-url="/books/ToggleStatus/${row.id}"
         ><i class="bx bx-trash me-1"></i>Toggle Status</a>
                                    </div>
                                </div>`;
                },
                orderable: false,
            }
        ]
    });
});