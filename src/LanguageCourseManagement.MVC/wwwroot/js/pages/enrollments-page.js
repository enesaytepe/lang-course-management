(function (window, $) {
    "use strict";

    var app = window.LanguageCourseManagement;

    var enrollmentsPage = {
        $page: null,
        $table: null,
        $antiForgeryForm: null,
        $form: null,
        dataTable: null,
        canEdit: false,

        canInitialize: function () {
            return $("#enrollments-page").length > 0
                || $("[data-enrollment-form]").length > 0;
        },

        init: function () {
            this.$page = $("#enrollments-page");
            this.$table = $("#enrollmentsTable");
            this.$antiForgeryForm = $("#enrollmentAntiforgeryForm");
            this.$form = $("[data-enrollment-form]");
            this.canEdit = app.Common.toBoolean(this.$page.data("can-edit"));

            if (!this.$antiForgeryForm.length) {
                this.$antiForgeryForm = this.$form.first();
            }

            if (this.$table.length) {
                this.initializeTable();
            }

            this.bindEvents();
            this.loadOptions();
        },

        bindEvents: function () {
            var self = this;

            $("[data-enrollment-form='create']")
                .off("submit.enrollmentsPage")
                .on("submit.enrollmentsPage", function (event) {
                    event.preventDefault();
                    self.create($(this));
                });

            $("[data-enrollment-form='update']")
                .off("submit.enrollmentsPage")
                .on("submit.enrollmentsPage", function (event) {
                    event.preventDefault();
                    self.update($(this));
                });

            if (this.$table.length) {
                this.$table
                    .off("click.enrollmentsPage", "[data-enrollment-cancel]")
                    .on("click.enrollmentsPage", "[data-enrollment-cancel]", function () {
                        self.cancel($(this).data("enrollment-cancel"));
                    });
            }
        },

        loadOptions: function () {
            var $student = $("#StudentId");
            var $course = $("#CourseId");

            if (!$student.length || !$course.length) {
                return;
            }

            $.when(
                $.getJSON("/api/students?pageIndex=0&pageSize=100"),
                $.getJSON("/api/courses?pageIndex=0&pageSize=100&isActive=true")
            )
                .done(function (students, courses) {
                    var studentItems = app.Common.getValue(students[0], "Items") || [];
                    var courseItems = app.Common.getValue(courses[0], "Items") || [];

                    $.each(studentItems, function (_, item) {
                        $student.append($("<option>", {
                            value: app.Common.getValue(item, "Id"),
                            text: (app.Common.getValue(item, "FirstName") || "") + " " + (app.Common.getValue(item, "LastName") || "")
                        }));
                    });

                    $.each(courseItems, function (_, item) {
                        $course.append($("<option>", {
                            value: app.Common.getValue(item, "Id"),
                            text: app.Common.getValue(item, "Name") || "-"
                        }));
                    });
                })
                .fail(app.Common.showApiError);
        },

        getPayload: function ($form) {
            var form = $form[0];
            return {
                StudentId: form.StudentId.value,
                CourseId: form.CourseId.value,
                DiscountAmount: Number(form.DiscountAmount.value) || 0,
                IdempotencyKey: form.IdempotencyKey.value,
                PaymentType: form.PaymentType.value
            };
        },

        showFormErrors: function ($form, xhr) {
            var response = xhr.responseJSON || {};
            var messages = [];
            var errors = response.errors || response.Errors;

            $form.find("[data-valmsg-for]").text("");
            $form.find("[data-validation-summary]").empty();

            if (errors && typeof errors === "object") {
                $.each(errors, function (key, value) {
                    var fieldMessages = $.isArray(value) ? value : [value];
                    var normalized = key.replace(/^\$?\./, "").toLowerCase();
                    var $field = $form.find("[data-valmsg-for]").filter(function () {
                        return String($(this).data("valmsg-for")).toLowerCase() === normalized;
                    }).first();
                    if ($field.length) {
                        $field.text(fieldMessages.join(" "));
                    } else {
                        messages = messages.concat(fieldMessages);
                    }
                });
            }

            var detail = response.detail || response.Detail;
            if (detail) {
                messages.push(detail);
            }
            messages = messages.concat(app.Common.getErrorMessages(xhr));
            messages = $.grep(messages, function (message, index) {
                return message && $.inArray(message, messages) === index;
            });
            if (messages.length) {
                var $summary = $form.find("[data-validation-summary]");
                $summary.removeClass("validation-summary-valid").addClass("validation-summary-errors")
                    .append($("<ul>").append($.map(messages, function (message) {
                        return $("<li>").text(message);
                    })));
            }
        },

        create: function ($form) {
            var self = this;
            var $submit = $form.find("[type='submit']");

            if ($submit.prop("disabled")) {
                return;
            }
            if ($form.valid && !$form.valid()) {
                return;
            }

            $submit.prop("disabled", true);
            $.ajax({
                url: "/api/enrollments",
                method: "POST",
                contentType: "application/json; charset=utf-8",
                headers: {
                    "X-XSRF-TOKEN": app.Common.getAntiforgeryToken($form)
                },
                data: JSON.stringify(this.getPayload($form))
            })
                .done(function (result) {
                    app.Common.showSuccess("Kayıt başarıyla oluşturuldu");
                    window.location.assign("/Enrollment/Details/" + encodeURIComponent(app.Common.getValue(result, "Id")));
                })
                .fail(function (xhr) {
                    self.showFormErrors($form, xhr);
                })
                .always(function () {
                    $submit.prop("disabled", false);
                    self.loadOptions();
                });
        },

        update: function ($form) {
            var self = this;
            var id = $form.data("enrollment-id");
            var $submit = $form.find("[type='submit']");

            if ($submit.prop("disabled")) {
                return;
            }

            $submit.prop("disabled", true);
            $.ajax({
                url: "/api/enrollments/" + encodeURIComponent(id),
                method: "PUT",
                contentType: "application/json; charset=utf-8",
                headers: {
                    "X-XSRF-TOKEN": app.Common.getAntiforgeryToken($form)
                },
                data: JSON.stringify({ Status: $form.find("[name='Status']").val() })
            })
                .done(function () {
                    app.Common.showSuccess("Kayıt durumu güncellendi");
                    window.location.assign("/Enrollment/Details/" + encodeURIComponent(id));
                })
                .fail(function (xhr) {
                    self.showFormErrors($form, xhr);
                })
                .always(function () {
                    $submit.prop("disabled", false);
                });
        },

        cancel: function (id) {
            var self = this;

            Swal.fire({
                icon: "warning",
                title: "Kayıt iptal edilsin mi?",
                text: "Aktif kayıt iptal edilecek.",
                showCancelButton: true,
                confirmButtonText: "İptal et",
                cancelButtonText: "Vazgeç"
            }).then(function (result) {
                if (!result.isConfirmed) {
                    return;
                }

                $.ajax({
                    url: "/api/enrollments/" + encodeURIComponent(id),
                    method: "DELETE",
                    headers: {
                        "X-XSRF-TOKEN": app.Common.getAntiforgeryToken(self.$antiForgeryForm)
                    }
                })
                    .done(function () {
                        app.Common.showSuccess("Kayıt iptal edildi");
                        self.dataTable.ajax.reload(null, false);
                    })
                    .fail(app.Common.showApiError);
            });
        },

        getColumns: function () {
            var columns = [
                {
                    data: "studentName",
                    defaultContent: "-",
                    render: function (data) {
                        return app.Common.escapeHtml(data || "-");
                    }
                },
                {
                    data: "courseName",
                    defaultContent: "-",
                    render: function (data) {
                        return app.Common.escapeHtml(data || "-");
                    }
                },
                {
                    data: "finalAmount",
                    render: function (data) {
                        return (Number(data) || 0).toLocaleString("tr-TR", { minimumFractionDigits: 2 }) + " ₺";
                    }
                },
                {
                    data: "status",
                    render: function (data) {
                        var text = data === "Completed" ? "Tamamlandı"
                            : data === "Cancelled" ? "İptal edildi"
                            : "Aktif";
                        return '<span class="status-pill">' + text + '</span>';
                    }
                },
                {
                    data: "paymentType",
                    render: function (data) {
                        return data === "Installment" ? "Taksitli" : "Nakit";
                    }
                }
            ];

            if (this.canEdit) {
                columns.push({
                    data: "id",
                    orderable: false,
                    searchable: false,
                    render: function (id, type, row) {
                        var safeId = app.Common.escapeHtml(id);
                        var routeId = encodeURIComponent(id);
                        var cancel = row.status === "Active"
                            ? '<button type="button" class="btn btn-sm btn-outline-danger" data-enrollment-cancel="' + safeId + '">İptal et</button>'
                            : '';

                        return '<div class="table-actions">'
                            + '<a class="btn btn-sm btn-outline-primary" href="/Enrollment/Details/' + routeId + '">Detay</a>'
                            + '<a class="btn btn-sm btn-outline-secondary" href="/Enrollment/Edit/' + routeId + '">Düzenle</a>'
                            + cancel
                            + '</div>';
                    }
                });
            }

            return columns;
        },

        initializeTable: function () {
            this.dataTable = this.$table.DataTable({
                processing: true,
                serverSide: true,
                pageLength: 10,
                lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
                ajax: function (request, callback) {
                    var pageSize = Math.min(Math.max(Number(request.length) || 10, 1), 100);
                    var pageIndex = Math.floor((Number(request.start) || 0) / pageSize);
                    var search = request.search && request.search.value ? request.search.value : "";
                    var url = "/api/enrollments?pageIndex=" + pageIndex
                        + "&pageSize=" + pageSize
                        + "&search=" + encodeURIComponent(search);

                    $.getJSON(url)
                        .done(function (response) {
                            var total = Number(app.Common.getValue(response, "Count")) || 0;
                            callback({
                                draw: request.draw,
                                recordsTotal: total,
                                recordsFiltered: total,
                                data: app.Common.getValue(response, "Items") || []
                            });
                        })
                        .fail(function (xhr) {
                            app.Common.showApiError(xhr);
                            callback({
                                draw: request.draw,
                                recordsTotal: 0,
                                recordsFiltered: 0,
                                data: []
                            });
                        });
                },
                columns: this.getColumns(),
                responsive: true,
                language: {
                    emptyTable: "Gösterilecek kayıt bulunamadı.",
                    search: "Ara:",
                    lengthMenu: "_MENU_ kayıt göster",
                    info: "_TOTAL_ kayıttan _START_ - _END_ arası gösteriliyor",
                    infoEmpty: "Kayıt bulunamadı",
                    paginate: {
                        first: "İlk",
                        last: "Son",
                        next: "Sonraki",
                        previous: "Önceki"
                    }
                }
            });
        }
    };

    app.Pages.EnrollmentsPage = enrollmentsPage;
    app.application.registerPage(enrollmentsPage);
})(window, jQuery);
