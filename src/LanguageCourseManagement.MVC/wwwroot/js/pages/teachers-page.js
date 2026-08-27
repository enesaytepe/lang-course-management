(function (window, $) {
    "use strict";

    var app = window.LanguageCourseManagement;

    var teachersPage = {
        $page: null,
        $table: null,
        $form: null,
        $standaloneForm: null,
        dataTable: null,
        canEdit: false,
        dayNames: ["Pazar", "Pazartesi", "Salı", "Çarşamba", "Perşembe", "Cuma", "Cumartesi"],
        originalAvailabilityIds: [],

        canInitialize: function () {
            return $("#teachers-page").length > 0 || $("[data-teacher-form]").length > 0;
        },

        init: function () {
            this.$page = $("#teachers-page");
            this.$table = $("#teachersTable");
            this.$form = $("#teacherDeleteAntiforgeryForm");
            this.$standaloneForm = $("[data-teacher-form]");
            if (!this.$form.length) {
                this.$form = this.$standaloneForm;
            }
            this.canEdit = app.Common.toBoolean(this.$page.data("can-edit"));
            this.originalAvailabilityIds = this.captureOriginalAvailabilityIds();

            if (this.$table.length) {
                this.initializeTable();
            }
            this.bindEvents();
        },

        captureOriginalAvailabilityIds: function () {
            var ids = [];
            $("#availabilityTable tbody tr input[name$='.Id']").each(function () {
                var val = $(this).val();
                if (val) {
                    ids.push(val);
                }
            });
            return ids;
        },

        bindEvents: function () {
            var self = this;

            if (this.$table.length) {
                this.$table
                    .off("click.teachers", "[data-teacher-delete]")
                    .on("click.teachers", "[data-teacher-delete]", function (event) {
                        self.handleDelete(event);
                    });
            }

            if (this.$standaloneForm.length) {
                this.$standaloneForm
                    .off("submit.teachers")
                    .on("submit.teachers", function (event) {
                        self.handleSubmit(event);
                    });
            }

            if (this.$standaloneForm.length) {
                $("#addAvailabilityRow")
                    .off("click.teachers")
                    .on("click.teachers", function () {
                        self.addAvailabilityRow();
                    });

                this.$standaloneForm
                    .off("click.teachers", "[data-remove-availability]")
                    .on("click.teachers", "[data-remove-availability]", function () {
                        $(this).closest("tr").remove();
                        self.reindexAvailabilityRows();
                    });
            }
        },

        addAvailabilityRow: function () {
            var $tbody = $("#availabilityTable tbody");
            var index = $tbody.find("tr").length;
            var options = "";

            for (var i = 0; i < 7; i++) {
                options += "<option value=\"" + i + "\">" + this.dayNames[i] + "</option>";
            }

            var row = "<tr>"
                + "<td>"
                + "<select name=\"Availabilities[" + index + "].DayOfWeek\" class=\"form-select form-select-sm\">" + options + "</select>"
                + "<input type=\"hidden\" name=\"Availabilities[" + index + "].Id\" value=\"\" />"
                + "</td>"
                + "<td><input type=\"time\" name=\"Availabilities[" + index + "].StartTime\" class=\"form-control form-control-sm\" value=\"09:00\" /></td>"
                + "<td><input type=\"time\" name=\"Availabilities[" + index + "].EndTime\" class=\"form-control form-control-sm\" value=\"17:00\" /></td>"
                + "<td><button type=\"button\" class=\"btn btn-sm btn-outline-danger\" data-remove-availability>Sil</button></td>"
                + "</tr>";

            $tbody.append(row);
        },

        reindexAvailabilityRows: function () {
            $("#availabilityTable tbody tr").each(function (index) {
                $(this).find("select, input").each(function () {
                    var name = $(this).attr("name");
                    if (name) {
                        $(this).attr("name", name.replace(/Availabilities\[\d+\]/, "Availabilities[" + index + "]"));
                    }
                });
            });
        },

        getPayload: function (form, update) {
            var availabilities = [];

            $("#availabilityTable tbody tr").each(function () {
                var row = $(this);
                var start = row.find("input[name$='.StartTime']").val();
                var end = row.find("input[name$='.EndTime']").val();
                if (start && end) {
                    availabilities.push({
                        Id: row.find("input[name$='.Id']").val() || null,
                        DayOfWeek: Number(row.find("select[name$='.DayOfWeek']").val()),
                        StartTime: start,
                        EndTime: end
                    });
                }
            });

            var result = {
                FirstName: form.FirstName.value,
                LastName: form.LastName.value,
                HomePhone: form.HomePhone.value || null,
                MobilePhone: form.MobilePhone.value,
                Email: form.Email.value || null,
                HireDate: form.HireDate.value,
                LanguageIds: $(form.LanguageIds).val() || [],
                BranchIds: $(form.BranchIds).val() || []
            };

            if (update) {
                result.IsActive = form.IsActive.checked;
            }

            return {
                teacher: result,
                availabilities: availabilities
            };
        },

        handleSubmit: function (event) {
            event.preventDefault();
            var self = this;
            var $form = $(event.currentTarget);
            var update = $form.data("teacher-form") === "update";
            var id = $form.data("teacher-id");
            var $submit = $form.find("[type='submit']");

            if ($submit.prop("disabled") || ($form.valid && !$form.valid())) {
                return;
            }
            $submit.prop("disabled", true);

            var data = this.getPayload($form[0], update);
            var currentAvailabilityIds = $.map(data.availabilities, function (a) { return a.Id; }).filter(Boolean);
            var removedIds = $.grep(self.originalAvailabilityIds, function (origId) {
                return $.inArray(origId, currentAvailabilityIds) === -1;
            });

            $.ajax({
                url: update ? "/api/teachers/" + encodeURIComponent(id) : "/api/teachers",
                method: update ? "PUT" : "POST",
                contentType: "application/json; charset=utf-8",
                headers: {
                    "X-XSRF-TOKEN": app.Common.getAntiforgeryToken(self.$form)
                },
                data: JSON.stringify(data.teacher)
            }).done(function (teacher) {
                var teacherId = teacher.id || teacher.Id;
                var requests = [];

                // DELETE removed availabilities
                $.each(removedIds, function (_, removedId) {
                    requests.push($.ajax({
                        url: "/api/teachers/" + encodeURIComponent(teacherId) + "/availabilities/" + encodeURIComponent(removedId),
                        method: "DELETE",
                        headers: {
                            "X-XSRF-TOKEN": app.Common.getAntiforgeryToken(self.$form)
                        }
                    }));
                });

                // Process each availability (PUT existing, POST new)
                $.each(data.availabilities, function (_, availability) {
                    if (availability.Id) {
                        // Existing availability: send PUT
                        requests.push($.ajax({
                            url: "/api/teachers/" + encodeURIComponent(teacherId) + "/availabilities/" + encodeURIComponent(availability.Id),
                            method: "PUT",
                            contentType: "application/json; charset=utf-8",
                            headers: {
                                "X-XSRF-TOKEN": app.Common.getAntiforgeryToken(self.$form)
                            },
                            data: JSON.stringify({
                                DayOfWeek: availability.DayOfWeek,
                                StartTime: availability.StartTime,
                                EndTime: availability.EndTime
                            })
                        }));
                    } else {
                        // New availability: send POST
                        requests.push($.ajax({
                            url: "/api/teachers/" + encodeURIComponent(teacherId) + "/availabilities",
                            method: "POST",
                            contentType: "application/json; charset=utf-8",
                            headers: {
                                "X-XSRF-TOKEN": app.Common.getAntiforgeryToken(self.$form)
                            },
                            data: JSON.stringify({
                                DayOfWeek: availability.DayOfWeek,
                                StartTime: availability.StartTime,
                                EndTime: availability.EndTime
                            })
                        }));
                    }
                });

                $.when.apply($, requests).always(function () {
                    window.location.assign("/Teacher/Details/" + encodeURIComponent(teacherId));
                });
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
            var columns = [
                {
                    data: null,
                    render: function (data) {
                        return app.Common.escapeHtml((data.firstName || "") + " " + (data.lastName || ""));
                    }
                },
                {
                    data: "mobilePhone",
                    render: function (data) {
                        return app.Common.escapeHtml(data || "-");
                    }
                },
                {
                    data: "hireDate",
                    render: function (data) {
                        if (data && data.year) {
                            return String(data.day).padStart(2, "0") + "."
                                + String(data.month).padStart(2, "0") + "."
                                + data.year;
                        }
                        return app.Common.escapeHtml(data || "-");
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

            if (this.canEdit) {
                columns.push({
                    data: "id",
                    orderable: false,
                    searchable: false,
                    render: function (id) {
                        var encoded = encodeURIComponent(id);
                        return '<div class="table-actions">'
                            + '<a class="btn btn-sm btn-outline-primary" href="/Teacher/Details/' + encoded + '">Detay</a> '
                            + '<a class="btn btn-sm btn-outline-secondary" href="/Teacher/Edit/' + encoded + '">Düzenle</a> '
                            + '<button type="button" class="btn btn-sm btn-outline-danger" data-teacher-delete="'
                            + app.Common.escapeHtml(id) + '">Sil</button>'
                            + '</div>';
                    }
                });
            }

            return columns;
        },

        initializeTable: function () {
            var self = this;

            this.dataTable = this.$table.DataTable({
                processing: true,
                serverSide: true,
                pageLength: 10,
                lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
                ajax: function (request, callback) {
                    var size = Math.min(Math.max(Number(request.length) || 10, 1), 100);
                    var index = Math.floor((Number(request.start) || 0) / size);
                    var search = request.search && request.search.value || "";

                    $.getJSON("/api/teachers?pageIndex=" + index
                        + "&pageSize=" + size
                        + "&search=" + encodeURIComponent(search))
                        .done(function (response) {
                            var count = Number(app.Common.getValue(response, "Count")) || 0;
                            callback({
                                draw: request.draw,
                                recordsTotal: count,
                                recordsFiltered: count,
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
                columns: this.columns(),
                responsive: true,
                language: {
                    emptyTable: "Gösterilecek öğretmen bulunamadı.",
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
            var id = $(event.currentTarget).data("teacher-delete");

            Swal.fire({
                icon: "warning",
                title: "Öğretmen silinsin mi?",
                text: "Bu işlem öğretmeni pasif duruma getirecek.",
                showCancelButton: true,
                confirmButtonText: "Sil",
                cancelButtonText: "Vazgeç"
            }).then(function (result) {
                if (!result.isConfirmed) {
                    return;
                }

                $.ajax({
                    url: "/api/teachers/" + encodeURIComponent(id),
                    method: "DELETE",
                    headers: {
                        "X-XSRF-TOKEN": app.Common.getAntiforgeryToken(self.$form)
                    }
                })
                    .done(function () {
                        app.Common.showSuccess("Öğretmen silindi");
                        self.refreshTable();
                    })
                    .fail(app.Common.showApiError);
            });
        }
    };

    app.Pages.TeachersPage = teachersPage;
    app.application.registerPage(teachersPage);
})(window, jQuery);
