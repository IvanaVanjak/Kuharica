$("#addItem").click(function () {
    $.ajax({
        url: this.href,
        cache: false,
        success: function (html) { $("#editorRows").append(html); }
    });
    return false;
});

$('body').on('click', 'a.deleteRow', function () {
    $(this).parents("div.editorRow:first").remove();
    return false;
});

$(document).on('focus', '#jela:not(.ui-autocomplete-input)', function () {
    $('#jela').autocomplete({
        source: '/Recept/Autocomplete',
        position: { collision: "flip" }
    });
});

$('body').on('click', '#button1', function () {
    var a = $('#jela').val();
    alert(a)
});

$('body').on('click', '#submit', function () {
    var a = $('#search').val();
    alert(a)
});

//$('#button1').click(function () {
//    //var a = $('#jelo').val();
//    var b = 'lalala';
//    alert(b);
//});

//$(document).ready(
//function () {
//    $('#jela').autocomplete({
//        source: '/Recept/Autocomplete'
//    });
//});


var url = '/Home/AutocompleteJelo';
$(document).on('focus', '#search:not(.ui-autocomplete-input)', function () {
    $('#search').autocomplete({
        source: function (request, response) {
            $.ajax({
                url: url,
                data: { term: request.term /*term: $("#search").val()*/ },
                dataType: 'json',
                type: 'POST',
                success: function (data) {
                    response($.map(data,
                function (item) {
                    return {
                        label: item.NazivJela,
                        id: item.JeloID
                    }
                }));
                }
            })
        },

        select:
    function (event, ui) {
        $('#search').val(ui.item.label);
        $('#JeloID').val(ui.item.id);
        return false;
    },
        minLength: 1,
        position: { collision: "flip" }
    });
});

$('.carousel').carousel();

//var urlJedinica = '/Recept/AutocompleteJedinicaMjere';
//$(document).on('focus', '#searchJedinica:not(.ui-autocomplete-input)', function () {
//    $('#searchJedinica').autocomplete({
//        source: function (request, response) {
//            $.ajax({
//                url: urlJedinica,
//                data: { term: request.term /*term: $("#search").val()*/ },
//                dataType: 'json',
//                type: 'POST',
//                success: function (data) {
//                    response($.map(data,
//                function (item) {
//                    return {
//                        label: item.Kratica,
//                        id: item.JedinicaMjereID
//                    }
//                }));
//                }
//            })
//        },

//        select:
//    function (event, ui) {
//        $('#searchJedinica').val(ui.item.label);
//        $('#JedinicaMjereID1').val(ui.item.id);
//        return false;
//    },
//        minLength: 1,
//        position: { collision: "flip" }
//    });
//});



//$(document).on("focus", ['#jela'], function () {
//    // If the autocomplete wasn't called yet:
//    $(this).autocomplete({             //   call it
//        source: '@Url.Action("Autocomplete")'     //     passing some parameters
//    });
//});

//$('a.deleteRow').live('click', null, function () {
//    $(this).parents("div.editorRow:first").remove();
//    return false;
//});


//$("a.deleteRow").onClick(function () {
//    $(this).parent().remove();
//    return false;
//});


