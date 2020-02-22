$(document).ready(function () {
    $('#tblTransaction').DataTable({
        "order": [[0, "desc"]],
        "columnDefs": [
            { "type": "date", "targets": 0 }
        ]
    });

    $('#tblUsers').DataTable({
        "order": [[1, "desc"]],
        "columnDefs": [
            { "type": "date", "targets": 1 }
        ]
    });

    $('#tblPaymentPartners').DataTable({
        "order": [[1, "desc"]],
        "columnDefs": [
            { "type": "date", "targets": 1 }
        ]
    });   

    $('#tblAdvertisements').DataTable({
        "order": [[1, "desc"]],
        "columnDefs": [
            { "type": "date", "targets": 1 }
        ]
    });   

    $('#tblErrorLog').DataTable({
        "order": [[0, "desc"]],
        "columnDefs": [
            { "type": "date", "targets": 0, "width":"5px"}
        ],
        //"columns": [{ "width": "250px" },
        //    { "width": "25px" },
        //    { "width": "450px" },
        //    { "width": "20px" },
        //    { "width": "20px" },
        //    { "width": "20px" }],
        fixedColumns: {
            leftColumns: 1,
            rightColumns: 1
        }
    });

    var fullURL = window.location.href;
    var domain = location.host;
    var domainLength = location.host.length;

    for (var i = 0; i <= fullURL.length - 1; i++) {
        var str = fullURL.substring(i, domainLength + i);
        if (str === domain) {
            var routeValue = fullURL.substring(domainLength + i, fullURL.length);
            localStorage.setItem('routeValue', routeValue);
            break;
        }
    }

    localStorage.setItem('activeTab', localStorage.getItem('routeValue'));

    var activeTab = localStorage.getItem('activeTab');
    if (activeTab) {

        if (activeTab === '/User/CreateTransaction') {
            activeTab = '/User';
        }

        $('#navbarNav a[href="' + activeTab + '"]').parent().addClass('green-highlight active');
    }   

    $('#tblTransaction tbody').on('mouseover', 'tr', function () {
        $('[data-toggle="tooltip"]').tooltip({
            trigger: 'hover',
            html: true
        });
    });   

    var errorDiv = $('#divResult .close');
    if (errorDiv) {
        errorDiv.focus();
    }

});

$(function () {
    $('.tabgroup > div').hide();
    $('.tabgroup > div:first-of-type').show();
    $('.tabs a').click(function(e) {
        e.preventDefault();
        var $this = $(this),
            tabgroup = '#' + $this.parents('.tabs').data('tabgroup'),
            others = $this.closest('li').siblings().children('a'),
            target = $this.attr('href');
        others.removeClass('active');
        $this.addClass('active');
        $(tabgroup).children('div').hide();
        $(target).show();

        // Scroll to tab content (for mobile)
        if ($(window).width() < 992) {
            $('html, body').animate({
                    scrollTop: $("#first-tab-group").offset().top
                },
                200);
        }
    });
});






