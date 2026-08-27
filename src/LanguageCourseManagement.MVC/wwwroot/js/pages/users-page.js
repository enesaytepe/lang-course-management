(function (window, $) {
    "use strict";

    var app = window.LanguageCourseManagement;

    var usersPage = {
        $page: null,
        $table: null,
        $form: null,
        $standaloneForm: null,
        dataTable: null,

        canInitialize: function () {
            return $("#users-page").length > 0 || $("[data-user-form]").length > 0;
        },

        init: function () {
            this.$page = $("#users-page");
            this.$table = $("#usersTable");
            this.$form = $("#userDeleteAntiforgeryForm");
            this.$standaloneForm = $("[data-user-form]");
            if (!this.$form.length) {
                this.$form = this.$standaloneForm;
            }

            if (this.$table.length) {
                this.initializeTable();
            }
            this.bindEvents();
        },

        bindEvents: function () {
            var self = this;

            if (this.$table.length) {
                this.$table
                    .off("click.usersPage", "[data-user-delete]")
                    .on("click.usersPage", "[data-user-delete]", function (event) {
                        self.handleDelete(event);
                    });
            }

            if (this.$standaloneForm.length) {
                this.$standaloneForm
                    .off("submit.usersPage")
                    .on("submit.usersPage", function (event) {
                        self.handleSubmit(event);
                    });
            }
        },

        getPayload: function (form, update) {
            var result = {
                UserName: form.UserName ? form.UserName.value : "",
                FullName: form.FullName.value,
                Email: form.Email.value,
                Role: form.Role.value
            };

            if (!update) {
                result.Password = form.Password ? form.Password.value : "";
            }

            return result;
        },

        handleSubmit: function (event) {
            event.preventDefault();
            var self = this;
            var $form = $(event.currentTarget);
            var formType = $form.data("user-form");
            var id = $form.data("user-id");
            var $submit = $form.find("[type='submit']");

            if ($submit.prop("disabled") || ($form.valid && !$form.valid())) {
                return;
            }
            $submit.prop("disabled", true);

            if (formType === "change-password") {
                self.handleChangePassword($form, id, $submit);
                return;
            }

            var update = formType === "update";
            var data = this.getPayload($form[0], update);

            $.ajax({
                url: update ? "/api/users/" + encodeURIComponent(id) : "/api/users",
                method: update ? "PUT" : "POST",
                contentType: "application/json; charset=utf-8",
                headers: {
                    "X-XSRF-TOKEN": app.Common.getAntiforgeryToken(self.$form)
                },
                data: JSON.stringify(data)
            }).done(function (user) {
                var userId = user.id || user.Id;
                window.location.assign("/User/Index");
            }).fail(function (xhr) {
                if (xhr.status === 400 || xhr.status === 409 || xhr.status === 422) {
                    self.showFormErrors($form, xhr);
                } else {
                    app.Common.showApiError(xhr);
                }
            }).always(function () {
                $submit.prop("disabled", false);
            });
        },

        handleChangePassword: function ($form, id, $submit) {
            var self = this;
            var data = {
                CurrentPassword: $form.find("[name='CurrentPassword']").val(),
                NewPassword: $form.find("[name='NewPassword']").val(),
                ConfirmPassword: $form.find("[name='ConfirmPassword']").val()
            };

            $.ajax({
                url: "/api/users/" + encodeURIComponent(id) + "/change-password",
                method: "POST",
                contentType: "application/json; charset=utf-8",
                headers: {
                    "X-XSRF-TOKEN": app.Common.getAntiforgeryToken(self.$form)
                },
                data: JSON.stringify(data)
            }).done(function () {
                app.Common.showSuccess("Şifre başarıyla değiştirildi");
                window.location.assign("/User/Index");
            }).fail(function (xhr) {
                if (xhr.status === 400 || xhr.status === 409 || xhr.status === 422) {
                    self.showFormErrors($form, xhr);
                } else {
                    app.Common.showApiError(xhr);
                }
            }).always(function () {
                $submit.prop("disabled", false);
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

        columns: function () {
            return [
                {
                    data: "userName",
                    render: function (data) {
                        return app.Common.escapeHtml(data || "-");
                    }
                },
                {
                    data: "fullName",
                    render: function (data) {
                        return app.Common.escapeHtml(data || "-");
                    }
                },
                {
                    data: "email",
                    render: function (data) {
                        return app.Common.escapeHtml(data || "-");
                    }
                },
                {
                    data: "roles",
                    render: function (data) {
                        if (!data || !data.length) return "-";
                        return app.Common.escapeHtml(data.join(", "));
                    }
                },
                {
                    data: "isActive",
                    render: function (data) {
                        return data
                            ? "<span class=\"status-pill status-active\">Aktif</span>"
                            : "<span class=\"status-pill\">Pasif</span>";
                    }
                }
            ];
        },

        initializeTable: function () {
            var self = this;

            this.dataTable = this.$table.DataTable({
                processing: true,
                serverSide: false,
                pageLength: 10,
                lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
                ajax: function (request, callback) {
                    $.getJSON("/api/users")
                        .done(function (data) {
                            callback({
                                draw: request.draw,
                                recordsTotal: data.length,
                                recordsFiltered: data.length,
                                data: data
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
                columns: this.columns().concat([{
                    data: "id",
                    orderable: false,
                    searchable: false,
                    render: function (id) {
                        var encoded = encodeURIComponent(id);
                        return '<div class="table-actions">'
                            + '<a class="btn btn-sm btn-outline-secondary" href="/User/Edit/' + encoded + '">Düzenle</a> '
                            + '<a class="btn btn-sm btn-outline-info" href="/User/ChangePassword/' + encoded + '">Şifre Değiştir</a> '
                            + '<button type="button" class="btn btn-sm btn-outline-danger" data-user-delete="'
                            + app.Common.escapeHtml(id) + '">Sil</button>'
                            + '</div>';
                    }
                }]),
                responsive: true,
                language: {
                    emptyTable: "Gösterilecek kullanıcı bulunamadı.",
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
            var id = $(event.currentTarget).data("user-delete");

            Swal.fire({
                icon: "warning",
                title: "Kullanıcı silinsin mi?",
                text: "Bu işlem geri alınamaz.",
                showCancelButton: true,
                confirmButtonText: "Sil",
                cancelButtonText: "Vazgeç"
            }).then(function (result) {
                if (!result.isConfirmed) {
                    return;
                }

                $.ajax({
                    url: "/api/users/" + encodeURIComponent(id),
                    method: "DELETE",
                    headers: {
                        "X-XSRF-TOKEN": app.Common.getAntiforgeryToken(self.$form)
                    }
                })
                    .done(function () {
                        app.Common.showSuccess("Kullanıcı silindi");
                        self.refreshTable();
                    })
                    .fail(app.Common.showApiError);
            });
        }
    };

    app.Pages.UsersPage = usersPage;
    app.application.registerPage(usersPage);
})(window, jQuery);
