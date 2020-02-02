$(function () {
	window.addEventListener('scroll', addWhiteBacgorundForToolbarIfPageScrolled)
	addWhiteBacgorundForToolbarIfPageScrolled();

	// Seting up log form and validations
	$("#login-form").validate(formsValidators.login(loginUser))

	// Setting up user creation forms, validation and process
	var registrationProcess = registerUser();
	$("#create-account-form").validate(formsValidators.createAccount(registrationProcess.createUserAndSendCode));
	$("#confirm-phone-number-form").validate(formsValidators.confirmPhoneNumber(registrationProcess.confirmPhoneNumber));
	$("#complete-registration-form").validate(formsValidators.completeRegistraion(registrationProcess.finishRegistration));

	// Setting inputs masks 
	$('[name="phoneNumber"]').inputmask('(999) 999-9999');
	$('[name="code"]:not(:hidden)').inputmask('999999');

	// Setting up reset password modal
	$('#create-password-reset-code-form').validate(formsValidators.textResetPasswordCode(textResetPasswordCode));

	if (shouldShowPasswordReset) {
		$("#set-new-password-modal").modal('show');
		$("#set-new-password-form").validate(formsValidators.setNewPassword(setNewPasswordWithCode));
	}

	// Clean up all forms that are in modal which was hidden
	$('.modal').on('hidden.bs.modal', function () {
		$(this).find('form').each(function () {
			var form = Form($(this));
			form.cleanUp();
		})	
	})

	// Closing mobile menu if any element was clicked
	$('.navbar-collapse').on('shown.bs.collapse', function () {
		var navbar = this;
		var hide = function () { $(navbar).collapse('hide') };
		$(navbar).find('a, button').on('click', hide);
	})
})

function addWhiteBacgorundForToolbarIfPageScrolled () {
    var distanceFromTop = document.documentElement.scrollTop ? document.documentElement.scrollTop : document.body.scrollTop;
    var topBar = $('.topbar')
    if (distanceFromTop > 20) {
        topBar.addClass('bg-white shadow-lg');
    } else {
        topBar.removeClass('bg-white shadow-lg');
    }
};

function loginUser(form) {
	var form = Form($(form));
	var data = form.getData();
	form.loading(true);
	form.hideError();

	$.ajax({
		type: "POST",
		url: "/Account/LoginAjax",
		data: data,
		success: function (response) {
			if (response.status == 0) {
				window.location = "/";
			} else {
				form.loading(false);
				form.showError();
			}
		}
	});

	return false;
};

function registerUser() {
	var userId;
	var phoneNumber
	var createAccountForm = $("#create-account-form");
	var confirmForm = $("#confirm-phone-number-form");
	var completeForm = $("#complete-registration-form");

	function createUserAndSendCode(form) {
		var form = Form($());
		var data = form.getData();
		data.phoneNumber = formatPhoneNumber(data.phoneNumber)

		form.loading(true);
		form.hideError();

		$.ajax({
			type: "POST",
			url: " /account/RegisterAndSendTextCode",
			data: data,
			success: function (response) {
				if (response.status == 0) {
					userId = response.userId;
					phoneNumber = response.phoneNumber;
					createAccountForm.hide();
					confirmForm.show();

				} else {
					form.loading(false);
				}
			}
		});

		return false;
	} 

	function confirmPhoneNumber(form) {
		var form = Form($(form));
		var data = form.getData();

		data.userId  = userId;
		data.phoneNumber = phoneNumber;

		form.loading(true);
		form.hideError();

		$.ajax({
			type: "POST",
			url: " /account/VerifyTextCode",
			data: data,
			success: function (response) {
				if (response.status == 0) {
					confirmForm.hide();
					$('#complete-registration').show();
					$('#complete-registration').find("[name='userId']").val(userId);
				} else {
					form.loading(false);
					form.showError();
				}
			}
		});

		return false;
	}

	function finishRegistration(form) {
		var form = Form($(form));
		var data = form.getData();

		data.userId = userId;
		form.loading(true);
		form.hideError();

		$.ajax({
			type: "POST",
			url: "/Account/FinishRegistration",
			data: data,
			success: function (response) {
				if (response.status == 0) {
					window.location = "/";
				} else {
					form.loading(false);
					form.showError();
				}
			}
		});

		return false;
	}

	return {
		createUserAndSendCode,
		confirmPhoneNumber,
		finishRegistration
	}
}

function textResetPasswordCode(form) {
	var form = Form($(form));
	var data = form.getData();

	data.phoneNumber = formatPhoneNumber(data.phoneNumber);

	form.loading(true);
	form.hideError();

	$.ajax({
		type: "POST",
		url: "/Account/TextPasswordResetCode",
		data: data,
		success: function (response) {
			if (response.status == 0) {
				form.cleanUp();
				form.closeModal();
				toastr.success('A password reset link has been sent');
			} else {
				form.loading(false);
				form.showError();
			}
		}
	});

	return false;
}

function setNewPasswordWithCode(form) {
	var form = Form($(form));
	var data = form.getData();

	form.loading(true);
	form.hideError();

	$.ajax({
		type: "POST",
		url: "/Account/ResetTextPassword",
		data: data,
		success: function (response) {
			if (response.status == 0) {
				form.cleanUp();
				form.closeModal();
				toastr.success('New password for your account has been set. Please log in.')
				$('#login-modal').modal().show()
			} else {
				form.loading(false);
				form.showError();
			}
		}
	});

	return false;
}

function Form(form) {
	var submitButton = form.find('[type="submit"]');
	var submitButtonText = submitButton.html();
	var spinner = '<div class="spinner-border text-light" role="status"></div>' 

	function loading(loading) {
		form.find('input, button').prop('disabled', loading)
		submitButton.html(loading ? spinner : submitButtonText);
	};

	function getData() {
		var data = {};
		var serializedData = form.serializeArray();

		for (var i = 0; i < serializedData.length; i++) {
			var input = serializedData[i];
			data[input.name] = input.value;
		}

		return data;
	}

	function toggleError() {
		form.find('.alert-warning').toggle();
	}

	function showError() {
		form.find('.alert-warning').show();
	}

	function hideError() {
		form.find('.alert-warning').hide();
	}

	function cleanUp() {
		loading(false);
		form[0].reset();
		form.validate().resetForm();
	}

	function closeModal() {
		form.closest('.modal').modal('hide');
	}

	return {
		loading,
		getData,
		toggleError,
		showError,
		hideError,
		cleanUp,
		closeModal,
	}
}


// Forms validations rules
var formsValidators = {
	login: function (handler) {
		return {
			rules: {
				Email: {
					required: true,
					email: true,
				},
				Password: "required",
			},
			messages: {
				Email: {
					required: "Please enter your email",
					email: "Please enter valid email",
				},
				Password: "Please enter password",
			},
			errorPlacement: function (error, element) {
				$(element).parents('.form-group').append(error)
			},
			submitHandler: handler,
		};
	}, 

	createAccount: function (handler) {
		return {
			rules: {
				firstName: "required",
				lastName: "required",
				phoneNumber: {
					required: true,
					phoneUS: true,
				},
			},
			messages: {
				firstName: "Please enter your firstname",
				lastName: "Please enter your lastname",
				phoneNumber: {
					required: "Please enter your phone number",
					phoneUS: "Please enter valid phone number"
				}
			},
			errorPlacement: function (error, element) {
				$(element).parents('.form-group').append(error)
			},
			submitHandler: handler,
		};
	}, 

	confirmPhoneNumber: function (handler) {
		return {
			rules: {
				code: "required"
			},
			messages: {
				code: "Please enter confirmation code"
			},
			errorPlacement: function (error, element) {
				$(element).parents('.form-group').append(error)
			},
			submitHandler: handler
		};
	},

	completeRegistraion: function (handler) {
		return {
			rules: {
				email: {
					required: true,
					email: true,
				},
				password: {
					required: true,
					wtPassword: true,
				},
				confirmpassword: {
					required: true,
					equalTo: "#create-account-password",
				},
			},
			messages: {
				email: {
					required: "Please enter your email",
					email: "Please enter valid email",
				},
				password: {
					required: "Please enter password",
					wtPassword: "Passwords need to be least 8 characters, have 1 uppercase, lowercase, and a digit"

				},
				confirmpassword: {
					required: "Please confirm password",
					equalTo: "Passwords don't match.",
				}
			},
			errorPlacement: function (error, element) {
				$(element).parents('.form-group').append(error)
			},
			submitHandler: handler
		}; 
	},

	textResetPasswordCode: function (handler) {
		return {
			rules: {
				phoneNumber: {
					required: true,
					phoneUS: true,
				},
			},
			messages: {
				phoneNumber: {
					required: "Please enter your phone number",
					phoneUS: "Please enter valid phone number"
				}
			},
			errorPlacement: function (error, element) {
				$(element).parents('.form-group').append(error)
			},
			submitHandler: handler,
		};
	},

	setNewPassword: function (handler) {
		return {
			rules: {
				password: {
					required: true,
					wtPassword: true,
				},
				confirmpassword: {
					required: true,
					equalTo: "#new-password",
				},
			},
			messages: {
				password: {
					required: "Please enter new password",
					wtPassword: "Passwords need to be least 8 characters, have 1 uppercase, lowercase, and a digit"

				},
				confirmpassword: {
					required: "Please confirm new password",
					equalTo: "Passwords don't match.",
				}
			},
			errorPlacement: function (error, element) {
				$(element).parents('.form-group').append(error)
			},
			submitHandler: handler,
		};
	},
}


function formatPhoneNumber(phoneNumberString) {
	return '+1' + phoneNumberString.replace(/\D/g, '');
}

// Custom validators 
jQuery.validator.addMethod("phoneUS", function (phone_number, element) {
	phone_number = phone_number.replace(/\s+/g, "");
	return this.optional(element) || phone_number.length > 9 &&
		phone_number.match(/^(\+?1-?)?(\([2-9]\d{2}\)|[2-9]\d{2})-?[2-9]\d{2}-?\d{4}$/);
}, "Please specify a valid phone number");


jQuery.validator.addMethod("wtPassword", function (password, element) {
	return this.optional(element) || password.match(/(?=^.{8,}$)(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&amp;*()_+}{&quot;:;'?/&gt;.&lt;,])(?!.*\s).*$/);
}, "Passwords need to be least 8 characters, have 1 uppercase, lowercase, and a digit");

// Toast settings
toastr.options = {
	"closeButton": false,
	"debug": false,
	"newestOnTop": false,
	"progressBar": true,
	"positionClass": "toast-top-center",
	"preventDuplicates": false,
	"onclick": null,
	"showDuration": "200",
	"hideDuration": "100",
	"timeOut": "5000",
	"extendedTimeOut": "1000",
	"showEasing": "swing",
	"hideEasing": "linear",
	"showMethod": "fadeIn",
	"hideMethod": "fadeOut"
}
