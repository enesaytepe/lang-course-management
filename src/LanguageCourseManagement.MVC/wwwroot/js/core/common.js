(function (window, $) {
    "use strict";

    var namespace = window.LanguageCourseManagement = window.LanguageCourseManagement || {};

    var application = {
        pages: [],

        registerPage: function (page) {
            if (!page || typeof page.init !== "function") {
                throw new TypeError("Page must define an init function.");
            }

            if ($.inArray(page, this.pages) === -1) {
                this.pages.push(page);
            }

            return this;
        },

        start: function () {
            $.each(this.pages, function (_, page) {
                if (typeof page.canInitialize === "function" && !page.canInitialize()) {
                    return;
                }

                page.init();
            });
        }
    };

    var common = {
        escapeHtml: function (value) {
            return $("<div>").text(value == null ? "" : value).html();
        },

        getValue: function (data, property) {
            if (!data) {
                return undefined;
            }

            return data[property] !== undefined
                ? data[property]
                : data[property.charAt(0).toLowerCase() + property.slice(1)];
        },

        toBoolean: function (value) {
            return value === true || value === "true";
        },

        getAntiforgeryToken: function ($form) {
            return $form.find('input[name="__RequestVerificationToken"]').val();
        },

        collectErrorMessages: function (value, all) {
            if (typeof value === "string") {
                all.push(value);
                return all;
            }

            if ($.isArray(value)) {
                $.each(value, function (_, item) {
                    common.collectErrorMessages(item, all);
                });
                return all;
            }

            if (value && typeof value === "object") {
                var nested = value.Errors || value.errors;
                if (nested !== undefined) {
                    common.collectErrorMessages(nested, all);
                } else {
                    $.each(value, function (_, item) {
                        common.collectErrorMessages(item, all);
                    });
                }
            }

            return all;
        },

        getErrorMessages: function (xhr) {
            var response = xhr.responseJSON || {};
            var messages = response.ErrorMessages || response.errorMessages || response.Errors || response.errors || response.Error || response.error;
            return common.collectErrorMessages(messages, []);
        },

        showApiError: function (xhr) {
            var title = "İşlem başarısız";

            if (xhr.status === 401) {
                title = "Oturum gerekli";
            } else if (xhr.status === 403) {
                title = "Yetkiniz yok";
            } else if (xhr.status === 409) {
                title = "Çakışan kayıt";
            } else if (xhr.status === 422 || xhr.status === 400) {
                title = "Doğrulama hatası";
            }

            var messages = common.getErrorMessages(xhr);
            return Swal.fire({
                icon: "error",
                title: title,
                text: messages.length ? messages.join("\n") : "Beklenmeyen bir hata oluştu."
            });
        },

        showSuccess: function (title) {
            return Swal.fire({
                icon: "success",
                title: title,
                timer: 1500,
                showConfirmButton: false
            });
        }
    };

    namespace.Common = common;
    namespace.Pages = namespace.Pages || {};
    namespace.application = namespace.application || application;
})(window, jQuery);
