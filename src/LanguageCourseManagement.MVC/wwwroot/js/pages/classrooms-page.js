(function (window, $) {
    "use strict";

    var app = window.LanguageCourseManagement;

    var classroomsPage = {
        $page: null,
        $table: null,
        $antiForgeryForm: null,
        $createModal: null,
        $createForm: null,
        $standaloneForm: null,
        createModal: null,
        dataTable: null,
        canEdit: false,

        canInitialize: function () {
            return ($("#classrooms-page").length > 0 && $("#classroomsTable").length > 0)
                || $("[data-classroom-form]").length > 0;
        },

        init: function () {
            this.$page = $("#classrooms-page");
            this.$table = $("#classroomsTable");
            this.$antiForgeryForm = $("#classroomDeleteAntiforgeryForm");
            this.$createModal = $("#classroomCreateModal");
            this.$createForm = $("#classroomCreateForm");
            this.$standaloneForm = $("[data-classroom-form]");

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
                    .off("click.classroomsPage", "[data-classroom-delete]")
                    .on("click.classroomsPage", "[data-classroom-delete]", function (event) {
                        self.handleDelete(event);
                    });
            }

            if (this.$createModal.length && this.$createForm.length) {
                this.createModal = bootstrap.Modal.getOrCreateInstance(this.$createModal[0]);

                this.$createModal
                    .off("shown.bs.modal.classroomsPage hidden.bs.modal.classroomsPage")
                    .on("shown.bs.modal.classroomsPage", function () {
                        self.clearForm(self.$createForm);
                        self.$createForm.find("[name='BranchId']").trigger("focus");
                    })
                    .on("hidden.bs.modal.classroomsPage", function () {
                        self.clearForm(self.$createForm);
                    });

                this.$createForm
                    .off("submit.classroomsPage")
                    .on("submit.classroomsPage", function (event) {
                        self.handleCreate(event);
                    });
            }

            if (this.$standaloneForm.length) {
                this.$standaloneForm
                    .off("submit.classroomsPage")
                    .on("submit.classroomsPage", function (event) {
                        self.handleStandaloneSubmit(event);
                    });
            }
        },

        clearForm: function ($form) {
            if (!$form || !$form.length) {
                return;
            }

            var validator = $form.validate();
            validator.resetForm();
            $form[0].reset();
            $form.find(".input-validation-error").removeClass("input-validation-error");
            $form.find("[data-validation-summary]")
                .empty()
                .removeClass("validation-summary-errors")
                .addClass("validation-summary-valid");
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

            this.submitClassroom(this.$createForm, false)
                .done(function () {
                    self.createModal.hide();
                    app.Common.showSuccess("Derslik oluşturuldu");
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

            var $form = $(event.currentTarget);
            var isUpdate = $form.data("classroom-form") === "update";
            var $submit = $form.find("[type='submit']");

            if ($submit.prop("disabled")) {
                return;
            }

            if ($form.valid && !$form.valid()) {
                return;
            }

            $submit.prop("disabled", true);

            this.submitClassroom($form, isUpdate)
                .done(function (classroom) {
                    var id = classroom.id || classroom.Id;
                    window.location.assign("/Classroom/Details/" + encodeURIComponent(id));
                })
                .fail(function (xhr) {
                    classroomsPage.showFormErrors($form, xhr);
                })
                .always(function () {
                    $submit.prop("disabled", false);
                });
        },

        getPayload: function ($form, isUpdate) {
            var form = $form[0];

            return {
                BranchId: form.BranchId.value,
                Name: form.Name.value,
                Description: form.Description.value || null,
                Capacity: Number(form.Capacity.value),
                IsActive: isUpdate ? form.IsActive.checked : undefined
            };
        },

        submitClassroom: function ($form, isUpdate) {
            var id = $form.data("classroom-id");

            return $.ajax({
                url: isUpdate
                    ? "/api/classrooms/" + encodeURIComponent(id)
                    : "/api/classrooms",
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
                    data: "branchName",
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
                    data: "capacity",
                    render: function (data) {
                        return Number(data) || 0;
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
                            '<a class="btn btn-sm btn-outline-primary" href="/Classroom/Details/' + routeId + '">Detay</a>' +
                            '<a class="btn btn-sm btn-outline-secondary" href="/Classroom/Edit/' + routeId + '">Düzenle</a>' +
                            '<button type="button" class="btn btn-sm btn-outline-danger" data-classroom-delete="' + id + '">Sil</button>' +
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

                    var url = "/api/classrooms?pageIndex=" + pageIndex
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
                    emptyTable: "Gösterilecek derslik bulunamadı.",
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
            var id = $(event.currentTarget).data("classroom-delete");

            Swal.fire({
                icon: "warning",
                title: "Derslik silinsin mi?",
                text: "Bu işlem dersliği silinmiş olarak işaretleyecek.",
                showCancelButton: true,
                confirmButtonText: "Sil",
                cancelButtonText: "Vazgeç"
            }).then(function (result) {
                if (!result.isConfirmed) {
                    return;
                }

                $.ajax({
                    url: "/api/classrooms/" + encodeURIComponent(id),
                    method: "DELETE",
                    headers: {
                        "X-XSRF-TOKEN": app.Common.getAntiforgeryToken(self.$antiForgeryForm)
                    }
                })
                    .done(function () {
                        app.Common.showSuccess("Derslik silindi");
                        self.refreshTable();
                    })
                    .fail(app.Common.showApiError);
            });
        }
    };

    app.Pages.ClassroomsPage = classroomsPage;
    app.application.registerPage(classroomsPage);
})(window, jQuery);
