(function (window, $) {
    "use strict";
    var app = window.LanguageCourseManagement;
    var page = {
        init: function () {
            var table = $("#coursesTable");
            page.canEdit = app.Common.toBoolean($("#courses-page").data("can-edit"));
            if (table.length) {
                table.DataTable({ processing: true, serverSide: true, pageLength: 10,
                    ajax: function (request, callback) {
                        var size = Math.min(Math.max(Number(request.length) || 10, 1), 100);
                        $.getJSON("/api/courses?pageIndex=" + Math.floor((Number(request.start) || 0) / size) + "&pageSize=" + size + "&search=" + encodeURIComponent((request.search || {}).value || ""))
                            .done(function (response) { var items = app.Common.getValue(response, "Items") || []; var count = Number(app.Common.getValue(response, "Count")) || 0; callback({ draw: request.draw, recordsTotal: count, recordsFiltered: count, data: items }); })
                            .fail(function (xhr) { app.Common.showApiError(xhr); callback({ draw: request.draw, recordsTotal: 0, recordsFiltered: 0, data: [] }); });
                    },
                    columns: [
                        { data: "branchName", render: function (x) { return app.Common.escapeHtml(x || "-"); } },
                        { data: "name", render: function (x) { return app.Common.escapeHtml(x || "-"); } },
                        { data: "languageName", render: function (x, t, row) { return app.Common.escapeHtml((x || "-") + " / " + (row.levelName || "-")); } },
                        { data: "teacherName", render: function (x) { return app.Common.escapeHtml(x || "-"); } },
                        { data: "classroomName", render: function (x) { return app.Common.escapeHtml(x || "-"); } },
                        { data: "startDate", render: function (x, t, row) { return app.Common.escapeHtml((x || "") + " - " + (row.endDate || "")); } },
                        { data: "status", render: function (x) { return app.Common.escapeHtml(x); } },
                         { data: "id", orderable: false, searchable: false, render: function (x) { var id = encodeURIComponent(x); var actions = '<a class="btn btn-sm btn-outline-primary" href="/Course/Details/' + id + '">Detay</a>'; if (page.canEdit) actions += ' <a class="btn btn-sm btn-outline-secondary" href="/Course/Edit/' + id + '">Düzenle</a>'; return actions; } }
                    ], responsive: true, language: { emptyTable: "Gösterilecek ders bulunamadı.", search: "Ara:", lengthMenu: "_MENU_ kayıt göster", info: "_TOTAL_ kayıttan _START_ - _END_ arası gösteriliyor", infoEmpty: "Kayıt bulunamadı", paginate: { first: "İlk", last: "Son", next: "Sonraki", previous: "Önceki" } }
                });
            }
            var form = $("[data-course-form]");
            if (form.length) { this.bindForm(form); }
        },
        bindForm: function (form) {
            var self = this;
            $("#addSchedule").on("click", function () { self.addSchedule(); });
            $("#courseSchedules").on("click", ".remove-schedule", function () { $(this).closest(".schedule-row").remove(); self.renumberSchedules(); self.loadEligibility(form); });
            form.on("change", "select, input[type=date], input[type=time]", function () { self.loadEligibility(form); });
        },
        addSchedule: function () { var i = $("#courseSchedules .schedule-row").length; $("#courseSchedules").append('<div class="row g-2 mb-2 schedule-row"><div class="col-md-4"><select name="Schedules[' + i + '].DayOfWeek" class="form-select"><option value="1">Pazartesi</option><option value="2">Salı</option><option value="3">Çarşamba</option><option value="4">Perşembe</option><option value="5">Cuma</option><option value="6">Cumartesi</option><option value="0">Pazar</option></select></div><div class="col-md-3"><input name="Schedules[' + i + '].StartTime" type="time" class="form-control" /></div><div class="col-md-3"><input name="Schedules[' + i + '].EndTime" type="time" class="form-control" /></div><div class="col-md-2"><button type="button" class="btn btn-outline-danger remove-schedule">Kaldır</button></div></div>'); },
        renumberSchedules: function () { $("#courseSchedules .schedule-row").each(function (i) { $(this).find("[name]").each(function () { $(this).attr("name", $(this).attr("name").replace(/Schedules\[\d+\]/, "Schedules[" + i + "]")); }); }); },
        getSchedules: function () { var result = []; $("#courseSchedules .schedule-row").each(function () { var row = $(this); result.push({ dayOfWeek: Number(row.find("select").val()), startTime: row.find("input[type=time]").eq(0).val(), endTime: row.find("input[type=time]").eq(1).val() }); }); return result; },
         loadEligibility: function (form) { var branchId = form.find("[name=BranchId]").val(), languageId = form.find("[name=OfferedLanguageId]").val(), startDate = form.find("[name=StartDate]").val(), endDate = form.find("[name=EndDate]").val(); var schedules = this.getSchedules(); if (!branchId || !languageId || !startDate || !endDate || !schedules.length) return; var id = form.data("course-id") || null; $.ajax({ url: "/api/courses/eligible-teachers", method: "POST", contentType: "application/json", headers: { "X-XSRF-TOKEN": app.Common.getAntiforgeryToken(form) }, data: JSON.stringify({ branchId: branchId, offeredLanguageId: languageId, courseLevelId: form.find("[name=CourseLevelId]").val(), startDate: startDate, endDate: endDate, schedules: schedules, excludeCourseId: id }) }).done(function (items) { var select = form.find("#TeacherId").empty().append('<option value="">Öğretmen seçin</option>'); $.each(items, function (_, x) { select.append($('<option>').val(x.id).text(x.fullName)); }); }); $.ajax({ url: "/api/courses/eligible-classrooms", method: "POST", contentType: "application/json", headers: { "X-XSRF-TOKEN": app.Common.getAntiforgeryToken(form) }, data: JSON.stringify({ branchId: branchId, startDate: startDate, endDate: endDate, schedules: schedules, excludeCourseId: id }) }).done(function (items) { var select = form.find("#ClassroomId").empty().append('<option value="">Derslik seçin</option>'); $.each(items, function (_, x) { select.append($('<option>').val(x.id).text(x.name + " (" + x.capacity + ")")); }); }); }
    };
    app.Pages.CoursesPage = page; app.application.registerPage(page);
})(window, jQuery);
