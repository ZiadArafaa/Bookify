$(document).ready(function () {
    var bestPictures = new Bloodhound({
        datumTokenizer: Bloodhound.tokenizers.obj.whitespace('value'),
        queryTokenizer: Bloodhound.tokenizers.whitespace,
        remote: {
            url: '/Search/FindBook?Key=%QUERY',
            wildcard: '%QUERY'
        }
    });

    $('#mySearch').typeahead({
        hint: true,
        highlight: true,
        minLength: 4
    }, {
        name: 'best-pictures',
        display: 'title',
        source: bestPictures,
        templates: {
            empty: [
                '<div class="m-2">',
                'Not Found Book!',
                '</div>'
            ].join('\n'),
            suggestion: Handlebars.compile('<div class="mb-2"><strong>{{title}}</strong> <br/> <span class="fs-6">by {{author}}</span></div>')
        }
    }).bind('typeahead:select', function (ev, book) {
        window.location.replace('/Search/Details?Key='+book.key)
    });
});