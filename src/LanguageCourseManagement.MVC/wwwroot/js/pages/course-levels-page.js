(function (window, $) {
    "use strict";

    var app = window.LanguageCourseManagement;

    var page = {
        $page: null,
        $table: null,
        $filter: null,
        $antiForgeryForm: null,
        $createModal: null,
        $createForm: null,
        $standaloneForm: null,
        dataTable: null,
        createModal: null,
        canEdit: false,

        canInitialize: function () {
            return ($("#course-levels-page").length > 0 && $("#course-levelsTable").length > 0)
                || $("[data-courselevel-form]").length > 0;
        },

        init: function () {
            this.$page = $("#course-levels-page");
            this.$table = $("#course-levelsTable");
            this.$filter = $("#courseLevelLanguageFilter");
            this.$antiForgeryForm = $("#courseLevelDeleteAntiforgeryForm");
            this.$createModal = $("#courseLevelCreateModal");
            this.$createForm = $("#courseLevelCreateForm");
            this.$standaloneForm = $("[data-courselevel-form]");
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

            if (this.$filter.length) {
                this.$filter
                    .off("change.courseLevelsPage")
                    .on("change.courseLevelsPage", function () {
                        self.dataTable.ajax.reload();
                    });
            }

            if (this.$table.length) {
                this.$table
                    .off("click.courseLevelsPage", "[data-courselevel-delete]")
                    .on("click.courseLevelsPage", "[data-courselevel-delete]", function (event) {
                        self.handleDelete(event);
                    });
            }

            if (this.$createModal.length && this.$createForm.length) {
                this.createModal = bootstrap.Modal.getOrCreateInstance(this.$createModal[0]);
                this.$createModal
                    .off("shown.bs.modal.courseLevelsPage hidden.bs.modal.courseLevelsPage")
                    .on("shown.bs.modal.courseLevelsPage", function () {
                        self.clearCreateForm();
                        self.$createForm.find("[name='Name']").trigger("focus");
                    })
                    .on("hidden.bs.modal.courseLevelsPage", function () {
                        self.clearCreateForm();
                    });
                this.$createForm
                    .off("submit.courseLevelsPage")
                    .on("submit.courseLevelsPage", function (event) {
                        self.handleCreate(event);
                    });
            }

            if (this.$standaloneForm.length) {
                this.$standaloneForm
                    .off("submit.courseLevelsPage")
                    .on("submit.courseLevelsPage", function (event) {
                        self.handleStandaloneSubmit(event);
                    });
            }
        },

        clearCreateForm: function () {
            if (!this.$createForm || !this.$createForm.length) {
                return;
            }
            var validator = this.$createForm.validate();
            validator.resetForm();
            this.$createForm[0].reset();
            this.$createForm.find(".input-validation-error").removeClass("input-validation-error");
            this.$createForm.find("[data-validation-summary]").empty()
                .removeClass("validation-summary-errors").addClass("validation-summary-valid");
        },

        handleCreate: function (event) {
            event.preventDefault();
            var self = this;
            var $submit = this.$createForm.find("[type='submit']");
            if ($submit.prop("disabled")) {
                return;
            }
            if (this.$createForm.valid && !this.$createForm.valid()) {
                return;
            }

            $submit.prop("disabled", true);
            this.submit(this.$createForm, false)
                .done(function () {
                    self.createModal.hide();
                    app.Common.showSuccess("Kurs seviyesi oluşturuldu");
                    self.refreshTable();
                })
                .fail(function (xhr) {
                    self.showFormErrors(self.$createForm, xhr);
                })
                .always(function () {
                    $submit.prop("disabled", false);
                });
        },

        handleStandaloneSubmit: function (event) {
            event.preventDefault();
            var self = this;
            var $form = $(event.currentTarget);
            var isUpdate = $form.data("courselevel-form") === "update";
            var $submit = $form.find("[type='submit']");
            if ($submit.prop("disabled")) {
                return;
            }
            if ($form.valid && !$form.valid()) {
                return;
            }

            $submit.prop("disabled", true);
            this.submit($form, isUpdate)
                .done(function (result) {
                    window.location.assign("/CourseLevel/Details/" + encodeURIComponent(result.id || result.Id));
                })
                .fail(function (xhr) {
                    self.showFormErrors($form, xhr);
                })
                .always(function () {
                    $submit.prop("disabled", false);
                });
        },

        getPayload: function ($form, isUpdate) {
            var form = $form[0];
            return {
                OfferedLanguageId: form.OfferedLanguageId.value,
                Name: form.Name.value,
                Description: form.Description.value || null,
                Order: Number(form.Order.value),
                IsActive: isUpdate ? form.IsActive.checked : undefined
            };
        },

        submit: function ($form, isUpdate) {
            var id = $form.data("courselevel-id");
            return $.ajax({
                url: isUpdate ? "/api/course-levels/" + encodeURIComponent(id) : "/api/course-levels",
                method: isUpdate ? "PUT" : "POST",
                contentType: "application/json; charset=utf-8",
                headers: {
                    "X-XSRF-TOKEN": app.Common.getAntiforgeryToken(this.$antiForgeryForm)
                },
                data: JSON.stringify(this.getPayload($form, isUpdate))
            });
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

        getColumns: function () {
            var columns = [
                {
                    data: "languageName",
                    defaultContent: "-",
                    render: function (data) {
                        return app.Common.escapeHtml(data || "-");
                    }
                },
                {
                    data: "name",
                    defaultContent: "-",
                    render: function (data) {
                        return app.Common.escapeHtml(data || "-");
                    }
                },
                {
                    data: "order"
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
                            '<a class="btn btn-sm btn-outline-primary" href="/CourseLevel/Details/' + routeId + '">Detay</a>' +
                            '<a class="btn btn-sm btn-outline-secondary" href="/CourseLevel/Edit/' + routeId + '">Düzenle</a>' +
                            '<button type="button" class="btn btn-sm btn-outline-danger" data-courselevel-delete="' + id + '">Sil</button>' +
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
                    var search = request.search && request.search.value ? request.search.value : "";
                    var language = page.$filter.val();
                    var url = "/api/course-levels?pageIndex=" + pageIndex
                        + "&pageSize=" + pageSize
                        + "&search=" + encodeURIComponent(search);
                    if (language) {
                        url += "&offeredLanguageId=" + encodeURIComponent(language);
                    }

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
                    emptyTable: "Gösterilecek kurs seviyesi bulunamadı.",
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
            var id = $(event.currentTarget).data("courselevel-delete");

            Swal.fire({
                icon: "warning",
                title: "Kurs seviyesi silinsin mi?",
                text: "Bu işlem seviyeyi pasif duruma getirecek.",
                showCancelButton: true,
                confirmButtonText: "Sil",
                cancelButtonText: "Vazgeç"
            }).then(function (result) {
                if (!result.isConfirmed) {
                    return;
                }

                $.ajax({
                    url: "/api/course-levels/" + encodeURIComponent(id),
                    method: "DELETE",
                    headers: {
                        "X-XSRF-TOKEN": app.Common.getAntiforgeryToken(self.$antiForgeryForm)
                    }
                })
                    .done(function () {
                        app.Common.showSuccess("Kurs seviyesi silindi");
                        self.refreshTable();
                    })
                    .fail(app.Common.showApiError);
            });
        }
    };

    app.Pages.CourseLevelsPage = page;
    app.application.registerPage(page);
})(window, jQuery);
