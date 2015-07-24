/// <reference path="jquery-1.11.2.js" />
/// <reference path="jquery-ui-1.11.4.js" />
/// <reference path="bootstrap.js" />
/// <reference path="odatajs-4.0.0.js" />

/* global odatajs */

// *** home ***

var $home = {
};

// *** observer design pattern ***

$home.observable = function () {
    this.handlers = [];
};

$home.observable.prototype.subscribe = function (fn) {
    this.handlers.push(fn);
};

$home.observable.prototype.unsubscribe = function (fn) {
    this.handlers = this.handlers.filter(function (handler) {
        if (handler !== fn) {
            return handler;
        }
    });
};

$home.observable.prototype.fire = function (argument, owner) {
    var scope = owner || window;
    this.handlers.forEach(function (handler) {
        handler.call(scope, argument);
    });
};

// *** UI ***

$home.ui = (function () {
    return {
        editableselect: function (element) {
            $("ul > li > a", element).click(function () {
                if (!$(this).parent().hasClass("divider")) {
                    $("input[type=text]", element).val($(this).text());
                    $("input[type=hidden]", element).val($(this).attr("data-content"));
                }
            });
        },

        selectbutton: function (element) {
            $("ul > li > a", element).click(function () {
                if (!$(this).parent().hasClass("divider")) {
                    $("button", element).html($(this).text() + " " + "<span class='caret'></span>");
                    $("input[type=hidden]", element).val($(this).attr("data-content"));
                }
            });
        }
    };
})();

// *** validation ***

$home.validation = (function () {
    return {
        validate: function (input, validator, control) {
            var result = validator.call($(input));

            if (!control) {
                control = $(input).parentsUntil(".form-group").parent();
            }

            if (!result && !$(control).hasClass("has-error")) {
                $(control).addClass("has-error");
            }

            return result;
        },

        cleanup: function (parent) {
            if (parent) {
                $("*", parent).removeClass("has-error");
            } else {
                $("*").removeClass("has-error");
            }
        },

        validateRequired: function (input, control) {
            return $home.validation.validate(input, function () {
                return $(this).val().length > 0;
            }, control);
        },

        validateFloat: function (input, control) {
            return $home.validation.validate(input, function () {
                if ($(this).val() !== "") {
                    return parseFloat($(this).val()) > 0;
                }
                return true;
            }, control);
        },

        validateInteger: function (input, control) {
            return $home.validation.validate(input, function () {
                if ($(this).val() !== "") {
                    return parseInt($(this).val()) > 0;
                }
                return true;
            }, control);
        }
    };
})();

// *** utility ***

$home.utility = (function () {
    return {
        queryString: {
            getParameterByName: function (name) {
                name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
                var regex = new RegExp("[\\?&]" + name + "=([^&#]*)");
                var results = regex.exec(location.search);
                return results === null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
            }
        },

        currency: {
            translateToCapital: function (value) {
                var units = ["", "拾", "佰", "仟", "万", "拾", "佰", "仟", "亿", "拾", "佰", "仟"];
                var numbers = ["零", "壹", "贰", "叁", "肆", "伍", "陆", "柒", "捌", "玖"];
                var text = value.toString();
                var result = "元正";
                var zero;
                for (var i = 0; i < text.length; i++) {
                    var digit = parseInt(text.charAt(text.length - 1 - i));
                    var unit = units[i];
                    var number = numbers[digit];
                    var carry = ((i % 4) === 0);
                    if (digit === 0) {
                        if (carry) {
                            result = (unit + result);
                        } else if (!zero) {
                            result = (number + result);
                        }
                    } else {
                        result = (number + unit + result);
                    }
                    zero = (digit === 0);
                }
                return result;
            }
        },

        date: {
            getDateFormat: function () {
                return "yyyy-M-d";
            },

            getDateTimeFormat: function () {
                return $home.utility.date.getDateFormat() + " h:m:s";
            },

            toLocalDateString: function (date) {
                var format = $home.utility.date.getDateFormat();
                return format.replace("yyyy", date.getFullYear().toString())
                    .replace("M", (date.getMonth() + 1).toString())
                    .replace("d", date.getDate().toString());
            },

            toLocalDateTimeString: function (date) {
                var format = $home.utility.date.getDateTimeFormat();
                return format.replace("yyyy", date.getFullYear().toString())
                    .replace("M", (date.getMonth() + 1).toString())
                    .replace("d", date.getDate().toString())
                    .replace("h", date.getHours().toString())
                    .replace("m", date.getMinutes().toString())
                    .replace("s", date.getSeconds().toString());
            },

            toServerDateString: function (date) {
                return "yyyy-M-d".replace("yyyy", date.getFullYear().toString())
                    .replace("M", (date.getMonth() + 1).toString())
                    .replace("d", date.getDate().toString());
            }
        },

        odata: {
            toODataDateTimeOffset: function (value) {
                if (value) {
                    value.__edmType = "Edm.DateTimeOffset";
                    value.__offset = "Z";
                }
                return value;
            }
        }
    };
})();

$(function () {
    if ($.fn.modal) {
        $.fn.modal.Constructor.DEFAULTS.backdrop = "static";
    }

    if ($.datepicker) {
        $.datepicker.setDefaults($.datepicker.regional["zh-CN"]);
    }

    $(document).ajaxError(function (event, xhr, settings, error) {
        console.error("%s %s\n%s", settings.type || settings.method, settings.url, error);
    });

    if (odatajs) {
        odatajs.oData.defaultError = function (error) {
            var request = error.request;
            var response = error.response;
            console.error("%s %s\n%s\n%s, %s\n%s", request.method, request.requestUri, error.message, response.statusCode, response.statusText, response.body);
        };
    }
});
