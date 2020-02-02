$(document).ready(function () {
    var scroll_start = 0;
    var startchange = $('#startchange');
    var offset = startchange.offset();
    if (startchange.length) {
        $(document).scroll(function () {
            scroll_start = $(this).scrollTop();

            if (scroll_start > 200) {

                $(".navbar-default").addClass('navSolidBG');
                $(".navbar-default .container").addClass('nav-margin');
                $(".navbar-default").addClass('nav-height');
            } else {

                $('.navbar-default').removeClass('navSolidBG');
                $('.navbar-default .container').removeClass('nav-margin');
                $('.navbar-default ').removeClass('nav-height');
            }

        });
    }

    $(function () {
        $('a[href*=#]:not([href=#])').click(function () {
            if (location.pathname.replace(/^\//, '') == this.pathname.replace(/^\//, '') && location.hostname == this.hostname) {
                var target = $(this.hash);
                target = target.length ? target : $('[name=' + this.hash.slice(1) + ']');
                if (target.length) {
                    $('html,body').animate({
                        scrollTop: (target.offset().top - 50)
                    }, 1000);
                    return false;
                }
            }
        });
    });

});
$(document).ready(function () {


    $(document).on('click', '.navbar-collapse.in', function (e) {
        if ($(e.target).is('a') && $(e.target).attr('class') != 'dropdown-toggle') {
            $(this).collapse('hide');
        }
    });


    // show login

    $("#btnLogin").on("click", function (e) {
        e.preventDefault();
        $("div.letsTalk").hide();
        $("div.letsTalkNext").hide();
        $("div.complete").hide();
        $("div.verifyEmail").hide();
        $("div.emailSent").hide();
        $("div.callYou").hide();
        $("div.login").fadeIn(500);
        $("#loginForm input[name='email']").focus();
    });

    $('button#btnScheduleCall, #btnScheduleCall2, #btnScheduleCall3').on("click", function (e) {
        e.preventDefault();
        console.log('yes');
        $("div.letsTalk").removeClass('hidden').fadeIn(500);
        $("div.login").hide();
        $("div.letsTalkNext").hide();
        $("div.complete").hide();
        $("div.verifyEmail").hide();
        $("div.emailSent").hide();
        $("div.callYou").hide();

        $("#letsTalkFirstName").focus();
    });


    $("#verifyEmailSubmit").on("click", function (e) {
        e.preventDefault();
        $("div.verifyEmail").hide();
        $("div.emailSent").fadeIn(500);
        $("div.letsTalkNext").hide();
        $("div.emailSent").append($("#btnClose"));


    });

    // $("#loginSubmit").on("click",function(e){
    //  e.preventDefault();
    //  // dashboard will be handled here...
    //     window.location.replace("dashboard.html");
    // });



    var timeoutId;
    $("#whyInfo").hover(function (event) {
        if (!timeoutId) {
            timeoutId = window.setTimeout(function () {
                timeoutId = null; // EDIT: added this line
                $("#message-why").css({
                    top: event.clientY,
                    left: event.clientX
                }).show("1000");
            }, 2000);
        }
    },
      function () {
          if (timeoutId) {
              window.clearTimeout(timeoutId);
              timeoutId = null;
          } else {
              $("#message-why").hide();
          }
      });


    $("#forgot-pw").click(function (e) {
        e.preventDefault();
        $(this).closest('div.lets-talk-container').hide();

        $("#send-reset-modal").fadeIn(500);
    });

    $("#reset-code-btn").click(function () {
        var phoneRegex = /(\d{3})\-(\d{3})\-(\d{4})/;

        var match = phoneRegex.exec($("#reset-code-number").val());

        $("#reset-status").removeClass().text("");

        if (!match) {
            $("#reset-status")
                .text("Please enter your phone number in the format of XXX-XXX-XXXX")
                .addClass("alert alert-danger");

            return;
        }

        var resetNumber = "+1" + match[1] + match[2] + match[3];

        $("#loading-cover").show();

        $.ajax({
            type: "POST",
            url: "/Account/TextPasswordResetCode",
            data: { phoneNumber: resetNumber },
            success: function (response) {
                $("#loading-cover").fadeOut(500);

                $("#reset-status").removeClass().text("");

                if (response.status != 0) {
                    $("#reset-status")
                        .text(response.message)
                        .addClass("alert alert-danger");
                } else {
                    $("#reset-status")
                        .text("A password reset link has been sent")
                        .addClass("alert alert-success");
                }
            }
        });


    });

    $('a#signup').on('click', function () {
        $(this).closest('div.lets-talk-container').hide();
        $('div.letsTalk').removeClass("hidden").fadeIn(500);
    });

    $('a#login').on('click', function () {
        $(this).closest('div.lets-talk-container').hide();
        $('div.login').removeClass("hidden").fadeIn(500);
    });

    $('button.cancel-btn').on('click', function () {
        $(this).closest('div.lets-talk-container').hide();
    });
    $(function () {
        var navMain = $(".navbar-collapse");
        navMain.on("click", "a", null, function () {
            navMain.collapse("hide");
        });
    });


});


