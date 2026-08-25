(function (window, $) {
    "use strict";

    var app = window.LanguageCourseManagement;

    var studentsPage = {
        $page: null,
        $table: null,
        $antiForgeryForm: null,
        $standaloneForm: null,
        dataTable: null,
        canEdit: false,

        canInitialize: function () {
            return $("#students-page").length > 0 && $("#studentsTable").length > 0
                || $("[data-student-form]").length > 0;
        },

        init: function () {
            this.$page = $("#students-page");
            this.$table = $("#studentsTable");
            this.$antiForgeryForm = $("#studentDeleteAntiforgeryForm");
            this.$standaloneForm = $("[data-student-form]");

            if (!this.$antiForgeryForm.length) {
                this.$antiForgeryForm = this.$standaloneForm;
            }

            this.canEdit = app.Common.toBoolean(this.$page.data("can-edit"));

            if (this.$table.length) {
                this.initializeTable();
            }

            this.bindEvents();
        },

        bindEvents: function () {
            var self = this;

            if (this.$table.length) {
                this.$table
                    .off("click.studentsPage", "[data-student-delete]")
                    .on("click.studentsPage", "[data-student-delete]", function (event) {
                        self.handleDelete(event);
                    });
            }

            if (this.$standaloneForm.length) {
                this.$standaloneForm
                    .off("submit.studentsPage")
                    .on("submit.studentsPage", function (event) {
                        self.handleStandaloneSubmit(event);
                    });
            }
        },

        handleStandaloneSubmit: function (event) {
            event.preventDefault();

            var $form = $(event.currentTarget);
            var isUpdate = $form.data("student-form") === "update";
            var $submit = $form.find("[type='submit']");

            if ($submit.prop("disabled")) {
                return;
            }

            if ($form.valid && !$form.valid()) {
                return;
            }

            $submit.prop("disabled", true);

            this.submitStudent($form, isUpdate)
                .done(function (student) {
                    var id = student.id || student.Id;
                    window.location.assign("/Student/Details/" + encodeURIComponent(id));
                })
                .fail(function (xhr) {
                    studentsPage.showFormErrors($form, xhr);
                })
                .always(function () {
                    $submit.prop("disabled", false);
                });
        },

        getPayload: function ($form) {
            var form = $form[0];
            var isUpdate = $form.data("student-form") === "update";

            return {
                FirstName: form.FirstName.value,
                LastName: form.LastName.value,
                HomePhone: form.HomePhone.value || null,
                MobilePhone: form.MobilePhone.value,
                Email: form.Email.value || null,
                IsActive: isUpdate ? form.IsActive.checked : undefined
            };
        },

        submitStudent: function ($form, isUpdate) {
            var id = $form.data("student-id");

            return $.ajax({
                url: isUpdate
                    ? "/api/students/" + encodeURIComponent(id)
                    : "/api/students",
                method: isUpdate ? "PUT" : "POST",
                contentType: "application/json; charset=utf-8",
                headers: {
                    "X-XSRF-TOKEN": app.Common.getAntiforgeryToken(this.$antiForgeryForm)
                },
                data: JSON.stringify(this.getPayload($form))
            });
        },

        showFormErrors: function ($form, xhr) {
            var response = xhr.responseJSON || {};
            var errors = response.errors || response.Errors;
            var messages = [];
            var detail = response.detail || response.Detail;

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

            if (detail) {
                messages.push(detail);
            }

            messages = messages.concat(app.Common.getErrorMessages(xhr));
            messages = $.grep(messages, function (message, index) {
                return message && $.inArray(message, messages) === index;
            });

            if (messages.length) {
                var $summary = $form.find("[data-validation-summary]");
                $summary
                    .removeClass("validation-summary-valid")
                    .addClass("validation-summary-errors")
                    .append($("<ul>").append($.map(messages, function (message) {
                        return $("<li>").text(message);
                    })));
            }
        },

        getColumns: function () {
            var columns = [
                {
                    data: null,
                    defaultContent: "-",
                    render: function (data) {
                        var firstName = app.Common.escapeHtml(data.firstName || "");
                        var lastName = app.Common.escapeHtml(data.lastName || "");
                        return firstName + " " + lastName;
                    }
                },
                {
                    data: "mobilePhone",
                    defaultContent: "-",
                    render: function (data) {
                        return app.Common.escapeHtml(data || "-");
                    }
                },
                {
                    data: "registrationDate",
                    render: function (data) {
                        if (!data) return "-";
                        var date = new Date(data);
                        var day = String(date.getDate()).padStart(2, "0");
                        var month = String(date.getMonth() + 1).padStart(2, "0");
                        var year = date.getFullYear();
                        return day + "." + month + "." + year;
                    }
                },
                {
                    data: "isActive",
                    render: function (data) {
                        return data
                            ? '<span class="status-pill status-active">Aktif</span>'
                            : '<span class="status-pill">Pasif</span>';
                    }
                }
            ];

            if (this.canEdit) {
                columns.push({
                    data: "id",
                    orderable: false,
                    searchable: false,
                    render: function (data) {
                        var id = app.Common.escapeHtml(data);
                        var routeId = encodeURIComponent(data);

                        return '<div class="table-actions">' +
                            '<a class="btn btn-sm btn-outline-primary" href="/Student/Details/' + routeId + '">Detay</a>' +
                            '<a class="btn btn-sm btn-outline-secondary" href="/Student/Edit/' + routeId + '">Düzenle</a>' +
                            '<button type="button" class="btn btn-sm btn-outline-danger" data-student-delete="' + id + '">Sil</button>' +
                            '</div>';
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
                    var search = request.search && request.search.value
                        ? request.search.value
                        : "";

                    var url = "/api/students?pageIndex=" + pageIndex
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
                    emptyTable: "Gösterilecek öğrenci bulunamadı.",
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
        },

        refreshTable: function () {
            if (this.dataTable) {
                this.dataTable.ajax.reload(null, false);
            }
        },

        handleDelete: function (event) {
            var self = this;
            var id = $(event.currentTarget).data("student-delete");

            Swal.fire({
                icon: "warning",
                title: "Öğrenci silinsin mi?",
                text: "Bu işlem öğrenciyi silinmiş olarak işaretleyecek.",
                showCancelButton: true,
                confirmButtonText: "Sil",
                cancelButtonText: "Vazgeç"
            }).then(function (result) {
                if (!result.isConfirmed) {
                    return;
                }

                $.ajax({
                    url: "/api/students/" + encodeURIComponent(id),
                    method: "DELETE",
                    headers: {
                        "X-XSRF-TOKEN": app.Common.getAntiforgeryToken(self.$antiForgeryForm)
                    }
                })
                    .done(function () {
                        app.Common.showSuccess("Öğrenci silindi");
                        self.refreshTable();
                    })
                    .fail(app.Common.showApiError);
            });
        }
    };

    app.Pages.StudentsPage = studentsPage;
    app.application.registerPage(studentsPage);
})(window, jQuery);
