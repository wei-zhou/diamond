/// <reference path="jquery-1.11.2.js" />
/// <reference path="jquery-ui-1.11.4.js" />
/// <reference path="bootstrap.js" />
/// <reference path="odatajs-4.0.0.js" />
/// <reference path="common.js" />

/* global odatajs, $home */

// *** pages ***

$home.pages = (function () {
    return {
        "dashboard-page": {
            title: $($("#navbar > ul > li > a").get(0)).text(),
            init: function () {
                $("#dashboard-page-print").click(function () {
                    $home.navigation.modal("print-sale");
                });
            }
        },

        "sale-management": {
            title: $($("#navbar > ul > li > a").get(1)).text(),
            init: function () {
                // --- members ---

                var MaxRowCount = 50,
                    fromDatePicker = $("#sale-management-from"),
                    toDatePicker = $("#sale-management-to"),
                    salePersonControl = $("#sale-management-sale-person"),
                    searchButton = $("#sale-management-search"),
                    searchResult = $("#sale-management-result"),
                    previousButton = $("#sale-management-prev"),
                    nextButton = $("#sale-management-next"),
                    reportButton = $("#sale-management-report"),
                    searchUrl = null,
                    skipCount = 0;

                // --- initialization ---

                odatajs.oData.read("/securityservice/Users?$orderby=DisplayName desc", function (data) {
                    $.each(data.value, function (index, user) {
                        $("<option/>").val(user.DisplayName).text(user.DisplayName).appendTo(salePersonControl);
                    });
                }, function () {
                    alert("初始化" + $home.pages["sale-management"].title + "失败。");
                });

                previousButton.parent().addClass("disabled");
                nextButton.parent().addClass("disabled");

                fromDatePicker.datepicker({ maxDate: "+0d" });
                toDatePicker.datepicker({ maxDate: "+0d" });

                searchButton.click(function () {
                    search(true, 0);
                });

                previousButton.click(function () {
                    if (!$(this).parent().hasClass("disabled")) {
                        var skip = skipCount - MaxRowCount;
                        if (skip < 0) {
                            skip = 0;
                        }
                        search(false, skip);
                    }
                });

                nextButton.click(function () {
                    if (!$(this).parent().hasClass("disabled")) {
                        search(false, skipCount + MaxRowCount);
                    }
                });

                reportButton.click(function () {
                    showReport();
                });

                // --- functions ---

                function calculateAmount(sale) {
                    var result = 0;
                    $.each(sale.Items, function (index, detail) {
                        result += detail.UnitPrice * detail.Quantity;
                    });
                    return result;
                }

                function formatContact(sale) {
                    var result = "";
                    $.each(sale.CustomerContacts, function (index, contact) {
                        var method = "";
                        $.each($home.pages.internal.contactMethods, function (index, item) {
                            if (item.value === contact.Method) {
                                method = item.text;
                            }
                        });
                        if (result.length > 0) {
                            result += "，";
                        }
                        result += method;
                        result += "：";
                        result += contact.Value;
                    });
                    result += "。";
                    return result;
                }

                function search(rebuildUrl, skip) {
                    if (rebuildUrl) {
                        var query = "Status eq Home.Services.SaleService.SaleStatus'Ok'",
                            from = fromDatePicker.datepicker("getDate"),
                            to = toDatePicker.datepicker("getDate"),
                            person = salePersonControl.val();

                        if (from) {
                            from = $home.utility.odata.toODataDateTimeOffset(from);
                            from = odatajs.oData.utils.formatDateTimeOffset(from);
                            query += (" and Created ge " + from);
                        }

                        if (to) {
                            to.setDate(to.getDate() + 1);
                            to = $home.utility.odata.toODataDateTimeOffset(to);
                            to = odatajs.oData.utils.formatDateTimeOffset(to);
                            query += (" and Created lt " + to);
                        }

                        if (person.length > 0) {
                            person = "'" + person + "'";
                            query += (" and SalesPersonName eq " + person);
                        }

                        searchUrl = "/salesservice/Sales?$expand=" +
                            encodeURIComponent("CustomerContacts,Items(") +
                            "$filter=" +
                            encodeURIComponent("Status eq Home.Services.SaleService.SaleStatus'Ok')") +
                            "&$filter=" +
                            encodeURIComponent(query) +
                            "&$orderby=" +
                            encodeURIComponent("Created,SalesPersonName");
                    }

                    var url = searchUrl + "&$skip=" + skip + "&$top=" + MaxRowCount;
                    odatajs.oData.read(url, function (data) {
                        $("tbody", searchResult).empty();

                        $.each(data.value, function (index, sale) {
                            $("<tr/>")
                                .append($("<td/>").append($("<a href='javascript:void(0)' role='button'/>").text(sale.NumberText)))
                                .append($("<td/>").text(sale.CustomerName))
                                .append($("<td/>").text(formatContact(sale)))
                                .append($("<td/>").text(calculateAmount(sale)))
                                .append($("<td/>").text(sale.SalesPersonName))
                                .append($("<td/>").text($home.utility.date.toLocalDateTimeString(new Date(sale.Created))))
                                .append($("<td/>").text($home.utility.date.toLocalDateTimeString(new Date(sale.Modified))))
                                .appendTo($("tbody", searchResult))
                                .data(sale);
                        });

                        $("tbody > tr > td > a", searchResult).click(function () {
                            var sale = $(this).parent().parent().data();
                            showSale(sale);
                        });

                        skipCount = skip;

                        if (skipCount === 0) {
                            previousButton.parent().addClass("disabled");
                        } else {
                            previousButton.parent().removeClass("disabled");
                        }

                        if (data.value.length < MaxRowCount) {
                            nextButton.parent().addClass("disabled");
                        } else {
                            nextButton.parent().removeClass("disabled");
                        }
                    }, function () {
                        alert("查询失败。");
                    });
                }

                function showSale(sale) {
                    $home.navigation.modal("sale-view", function () {
                        search(false, skipCount);
                    }, sale);
                }

                function showReport() {
                    $home.navigation.modal("sale-report");
                }
            }
        },

        // "customer-management": {
        //     title: $($("#navbar > ul > li > a").get(2)).text(),
        //     init: function () {
        //     }
        // },

        "product-management": {
            title: $($("#navbar > ul > li > a").get(2)).text(),
            init: function () {
                $("#product-management-diamond-on-sale").click(function () {
                    $home.navigation.navigate("diamond-on-sale");
                });

                $("#product-management-diamond-import").click(function () {
                    $home.navigation.modal("diamond-import");
                });

                $("#product-management-diamond-history").click(function () {
                    $home.navigation.navigate("diamond-history");
                });
            }
        },

        // "control-panel": {
        //     title: $($("#navbar > ul > li > a").get(4)).text(),
        //     init: function () {
        //     }
        // },

        // "help-page": {
        //     title: $($("#navbar > ul > li > a").get(5)).text(),
        //     init: function () {
        //     }
        // },

        "sale-view": {
            title: "销售记录",
            init: function (sale) {
                var CONTROL_TEMPLATE =
                    "<div class='input-group'>" +
                    "    <div class='input-group-btn'>" +
                    "        <button type='button' class='btn btn-default btn-sm dropdown-toggle' data-toggle='dropdown' aria-expanded='false'><span class='caret'></span></button>" +
                    "        <ul class='dropdown-menu' role='menu'></ul>" +
                    "        <input type='hidden'/>" +
                    "    </div>" +
                    "    <input type='text' class='form-control input-sm' />" +
                    "</div>",
                    CONTACT_TEMPLATE =
                    "<div class='col-md-3'>" +
                    "    <div class='form-group'>" +
                    "    </div>" +
                    "</div>",
                    LINE_ROW_TEMPLATE =
                    "<div class='row'>" +
                    "</div>",
                    LINE_FIELD_MIDDLE_TEMPLATE =
                    "<div class='col-md-2'>" +
                    "    <div class='form-group'>" +
                    "    </div>" +
                    "</div>",
                    LINE_FIELD_LARGE_TEMPLATE =
                    "<div class='col-md-6'>" +
                    "    <div class='form-group'>" +
                    "    </div>" +
                    "</div>",
                    receiptNumberField = $("#sale-view-receipt-number"),
                    createdField = $("#sale-view-created"),
                    createdByField = $("#sale-view-created-by"),
                    modifiedField = $("#sale-view-modified"),
                    modifiedByField = $("#sale-view-modified-by"),
                    salerField = $("#sale-view-saler"),
                    customerInfo = $("#sale-view-customer-info"),
                    customerField = $("#sale-view-name"),
                    lineInfo = $("#sale-view-line-info"),
                    totalPriceField = $("#sale-view-total-price"),
                    editButton = $("#sale-view-edit"),
                    saveButton = $("#sale-view-save"),
                    deleteButton = $("#sale-view-delete");

                odatajs.oData.read("/securityservice/Users?$orderby=DisplayName", function (data) {
                    // --- set controls ---

                    $.each(data.value, function (index, user) {
                        $("<option/>").val(user.DisplayName).text(user.DisplayName).appendTo(salerField);
                    });

                    // --- init controls ---

                    receiptNumberField.val(sale.NumberText);
                    createdField.val($home.utility.date.toLocalDateString(new Date(sale.Created)));
                    createdByField.val(sale.CreatedBy);
                    modifiedField.val($home.utility.date.toLocalDateString(new Date(sale.Modified)));
                    modifiedByField.val(sale.ModifiedBy);
                    salerField.val(sale.SalesPersonName);
                    customerField.val(sale.CustomerName);

                    $.each(sale.CustomerContacts, function (index, contact) {
                        var id = "sale-view-customer-contact-" + (index + 1);
                        var method = $(CONTROL_TEMPLATE);
                        $.each($home.pages.internal.contactMethods, function (index, m) {
                            var content = $("<li><a href='javascript:void(0)'></a></li>");
                            $("a", content).attr("data-content", m.value).text(m.text);
                            $("ul", method).append(content);
                        });
                        $("input[type=text]", method).attr("id", id);
                        $home.ui.selectbutton(method);

                        $("a", method).filter(function () {
                            return $(this).attr("data-content") === contact.Method;
                        }).click();

                        $("input[type=text]", method).val(contact.Value);

                        var template = $(CONTACT_TEMPLATE);
                        $(".form-group", template).append($("<label/>").text("联系方式 " + (index + 1)).attr("for", id))
                                                  .append(method);
                        $(".row", customerInfo).append(template);
                    });

                    $.each(sale.Items, function (index, line) {
                        var nameId = "sale-view-product-name-" + (index + 1),
                            certificateId = "sale-view-product-certificate-" + (index + 1),
                            caretId = "sale-view-product-caret-" + (index + 1),
                            cutId = "sale-view-product-cut-" + (index + 1),
                            clarityId = "sale-view-product-clarity-" + (index + 1),
                            colorId = "sale-view-product-color-" + (index + 1),
                            descriptionId = "sale-view-product-description-" + (index + 1),
                            quantityId = "sale-view-product-quantity-" + (index + 1),
                            unitPriceId = "sale-view-product-unit-price-" + (index + 1),
                            nameField = $(CONTROL_TEMPLATE),
                            certificateField = $("<input type='text' class='form-control input-sm'/>").attr("id", certificateId),
                            caretField = $("<input type='number' class='form-control input-sm'/>").attr("id", caretId),
                            cutField = $("<select class='form-control input-sm'/>").attr("id", cutId),
                            clarityField = $("<select class='form-control input-sm'/>").attr("id", clarityId),
                            colorField = $("<select class='form-control input-sm'/>").attr("id", colorId),
                            descriptionField = $("<input type='text' class='form-control input-sm'/>").attr("id", descriptionId),
                            quantityField = $("<input type='number' class='form-control input-sm quantity'/>").attr("id", quantityId),
                            unitPriceField = $("<input type='number' class='form-control input-sm price'/>").attr("id", unitPriceId);
                        $("input[type=text]", nameField).attr("id", nameId);
                        //// product name
                        $.each($home.pages.internal.products, function (index, item) {
                            var anchor = $("<a href='javascript:void(0)'></a>").text(item);
                            var listItem = $("<li/>").append(anchor);
                            $("ul", nameField).append(listItem);
                        });
                        //// diamond cut
                        cutField.append("<option value=''/>");
                        $.each($home.pages.internal.diamondCuts, function (index, item) {
                            cutField.append($("<option/>").val(item).text(item));
                        });
                        //// diamond clarity
                        clarityField.append("<option value=''/>");
                        $.each($home.pages.internal.diamondClarities, function (index, item) {
                            clarityField.append($("<option/>").val(item).text(item));
                        });
                        //// diamond color
                        colorField.append("<option value=''/>");
                        $.each($home.pages.internal.diamondColors, function (index, item) {
                            colorField.append($("<option/>").val(item).text(item));
                        });

                        $("input", nameField).val(line.ProductName);
                        if (line.Certificate) certificateField.val(line.Certificate);
                        if (line.Caret) caretField.val(line.Caret);
                        if (line.Cut) cutField.val(line.Cut);
                        if (line.Clarity) clarityField.val(line.Clarity);
                        if (line.Color) colorField.val(line.Color);
                        if (line.ProductDescription) descriptionField.val(line.ProductDescription);
                        quantityField.val(line.Quantity);
                        unitPriceField.val(line.UnitPrice);

                        var row = $(LINE_ROW_TEMPLATE);
                        //// name
                        var field = $(LINE_FIELD_MIDDLE_TEMPLATE);
                        $(".form-group", field).append($("<label/>").attr("for", nameId).text("名称")).append(nameField);
                        row.append(field);
                        //// certificate
                        field = $(LINE_FIELD_MIDDLE_TEMPLATE);
                        $(".form-group", field).append($("<label/>").attr("for", certificateId).text("证书")).append(certificateField);
                        row.append(field);
                        //// caret
                        field = $(LINE_FIELD_MIDDLE_TEMPLATE);
                        $(".form-group", field).append($("<label/>").attr("for", caretId).text("钻重")).append(caretField);
                        row.append(field);
                        //// cut
                        field = $(LINE_FIELD_MIDDLE_TEMPLATE);
                        $(".form-group", field).append($("<label/>").attr("for", cutId).text("切工")).append(cutField);
                        row.append(field);
                        //// clarity
                        field = $(LINE_FIELD_MIDDLE_TEMPLATE);
                        $(".form-group", field).append($("<label/>").attr("for", clarityId).text("净度")).append(clarityField);
                        row.append(field);
                        //// color
                        field = $(LINE_FIELD_MIDDLE_TEMPLATE);
                        $(".form-group", field).append($("<label/>").attr("for", colorId).text("颜色")).append(colorField);
                        row.append(field);
                        //// add first row
                        lineInfo.append(row);
                        //// second row
                        row = $(LINE_ROW_TEMPLATE);
                        //// description
                        field = $(LINE_FIELD_LARGE_TEMPLATE);
                        $(".form-group", field).append($("<label/>").attr("for", descriptionId).text("描述")).append(descriptionField);
                        row.append(field);
                        //// quantity
                        field = $(LINE_FIELD_MIDDLE_TEMPLATE);
                        $(".form-group", field).append($("<label/>").attr("for", quantityId).text("数量")).append(quantityField);
                        row.append(field);
                        //// unit price
                        field = $(LINE_FIELD_MIDDLE_TEMPLATE);
                        $(".form-group", field).append($("<label/>").attr("for", unitPriceId).text("价格")).append(unitPriceField);
                        row.append(field);
                        //// add second row
                        lineInfo.append(row);

                        $home.ui.editableselect(nameField);
                    });

                    calculateTotalPrice();

                    setUIEditable(false);

                    editButton.click(function () {
                        setUIEditable(true);
                    });
                    saveButton.click(save);
                    deleteButton.click(remove);

                    lineInfo.off("focusout", "input.quantity, input.price").on("focusout", "input.quantity, input.price", calculateTotalPrice);
                }, function () {
                    alert("初始化销售记录对话框失败。");
                });

                function setUIEditable(editable) {
                    if (editable) {
                        salerField.removeProp("disabled");
                        customerField.removeProp("readonly");
                        $("input", customerInfo).removeProp("readonly");
                        $("select, button", customerInfo).removeProp("disabled");
                        $("input", lineInfo).not("#sale-view-total-price").removeProp("readonly");
                        $("select, button", lineInfo).removeProp("disabled");
                        editButton.prop("disabled", true);
                        saveButton.removeProp("disabled");
                        deleteButton.removeProp("disabled");
                    } else {
                        salerField.prop("disabled", true);
                        customerField.prop("readonly", true);
                        $("input", customerInfo).prop("readonly", true);
                        $("select, button", customerInfo).prop("disabled", true);
                        $("input", lineInfo).prop("readonly", true);
                        $("select, button", lineInfo).prop("disabled", true);
                        editButton.removeProp("disabled");
                        saveButton.prop("disabled", true);
                        deleteButton.prop("disabled", true);
                    }
                }

                function calculateTotalPrice() {
                    var totalPrice = 0;
                    $("> div", lineInfo).each(function () {
                        var quantityField = $("input.quantity", $(this));
                        var unitPriceField = $("input.price", $(this));
                        if (quantityField.size() === 1 && unitPriceField.size() === 1) {
                            var quantity = parseInt(quantityField.val());
                            var unitPrice = parseInt(unitPriceField.val());
                            totalPrice += quantity * unitPrice;
                        }
                    });
                    totalPriceField.val(totalPrice);
                }

                function save() {
                    sale.SalesPersonName = salerField.val();
                    sale.CustomerName = customerField.val();
                    sale.CustomerContacts[0].Method = $("input[type=hidden]", $("#sale-view-customer-contact-1").parent()).val();
                    sale.CustomerContacts[0].Value = $("#sale-view-customer-contact-1").val();
                    sale.CustomerContacts[1].Method = $("input[type=hidden]", $("#sale-view-customer-contact-2").parent()).val();
                    sale.CustomerContacts[1].Value = $("#sale-view-customer-contact-2").val();
                    var line,
                        lineIndex = 0;
                    $("> div", lineInfo).not(":first").each(function (index) {
                        var row = $(this);
                        if (index % 2 === 0) {
                            line = sale.Items[lineIndex];
                            lineIndex++;
                            line.ProductName = $($("input[type=text]", row)[0]).val();

                            if ($($("input[type=text]", row)[1]).val().length > 0) {
                                line.Certificate = $($("input[type=text]", row)[1]).val();
                            } else {
                                line.Certificate = undefined;
                            }

                            if ($($("input[type=number]", row)[0]).val().length > 0) {
                                line.Caret = parseFloat($($("input[type=number]", row)[0]).val());
                            } else {
                                line.Caret = undefined;
                            }

                            if ($($("select", row)[0]).val().length > 0) {
                                line.Cut = $($("select", row)[0]).val();
                            } else {
                                line.Cut = undefined;
                            }

                            if ($($("select", row)[1]).val().length > 0) {
                                line.Clarity = $($("select", row)[1]).val();
                            } else {
                                line.Clarity = undefined;
                            }

                            if ($($("select", row)[2]).val().length > 0) {
                                line.Color = $($("select", row)[2]).val();
                            } else {
                                line.Color = undefined;
                            }
                        } else {
                            line.ProductDescription = $($("input[type=text]", row)[0]).val();
                            line.Quantity = parseInt($($("input[type=number]", row)[0]).val());
                            line.UnitPrice = parseInt($($("input[type=number]", row)[1]).val());
                        }
                    });

                    sale["CustomerContacts@odata.context"] = undefined;
                    sale["Items@odata.context"] = undefined;
                    odatajs.oData.request({
                        requestUri: "/salesservice/Sales(" + sale.Id + ")",
                        method: "PUT",
                        data: sale,
                        headers: {
                            "if-match": sale["@odata.etag"]
                        }
                    }, function () {
                        alert("保存成功。");
                        $("#sale-view").modal("hide");
                    }, function () {
                        alert("保存失败。");
                    });
                }

                function remove() {
                    if (confirm("确定要删除吗？")) {
                        sale["CustomerContacts@odata.context"] = undefined;
                        sale["Items@odata.context"] = undefined;
                        sale.CustomerContacts = undefined;
                        sale.Items = undefined;
                        sale.Status = "Bad";
                        odatajs.oData.request({
                            requestUri: "/salesservice/Sales(" + sale.Id + ")",
                            method: "PATCH",
                            data: sale,
                            headers: {
                                "if-match": sale["@odata.etag"]
                            }
                        }, function () {
                            alert("删除成功。");
                            $("#sale-view").modal("hide");
                        }, function () {
                            alert("删除失败。");
                        });
                    }
                }
            }
        },

        "sale-report": {
            title: "销售报表",
            init: function () {
                var fromDatePicker = $("#sale-report-from"),
                    toDatePicker = $("#sale-report-to"),
                    viewButton = $("#sale-report-view"),
                    cancelButton = $("#sale-report-cancel"),
                    ssis = new $home.ssis({
                        name: "sales",
                        prefix: "sale-report",
                        changed: changedCallback
                    });

                // TODO: for long running operation
                // fromDatePicker.prop("disabled", true).datepicker({ maxDate: "+0d" });
                // toDatePicker.prop("disabled", true).datepicker({ maxDate: "+0d" });
                // viewButton.prop("disabled", true);
                fromDatePicker.datepicker({ maxDate: "+0d" });
                toDatePicker.datepicker({ maxDate: "+0d" });
                cancelButton.prop("disabled", true);

                viewButton.click(function () {
                    var from = fromDatePicker.datepicker("getDate"),
                        to = toDatePicker.datepicker("getDate");
                    to.setDate(to.getDate() + 1);
                    ssis.start([
                        { Name: "StartDate", Type: "DateTime", Value: $home.utility.date.toServerDateString(from) },
                        { Name: "EndDate", Type: "DateTime", Value: $home.utility.date.toServerDateString(to) },
                    ]);
                });

                cancelButton.click(function () {
                    ssis.stop();
                });

                function changedCallback(value) {
                    switch (value.Status) {
                        case "Completed":
                            fromDatePicker.removeAttr("disabled");
                            toDatePicker.removeAttr("disabled");
                            viewButton.removeAttr("disabled");
                            cancelButton.prop("disabled", true);
                            break;
                        case "Running":
                            fromDatePicker.prop("disabled", true);
                            toDatePicker.prop("disabled", true);
                            viewButton.prop("disabled", true);
                            cancelButton.removeAttr("disabled");
                            break;
                    }
                }
            }
        },

        "diamond-on-sale": {
            title: "在售裸钻",
            init: function () {

            }
        },

        "diamond-import": {
            title: "裸钻导入",
            init: function () {

            }
        },

        "diamond-history": {
            title: "裸钻上传历史",
            init: function () {

            }
        },

        "print-sale": {
            title: "打印售后服务卡和收据",
            init: function () {
                // members
                var sale = {},

                    dialog = $("#print-sale"),

                    userInfo = $("#print-sale-user-info"),
                    customerInfo = $("#print-sale-customer-info"),
                    goodInfo = $("#print-sale-good-info"),

                    customerWayButton = $("#print-sale-customer-way-button"),
                    goodNameButton = $("#print-sale-good-name-button"),
                    addGoodButton = $("#print-sale-add-good"),
                    saveButton = $("#print-sale-save"),
                    printServiceButton = $("#print-sale-print-service"),
                    printReceiptButton = $("#print-sale-print-receipt"),

                    userName = $("#print-sale-user"),
                    receiptNumber = $("#print-sale-receipt-number"),

                    customerName = $("#print-sale-customer-name"),
                    customerPhone = $("#print-sale-customer-phone"),
                    customerContact = $("#print-sale-customer-contact"),
                    customerWay = $("#print-sale-customer-way"),

                    goodName = $("#print-sale-good-name"),
                    goodCertificate = $("#print-sale-good-certificate"),
                    goodCaret = $("#print-sale-good-caret"),
                    goodCut = $("#print-sale-good-cut"),
                    goodClarity = $("#print-sale-good-clarity"),
                    goodColor = $("#print-sale-good-color"),
                    goodDescription = $("#print-sale-good-description"),
                    goodQuantity = $("#print-sale-good-quantity"),
                    goodPrice = $("#print-sale-good-price");

                // init dialog
                odatajs.oData.read("/securityservice/Users?$orderby=DisplayName", function (data) {
                    // user
                    $.each(data.value, function (index, user) {
                        $("<option/>").val(user.Id).text(user.DisplayName).appendTo(userName);
                    });

                    // customer way
                    $.each($home.pages.internal.contactMethods, function (index, method) {
                        var content = $("<li><a href='javascript:void(0)'></a></li>");
                        $("a", content).attr("data-content", method.value).text(method.text);
                        customerWayButton.next().append(content);
                    });
                    $home.ui.selectbutton(customerWayButton.parent());
                    $("ul > li > a:first", customerWayButton.parent()).click();

                    // good name
                    $.each($home.pages.internal.products, function (index, good) {
                        var anchor = $("<a href='javascript:void(0)'></a>").text(good);
                        var listItem = $("<li/>").append(anchor);
                        $(goodNameButton.next()).append(listItem);
                    });
                    $home.ui.editableselect(goodName.parent());

                    // diamond cut
                    $.each($home.pages.internal.diamondCuts, function (index, item) {
                        goodCut.append($("<option/>").val(item).text(item));
                    });

                    // diamond clarity
                    $.each($home.pages.internal.diamondClarities, function (index, item) {
                        goodClarity.append($("<option/>").val(item).text(item));
                    });

                    // diamond color
                    $.each($home.pages.internal.diamondColors, function (index, item) {
                        goodColor.append($("<option/>").val(item).text(item));
                    });

                    // dialog
                    dialog.on("shown.bs.modal", function () {
                        userName.val($home.authentication.getUserInfo().id);
                        customerName.focus();
                    });

                    // enable UI
                    setEnabled(true);

                    // add good
                    addGoodButton.click(add);

                    // save
                    saveButton.click(save);
                }, function () {
                    alert("初始化打印售后服务卡和收据对话框失败。");
                });

                // disable enable
                function setEnabled(enabled) {
                    $("fieldset input, fieldset textarea", dialog).not(receiptNumber).prop("readonly", !enabled);
                    $("fieldset button, fieldset select", dialog).prop("disabled", !enabled);
                    if (enabled) {
                        saveButton.removeProp("disabled");
                        printServiceButton.prop("disabled", true);
                        printReceiptButton.prop("disabled", true);
                        $("table > tbody > tr > td > a", dialog).removeClass("disabled");
                    } else {
                        saveButton.prop("disabled", true);
                        printServiceButton.removeProp("disabled");
                        printReceiptButton.removeProp("disabled");
                        $("table > tbody > tr > td > a", dialog).addClass("disabled", true);
                    }
                }

                // cleanup
                function cleanupUser() {
                    $home.validation.cleanup(userInfo);
                }
                function cleanupCustomer() {
                    $home.validation.cleanup(customerInfo);
                }
                function cleanupGood() {
                    $home.validation.cleanup(goodInfo);
                }
                function clearGood() {
                    goodName.val("");
                    goodCertificate.val("");
                    goodCaret.val("");
                    goodCut.val("");
                    goodClarity.val("");
                    goodColor.val("");
                    goodDescription.val("");
                    goodQuantity.val("");
                    goodPrice.val("");
                    cleanupGood();
                }

                // calculate total price
                function calculateTotalPrice() {
                    var totalPrice = 0;
                    $.each($("table > tbody > tr", dialog), function (index, row) {
                        var quantity = parseInt($("td.quantity", row).text());
                        var price = parseInt($("td.price", row).text());
                        totalPrice += (quantity * price);
                    });
                    $("table > tfoot > tr > th.price", dialog).text(totalPrice.toString());
                }

                // edit good
                function editGood() {
                    if (!$(this).hasClass("disabled")) {
                        var row = $(this).parent().parent();
                        var detail = row.data();
                        goodName.val(detail.ProductName);
                        goodCertificate.val(detail.Certificate);
                        goodCaret.val(detail.Caret);
                        goodCut.val(detail.Cut);
                        goodClarity.val(detail.Clarity);
                        goodColor.val(detail.Color);
                        goodDescription.val(detail.ProductDescription);
                        goodQuantity.val(detail.Quantity);
                        goodPrice.val(detail.UnitPrice);
                        row.remove();
                        calculateTotalPrice();
                        goodNameButton.focus();
                    }
                }

                // remove good
                function removeGood() {
                    if (!$(this).hasClass("disabled")) {
                        var row = $(this).parent().parent();
                        row.remove();
                        calculateTotalPrice();
                        goodNameButton.focus();
                    }
                }

                // save
                function save() {
                    cleanupUser();
                    cleanupCustomer();

                    if (!($home.validation.validateRequired(userName))) {
                        alert("请检查用户信息，有未填写的内容，请填写完整。");
                        return;
                    }

                    if (!($home.validation.validateRequired(customerName) &&
                          $home.validation.validateRequired(customerPhone) &&
                          $home.validation.validateRequired(customerContact))) {
                        alert("请检查客户信息，有未填写的内容，请填写完整。");
                        return;
                    }

                    sale.Items = [];
                    $.each($("table > tbody > tr", dialog), function (index, row) {
                        sale.Items.push($(row).data());
                    });
                    if (sale.Items.length === 0) {
                        alert("没有添加任何商品，请添加商品之后继续。");
                        return;
                    }

                    setEnabled(false);

                    sale.SalesPersonName = $("#print-sale-user-info option:selected").text();
                    sale.CustomerName = customerName.val();
                    sale.CustomerContacts = [
                        { Method: "Phone", Value: customerPhone.val() },
                        { Method: customerWay.val(), Value: customerContact.val() }
                    ];

                    odatajs.oData.request({
                        requestUri: "/salesservice/Sales",
                        method: "POST",
                        data: sale
                    }, function (data) {
                        receiptNumber.val(data.NumberText);
                        printServiceButton.attr("href", "/pages/print-service?id=" + data.Id);
                        printReceiptButton.attr("href", "/pages/print-receipt?id=" + data.Id);
                        setEnabled(false);
                        alert("保存成功，可以开始打印。");
                    }, function () {
                        alert("保存失败。");
                        setEnabled(true);
                    });
                }

                // add good
                function add() {
                    function formatDescription() {
                        var result = "";

                        if (goodCertificate.val().length > 0) {
                            result += "证书：" + goodCertificate.val() + " ";
                        }
                        if (goodCaret.val().length > 0) {
                            result += "钻重：" + goodCaret.val() + " ";
                        }
                        if (goodCut.val().length > 0) {
                            result += "切工：" + goodCut.val() + " ";
                        }
                        if (goodClarity.val().length > 0) {
                            result += "净度：" + goodClarity.val() + " ";
                        }
                        if (goodColor.val().length > 0) {
                            result += "颜色：" + goodColor.val() + " ";
                        }
                        if (goodDescription.val().length > 0) {
                            result += goodDescription.val();
                        }

                        return result;
                    }

                    cleanupGood();

                    if (!($home.validation.validateRequired(goodName) &&
                          $home.validation.validateFloat(goodCaret) &&
                          $home.validation.validateRequired(goodQuantity) &&
                          $home.validation.validateInteger(goodQuantity) &&
                          $home.validation.validateRequired(goodPrice) &&
                          $home.validation.validateInteger(goodPrice))) {
                        alert("请检查商品信息，有未填写的内容，请填写完整。");
                        return;
                    }

                    var quantity = parseInt(goodQuantity.val()),
                        unitPrice = parseInt(goodPrice.val()),
                        detail = {
                            ProductName: goodName.val(),
                            ProductDescription: goodDescription.val(),
                            Quantity: quantity,
                            UnitPrice: unitPrice
                        };
                    if (goodCertificate.val().length > 0) {
                        detail.Certificate = goodCertificate.val();
                    }
                    if (goodCaret.val().length > 0) {
                        detail.Caret = parseFloat(goodCaret.val());
                    }
                    if (goodCut.val().length > 0) {
                        detail.Cut = goodCut.val();
                    }
                    if (goodClarity.val().length > 0) {
                        detail.Clarity = goodClarity.val();
                    }
                    if (goodColor.val().length > 0) {
                        detail.Color = goodColor.val();
                    }

                    var row = $("<tr></tr>")
                        .append($("<td></td>").text(goodName.val()))
                        .append($("<td></td>").text(formatDescription()))
                        .append($("<td class='quantity'></td>").text(quantity.toString()))
                        .append($("<td class='price'></td>").text(unitPrice.toString()))
                        .append("<td class='edit'>" +
                                "<a href='javascript:void(0)' title='修改' role='button'><span class='glyphicon glyphicon-pencil' aria-hidden='true'></span></a>" +
                                "<a href='javascript:void(0)' title='删除' role='button'><span class='glyphicon glyphicon-remove' aria-hidden='true'></span></a>" +
                                "</td>")
                        .data(detail)
                        .appendTo("table > tbody", dialog);
                    $("a:first", row).click(editGood);
                    $("a:last", row).click(removeGood);
                    calculateTotalPrice();

                    clearGood();
                    goodNameButton.focus();
                }
            }
        },

        internal: {
            products: ["裸钻", "女戒", "男戒", "对戒", "吊坠", "项链", "手链", "耳钉", "翡翠", "碧玺", "男戒托", "女戒托"],
            contactMethods: [{ value: "WeiXin", text: "微信" }, { value: "QQ", text: "QQ" }, { value: "Email", text: "电子邮件" }, { value: "Phone", text: "电话" }, { value: "Other", text: "其它" }],
            diamondCuts: ["EX", "VG", "G", "F", "P"],
            diamondClarities: ["FL", "IF", "VVS1", "VVS2", "VS1", "VS2", "SI1", "SI2", "I1", "I2", "I3"],
            diamondColors: ["D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N"]
        }
    };
})();

// *** navigation ***

$home.navigation = (function () {
    return {
        navigate: function (id) {
            var setting = $home.pages[id];
            var args = $home.navigation.internal.createArguments(arguments, 1);
            $home.navigation.internal.invoke(id).done(function (html) {
                $("h1").text(setting.title);
                $("#content").html(html);
                setting.init.apply(null, args);
            }).fail(function (xhr) {
                if (xhr.status === 401) {
                    alert("您没有权限导航到" + setting.title + "。");
                } else {
                    alert("导航到" + setting.title + "失败。");
                }
            });
        },

        modal: function (id, handler) {
            var setting = $home.pages[id];
            var args = $home.navigation.internal.createArguments(arguments, 2);
            $home.navigation.internal.invoke(id).done(function (html) {
                $("#dialog").html(html);
                setting.init.apply(null, args);
                $("#" + id).on("hidden.bs.modal", function () {
                    $("#dialog").empty();
                    if (handler) {
                        handler();
                    }
                }).modal("show");
            }).fail(function (xhr) {
                if (xhr.status === 401) {
                    alert("您没有权限打开" + setting.title + "对话框。");
                } else {
                    alert("打开" + setting.title + "对话框失败。");
                }
            });
        },

        embed: function (id, container) {
            var setting = $home.pages[id];
            var args = $home.navigation.internal.createArguments(arguments, 2);
            $home.navigation.internal.invoke(id).done(function (html) {
                $(container).html(html);
                setting.init.apply(null, args);
            }).fail(function (xhr) {
                if (xhr.status === 401) {
                    alert("您没有权限查看" + setting.title + "。");
                } else {
                    alert("查看" + setting.title + "失败。");
                }
            });
        },

        internal: {
            invoke: function (id) {
                return $.ajax({
                    url: "/pages/" + id,
                    method: "GET",
                    cache: true
                });
            },

            createArguments: function (args, count) {
                if (args.length > count) {
                    var result = [];
                    $.each(args, function (index, item) {
                        if (index > (count - 1)) {
                            result.push(item);
                        }
                    });
                    return result;
                }
            }
        }
    };
})();

// *** authentication ***

$home.authentication = (function () {
    return {
        timeout: new $home.observable(),

        isTimeout: function () {
            return $home.authentication.internal.isTimeout;
        },

        restart: function () {
            if ($home.authentication.internal.timeoutHandle) {
                clearTimeout($home.authentication.internal.timeoutHandle);
            }
            $home.authentication.internal.timeoutHandle = setTimeout(function () {
                $.ajax({
                    type: "POST",
                    url: "/account/logout",
                    headers: { RequestVerificationToken: $("#login-form input[name=__AjaxRequestVerificationToken]").val() },
                    cache: false
                }).complete(function () {
                    clearTimeout($home.authentication.internal.timeoutHandle);
                    $home.authentication.internal.isTimeout = true;
                    $home.authentication.timeout.fire();
                    $home.authentication.exit();
                });
            }, 60 * 60 * 1000);
        },

        exit: function () {
            $("body").append("<div class='home-timeout'><div>您的登录超时，请<a href='/account/login'>重新登录</a>。</div></div>");
        },

        initialize: function () {
            $home.authentication.restart();
            $home.authentication.internal.initializeAjax();
            $home.authentication.internal.initializeOData();
            $.ajax({
                type: "GET",
                url: "/account/check",
                dataType: "json",
                cache: false
            }).done(function (data) {
                if (data.IsLoggedIn) {
                    $home.authentication.internal.userId = data.Id;
                    $home.authentication.internal.userName = data.Name;
                    $home.authentication.internal.userRole = data.Role;
                    $home.authentication.internal.initializeUI();
                    $("#identity").text(data.Name);
                    $home.navigation.navigate("dashboard-page");
                } else {
                    $home.authentication.login();
                }
            }).fail(function () {
                alert("检查登录状态失败。");
            });
        },

        login: function () {
            document.location.assign("/account/login");
        },

        logout: function () {
            $("#logout-form").get(0).submit();
            $home.authentication.internal.userId = null;
            $home.authentication.internal.userName = null;
            $home.authentication.internal.userRole = null;
        },

        getUserInfo: function () {
            return {
                id: $home.authentication.internal.userId,
                Name: $home.authentication.internal.userName,
                Role: $home.authentication.internal.userRole
            };
        },

        Role: {
            Administrator: "Administrator",
            Manager: "Manager",
            Employee: "Employee"
        },

        internal: {
            isTimeout: false,
            timeoutHandle: null,
            userId: null,
            userName: null,
            userRole: null,
            initializeUI: function () {
                $.ajax({
                    type: "GET",
                    url: "/permissions",
                    dataType: "json"
                }).done(function (data) {
                    data = JSON.parse(data);

                    function check(name, listItem) {
                        var permitted;
                        $.each(data, function (index, record) {
                            if (record.name === name) {
                                $.each(record.roles, function (index, role) {
                                    if (role === $home.authentication.internal.userRole) {
                                        permitted = true;
                                    }
                                });
                            }
                        });
                        if (!permitted) {
                            $(listItem).addClass("disabled").hide();
                        }
                    }

                    $.each($("#navigator > li"), function (index, listItem) {
                        switch (index) {
                            case 0: check("dashboard-page", listItem); break;
                            case 1: check("sale-management", listItem); break;
                                //// TODO: case 2: check("customer-management", listItem); break;
                            case 2: check("product-management", listItem); break;
                                //// TODO: case 4: check("control-panel", listItem); break;
                                //// TODO: case 5: check("help-page", listItem); break;
                        }
                    });
                }).fail(function () {
                    alert("检查权限失败。");
                });
            },

            initializeAjax: function () {
                $(document).ajaxSuccess(function () {
                    if (!$home.authentication.isTimeout()) {
                        $home.authentication.restart();
                    }
                }).ajaxError(function (event, xhr, settings, error) {
                    if (!$home.authentication.isTimeout() && error.toLowerCase() !== "not authorized") {
                        $home.authentication.restart();
                    }
                });
            },

            initializeOData: function () {
                var odataSuccess = odatajs.oData.defaultSuccess || $.noop;
                var odataError = odatajs.oData.defaultError || $.noop;
                odatajs.oData.defaultSuccess = function () {
                    odataSuccess();
                    if (!$home.authentication.isTimeout()) {
                        $home.authentication.restart();
                    }
                };
                odatajs.oData.defaultError = function (error) {
                    odataError(error);
                    if (!$home.authentication.isTimeout() && error.response.statusCode !== 401) {
                        $home.authentication.restart();
                    }
                };
            }
        }
    };
})();

// *** SSIS ***

$home.ssis = (function () {
    function triggerChanged() {
        var uiRunning = this.uiRunning,
            uiCompleted = this.uiCompleted,
            uiCancelled = this.uiCancelled,
            options = this.options,
            timerHandle = this.timerHandle;
        odatajs.oData.read("/ssisservice/GetStatus(name='" + encodeURIComponent(this.options.name) + "')", function (data) {
            switch (data.Status) {
                case "Completed":
                    clearInterval(timerHandle);
                    uiRunning.hide();
                    uiCompleted.show();
                    uiCancelled.hide();
                    break;
                case "Running":
                    uiRunning.hide();
                    uiCompleted.hide();
                    uiCancelled.show();
                    break;
            }
            options.changed.call(ssis, data);
        });
    }

    // It subscribes the following events
    // 1. start
    //    [ { Name: "", Type: "", Value: "" } ]
    // 2. stop
    // It fires the following events
    // 1. changed
    //    { Status: "Running | Completed" }
    var ssis = function () {
        this.options = $.extend({
            name: null,
            prefix: null,
            changed: $.noop
        }, (arguments.length === 0 ? {} : arguments[0]));
        this.uiRunning = $("#" + this.options.prefix + "-ssis-running").hide();
        this.uiCompleted = $("#" + this.options.prefix + "-ssis-completed").hide();
        this.uiCancelled = $("#" + this.options.prefix + "-ssis-cancelled").hide();
        // TODO: for long running operation
        // triggerChanged.call(this);
    };

    ssis.prototype.start = function () {
        var uiRunning = this.uiRunning,
            uiCompleted = this.uiCompleted,
            uiCancelled = this.uiCancelled,
            timerHandle = this.timerHandle,
            createTimer = this.createTimer,
            self = this;
        odatajs.oData.request({
            requestUri: "/ssisservice/Run()",
            method: "POST",
            data: {
                name: this.options.name,
                parameters: arguments[0]
            }
        }, function () {
            uiRunning.show();
            uiCompleted.hide();
            uiCancelled.hide();
            clearInterval(timerHandle);
            createTimer.call(self);
            triggerChanged.call(self);
        }, function () {
            alert("运行报表失败。");
        });
    };

    ssis.prototype.stop = function () {
        var uiRunning = this.uiRunning,
            uiCompleted = this.uiCompleted,
            uiCancelled = this.uiCancelled,
            timerHandle = this.timerHandle;
        odatajs.oData.request({
            requestUri: "/ssisservice/Stop()",
            method: "POST",
            data: {
                name: this.options.name
            }
        }, function () {
            uiRunning.hide();
            uiCompleted.hide();
            uiCancelled.show();
            clearInterval(timerHandle);
        }, function () {
            alert("取消报表运行失败。");
        });
    };

    ssis.prototype.createTimer = function () {
        var self = this;
        this.timerHandle = setInterval(function () {
            triggerChanged.call(self);
        }, 10 * 1000 /* 10 seconds */);
    };

    return ssis;
})();

// *** init ***

$(function () {
    $home.authentication.initialize();

    $("#navigator > li > a").click(function () {
        var item = $(this).parent();
        if (!item.hasClass("disabled")) {
            var index = $("#navigator > li").index(item);
            switch (index) {
                case 0: $home.navigation.navigate("dashboard-page"); break;
                case 1: $home.navigation.navigate("sale-management"); break;
                    //// TODO: case 2: $home.navigation.navigate("customer-management"); break;
                case 2: $home.navigation.navigate("product-management"); break;
                    //// TODO: case 4: $home.navigation.navigate("control-panel"); break;
                    //// TODO: case 5: $home.navigation.navigate("help-page"); break;
            }
        }
    });

    $("#logout").click($home.authentication.logout);
});
