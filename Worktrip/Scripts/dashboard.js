function clearYesNoQuestion(name) {
    $("input[name='" + name + "']")[0].checked = false;
    $("input[name='" + name + "']")[1].checked = false;
};


function refreshPersonalInfo(userInfo) {
    $(".input-confirm").addClass("hidden");

    $("#Personal-info input").each(function () {
        var info = userInfo[$(this).attr("name")];

        var $editButton = $(this).next(".input-group-btn");

        $(this)
            .val("")
            .removeAttr("disabled");

        $editButton.addClass("hidden");

        if (info != null) {
            console.log(this);
            console.log(info)
            $(this)
                .val(info)
                .attr("disabled", "");

            $editButton.removeClass("hidden");
        }
    });



    //populate airline and base airport based on tax year
    var taxYear = $("#taxYear").text();

    //$("#base-airport-select").empty();

    if (userInfo.TaxInfos[taxYear]) {

        //var existingCode = userInfo.TaxInfos[taxYear].BaseAirportCode;

        //if (existingCode) {
        //    $("#base-airport-select")
        //        .val(existingCode);

        //    if (!$("#base-airport-select").val()){

        //        $("#base-airport-select")
        //            .append("<option value='" + existingCode + "'></option>")
        //            .val(existingCode);
        //    }

        //    $("#base-airport-select").nextAll(".input-group-btn").removeClass("hidden");
        //    $("#base-airport-select").attr("disabled", "");
        //} else {
        //    $("#base-airport-select").val("");
        //    $("#base-airport-select").removeAttr("disabled");
        //    $("#base-airport-select").nextAll(".input-group-btn").addClass("hidden");
        //}

        var existingAirline = userInfo.TaxInfos[taxYear].Airline;

        if (existingAirline) {
            $("#airline-select").val(existingAirline).trigger("change");

            $("#airline-select").nextAll(".input-group-btn").removeClass("hidden");
            $("#airline-select").attr("disabled", "");

        } else {
            $("#airline-select").val("");
            $("#airline-select").removeAttr("disabled");
            $("#airline-select").nextAll(".input-group-btn").addClass("hidden");
        }

    } else {
        //$("#base-airport-select").val("");
        //$("#base-airport-select").removeAttr("disabled");
        //$("#base-airport-select").nextAll(".input-group-btn").addClass("hidden");

        $("#airline-select").val("");
        $("#airline-select").removeAttr("disabled");
        $("#airline-select").nextAll(".input-group-btn").addClass("hidden");
    }

    refreshStatusbar(userInfo, taxYear);
}

function refreshTaxInfo(userInfo, taxYear) {
    $("#training-days-title").text("2. How many days were you training in " + taxYear + "?");

    $("#tax-question input").each(function () {
        if (!userInfo.TaxInfos) {
            return;
        }

        var taxInfo = userInfo.TaxInfos[taxYear] || {};

        var info = taxInfo[$(this).attr("name")];

        $(this)
            .val("")
            .removeAttr("disabled");

        var $addOn = $(this).prev(".input-group-addon");
        var $editButton = $(this).next(".input-group-btn");

        $addOn.removeClass("input-group-disabled");
        $editButton.addClass("hidden");

        if (info != null) {
            $(this)
                .val(info)
                .attr("disabled", "");

            $addOn.addClass("input-group-disabled");
            $editButton.removeClass("hidden");
        }
    });

    if (userInfo.TaxInfos[taxYear]) {
        console.log(userInfo.TaxInfos[taxYear])
        if (userInfo.TaxInfos[taxYear].Married) {
            $("input[name='married']")[0].checked = true;
            document.getElementById("marriedLabel").style.visibility = "visible";
        } else {
            $("input[name='married']")[1].checked = true;
        }
        if (userInfo.TaxInfos[taxYear].Dependent) {
            $("input[name='dependent']")[0].checked = true;
            document.getElementById("dependentLabel").style.visibility = "visible";
        } else {
            $("input[name='dependent']")[1].checked = true;
        }
        if (userInfo.TaxInfos[taxYear].StudentLoans) {
            $("input[name='loans']")[0].checked = true;
            document.getElementById("loansLabel").style.visibility = "visible";
        } else {
            $("input[name='loans']")[1].checked = true;
        }
        if (userInfo.TaxInfos[taxYear].Stocks) {
            $("input[name='stocks']")[0].checked = true;
            document.getElementById("stocksLabel").style.visibility = "visible";
        } else {
            $("input[name='stocks']")[1].checked = true;
        }
        if (userInfo.TaxInfos[taxYear].House) {
            $("input[name='house']")[0].checked = true;
            document.getElementById("houseLabel").style.visibility = "visible";
        } else {
            $("input[name='house']")[1].checked = true;
        }
        if (userInfo.TaxInfos[taxYear].HSA) {
            $("input[name='hsa']")[0].checked = true;
            document.getElementById("hsaLabel").style.visibility = "visible";
        } else {
            $("input[name='hsa']")[1].checked = true;
        }
        if (userInfo.TaxInfos[taxYear].C1098T) {
            $("input[name='1098T']")[0].checked = true;
            document.getElementById("1098TLabel").style.visibility = "visible";
        } else {
            $("input[name='1098T']")[1].checked = true;
        }
        if (userInfo.TaxInfos[taxYear].C1099R) {
            $("input[name='1099R']")[0].checked = true;
            document.getElementById("1099RLabel").style.visibility = "visible";
        } else {
            $("input[name='1099R']")[1].checked = true;
        }

        if (userInfo.TaxInfos[taxYear].NewHire) {
            $("input[name='newHire']")[0].checked = true;
            document.getElementById("newHireLabel").style.visibility = "visible";
        } else {
            $("input[name='newHire']")[1].checked = true;
        }
        if (userInfo.TaxInfos[taxYear].Other) {
            $("input[name='other']")[0].checked = true;
            document.getElementById("otherLabel").style.visibility = "visible";
        } else {
            $("input[name='other']")[1].checked = true;
        }
        if (userInfo.TaxInfos[taxYear].ScheduleK1) {
            $("input[name='scheduleK1']")[0].checked = true;
            document.getElementById("scheduleK1Label").style.visibility = "visible";
        } else {
            $("input[name='scheduleK1']")[1].checked = true;
        }
        if (userInfo.TaxInfos[taxYear].OwnBusiness) {
            $("input[name='ownBusiness']")[0].checked = true;
            document.getElementById("ownBusinessLabel").style.visibility = "visible";
        } else {
            $("input[name='ownBusiness']")[1].checked = true;
        }
        if (userInfo.TaxInfos[taxYear].C1099G) {
            $("input[name='1099G']")[0].checked = true;
            document.getElementById("1099GLabel").style.visibility = "visible";
        } else {
            $("input[name='1099G']")[1].checked = true;
        }
        if (userInfo.TaxInfos[taxYear].C1099INT) {
            $("input[name='1099INT']")[0].checked = true;
            document.getElementById("1099INTLabel").style.visibility = "visible";
        } else {
            $("input[name='1099INT']")[1].checked = true;
        }
        if (userInfo.TaxInfos[taxYear].MultipleW2s) {
            $("input[name='multipleW2s']")[0].checked = true;
            document.getElementById("multipleW2sLabel").style.visibility = "visible";
        } else {
            $("input[name='multipleW2s']")[1].checked = true;
        }

        if (userInfo.TaxInfos[taxYear].Itemize) {
            $("input[name='itemize']")[0].checked = true;
            document.getElementById("itemizeLabel").style.visibility = "visible";
            $('[name="question-driveToWork"], [name="question-flyReserveDays"]').css("display", "inherit");

            if (userInfo.TaxInfos[taxYear].DriveToWork) {
                $("input[name='driveToWork']")[0].checked = true;
            } else {
                $("input[name='driveToWork']")[1].checked = true;
            }

            if (userInfo.TaxInfos[taxYear].FlyReserveDays) {
                $("input[name='flyReserveDays']")[0].checked = true;
            } else {
                $("input[name='flyReserveDays']")[1].checked = true;
            }

        } else {
            $("input[name='itemize']")[1].checked = true;
        }

        //$("#international-layover-checkbox").prop('checked', userInfo.TaxInfos[taxYear].InternationalLayovers);
        ////show or hide navigation Items based on InternationalLayovers boolean
        //if (userInfo.TaxInfos[taxYear].InternationalLayovers) {
        //    //$("#tQuest-status-item").prop("style").display = "inherit";
        //    //$("#tQuest-arrow").prop("style").display = "inherit";
        //    $("#tQuest-side-item").prop("style").display = "inherit";
        //} else {
        //    //$("#tQuest-status-item").prop("style").display = "none";
        //    //$("#tQuest-arrow").prop("style").display = "none";
        //    $("#tQuest-side-item").prop("style").display = "none";
        //}

        var existingState = userInfo.TaxInfos[taxYear].DLState;

        if (existingState) {
            $("#dl-select").val(existingState);

            $("#dl-select").nextAll(".input-group-btn").removeClass("hidden");
            $("#dl-select").attr("disabled", "");
        } else {
            $("#dl-select").val("");
            $("#dl-select").removeAttr("disabled");
            $("#dl-select").nextAll(".input-group-btn").addClass("hidden");
        }
    } else {
        $("#dl-select").val("");
        $("#dl-select").removeAttr("disabled");
        $("#dl-select").nextAll(".input-group-btn").addClass("hidden");
    }

    //update refund amount if it exists
    $("#refund-amount").text(
        userInfo.TaxInfos && userInfo.TaxInfos[taxYear] && userInfo.TaxInfos[taxYear].TaxReturn ?
            "$" + userInfo.TaxInfos[taxYear].TaxReturn : "Processing"
    );

    refreshStatusbar(userInfo, taxYear);
}

function refreshQuestions(userInfo, taxYear) {
    $("#asked-questions ul").empty();

    if (!userInfo.Questions || !userInfo.Questions[taxYear]) {
        $("#asked-questions").addClass("hidden");
        return;
    }

    $("#asked-questions").removeClass("hidden");

    for (var q in userInfo.Questions[taxYear]) {
        var question = userInfo.Questions[taxYear][q];

        $("#asked-questions ul").append(
            "<li>" +
            "<div>" + question.QuestionText + "</div>" +
            (question.AnswerText ? "<div class='q-answer'>" + question.AnswerText + " <span class='q-answerer'>- " + (question.AnsweredBy || "") + "</span></div>" : "") +
            "</li>"
        );
    }
}

function bTofa(bool) {
    return bool ? "fa-check-circle" : "fa-exclamation-circle";
}

function fillStatusIcon($el, status) {
    $el.attr("class", "").addClass("fa " + bTofa(status));
}

function refreshStatusbar(userInfo, year) {
    var pInfoDone = true;

    if (!userInfo.TaxInfos || !userInfo.TaxInfos[year]) {
        pInfoDone = false;
    } else {
        for (var p in userInfo) {
            if (userInfo[p] == null && p != "MiddleName") {
                pInfoDone = false;
            }
        }

        if (userInfo.TaxInfos[year].BaseAirportCode == null ||
            userInfo.TaxInfos[year].Airline == null) {
            pInfoDone = false;
        }
    }

    fillStatusIcon($("#pInfo-status .fa"), pInfoDone);


    var questionsDone = true;

    if (!userInfo.TaxInfos || !userInfo.TaxInfos[year]) {
        questionsDone = false;
    } else {
        for (var t in userInfo.TaxInfos[year]) {
            if (userInfo.TaxInfos[year][t] == null) {
                questionsDone = false;
            }
        }
    }

    fillStatusIcon($("#tQuest-status .fa"), questionsDone);
}

function refreshPaymentView(userInfo) {
    //$("#payment-div").show();
    //$("#payment-not-ready").hide();
    //return;

    $("#make-payment>div").hide();

    if (userInfo.Statuses && userInfo.Statuses[curTaxYear]) {
        switch (userInfo.Statuses[curTaxYear].Status) {
            case "WaitingCustomerPayment":
                $("#payment-div").show();
                break;
            case "Finished":
                $("#payment-finished").show();
                break;
            default:
                $("#payment-not-ready").show();
                break;
        }
    } else {
        $("#payment-not-ready").show();
    }
}

function refreshDashboardViews(userInfo, year) {

    refreshPersonalInfo(userInfo);

    refreshTaxInfo(userInfo, year);

    refreshQuestions(userInfo, year);

    refreshPaymentView(userInfo, year);
}

var brainTreeSetup = false;
function setupBrainTree() {

    if (brainTreeSetup || brainTreeInited) {
        return;
    }

    brainTreeSetup = true;

    braintree.setup(client_token, "dropin", {
        container: "bt-dropin",
        paypal: {
            headless: true
        },
        onReady: function () {
            brainTreeInited = true;
            $("#credit-cards-img").fadeIn(500);
        },
        onPaymentMethodReceived: function (obj) {
            // Do some logic in here.
            // When you're ready to submit the form:
            $("#loading-cover").show();

            $.ajax({
                url: "/Home/MakePayment",
                type: "POST",
                data: {
                    paymentNonce: obj.nonce,
                    transactionYear: curTaxYear
                },
                success: function (response) {
                    $("#loading-cover").fadeOut(500);

                    if (response.status == 0) {
                        $("#make-payment>div").hide();
                        $("#payment-finished").fadeIn(500);
                    } else {
                        alert("Something went wrong with processing your payment, please try again later or contact us");
                    }
                }
            });
        }
    });
}

function dataURIToBlob(dataURI, callback) {

    var binStr = atob(dataURI.split(',')[1]),
        len = binStr.length,
        arr = new Uint8Array(len),
        mimeString = dataURI.split(',')[0].split(':')[1].split(';')[0]

    for (var i = 0; i < len; i++) {
        arr[i] = binStr.charCodeAt(i);
    }

    return new Blob([arr], {
        type: mimeString
    });

}

function downloadDataURI(dataURI, filename) {

    var blob = dataURIToBlob(dataURI);
    var url = URL.createObjectURL(blob);
    var blobAnchor = document.createElement('a');

    blobAnchor.download = filename;
    blobAnchor.href = url;
    blobAnchor.style.display = "none;";

    document.body.appendChild(blobAnchor);

    blobAnchor.click();

    requestAnimationFrame(function () {
        URL.revokeObjectURL(url);
        document.body.removeChild(blobAnchor);
    });
}

function getTaxYear() {
    var selectedTaxYear = $("#taxYear").text();
    var curUserId = prefetchedUserTaxInfo.Id;
    document.getElementById("hiddenTaxYearLogin").value = selectedTaxYear;
    document.getElementById("hiddenUserId").value = curUserId;
}

function downloadTaxReturn() {
    $.ajax({
        cache: false,
        url: "/Home/GetuserTaxReturn",
        type: "POST",
        data: {
            userId: prefetchedUserTaxInfo.Id,
            taxYear: $("#taxYear").text()
        },
        success: function (response) {
            if (response.status == 0 && response.files.length > 0) {
                if (!$('div#tax-return-container').is(":visible")) {
                    console.log('in' + elemId);
                    $('div.box-dashboard-container').hide();
                    $('div#Personal-info').hide();
                    $('div#tax-question').hide();
                    $('div#make-payment').hide();
                    $('div#tax-documents-container').hide();
                    $('div#tax-return-container').fadeIn(500);
                    $('div#no-tax-return-container').hide();
                    $('div#ynQuestions').hide();
                    $('div.btn-container').css('background', '#999');
                    elem.closest('div.btn-container').css('background', '#E74139');

                    if (!$('div#options .menu-dashboard').is(":visible"))
                        $('div.left-menu-collapse').slideUp(500);
                }
            }
            else {
                if (!$('div#tax-return-container').is(":visible")) {
                    console.log('in' + elemId);
                    $('div.box-dashboard-container').hide();
                    $('div#Personal-info').hide();
                    $('div#tax-question').hide();
                    $('div#make-payment').hide();
                    $('div#tax-documents-container').hide();
                    $('div#tax-return-container').hide();
                    $('div#no-tax-return-container').fadeIn(500);
                    $('div#ynQuestions').hide();
                    $('div.btn-container').css('background', '#999');
                    elem.closest('div.btn-container').css('background', '#E74139');

                    if (!$('div#options .menu-dashboard').is(":visible"))
                        $('div.left-menu-collapse').slideUp(500);
                }
            }
        },
        complete: function () {
            $("#loading-cover").fadeOut(500);
        }
    });
}

$(function () {
    $("#welcome-popup").css("opacity", 1);

    var dobPicker = $('#dob-input').datetimepicker({
        format: 'L',
        ignoreReadonly: true
    });

    dobPicker.on('dp.change', function () {
        $("#save-personal-info-btn").trigger("click", { "passive": true });
    })

    refreshDashboardViews(prefetchedUserTaxInfo, $("#taxYear").text());

    $("#Personal-info>div>.input-group>input").change(function () {
        console.log('dasdasda')
        //check for valid zip code format
        if ($(this).attr("name") == "Zip") {
            var isValidZip = /(^\d{5}$)|(^\d{5}-\d{4}$)/.test(this.value);
            if (isValidZip == true) {
                $("#save-personal-info-btn").trigger("click", { "passive": true });
                $("#zip-error-label").prop("style").display = "none";
            } else {
                $("#zip-error-label").prop("style").display = "initial";
            }
        } else {
            $("#save-personal-info-btn").trigger("click", { "passive": true });
        }
    });

    $("#Personal-info>div>.input-group>select").on("select2:select", function () {
        $("#save-personal-info-btn").trigger("click", { "passive": true });
    });

    $("#tax-question input").change(function () {
        $("#save-tax-info-btn").trigger("click", { "passive": true });
    })

    $("#save-ynQuestions-btn").click(function (e, obj) {

        if (!obj || !obj.passive) {
            $("#loading-cover").show();
        }
        var updateInfo = { TaxInfos: {} };
        var taxYear = $("#taxYear").text();

        updateInfo.TaxInfos[taxYear] = {
            Year: taxYear
        };

        if ($("input[name='married']:checked").val() == 'yes') {
            updateInfo.TaxInfos[taxYear]['Married'] = true;
        } else {
            updateInfo.TaxInfos[taxYear]['Married'] = false;
        }
        if ($("input[name='dependent']:checked").val() == 'yes') {
            updateInfo.TaxInfos[taxYear]['Dependent'] = true;
        } else {
            updateInfo.TaxInfos[taxYear]['Dependent'] = false;
        }
        if ($("input[name='loans']:checked").val() == 'yes') {
            updateInfo.TaxInfos[taxYear]['StudentLoans'] = true;
        } else {
            updateInfo.TaxInfos[taxYear]['StudentLoans'] = false;
        }
        if ($("input[name='stocks']:checked").val() == 'yes') {
            updateInfo.TaxInfos[taxYear]['Stocks'] = true;
        } else {
            updateInfo.TaxInfos[taxYear]['Stocks'] = false;
        }
        if ($("input[name='house']:checked").val() == 'yes') {
            updateInfo.TaxInfos[taxYear]['House'] = true;
        } else {
            updateInfo.TaxInfos[taxYear]['House'] = false;
        }
        if ($("input[name='hsa']:checked").val() == 'yes') {
            updateInfo.TaxInfos[taxYear]['HSA'] = true;
        } else {
            updateInfo.TaxInfos[taxYear]['HSA'] = false;
        }
        if ($("input[name='1098T']:checked").val() == 'yes') {
            updateInfo.TaxInfos[taxYear]['C1098T'] = true;
        } else {
            updateInfo.TaxInfos[taxYear]['C1098T'] = false;
        }
        if ($("input[name='1099R']:checked").val() == 'yes') {
            updateInfo.TaxInfos[taxYear]['C1099R'] = true;
        } else {
            updateInfo.TaxInfos[taxYear]['C1099R'] = false;
        }
        if ($("input[name='newHire']:checked").val() == 'yes') {
            updateInfo.TaxInfos[taxYear]['NewHire'] = true;
        } else {
            updateInfo.TaxInfos[taxYear]['NewHire'] = false;
        }

        if ($("input[name='other']:checked").val() == 'yes') {
            updateInfo.TaxInfos[taxYear]['Other'] = true;
        } else {
            updateInfo.TaxInfos[taxYear]['Other'] = false;
        }
        if ($("input[name='scheduleK1']:checked").val() == 'yes') {
            updateInfo.TaxInfos[taxYear]['ScheduleK1'] = true;
        } else {
            updateInfo.TaxInfos[taxYear]['ScheduleK1'] = false;
        }
        if ($("input[name='1099G']:checked").val() == 'yes') {
            updateInfo.TaxInfos[taxYear]['C1099G'] = true;
        } else {
            updateInfo.TaxInfos[taxYear]['C1099G'] = false;
        }
        if ($("input[name='1099INT']:checked").val() == 'yes') {
            updateInfo.TaxInfos[taxYear]['C1099INT'] = true;
        } else {
            updateInfo.TaxInfos[taxYear]['C1099INT'] = false;
        }
        if ($("input[name='ownBusiness']:checked").val() == 'yes') {
            updateInfo.TaxInfos[taxYear]['OwnBusiness'] = true;
        } else {
            updateInfo.TaxInfos[taxYear]['OwnBusiness'] = false;
        }
        if ($("input[name='multipleW2s']:checked").val() == 'yes') {
            updateInfo.TaxInfos[taxYear]['MultipleW2s'] = true;
        } else {
            updateInfo.TaxInfos[taxYear]['MultipleW2s'] = false;
        }
        if ($("input[name='itemize']:checked").val() == 'yes') {
            updateInfo.TaxInfos[taxYear]['Itemize'] = true;

            if ($("input[name='driveToWork']:checked").val() == 'yes') {
                updateInfo.TaxInfos[taxYear]['DriveToWork'] = true;
            } else {
                updateInfo.TaxInfos[taxYear]['DriveToWork'] = false;
            }

            if ($("input[name='flyReserveDays']:checked").val() == 'yes') {
                updateInfo.TaxInfos[taxYear]['FlyReserveDays'] = true;
            } else {
                updateInfo.TaxInfos[taxYear]['FlyReserveDays'] = false;
            }

        } else {
            updateInfo.TaxInfos[taxYear]['Itemize'] = false;
            updateInfo.TaxInfos[taxYear]['FlyReserveDays'] = null;
            updateInfo.TaxInfos[taxYear]['DriveToWork'] = null;
        }



        $.ajax({
            url: "/Home/UpdateYesNoQuestions",
            type: "POST",
            data: updateInfo,
            success: function (userInfo) {
                refreshTaxInfo(userInfo, $("#taxYear").text());
            },
            complete: function () {
                $("#loading-cover").fadeOut(500);

                if (!obj || !obj.passive) {
                    $(window).scrollTop(0);
                    $('div.left-menu-collapse').slideDown(500);
                    if (!$('div#tax-return-container').is(":visible")) {
                        console.log('in' + elemId);
                        $('div.box-dashboard-container').hide();
                        $('div#Personal-info').hide();
                        $('div#tax-question').hide();
                        $('div#make-payment').hide();
                        $('div#tax-documents-container').fadeIn(500);
                        $('div#tax-return-container').hide();
                        $('div#no-tax-return-container').hide();
                        $('div#ynQuestions').hide();
                        $('div.btn-container').css('background', '#999');
                        $("#tax-docs-side-item").css('background', '#E74139');

                        if (!$('div#options .menu-dashboard').is(":visible"))
                            $('div.left-menu-collapse').slideUp(500);
                    }
                }
            }
        });
    });

    $("#save-personal-info-btn").click(function (e, obj) {

        if ($("#bankaccountnumber-input").is(":enabled") &&
            $("#bankaccountnumber-input").val()) {

            $("#bankaccountnumber-confirm").removeClass("input-invalid");

            if ($("#bankaccountnumber-confirm").is(":visible")) {
                if ($("#bankaccountnumber-input").val() != $("#bankaccountnumber-confirm > input").val()) {
                    //display alert
                    $("#bankaccountnumber-confirm > input").addClass("input-invalid");
                    return;
                }
            }
        }

        if ($("#routingnumber-input").is(":enabled") &&
            $("#routingnumber-input").val()) {

            $("#routingnumber-confirm").removeClass("input-invalid");

            if ($("#routingnumber-confirm").is(":visible")) {
                if ($("#routingnumber-input").val() != $("#routingnumber-confirm > input").val()) {
                    //display alert
                    $("#routingnumber-confirm > input").addClass("input-invalid");
                    return;
                }
            }
        }

        if (!obj || !obj.passive) {
            $("#loading-cover").show();
        }

        var updateInfo = {};

        $("#Personal-info input").each(function () {
            var fieldProperty = $(this).attr("name");

            var info = prefetchedUserTaxInfo[fieldProperty];

            if (info != null || info === null) {
                //Filter out undefined fields
                updateInfo[fieldProperty] = $(this).val();
            }
        });

        var state = $("#state-select").val();
        if (state != null || state === null) {
            updateInfo["State"] = state;
        }

        var taxYear = $("#taxYear").text();

        if (taxYear && ($("#airline-select").val() || $("#base-airport-select").val())) {
            updateInfo.TaxInfos = {};

            updateInfo.TaxInfos[taxYear] = {
                Year: taxYear,
                BaseAirportCode: $("#base-airport-select").val(),
                Airline: $("#airline-select").val()
            };
        }

        $.ajax({
            url: "/Home/UpdatePersonalInfo",
            type: "POST",
            data: updateInfo,
            success: function (userInfo) {
                refreshPersonalInfo(userInfo);
            },
            complete: function () {
                $("#loading-cover").fadeOut(500);

                if (!obj || !obj.passive) {
                    $(window).scrollTop(0);
                    $('div.left-menu-collapse').slideDown(500);
                }
            }
        });
    });

    $("#international-layover-checkbox").change(function (e, obj) {
        if (!obj || !obj.passive) {
            $("#loading-cover").show();
        }

        var taxYear = $("#taxYear").text();

        var updateInfo = { TaxInfos: {} };

        updateInfo.TaxInfos[taxYear] = { Year: taxYear };

        $("#tax-question input").each(function () {
            var fieldProperty = $(this).attr("name");

            var val = $(this).val();

            if (fieldProperty == "DaysInTrainingOrAway") {
                //parse in case we get inputs like '10 days'
                val = parseInt(val);
            } else {
                val = parseFloat(val.replace(/[^0-9\.]/g, ""));
            }

            if (isNaN(val)) {
                val = "";
            }

            //Filter out undefined fields
            updateInfo.TaxInfos[taxYear][fieldProperty] = val;
        });

        updateInfo.TaxInfos[taxYear]['DLState'] = $("#dl-select").val();
        updateInfo.TaxInfos[taxYear]['InternationalLayovers'] = e.target.checked;

        $.ajax({
            url: "/Home/UpdateTaxInfo",
            type: "POST",
            data: updateInfo,
            success: function (userInfo) {
                refreshTaxInfo(userInfo, $("#taxYear").text());
            },
            complete: function () {
                $("#loading-cover").fadeOut(500);

            }
        });
    });

    $("#save-tax-info-btn").click(function (e, obj) {
        if (!obj || !obj.passive) {
            $("#loading-cover").show();
        }

        var taxYear = $("#taxYear").text();

        var updateInfo = { TaxInfos: {} };

        updateInfo.TaxInfos[taxYear] = { Year: taxYear };

        $("#tax-question input").each(function () {
            var fieldProperty = $(this).attr("name");

            var val = $(this).val();

            if (fieldProperty == "DaysInTrainingOrAway") {
                //parse in case we get inputs like '10 days'
                val = parseInt(val);
            } else {
                val = parseFloat(val.replace(/[^0-9\.]/g, ""));
            }

            if (isNaN(val)) {
                val = "";
            }

            //Filter out undefined fields
            updateInfo.TaxInfos[taxYear][fieldProperty] = val;
        });

        updateInfo.TaxInfos[taxYear]['DLState'] = $("#dl-select").val();

        $.ajax({
            url: "/Home/UpdateTaxInfo",
            type: "POST",
            data: updateInfo,
            success: function (userInfo) {
                refreshTaxInfo(userInfo, $("#taxYear").text());
            },
            complete: function () {
                $("#loading-cover").fadeOut(500);

            }
        });
    });



    $("input[name='married']").change(function (e, obj) {
        if ($(this).val() == 'yes') {
            document.getElementById("marriedLabel").style.visibility = "visible";
        } else {
            document.getElementById("marriedLabel").style.visibility = "hidden";
        }
    });
    $("input[name='married']").change(function (e, obj) {
        if ($(this).val() == 'yes') {
            document.getElementById("marriedLabel").style.visibility = "visible";
        } else {
            document.getElementById("marriedLabel").style.visibility = "hidden";
        }
    });
    $("input[name='dependent']").change(function (e, obj) {
        if ($(this).val() == 'yes') {
            document.getElementById("dependentLabel").style.visibility = "visible";
        } else {
            document.getElementById("dependentLabel").style.visibility = "hidden";
        }
    });
    $("input[name='loans']").change(function (e, obj) {
        if ($(this).val() == 'yes') {
            document.getElementById("loansLabel").style.visibility = "visible";
        } else {
            document.getElementById("loansLabel").style.visibility = "hidden";
        }
    });
    $("input[name='stocks']").change(function (e, obj) {
        if ($(this).val() == 'yes') {
            document.getElementById("stocksLabel").style.visibility = "visible";
        } else {
            document.getElementById("stocksLabel").style.visibility = "hidden";
        }
    });
    $("input[name='house']").change(function (e, obj) {
        if ($(this).val() == 'yes') {
            document.getElementById("houseLabel").style.visibility = "visible";
        } else {
            document.getElementById("houseLabel").style.visibility = "hidden";
        }
    });
    $("input[name='hsa']").change(function (e, obj) {
        if ($(this).val() == 'yes') {
            document.getElementById("hsaLabel").style.visibility = "visible";
        } else {
            document.getElementById("hsaLabel").style.visibility = "hidden";
        }
    });
    $("input[name='1098T']").change(function (e, obj) {
        if ($(this).val() == 'yes') {
            document.getElementById("1098TLabel").style.visibility = "visible";
        } else {
            document.getElementById("1098TLabel").style.visibility = "hidden";
        }
    });
    $("input[name='1099R']").change(function (e, obj) {
        if ($(this).val() == 'yes') {
            document.getElementById("1099RLabel").style.visibility = "visible";
        } else {
            document.getElementById("1099RLabel").style.visibility = "hidden";
        }
    });

    $("input[name='1099G']").change(function (e, obj) {
        if ($(this).val() == 'yes') {
            document.getElementById("1099GLabel").style.visibility = "visible";
        } else {
            document.getElementById("1099GLabel").style.visibility = "hidden";
        }
    });
    $("input[name='1099INT']").change(function (e, obj) {
        if ($(this).val() == 'yes') {
            document.getElementById("1099INTLabel").style.visibility = "visible";
        } else {
            document.getElementById("1099INTLabel").style.visibility = "hidden";
        }
    });
    $("input[name='other']").change(function (e, obj) {
        if ($(this).val() == 'yes') {
            document.getElementById("otherLabel").style.visibility = "visible";
        } else {
            document.getElementById("otherLabel").style.visibility = "hidden";
        }
    });
    $("input[name='ownBusiness']").change(function (e, obj) {
        if ($(this).val() == 'yes') {
            document.getElementById("ownBusinessLabel").style.visibility = "visible";
        } else {
            document.getElementById("ownBusinessLabel").style.visibility = "hidden";
        }
    });
    $("input[name='scheduleK1']").change(function (e, obj) {
        if ($(this).val() == 'yes') {
            document.getElementById("scheduleK1Label").style.visibility = "visible";
        } else {
            document.getElementById("scheduleK1Label").style.visibility = "hidden";
        }
    });
    $("input[name='newHire']").change(function (e, obj) {
        if ($(this).val() == 'yes') {
            document.getElementById("newHireLabel").style.visibility = "visible";
        } else {
            document.getElementById("newHireLabel").style.visibility = "hidden";
        }
    });
    $("input[name='multipleW2s']").change(function (e, obj) {
        if ($(this).val() == 'yes') {
            document.getElementById("multipleW2sLabel").style.visibility = "visible";
        } else {
            document.getElementById("multipleW2sLabel").style.visibility = "hidden";
        }
    });

    $("input[name='itemize']").change(function (e, obj) {
        if ($(this).val() == 'yes') {
            document.getElementById("itemizeLabel").style.visibility = "visible";
            $('[name="question-driveToWork"], [name="question-flyReserveDays"]').css("display", "inherit")
        } else {
            document.getElementById("itemizeLabel").style.visibility = "hidden";
            $('[name="question-driveToWork"], [name="question-flyReserveDays"]').css("display", "none")
            clearYesNoQuestion('driveToWork');
            clearYesNoQuestion('flyReserveDays');
        }
    });



    $("#airline-select").select2({
        theme: "bootstrap",
        placeholder: "Select your airline",
        allowClear: true,
        width: '100%'
    });

    $("#base-airport-select").select2({
        theme: "bootstrap",
        placeholder: "Search airports...",
        width: '100%',
        allowClear: true,
        ajax: {
            url: "/Home/SearchAirports",
            dataType: 'json',
            delay: 250,
            processResults: function (data) {
                var results = [];

                $.each(data, function (index, airport) {
                    results.push({
                        id: airport.Id,
                        text: airport.Text
                    });
                });

                return {
                    results: results
                };
            },
            cache: true
        },
        templateResult: function (item) {
            if (item.id && item.text) {
                return "<span>" + item.id + " (" + item.text + ")</span>";
            }

            return item.id || item.text;
        },
        templateSelection: function (item) {
            if (item.id && item.text) {
                return "<span>" + item.id + " (" + item.text + ")</span>";
            }

            return item.id || item.text;
        },
        escapeMarkup: function (markup) { return markup; },
        minimumInputLength: 1,
        language: {
            inputTooShort: function (args) {
                return "Search by airport code or name";
            }
        }
    });

    // edit button on click
    $('.input-edit').on('click', function (e) {
        $(this)
            .parent()
            .addClass("hidden")
            .prevAll("input,select")
            .removeAttr("disabled")
            .focus()
            .prev(".input-group-addon").removeClass("input-group-disabled");
    });

    // tax year change
    $('.dropdown-a').on('click', function (e) {
        e.preventDefault();

        var value = $(this).text();

        $('#taxYear, #base-airport-date').text(value);

        $("#loading-cover").show();

        $(".dropzoneArea").each(function () {
            //remove all dropzone files
            $(this)[0].dropzone.removeAllFiles();
        });

        $.ajax({
            url: "/Home/GetPersonalInfo",
            type: "POST",
            data: {},
            success: function (userInfo) {
                refreshDashboardViews(userInfo, $("#taxYear").text());
                //Return view to Personal Info
                if (!$('div#Personal-info').is(":visible")) {
                    console.log('in' + elemId);
                    $('div.box-dashboard-container').hide();
                    $('div#Personal-info').fadeIn(500);
                    $('div#tax-question').hide();
                    $('div#make-payment').hide();
                    $('div#tax-documents-container').hide();
                    $('div#tax-return-container').hide();
                    $('div#no-tax-return-container').hide();
                    $('div#ynQuestions').hide();
                    $('div.btn-container').css('background', '#999');
                    elem.closest('div.btn-container').css('background', '#E74139');

                    if (!$('div#options .menu-dashboard').is(":visible"))
                        $('div.left-menu-collapse').slideUp(500);
                }
            },
            complete: function () {
                $("#loading-cover").fadeOut(500);
            }
        });


    });

    $("#submit-question-btn").click(function () {

        if ($("#question-textarea").val() == "") {
            return;
        }

        $("#loading-cover").show();

        $.ajax({
            url: "/Home/SubmitQuestion",
            type: "POST",
            data: {
                question: $("#question-textarea").val(),
                taxYear: parseInt($("#taxYear").text())
            },
            success: function (userInfo) {
                refreshQuestions(userInfo, $("#taxYear").text());
                $("#question-textarea").val("");
            },
            complete: function () {
                $("#loading-cover").fadeOut(500);
            }
        });
    });

    $(".guide-container").click(function () {
        var linkId = "#" + $(this).attr("id").replace(/-.*/, "");

        $(linkId).click();
    })

    $(".popup-close").click(function () {
        $(this).closest(".dashboard-popup").fadeOut(300);
    });

    $(".dashboard-popup").on("click", function (e) {
        if ($(e.target).attr("class") == $(this).attr("class")) {
            $(this).fadeOut(300);
        }
    });

    if (prefetchedUserTaxInfo.FirstTimeLogin) {
        $("#welcome-popup").removeClass("hidden");

        $.ajax({
            type: "POST",
            url: "/Home/UpdateFirstTimeLogin"
        });
    }

    $(".content-center").on("click", function () {
        if (!$('div#options .menu-dashboard').is(":visible")) {
            $('div.left-menu-collapse').slideUp(500);
        }
    });

    $("#payment-submit").click(function () {
        $("#payment-btn-hidden").click();
    });

    $("#bankaccountnumber-input").on("keyup", function () {
        $("#bankaccountnumber-confirm").removeClass("hidden");
        $("#bankaccountnumber-confirm > input").removeClass("input-invalid");
    });

    $("#bankaccountnumber-confirm > input").on("keyup", function () {
        $(this).removeClass("input-invalid");
    });

    $("#routingnumber-input").on("keyup", function () {
        $("#routingnumber-confirm").removeClass("hidden");
        $("#routingnumber-confirm > input").removeClass("input-invalid");
    });

    $("#routingnumber-confirm > input").on("keyup", function () {
        $(this).removeClass("input-invalid");
    });

    $("#dd-tooltip").tooltip().click(function (e) {
        e.preventDefault();
    });

    Dropzone.options.taxDropzone = {
        maxFilesize: 10,
        dictDefaultMessage: "Drop files here or click to upload",
        autoProcessQueue: true,
        init: function () {
            this.on("addedfile", function (file) {
                //if (file.type.indexOf('pdf') >= 0) {
                //    var pdfURL = URL.createObjectURL(file);

                //    parsePDFText(pdfURL);
                //}
            });

            this.on("sending", function (file, xhr, formData) {
                formData.append("taxYear", parseInt($("#taxYear").text()));

            });

            this.on("success", function (file) {
                var response = JSON.parse(file.xhr.response);

                if (response.status == 0) {
                    setTimeout(function () {
                        $(".dz-success-mark", file.previewElement).css("opacity", 1);
                    }, 500);

                    var $uploadAlert = $("<div class='alert alert-success' role='alert'>" + file.name + " was successfully uploaded</div>");
                    $uploadAlert.appendTo($("#taxdropzone-alerts"));

                    setTimeout(function () {
                        $uploadAlert.slideUp(500);
                    }, 2000);
                } else {
                    alert("We failed to upload your file, please try again later");
                }
            });

            this.on("complete", function (file) {
            });

            this.on("queuecomplete", function () {

            });
        }
    }


    Dropzone.options.csvDropzone = {
        maxFilesize: 3,
        dictDefaultMessage: "Drop files here or click to upload",
        acceptedFiles: ".csv,.xls,.xlsx",
        autoProcessQueue: true,
        init: function () {
            this.on("addedfile", function (file) {
                if (file.name.match(/\.csv$/i)) {
                    var reader = new FileReader();
                    var parsedCSV = {};
                    var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                    var layoverDict = [];

                    reader.onload = function (e) {
                        var lines = e.target.result.split('\n');

                        parsedCSV.numRows = lines.length;

                        lines.forEach(function (line, rowNum) {
                            line.split(',').forEach(function (word, wordNum) {
                                parsedCSV[chars[wordNum] + (rowNum + 1)] = word.trim();
                            });
                        });

                        for (var rowIt = 2; rowIt <= parsedCSV.numRows; rowIt) {
                            var curRotation = parsedCSV['A' + rowIt];
                            var lastLayover = null;
                            var lastLayoverDate = null;

                            while (parsedCSV['A' + rowIt] == curRotation) {

                                var layover = parsedCSV['J' + rowIt];

                                if (layover) {
                                    lastLayover = layover;
                                    lastLayoverDate = new Date(parsedCSV['P' + rowIt]);

                                    if (!layoverDict[layover]) {
                                        layoverDict[layover] = 0;
                                    }

                                    if (parsedCSV['A' + (rowIt + 1)] == curRotation) {
                                        var milliseconds = (new Date(parsedCSV['P' + (rowIt + 1)])) - (new Date(parsedCSV['P' + rowIt]));

                                        var days = milliseconds / 1000 / 3600 / 24;

                                        layoverDict[layover] += days;
                                    }
                                }

                                rowIt++;
                            }

                            if (lastLayover) {
                                layoverDict[lastLayover]++;
                            }
                        }

                        //flatten layoverDict

                        var layovers = [];

                        for (var l in layoverDict) {
                            layovers.push({ AirportCode: l, Days: layoverDict[l] });
                        }

                        $.ajax({
                            url: "/Home/CalculatePerDiem",
                            type: "POST",
                            data: {
                                taxYear: parseInt($("#taxYear").text()),
                                layovers: layovers
                            },
                            success: function (response) {
                                $("#loading-cover").fadeOut(500);
                            }
                        });

                    };

                    reader.readAsText(file);
                }

            });

            this.on("sending", function (file, xhr, formData) {
                formData.append("taxYear", parseInt($("#taxYear").text()));
                //formData.append("subFolder", "Layover CSVs");
            });

            this.on("success", function (file) {
                var response = JSON.parse(file.xhr.response);

                if (response.status == 0) {
                    setTimeout(function () {
                        $(".dz-success-mark", file.previewElement).css("opacity", 1);
                    }, 500);

                    var $uploadAlert = $("<div class='alert alert-success' role='alert'>" + file.name + " was successfully uploaded</div>");
                    $uploadAlert.appendTo($("#csvdropzone-alerts"));

                    setTimeout(function () {
                        $uploadAlert.slideUp(500);
                    }, 2000);

                } else {
                    alert("We failed to upload your file, please try again later");
                }
            });

            this.on("complete", function (file) {
            });

            this.on("queuecomplete", function () {
            });
        }
    }
});

function getPageText(pageNum, PDFDocumentInstance) {
    // Return a Promise that is solved once the text of the page is retrieven
    return new Promise(function (resolve, reject) {
        PDFDocumentInstance.getPage(pageNum).then(function (pdfPage) {
            pdfPage.getTextContent().then(function (textContent) {
                resolve(textContent.items);
            });
        });
    });
}

function getAllText(PDFDocumentInstance) {
    return new Promise(function (resolve, reject) {
        var result = [];

        (function textWorker(pageNumber) {
            getPageText(pageNumber, PDFDocumentInstance).then(function (textArray) {
                result = result.concat(textArray);

                if (pageNumber < PDFDocumentInstance.pdfInfo.numPages) {
                    textWorker(pageNumber + 1);
                } else {
                    resolve(result);
                }
            });
        })(1);
    });
}

function parsePDFText(pdfURL) {
    PDFJS.getDocument(pdfURL).then(function (PDFDocumentInstance) {
        getPageText(1, PDFDocumentInstance).then(function (textArray) {
            if (isIRSTranscript(textArray)) {
                IRSDemo(PDFDocumentInstance, textArray);
            }
        });

    }, function (reason) {
        // PDF loading error
        console.error(reason);
    });
}

function IRSDemo(PDFDocumentInstance, page1) {
    var strIt = 0;
    var taxYear = '';
    var username = '';
    for (var t of page1) {
        if (t.str.indexOf('Tax Period Requested') >= 0) {
            taxYear = t.str.match(/.*\,.?(\d{4})/)[1];
        }

        if (/employee|participant|recipient/i.test(t.str)) {
            username = page1[strIt + 1].str.trim();
        }
        strIt++;
    }

    $("#IRS-demo-modal-title").text(`${username} ${taxYear}`);

    $('#IRS-demo-modal').modal();



    populateIRSDemo(1, PDFDocumentInstance);
}

function populateIRSDemo(pageNumber, PDFDocumentInstance) {
    getAllText(PDFDocumentInstance).then(function (textArray) {
        var parsedData = [];

        for (var ti = 0; ti < textArray.length; ti++) {
            var text = textArray[ti].str;
            var tagSearch = text.toLowerCase().trim();

            if (formDict[tagSearch]) {
                var tagData = formDict[tagSearch];
                var parsedObj = {
                    form: text.trim(),
                    class: formDict[tagSearch].class
                };

                for (var tagi = ti + 1; tagi < textArray.length; tagi++) {
                    var formText = textArray[tagi].str;

                    if (formDict[textArray[tagi].str.toLowerCase().trim()]) {
                        // we've found the beginning of a new form
                        break;
                    }

                    for (var tagName in tagData) {
                        var tag = tagData[tagName];

                        if (!parsedObj[tagName] && (new RegExp(tagData[tagName].name, 'i')).test(formText)) {
                            parsedObj[tagName] = {
                                tagName: tagData[tagName].name
                            };

                            if (typeof tag.offset == 'number') {
                                parsedObj[tagName].text = textArray[tagi + tag.offset].str.trim();
                            } else {
                                parsedObj[tagName].text = formText.substr(formText.lastIndexOf(tag.offset) + 1).trim();
                            }
                        }
                    }
                }

                parsedData.push(parsedObj);
            }
        }

        animateIRSListAdd(parsedData, 0);
    });
}

function animateIRSListAdd(items, i) {
    if (!items[i]) return;

    if (i < items.length) {
        setTimeout(() => animateIRSListAdd(items, i + 1), Math.random() * 1000 + 300);
    }

    var item = items[i];

    $('#IRS-demo-modal-list').append(`
                <li class="new-item ${item.class}">
                    <table class="irs-form-table">
                        <tbody>
                            <tr><td style='width:200px'>Form:</td><td>${item.form}</td></tr>
                            <tr><td>${item.fromTag.tagName}:</td><td>${item.fromTag.text}</td></tr>
                            <tr><td>${item.idTag.tagName}:</td><td>${item.idTag.text}</td></tr>
                            ${item.accTag ? '<tr><td>' + item.accTag.tagName + ':</td><td>' + item.accTag.text + '</td></tr>' : ''}
                        </tbody>
                    </table>
                </li>
            `);

}

function isIRSTranscript(textArray) {
    return textArray[0].str.trim() == 'This Product Contains Sensitive Taxpayer Data';
}
var formDict = {
    'form w-2 wage and tax statement': {
        fromTag: {
            name: 'Employer',
            offset: 2
        },
        toTag: {
            name: 'Employee',
            offset: 2
        },
        idTag: {
            name: 'EIN',
            offset: ':'
        },
        class: 'form-w-2'
    },
    'form 5498 sa': {
        fromTag: {
            name: 'Trustee',
            offset: 2
        },
        toTag: {
            name: 'Participant',
            offset: 2
        },
        idTag: {
            name: 'FIN',
            offset: ':'
        },
        accTag: {
            name: 'Account Number',
            offset: '.'
        },
        class: 'form-5498-sa'
    },
    'form 1099-b proceeds from broker and barter exchange transactions': {
        fromTag: {
            name: 'Payer',
            offset: 2
        },
        toTag: {
            name: 'Recipient',
            offset: 2
        },
        idTag: {
            name: 'FIN',
            offset: ':'
        },
        accTag: {
            name: 'Account Number',
            offset: '.'
        },
        class: 'form-1099-b'
    },
    'form 1099-div': {
        fromTag: {
            name: 'Payer',
            offset: 2
        },
        toTag: {
            name: 'Recipient',
            offset: 2
        },
        idTag: {
            name: 'FIN',
            offset: ':'
        },
        accTag: {
            name: 'Account Number',
            offset: '.'
        },
        class: 'form-1099-div'
    },
    'form 1099-int': {
        fromTag: {
            name: 'Payer',
            offset: 2
        },
        toTag: {
            name: 'Recipient',
            offset: 2
        },
        idTag: {
            name: 'FIN',
            offset: ':'
        },
        accTag: {
            name: 'Account Number',
            offset: '.'
        },
        class: 'form-1099-int'
    },
    'form 1099-sa': {
        fromTag: {
            name: 'Payer',
            offset: 2
        },
        toTag: {
            name: 'Recipient',
            offset: 2
        },
        idTag: {
            name: 'FIN',
            offset: ':'
        },
        accTag: {
            name: 'Account Number',
            offset: '.'
        },
        class: 'form-1099-sa'
    }
};
