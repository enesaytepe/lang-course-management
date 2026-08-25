(function (window, $) {
    "use strict";

    var app = window.LanguageCourseManagement;

    var facilitiesPage = {
        $page: null,
        $table: null,
        $antiForgeryForm: null,
        $form: null,
        dataTable: null,
        canEdit: false,

        canInitialize: function () {
            return $("#facilities-page").length > 0 || $("[data-facility-form]").length > 0;
        },

        init: function () {
            this.$page = $("#facilities-page");
            this.$table = $("#facilitiesTable");
            this.$antiForgeryForm = $("#facilityDeleteAntiforgeryForm");
            this.$form = $("[data-facility-form]");
            this.canEdit = app.Common.toBoolean(this.$page.data("can-edit"));

            if (!this.$antiForgeryForm.length) {
                this.$antiForgeryForm = this.$form;
            }

            if (this.$table.length) {
                this.initializeTable();
                this.$table
                    .off("click.facilitiesPage", "[data-facility-delete]")
                    .on("click.facilitiesPage", "[data-facility-delete]", this.handleDelete.bind(this));
            }

            this.$form
                .off("submit.facilitiesPage")
                .on("submit.facilitiesPage", this.handleSubmit.bind(this));
        },

        getPayload: function ($form, isUpdate) {
            var form = $form[0];
            var isActive = isUpdate
                ? $form.find("[name='IsActive']").filter(":checkbox").prop("checked")
                : true;

            return {
                Name: form.Name.value,
                Description: form.Description.value || null,
                IsActive: isActive
            };
        },

        submitForm: function ($form, isUpdate) {
            var id = $form.data("facility-id");

            return $.ajax({
                url: isUpdate ? "/api/facilities/" + encodeURIComponent(id) : "/api/facilities",
                method: isUpdate ? "PUT" : "POST",
                contentType: "application/json; charset=utf-8",
                headers: {
                    "X-XSRF-TOKEN": app.Common.getAntiforgeryToken($form)
                },
                data: JSON.stringify(this.getPayload($form, isUpdate))
            });
        },

        handleSubmit: function (event) {
            event.preventDefault();

            var self = this;
            var $form = $(event.currentTarget);
            var isUpdate = $form.data("facility-form") === "edit";
            var $submit = $form.find("[type='submit']");

            if ($submit.prop("disabled") || ($form.valid && !$form.valid())) {
                return;
            }

            $submit.prop("disabled", true);
            this.submitForm($form, isUpdate)
                .done(function (facility) {
                    var id = facility.id || facility.Id;
                    window.location.assign("/Facility/Details/" + encodeURIComponent(id));
                })
                .fail(function (xhr) {
                    self.showFormErrors($form, xhr);
                })
                .always(function () {
                    $submit.prop("disabled", false);
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

            if (!messages.length) {
                app.Common.showApiError(xhr);
                return;
            }

            $form.find("[data-validation-summary]")
                .removeClass("validation-summary-valid")
                .addClass("validation-summary-errors")
                .append($("<ul>").append($.map(messages, function (message) {
                    return $("<li>").text(message);
                })));
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
                    data: "description",
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
                },
                {
                    data: "id",
                    orderable: false,
                    searchable: false,
                    render: function (data) {
                        var routeId = encodeURIComponent(data);
                        var safeId = app.Common.escapeHtml(data);
                        var actions = '<a class="btn btn-sm btn-outline-primary" href="/Facility/Details/' + routeId + '">Detay</a>';

                        if (facilitiesPage.canEdit) {
                            actions += '<a class="btn btn-sm btn-outline-secondary" href="/Facility/Edit/' + routeId + '">Düzenle</a>'
                                + '<button type="button" class="btn btn-sm btn-outline-danger" data-facility-delete="' + safeId + '">Sil</button>';
                        }

                        return '<div class="table-actions">' + actions + '</div>';
                    }
                }
            ];

            return columns;
        },

        initializeTable: function () {
            var listEndpoint = this.$page.data("list-endpoint") || "/api/facilities/crud-list";
            var includeInactive = this.canEdit;

            this.dataTable = this.$table.DataTable({
                processing: true,
                serverSide: true,
                pageLength: 10,
                lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
                ajax: function (request, callback) {
                    var pageSize = Math.min(Math.max(Number(request.length) || 10, 1), 100);
                    var pageIndex = Math.floor((Number(request.start) || 0) / pageSize);
                    var search = request.search && request.search.value ? request.search.value : "";
                    var url = listEndpoint + "?pageIndex=" + pageIndex
                        + "&pageSize=" + pageSize
                        + "&search=" + encodeURIComponent(search)
                        + "&includeInactive=" + (includeInactive ? "true" : "false");

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
                            callback({ draw: request.draw, recordsTotal: 0, recordsFiltered: 0, data: [] });
                        });
                },
                columns: this.getColumns(),
                responsive: true,
                language: {
                    emptyTable: "Gösterilecek tesis bulunamadı.",
                    search: "Ara:",
                    lengthMenu: "_MENU_ kayıt göster",
                    info: "_TOTAL_ kayıttan _START_ - _END_ arası gösteriliyor",
                    infoEmpty: "Kayıt bulunamadı",
                    paginate: { first: "İlk", last: "Son", next: "Sonraki", previous: "Önceki" }
                }
            });
        },

        handleDelete: function (event) {
            var self = this;
            var id = $(event.currentTarget).data("facility-delete");

            Swal.fire({
                icon: "warning",
                title: "Tesis silinsin mi?",
                text: "Bu işlem tesisi pasif duruma getirecek.",
                showCancelButton: true,
                confirmButtonText: "Sil",
                cancelButtonText: "Vazgeç"
            }).then(function (result) {
                if (!result.isConfirmed) {
                    return;
                }

                $.ajax({
                    url: "/api/facilities/" + encodeURIComponent(id),
                    method: "DELETE",
                    headers: { "X-XSRF-TOKEN": app.Common.getAntiforgeryToken(self.$antiForgeryForm) }
                })
                    .done(function () {
                        app.Common.showSuccess("Tesis silindi");
                        self.dataTable.ajax.reload(null, false);
                    })
                    .fail(app.Common.showApiError);
            });
        }
    };

    app.Pages.FacilitiesPage = facilitiesPage;
    app.application.registerPage(facilitiesPage);
})(window, jQuery);
