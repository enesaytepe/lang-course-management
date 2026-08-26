(function (window, $) {
    "use strict";

    var app = window.LanguageCourseManagement;

    var page = {
        $page: null,
        $table: null,
        $form: null,
        dataTable: null,
        canEdit: false,

        canInitialize: function () {
            return ($("#courses-page").length > 0 && $("#coursesTable").length > 0)
                || $("[data-course-form]").length > 0;
        },

        init: function () {
            this.$page = $("#courses-page");
            this.$table = $("#coursesTable");
            this.$form = $("[data-course-form]");
            this.canEdit = app.Common.toBoolean(this.$page.data("can-edit"));

            if (this.$table.length) {
                this.initializeTable();
            }
            if (this.$form.length) {
                this.bindForm(this.$form);
            }
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
                    var pageIndex = Math.floor((Number(request.start) || 0) / size);
                    var search = request.search && request.search.value ? request.search.value : "";
                    var url = "/api/courses?pageIndex=" + pageIndex
                        + "&pageSize=" + size
                        + "&search=" + encodeURIComponent(search);

                    $.getJSON(url)
                        .done(function (response) {
                            var items = app.Common.getValue(response, "Items") || [];
                            var count = Number(app.Common.getValue(response, "Count")) || 0;
                            callback({
                                draw: request.draw,
                                recordsTotal: count,
                                recordsFiltered: count,
                                data: items
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
                columns: self.getColumns(),
                responsive: true,
                language: {
                    emptyTable: "Gösterilecek ders bulunamadı.",
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

        getColumns: function () {
            var self = this;
            var columns = [
                {
                    data: "branchName",
                    render: function (x) {
                        return app.Common.escapeHtml(x || "-");
                    }
                },
                {
                    data: "name",
                    render: function (x) {
                        return app.Common.escapeHtml(x || "-");
                    }
                },
                {
                    data: "languageName",
                    render: function (x, t, row) {
                        return app.Common.escapeHtml((x || "-") + " / " + (row.levelName || "-"));
                    }
                },
                {
                    data: "teacherName",
                    render: function (x) {
                        return app.Common.escapeHtml(x || "-");
                    }
                },
                {
                    data: "classroomName",
                    render: function (x) {
                        return app.Common.escapeHtml(x || "-");
                    }
                },
                {
                    data: "startDate",
                    render: function (x, t, row) {
                        return app.Common.escapeHtml((x || "") + " - " + (row.endDate || ""));
                    }
                },
                {
                    data: "status",
                    render: function (x) {
                        return app.Common.escapeHtml(x);
                    }
                }
            ];

            if (self.canEdit) {
                columns.push({
                    data: "id",
                    orderable: false,
                    searchable: false,
                    render: function (x) {
                        var id = encodeURIComponent(x);
                        var actions = '<a class="btn btn-sm btn-outline-primary" href="/Course/Details/' + id + '">Detay</a>';
                        actions += ' <a class="btn btn-sm btn-outline-secondary" href="/Course/Edit/' + id + '">Düzenle</a>';
                        return actions;
                    }
                });
            }

            return columns;
        },

        bindForm: function (form) {
            var self = this;

            $("#addSchedule")
                .off("click.coursesPage")
                .on("click.coursesPage", function () {
                    self.addSchedule();
                });

            $("#courseSchedules")
                .off("click.coursesPage", ".remove-schedule")
                .on("click.coursesPage", ".remove-schedule", function () {
                    $(this).closest(".schedule-row").remove();
                    self.renumberSchedules();
                    self.loadEligibility(form);
                });

            form
                .off("change.coursesPage", "select, input[type=date], input[type=time]")
                .on("change.coursesPage", "select, input[type=date], input[type=time]", function () {
                    self.loadEligibility(form);
                });
        },

        addSchedule: function () {
            var i = $("#courseSchedules .schedule-row").length;
            var html = '<div class="row g-2 mb-2 schedule-row">'
                + '<div class="col-md-4">'
                +     '<select name="Schedules[' + i + '].DayOfWeek" class="form-select">'
                +         '<option value="1">Pazartesi</option>'
                +         '<option value="2">Salı</option>'
                +         '<option value="3">Çarşamba</option>'
                +         '<option value="4">Perşembe</option>'
                +         '<option value="5">Cuma</option>'
                +         '<option value="6">Cumartesi</option>'
                +         '<option value="0">Pazar</option>'
                +     '</select>'
                + '</div>'
                + '<div class="col-md-3">'
                +     '<input name="Schedules[' + i + '].StartTime" type="time" class="form-control" />'
                + '</div>'
                + '<div class="col-md-3">'
                +     '<input name="Schedules[' + i + '].EndTime" type="time" class="form-control" />'
                + '</div>'
                + '<div class="col-md-2">'
                +     '<button type="button" class="btn btn-outline-danger remove-schedule">Kaldır</button>'
                + '</div>'
                + '</div>';

            $("#courseSchedules").append(html);
        },

        renumberSchedules: function () {
            $("#courseSchedules .schedule-row").each(function (i) {
                $(this).find("[name]").each(function () {
                    $(this).attr("name", $(this).attr("name").replace(/Schedules\[\d+\]/, "Schedules[" + i + "]"));
                });
            });
        },

        getSchedules: function () {
            var result = [];
            $("#courseSchedules .schedule-row").each(function () {
                var row = $(this);
                result.push({
                    dayOfWeek: Number(row.find("select").val()),
                    startTime: row.find("input[type=time]").eq(0).val(),
                    endTime: row.find("input[type=time]").eq(1).val()
                });
            });
            return result;
        },

        loadEligibility: function (form) {
            var branchId = form.find("[name=BranchId]").val();
            var languageId = form.find("[name=OfferedLanguageId]").val();
            var startDate = form.find("[name=StartDate]").val();
            var endDate = form.find("[name=EndDate]").val();
            var schedules = this.getSchedules();
            var id = form.data("course-id") || null;

            if (!branchId || !languageId || !startDate || !endDate || !schedules.length) {
                return;
            }

            $.ajax({
                url: "/api/courses/eligible-teachers",
                method: "POST",
                contentType: "application/json",
                headers: {
                    "X-XSRF-TOKEN": app.Common.getAntiforgeryToken(form)
                },
                data: JSON.stringify({
                    branchId: branchId,
                    offeredLanguageId: languageId,
                    courseLevelId: form.find("[name=CourseLevelId]").val(),
                    startDate: startDate,
                    endDate: endDate,
                    schedules: schedules,
                    excludeCourseId: id
                })
            })
                .done(function (items) {
                    var select = form.find("#TeacherId").empty()
                        .append("<option value=\"\">Öğretmen seçin</option>");
                    $.each(items, function (_, x) {
                        select.append($("<option>").val(x.id).text(x.fullName));
                    });
                })
                .fail(app.Common.showApiError);

            $.ajax({
                url: "/api/courses/eligible-classrooms",
                method: "POST",
                contentType: "application/json",
                headers: {
                    "X-XSRF-TOKEN": app.Common.getAntiforgeryToken(form)
                },
                data: JSON.stringify({
                    branchId: branchId,
                    startDate: startDate,
                    endDate: endDate,
                    schedules: schedules,
                    excludeCourseId: id
                })
            })
                .done(function (items) {
                    var select = form.find("#ClassroomId").empty()
                        .append("<option value=\"\">Derslik seçin</option>");
                    $.each(items, function (_, x) {
                        select.append($("<option>").val(x.id).text(x.name + " (" + x.capacity + ")"));
                    });
                })
                .fail(app.Common.showApiError);
        },

        refreshTable: function () {
            if (this.dataTable) {
                this.dataTable.ajax.reload(null, false);
            }
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
        }
    };

    app.Pages.CoursesPage = page;
    app.application.registerPage(page);
})(window, jQuery);
