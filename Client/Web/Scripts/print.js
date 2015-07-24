/// <reference path="jquery-1.11.2.js" />
/// <reference path="odatajs-4.0.0.js" />
/// <reference path="common.js" />

/* global odatajs, $home */

$home.print = (function () {
    return {
        "print-service": {
            init: function () {
                $home.print.internal.getSaleById($home.utility.queryString.getParameterByName("id"), function (data) {
                    function addProperty(target, name, value) {
                        if (value === undefined || value === null) {
                            return;
                        }

                        var old = $(target).text();
                        if (name === undefined || name === null) {
                            name = "";
                        }

                        $(target).text(old + " " + name + value + " ");
                    }

                    function addProduct(target, detail) {
                        addProperty(target, "证书", detail.Certificate);
                        addProperty(target, "净度", detail.Clarity);
                        addProperty(target, "颜色", detail.Color);
                        addProperty(target, "重量", detail.Caret);
                        addProperty(target, "切工", detail.Cut);
                        addProperty(target, "数量", detail.Quantity);
                        addProperty(target, "", detail.ProductDescription);
                    }

                    var date = new Date(data.Created);
                    $("#number, #stub-number").text(data.NumberText);
                    $("#year").text(date.getFullYear());
                    $("#month").text(date.getMonth() + 1);
                    $("#day").text(date.getDate());
                    $("#stub-customer-name").text(data.CustomerName);
                    $("#stub-customer-phone").text(data.CustomerContacts[0].Value);
                    $("#stub-customer-email").text(data.CustomerContacts[1].Value);

                    $.each(data.Items, function (index, detail) {
                        var productName = detail.ProductName.trim();
                        addProperty("#stub-product-name", "", productName);
                        switch (productName) {
                            case "裸钻":
                                addProperty("#certificate", "", detail.Certificate);
                                addProperty("#stub-certificate", "", detail.Certificate);
                                addProperty("#caret", "", detail.Caret);
                                addProperty("#stub-caret", "", detail.Caret);
                                addProperty("#cut", "", detail.Cut);
                                addProperty("#stub-cut", "", detail.Cut);
                                addProperty("#clarity", "", detail.Clarity);
                                addProperty("#stub-clarity", "", detail.Clarity);
                                addProperty("#color", "", detail.Color);
                                addProperty("#stub-color", "", detail.Color);
                                addProperty("#other", "", detail.ProductDescription);
                                break;
                            case "女戒":
                                addProduct("#female-ring", detail);
                                break;
                            case "男戒":
                                addProduct("#male-ring", detail);
                                break;
                            case "对戒":
                                addProduct("#match-ring", detail);
                                break;
                            case "吊坠":
                                addProduct("#pendant", detail);
                                break;
                            case "手链":
                                addProduct("#bracelet", detail);
                                break;
                            case "耳钉":
                                addProduct("#earnail", detail);
                                break;
                            default:
                                addProperty("#other", "", productName);
                                addProduct("#other", detail);
                                break;
                        }
                    });
                });
            }
        },

        "print-receipt": {
            init: function () {
                $home.print.internal.getSaleById($home.utility.queryString.getParameterByName("id"), function (data) {
                    var total = getTotalPrice(data);
                    $("#number").text(data.NumberText);
                    $("#date").text($home.utility.date.toLocalDateString(new Date(data.Created)));
                    $("#payer").text(data.CustomerName);
                    $("#capital").text($home.utility.currency.translateToCapital(total));
                    $("#lowercase").text(total);
                    $("#payee, #worker").text(data.SalesPersonName);
                    $.each(data.Items, function (index, detail) {
                        $("<tr/>").append($("<td/>").text(getProductDescription(detail)))
                            .append($("<td style='text-align: center;'/>").text(detail.Quantity))
                            .append($("<td style='text-align: right;'/>").text(detail.Quantity * detail.UnitPrice))
                            .appendTo("#details tbody");
                    });
                });

                function getTotalPrice(data) {
                    var total = 0;
                    $.each(data.Items, function (index, detail) {
                        total += (detail.UnitPrice * detail.Quantity);
                    });
                    return total;
                }

                function addProperty(name, value) {
                    if (value === undefined || value === null) {
                        return "";
                    }

                    return name + value + " ";
                }

                function getProductDescription(detail) {
                    return addProperty("", detail.ProductName) +
                        addProperty("证书", detail.Certificate) +
                        addProperty("净度", detail.Clarity) +
                        addProperty("颜色", detail.Color) +
                        addProperty("重量", detail.Caret) +
                        addProperty("切工", detail.Cut) +
                        addProperty("", detail.ProductDescription);
                }
            }
        },

        internal: {
            getSaleById: function (id, success) {
                var url = "/salesservice/Sales(" + id + ")?$expand=CustomerContacts,Items";
                odatajs.oData.read(url, function (data) {
                    success(data);
                }, function () {
                    alert("初始化打印内容失败。");
                });
            }
        }
    };
})();