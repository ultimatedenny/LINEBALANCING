/*
 * Content onload
 * FAB Function (show hide)
 * Side Menu (show hide)
 * outstanding message (show hide)
 * Modal Form & Message Export Import Data
 * Message Submit Process Checking
 * btn Open All & Collapse All
 * Lock Screen Orientation
 * Tab Stopwatch (stopwatch & Manual)
 * Import File Reset
 */

//Content on load Function
$(window).on('load', function () {
    // When the page has loaded
    setTimeout(function () {
        $('.setDelay').removeClass('hideCont');
    }, 1000);

    setTimeout(function () {
        $('.setDelayText').removeClass('hideContText');
    }, 300);
});


//FAB Function
$(".c-fab__main").click(function () {
    $(".c-fab__box-item").toggle();
});
$(".c-menu-box__f-btn-main").click(function () {
    $(".c-menu-box__f-btn-main-item").toggle();
});


//side menu Function
$(".c-menu-box__f-btn-item-one,.c-menu-box__master-data").click(function () {
    $("#mySidenavMaster").addClass("width-side");
    $(".c-menu-box__f-btn-main-item").hide();
});

$(".c-menu-box__f-btn-item-two,.c-menu-box__setting-account").click(function () {
    $("#mySidenavSetting").addClass("width-side");
    $(".c-menu-box__f-btn-main-item").hide();
});

$(".closebtnSetting").click(function () {
    $("#mySidenavSetting").removeClass("width-side");
});

$(".closebtnMaster").click(function () {
    $("#mySidenavMaster").removeClass("width-side");
});

//message outstanding Function
$(window).on('load', function () {
    $('#messageOutstanding').modal('show');
});


//Modal Form & Message Export Import Data
$(".ExportImportData").click(function () {
    $('#ImportExportModal').modal('show');
    $(".c-fab__box-item").hide();
});

$("#AddData").click(function () {
    $('#AddEditModal').modal('show');
    $(".c-fab__box-item").hide();
});

$("#export").click(function () {
    $(".c-fab__box-item").hide();
});

$("#import").click(function () {
    $(".c-fab__box-item").hide();
});


//$("#EditData").click(function ()
//{
//    $('#AddEditModal').modal('show');
//    $(".c-fab__box-item").hide();
//});

//Message Submit Process Checking
$("#SubmitProcessChecking").click(function () {
    $('#MessageApplyProcessChecking').modal('show');
});

//btn Open All & Collapse All
$(".btn-expand-all").on("click", function () {
    $(this).closest('.l-box__list').find('.collapse').collapse('show');
});

$(".btn-collapse-all").on("click", function () {
    $(this).closest('.l-box__list').find('.collapse').collapse('hide');
});


//Lock Screen Orientation
function lock(landscape) {
    fullScreen();
    screen.orientation.lock(landscape);
}
$(window).bind("orientationchange", function () {
    var orientation = window.orientation;
    var new_orientation = (orientation) ? 0 : 180 + orientation;
    $('body').css({
        "-webkit-transform": "rotate(" + new_orientation + "deg)"
    });
});

//Tab Stopwatch (stopwatch & Manual)
//$(".btn-tab-stopwatch").click(function () {
//    $(".btn-tab-stopwatch").removeClass("active-stopwatch");
//    $(this).addClass("active-stopwatch");
//});

//Tab Stopwatch (stopwatch & Manual)
//$(".btn-st").click(function () {
//    $(".btn-mn").removeClass("active-stopwatch");
//    $(".btn-st").addClass("active-stopwatch");
//});

//$(".btn-mn").click(function () {
//    $(".btn-st").removeClass("active-stopwatch");
//    $(".btn-mn").addClass("active-stopwatch");
//});


// Validate upload
function validateFormImport() {
    var file = $('#fileImport').val();
    if (file === '') {
        $('#ImportModal').addClass('opacity-0');

        $('#NotificationModal').modal('show');
        $('#success').hide();
        $('#fail').show();
        $('#notificationMessage').text("Please select file with extension .xlsx or .xls !");
        $('#closeNotification').show();
        $('#buttonContinue').hide();

        return false;
    }
}


$('#fileImport').change(function (e) {
    var fileName = e.target.files[0].name;
    $('.file-name').text(fileName);
});


// Reset close button modal import
function resetUpload() {
    $('#fileImport').val('');
    $('.file-name').empty();
}

// Reset button import file
$('#resetFileImport').click(function () {
    resetUpload();
});

// Reset close button modal import
$('#closeImportModal').click(function () {
    resetUpload();
});



// Close notification
function closeNotification() {
    $("#NotificationModal").hide();
    $('#AddEditModal').removeClass('opacity-0');
    $('#ImportModal').removeClass('opacity-0');
}

// Close button notification
$("#closeNotification").on("click", function () {
    closeNotification();
});

// Close upper right button notification modal
$("#closeNotificationModal").on("click", function () {
    closeNotification();
});

// Close add & edit modal
$("#closeAddEditModal").on("click", function () {
    $('#AddEditModal').modal('hide');
});

// Button continue when deleting item
$("#buttonContinue").on("click", function () {
    $('#DeleteModal').modal('show');
});

// Delay typing
function delayTyping(callback, ms) {
    var timer = 0;
    return function () {
        var context = this, args = arguments;
        clearTimeout(timer);
        timer = setTimeout(function () {
            callback.apply(context, args);
        }, ms || 0);
    };
}