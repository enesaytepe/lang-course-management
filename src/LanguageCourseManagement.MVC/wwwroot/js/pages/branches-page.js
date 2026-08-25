(function (window, $) {
    "use strict";

    var app = window.LanguageCourseManagement;

    var branchesPage = {
        $page: null,
        $table: null,
        $form: null,
        $createModal: null,
        $createForm: null,
        $standaloneForm: null,
        createModal: null,
        dataTable: null,
        canEdit: false,

        canInitialize: function () {
            return $("#branches-page").length > 0 && $("#branchesTable").length > 0
                || $("[data-branch-form]").length > 0;
        },

        init: function () {
            this.$page = $("#branches-page");
            this.$table = $("#branchesTable");
            this.$form = $("#branchDeleteAntiforgeryForm");
            this.$createModal = $("#branchCreateModal");
            this.$createForm = $("#branchCreateForm");
            this.$standaloneForm = $("[data-branch-form]");
            if (!this.$form.length) {
                this.$form = this.$standaloneForm;
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
                    .off("click.branchesPage", "[data-branch-delete]")
                    .on("click.branchesPage", "[data-branch-delete]", function (event) {
                        self.handleDelete(event);
                    });
            }

            if (this.$createModal.length && this.$createForm.length) {
                this.createModal = bootstrap.Modal.getOrCreateInstance(this.$createModal[0]);
                this.$createModal
                    .off("shown.bs.modal.branchesPage hidden.bs.modal.branchesPage")
                    .on("shown.bs.modal.branchesPage", function () {
                        self.clearCreateForm();
                        self.$createForm.find("[name='Name']").trigger("focus");
                    })
                    .on("hidden.bs.modal.branchesPage", function () {
                        self.clearCreateForm();
                    });
                this.$createForm
                    .off("submit.branchesPage")
                    .on("submit.branchesPage", function (event) {
                        self.handleCreate(event);
                    });
            }

            if (this.$standaloneForm.length) {
                this.$standaloneForm
                    .off("submit.branchesPage")
                    .on("submit.branchesPage", function (event) {
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
            this.submitBranch(this.$createForm, false)
                .done(function () {
                    self.createModal.hide();
                    app.Common.showSuccess("Şube oluşturuldu");
                    self.refreshTable();
                })
                .fail(function (xhr) {
                    if (xhr.status === 400 || xhr.status === 422 || xhr.status === 409) {
                        self.showCreateErrors(xhr);
                    } else {
                        app.Common.showApiError(xhr);
                    }
                })
                .always(function () {
                    $submit.prop("disabled", false);
                });
        },

        handleStandaloneSubmit: function (event) {
            event.preventDefault();
            var $form = $(event.currentTarget);
            var isUpdate = $form.data("branch-form") === "update";
            var $submit = $form.find("[type='submit']");
            if ($submit.prop("disabled")) {
                return;
            }
            if ($form.valid && !$form.valid()) {
                return;
            }

            $submit.prop("disabled", true);
            this.submitBranch($form, isUpdate)
                .done(function (branch) {
                    var branchId = branch.id || branch.Id;
                    window.location.assign("/Branch/Details/" + encodeURIComponent(branchId));
                })
                .fail(app.Common.showApiError)
                .always(function () {
                    $submit.prop("disabled", false);
                });
        },

        getBranchPayload: function ($form, isUpdate) {
            var form = $form[0];
            return {
                Name: form.Name.value,
                Address: form.Address.value,
                PublicTransportationDirections: form.PublicTransportationDirections.value || null,
                PrivateVehicleDirections: form.PrivateVehicleDirections.value || null,
                Latitude: Number(form.Latitude.value),
                Longitude: Number(form.Longitude.value),
                PhoneNumber: form.PhoneNumber.value || null,
                IsActive: isUpdate ? form.IsActive.checked : undefined,
                FacilityIds: $(form.FacilityIds).val() || []
            };
        },

        submitBranch: function ($form, isUpdate) {
            var id = $form.data("branch-id");
            return $.ajax({
                url: isUpdate ? "/api/branches/" + encodeURIComponent(id) : "/api/branches",
                method: isUpdate ? "PUT" : "POST",
                contentType: "application/json; charset=utf-8",
                headers: {
                    "X-XSRF-TOKEN": app.Common.getAntiforgeryToken(this.$form)
                },
                data: JSON.stringify(this.getBranchPayload($form, isUpdate))
            });
        },

        showCreateErrors: function (xhr) {
            var response = xhr.responseJSON || {};
            var messages = [];
            var errors = response.errors || response.Errors;
            var self = this;
            this.$createForm.find("[data-validation-summary]").empty();

            if (errors && typeof errors === "object") {
                $.each(errors, function (key, value) {
                    var fieldMessages = $.isArray(value) ? value : [value];
                    var normalized = key.replace(/^\$?\./, "").toLowerCase();
                    var $field = self.$createForm.find("[data-valmsg-for]").filter(function () {
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
                var $summary = this.$createForm.find("[data-validation-summary]");
                $summary.removeClass("validation-summary-valid").addClass("validation-summary-errors")
                    .append($("<ul>").append($.map(messages, function (message) {
                        return $("<li>").text(message);
                    })));
            }
        },

        getColumns: function () {
            var columns = [
                {
                    data: "name",
                    defaultContent: "-",
                    render: function (data) {
                        return app.Common.escapeHtml(data || "-");
                    }
                },
                {
                    data: "address",
                    defaultContent: "-",
                    render: function (data) {
                        return app.Common.escapeHtml(data || "-");
                    }
                },
                {
                    data: "phoneNumber",
                    defaultContent: "-",
                    render: function (data) {
                        return app.Common.escapeHtml(data || "-");
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
                            '<a class="btn btn-sm btn-outline-primary" href="/Branch/Details/' + routeId + '">Detay</a>' +
                            '<a class="btn btn-sm btn-outline-secondary" href="/Branch/Edit/' + routeId + '">Düzenle</a>' +
                            '<button type="button" class="btn btn-sm btn-outline-danger" data-branch-delete="' + id + '">Sil</button>' +
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
                    var url = "/api/branches?pageIndex=" + pageIndex
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
                    emptyTable: "Gösterilecek şube bulunamadı.",
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
            this.dataTable.ajax.reload(null, false);
        },

        handleDelete: function (event) {
            var self = this;
            var id = $(event.currentTarget).data("branch-delete");

            Swal.fire({
                icon: "warning",
                title: "Şube silinsin mi?",
                text: "Bu işlem şubeyi pasif duruma getirecek.",
                showCancelButton: true,
                confirmButtonText: "Sil",
                cancelButtonText: "Vazgeç"
            }).then(function (result) {
                if (!result.isConfirmed) {
                    return;
                }

                $.ajax({
                    url: "/api/branches/" + encodeURIComponent(id),
                    method: "DELETE",
                    headers: {
                        "X-XSRF-TOKEN": app.Common.getAntiforgeryToken(self.$form)
                    }
                })
                    .done(function () {
                        app.Common.showSuccess("Şube silindi");
                        self.refreshTable();
                    })
                    .fail(app.Common.showApiError);
            });
        }
    };

    app.Pages.BranchesPage = branchesPage;
    app.application.registerPage(branchesPage);
})(window, jQuery);
