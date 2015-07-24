/// <reference path="jquery-1.11.2.js" />
/// <reference path="jquery-ui-1.11.4.js" />
/// <reference path="bootstrap.js" />
/// <reference path="odatajs-4.0.0.js" />
/// <reference path="common.js" />

/* global $home */

// *** pages ***

$home.pages = (function () {
    return {
        "login": {
            init: function () {
                $("#login-message").hide();
                $("#login-name").focus();
                $("#login-name, #login-password").keypress(function (e) {
                    if (e.keyCode === 13) {
                        $("#login-button").click();
                    }
                });
                $("#login-button").click(function (e) {
                    e.preventDefault();
                    $.ajax({
                        type: "POST",
                        url: "/account/login",
                        data: JSON.stringify({ name: $("#login-name").val(), password: $("#login-password").val() }),
                        contentType: "application/json",
                        headers: { RequestVerificationToken: $("#login input[name=__AjaxRequestVerificationToken]").val() },
                        cache: false
                    }).done(function () {
                        document.location.assign("/");
                    }).fail(function () {
                        $("#login-message").show();
                    });
                });
            }
        }
    };
})();