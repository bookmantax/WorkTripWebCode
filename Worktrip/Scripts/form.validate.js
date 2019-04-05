function validateEmail(email) {
    var re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    return re.test(email);
}

$(document).ready(function () {
	var errorStructStart = '<div id="message" style=" z-index:1000; padding-top:10px;"><div class="alert alert-danger" ><p><strong>Error! </strong><button type="button" class="close" data-dismiss="alert">×</button>';
	//var errorStructStart = '<div id="message"><div style="padding: 5px;"><div id="inner-message" class="alert alert-error"><button type="button" class="close" data-dismiss="alert">×</button>';
	var errorStructEnd = '</p></div></div>';
	var cotainerDiv;
	// show login
	var isVerificationDone = false;

	var newUserId, newPhoneNumber;

	$("input[name='phone']").on("keyup", function () {

	    if ($(this).val().length == 3) {
	        var $nextInput = $(this).closest('.field').next('.field').find("input[name='phone']");

	        $nextInput.focus();
	    }

	});

	$("button#letsTalkVerify").on("click", function (e) {
		e.preventDefault();
		$('div#message').remove();
		containerDiv = $(this).closest("div.lets-talk-inner-wrapper");

		if(loopForm("letsTalkForm")){
			//console.log('yes');
		    // $('img#imgCodeSuccess').removeClass("hidden");

		    var number = "+1";
		    $("input[name='phone']").each(function () {
		        number += $(this).val();
		    });

		    $('div.phoneCodeDiv').removeClass('hidden');
		    $(".phoneCodeDiv").children("#afterCode,.row").addClass("hidden");

		    $('div#loadingAnim').removeClass('hidden');

		    $("button#letsTalkVerify").addClass("hidden");
		    $("div#beforeCode").addClass("hidden");
		    $('div#mobile-phone-fields').addClass('hidden');

		    //logout of facebook if logged in
		    //$.getScript('//connect.facebook.net/en_US/sdk.js', function () {
		    //    FB.init({
		    //        appId: '247923912298956',
		    //        version: 'v2.7'
		    //    });
		    //    FB.getLoginStatus(function (response) { FB.logout(); });
		    //});

		    $.ajax({
		        url: "/account/RegisterAndSendTextCode",
		        type: "POST",
		        data: {
		            phoneNumber: number,
		            firstName: $("#letsTalkFirstName").val(),
                    lastName: $("#letsTalkLastName").val(),
                    promoCode: $("#letsTalkPromoCode").val()
		        },
		        success: function (response) {
		            console.log(response);

		            if (response.status == 0) {
		                newUserId = response.userId;
		                newPhoneNumber = response.phoneNumber;

		                $(".userId-input").val(newUserId);

		                $(".phoneCodeDiv").children("#afterCode,.row").removeClass("hidden");

		                $('button#letsTalkNext').removeClass('disabled');
		            } else {
		                showError(response.message);

		                $("button#letsTalkVerify").removeClass("hidden");
		                $("div#beforeCode").removeClass("hidden");
		                $('div#mobile-phone-fields').removeClass('hidden');
		            }

		            $('div#loadingAnim').addClass('hidden');
		        }
		    });

		}

	});

	$("#facebook-register").click(function (e) {
	    //if (!$("#cbTerms").is(":checked")) {
	    //    $(".checkbox-inline").css("color", "red");
	    //    e.preventDefault();
	    //}
	});

	$("#backPhoneLink").on("click", function (e) {
	    e.preventDefault();

	    $("button#letsTalkVerify").removeClass("hidden");
	    $("div#beforeCode").removeClass("hidden");
	    $('div#mobile-phone-fields').removeClass('hidden');

	    $(".phoneCodeDiv").children("#afterCode,.row").addClass("hidden");
	    $('button#letsTalkNext').addClass('disabled');
	});

	$("button#letsTalkNext").on("click", function (e) {
	    e.preventDefault();

	    var verificationCode = $('input#phoneCode').val();

	    if (!verificationCode) {
	        showError("Phone Code must not be empty");
	        return;
	    }

	    $('div#message').remove();
	    containerDiv = $(this).closest("div.lets-talk-inner-wrapper");
	    $('div#loadingAnim').removeClass('hidden');

	    $.ajax({
	        url: "/account/verifytextcode",
	        type: "POST",
	        data: {
                userId: newUserId,
                phoneNumber: newPhoneNumber,
                code: verificationCode
	        },
	        success: function (response) {
	            console.log(response);

	            if (response.status == 0) {
	                isVerificationDone = true;

	                $('div.phoneCodeDiv').addClass('hidden');
	                $(".phoneCodeDiv").children("#afterCode,.row").addClass("hidden");

	                $("button#letsTalkVerify").removeClass("hidden");
	                $("div#beforeCode").removeClass("hidden");
	                $('div#mobile-phone-fields').removeClass('hidden');

	                $('div.letsTalk').addClass("hidden");

	                $("div.letsTalkNext").fadeIn(500).show();
	            } else {
	                //failed verification
	                showError("Your verification code is incorrect");
	            }

	            $('div#loadingAnim').addClass('hidden');

	        }
	    });
	});

	$("#loginFormSubmit").on("click", function (e) {
		e.preventDefault();
		containerDiv = $(this).closest("div.lets-talk-inner-wrapper");
		$('div#message').remove();

		if (loopForm("loginForm")) {

		    $(".login").hide();

		    $.ajax({
		        type: "POST",
		        url: "/Account/LoginAjax",
		        data: {
		            Email: $("#loginForm input[name='email']").val(),
		            Password: $("#loginForm input[name='password']").val(),
		            UserId: $("#signupForm input[name='userid']").val()
		        },
		        success: function (response) {
		            if (response.status == 0) {
		                window.location = "/";
		            } else {
		                $(".login").show();

		                showError("This password does not match with the email");
		            }
		        }
		    });

		}

		return false;
    });

    $("#taxReturnFormSubmit").on("click", function (e) {
        e.preventDefault();
        containerDiv = $(this).closest("div.lets-talk-inner-wrapper");
        $('div#message').remove();

        if (loopForm("loginForm")) {

            $(".login").hide();

            $.ajax({
                type: "POST",
                url: "/Account/LoginAjax",
                data: {
                    Email: $("#loginForm input[name='email']").val(),
                    Password: $("#loginForm input[name='password']").val(),
                    UserId: $("#signupForm input[name='userid']").val()
                },
                success: function (response) {
                    if (response.status == 0) {
                        window.location = "/";
                    } else {
                        $(".login").show();

                        showError("This password does not match with the email");
                    }
                }
            });

        }

        return false;
    });

	$("button#letsTalkFormSubmit").on("click", function (e) {
		e.preventDefault();
		$('div#message').remove();
		containerDiv = $(this).closest("div.lets-talk-inner-wrapper");

		if (loopForm("signupForm")) {
			if (!$('#cbTerms').prop('checked')) {
				var message = "Please accept terms of use";
				showError(message);
				$(".checkbox-inline").css("color", "red");
				return false;
			} else {

			    $("div.letsTalkNext").fadeOut(500);

			    $.ajax({
			        type: "POST",
			        url: "/Account/FinishRegistration",
			        data:{
			            Email: $("#signupForm input[name='email']").val(),
			            Password: $("#signupForm input[name='password']").val(),
			            ConfirmPassword: $("#signupForm input[name='confirmpassword']").val(),
			            UserId: $("#signupForm input[name='userid']").val()
			        },
			        success: function (response) {
			            if (response.status == 0) {
			                location.reload();
			            } else {
			                showError("Something went wrong. We'll give you a call soon regarding this.");
			            }
			        }
			    });
			}
		} else {
			return false;
		}

	});


	function showError(message) {
		// body...
		var errorDiv = errorStructStart + message + errorStructEnd;
		// console.log($(this).closest("div.lets-talk-inner-wrapper"));
		containerDiv.append(errorDiv);

	}


	function validateEmpty(elem) {
		// body...
		//$this = $("#"+id);

		var val2 = elem.prop('value');

		if (val2 == null || val2 == "") {
			// empty

			return false;
		}
		return true;
	}


	function loopForm(formId) {
		// body...check
		var t = 0;
		var checker = true;
		$("form#" + formId + " *").filter(':input').each(function () {
			var elem = $(this);

			if (elem.prop('nodeName') != "BUTTON") {
				
				console.log('in');

                if (elem.attr('name') != "promoCode") {
                    if (!validateEmpty(elem)) {
                        if (elem.is(':visible')) {

                            var name = "";
                            var attname = elem.attr('name');
                            console.log(elem);
                            console.log(attname);
                            if (attname.indexOf("first") >= 0) {
                                name = "First Name";
                            } else if (attname.indexOf("last") >= 0) {
                                name = "Last Name";
                            } else if (attname.indexOf("email") >= 0) {
                                name = "Email "
                            } else if (attname.indexOf("password") >= 0) {
                                name = "Password";
                            } else if (attname.indexOf("confirm") >= 0) {
                                name = "Confirm Password";
                            } else if (attname.indexOf("phone") >= 0) {
                                name = "Phone";

                            }

                            showError(name + " must not be empty");
                            console.log('empty');
                            checker = false;
                            return false;
                        }


                        return false;
                    }
                }

				if (elem.attr('name') == "email") {
				    if (!validateEmail(elem.val())) {
				        showError("Please enter a valid email");
				        checker = false;
				        return false;
				    }
				}

				console.log(elem.attr('name'));
				if (elem.attr('name') == "password") {

					if (("" + elem.attr('id')).indexOf("signup") >= 0) {
					    if (/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$/.test(elem.val()) == false) {
							var message = "Passwords need to be least 8 characters, have 1 uppercase, lowercase, and a digit";
							showError(message);
							checker = false;
							return false;
						}
					    if (formId == "signupForm") {
					        if (elem.prop('value') != $('#signupConfirmPassword').prop('value')) {

					            var message = "Password doesn't match";
					            showError(message);
					            checker = false;
					            return false;
					        }
					    }
					}
				}

				// for phone number
				if (elem.attr('name') == "phone") {

					var num = elem.prop('value');
					var regex = new RegExp("^[0-9]*$");
					t++;
					if (!regex.test(num)) {

						showError(elem.attr('name') + " contains numbers only");
						checker = false;
						return false;
					}


				}


			}
		});
		console.log('out');
		if(checker)
			return true;
		else
			return false;
	}


});