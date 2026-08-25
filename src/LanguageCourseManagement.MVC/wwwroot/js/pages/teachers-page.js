(function (window, $) {
    "use strict";
    var app = window.LanguageCourseManagement;
    var teachersPage = {
        $page: null, $table: null, $form: null, $standaloneForm: null, dataTable: null, canEdit: false,
        dayNames: ["Pazar", "Pazartesi", "Salı", "Çarşamba", "Perşembe", "Cuma", "Cumartesi"],
        canInitialize: function () { return $("#teachers-page").length > 0 || $("[data-teacher-form]").length > 0; },
        init: function () {
            this.$page = $("#teachers-page"); this.$table = $("#teachersTable"); this.$form = $("#teacherDeleteAntiforgeryForm"); this.$standaloneForm = $("[data-teacher-form]");
            if (!this.$form.length) this.$form = this.$standaloneForm;
            this.canEdit = app.Common.toBoolean(this.$page.data("can-edit"));
            if (this.$table.length) this.initializeTable();
            this.bindEvents();
        },
        bindEvents: function () {
            var self = this;
            this.$table.off("click.teachers", "[data-teacher-delete]").on("click.teachers", "[data-teacher-delete]", function (event) { self.handleDelete(event); });
            this.$standaloneForm.off("submit.teachers").on("submit.teachers", function (event) { self.handleSubmit(event); });
            $("#addAvailabilityRow").off("click.teachers").on("click.teachers", function () { self.addAvailabilityRow(); });
            this.$standaloneForm.off("click.teachers", "[data-remove-availability]").on("click.teachers", "[data-remove-availability]", function () { $(this).closest("tr").remove(); self.reindexAvailabilityRows(); });
        },
        addAvailabilityRow: function () {
            var $tbody = $("#availabilityTable tbody"), index = $tbody.find("tr").length, options = "";
            for (var i = 0; i < 7; i++) options += '<option value="' + i + '">' + this.dayNames[i] + '</option>';
            $tbody.append('<tr><td><select name="Availabilities[' + index + '].DayOfWeek" class="form-select form-select-sm">' + options + '</select><input type="hidden" name="Availabilities[' + index + '].Id" value="" /></td><td><input type="time" name="Availabilities[' + index + '].StartTime" class="form-control form-control-sm" value="09:00" /></td><td><input type="time" name="Availabilities[' + index + '].EndTime" class="form-control form-control-sm" value="17:00" /></td><td><button type="button" class="btn btn-sm btn-outline-danger" data-remove-availability>Sil</button></td></tr>');
        },
        reindexAvailabilityRows: function () { $("#availabilityTable tbody tr").each(function (index) { $(this).find("select, input").each(function () { var name = $(this).attr("name"); if (name) $(this).attr("name", name.replace(/Availabilities\[\d+\]/, "Availabilities[" + index + "]")); }); }); },
        payload: function (form, update) {
            var availabilities = [];
            $("#availabilityTable tbody tr").each(function () { var row = $(this), start = row.find("input[name$='.StartTime']").val(), end = row.find("input[name$='.EndTime']").val(); if (start && end) availabilities.push({ Id: row.find("input[name$='.Id']").val() || null, DayOfWeek: Number(row.find("select[name$='.DayOfWeek']").val()), StartTime: start, EndTime: end }); });
            var result = { FirstName: form.FirstName.value, LastName: form.LastName.value, HomePhone: form.HomePhone.value || null, MobilePhone: form.MobilePhone.value, Email: form.Email.value || null, HireDate: form.HireDate.value, LanguageIds: $(form.LanguageIds).val() || [], BranchIds: $(form.BranchIds).val() || [] };
            if (update) result.IsActive = form.IsActive.checked;
            return { teacher: result, availabilities: availabilities };
        },
        handleSubmit: function (event) {
            event.preventDefault(); var self = this, form = $(event.currentTarget), update = form.data("teacher-form") === "update", id = form.data("teacher-id"), submit = form.find("[type='submit']");
            if (submit.prop("disabled") || (form.valid && !form.valid())) return; submit.prop("disabled", true);
            var data = this.payload(form[0], update);
            $.ajax({ url: update ? "/api/teachers/" + encodeURIComponent(id) : "/api/teachers", method: update ? "PUT" : "POST", contentType: "application/json; charset=utf-8", headers: { "X-XSRF-TOKEN": app.Common.getAntiforgeryToken(this.$form) }, data: JSON.stringify(data.teacher) }).done(function (teacher) {
                var teacherId = teacher.id || teacher.Id, requests = $.map(data.availabilities, function (availability) { if (availability.Id) return null; return $.ajax({ url: "/api/teachers/" + encodeURIComponent(teacherId) + "/availabilities", method: "POST", contentType: "application/json; charset=utf-8", headers: { "X-XSRF-TOKEN": app.Common.getAntiforgeryToken(self.$form) }, data: JSON.stringify({ DayOfWeek: availability.DayOfWeek, StartTime: availability.StartTime, EndTime: availability.EndTime }) }); });
                $.when.apply($, requests).always(function () { window.location.assign("/Teacher/Details/" + encodeURIComponent(teacherId)); });
            }).fail(function (xhr) { if (xhr.status === 400 || xhr.status === 409 || xhr.status === 422) self.showErrors(xhr); else app.Common.showApiError(xhr); }).always(function () { submit.prop("disabled", false); });
        },
        showErrors: function (xhr) { var response = xhr.responseJSON || {}, messages = [], errors = response.errors || response.Errors, self = this; this.$standaloneForm.find("[data-validation-summary]").empty(); $.each(errors || {}, function (key, value) { var values = $.isArray(value) ? value : [value], field = self.$standaloneForm.find("[data-valmsg-for]").filter(function () { return String($(this).data("valmsg-for")).toLowerCase() === key.replace(/^\$?\./, "").toLowerCase(); }).first(); if (field.length) field.text(values.join(" ")); else messages = messages.concat(values); }); if (response.detail) messages.push(response.detail); messages = messages.concat(app.Common.getErrorMessages(xhr)); if (messages.length) this.$standaloneForm.find("[data-validation-summary]").removeClass("validation-summary-valid").addClass("validation-summary-errors").append($('<ul>').append($.map(messages, function (message) { return $('<li>').text(message); }))); },
        columns: function () { var columns = [{ data: null, render: function (data) { return app.Common.escapeHtml((data.firstName || "") + " " + (data.lastName || "")); } }, { data: "mobilePhone", render: function (data) { return app.Common.escapeHtml(data || "-"); } }, { data: "hireDate", render: function (data) { if (data && data.year) return String(data.day).padStart(2, "0") + "." + String(data.month).padStart(2, "0") + "." + data.year; return app.Common.escapeHtml(data || "-"); } }, { data: "isActive", render: function (data) { return data ? '<span class="status-pill status-active">Aktif</span>' : '<span class="status-pill">Pasif</span>'; } }]; if (this.canEdit) columns.push({ data: "id", orderable: false, searchable: false, render: function (id) { var encoded = encodeURIComponent(id); return '<div class="table-actions"><a class="btn btn-sm btn-outline-primary" href="/Teacher/Details/' + encoded + '">Detay</a> <a class="btn btn-sm btn-outline-secondary" href="/Teacher/Edit/' + encoded + '">Düzenle</a> <button type="button" class="btn btn-sm btn-outline-danger" data-teacher-delete="' + app.Common.escapeHtml(id) + '">Sil</button></div>'; } }); return columns; },
        initializeTable: function () { var self = this; this.dataTable = this.$table.DataTable({ processing: true, serverSide: true, pageLength: 10, ajax: function (request, callback) { var size = Math.min(Math.max(Number(request.length) || 10, 1), 100), index = Math.floor((Number(request.start) || 0) / size), search = request.search && request.search.value || ""; $.getJSON("/api/teachers?pageIndex=" + index + "&pageSize=" + size + "&search=" + encodeURIComponent(search)).done(function (response) { var count = Number(app.Common.getValue(response, "Count")) || 0; callback({ draw: request.draw, recordsTotal: count, recordsFiltered: count, data: app.Common.getValue(response, "Items") || [] }); }).fail(function (xhr) { app.Common.showApiError(xhr); callback({ draw: request.draw, recordsTotal: 0, recordsFiltered: 0, data: [] }); }); }, columns: this.columns(), responsive: true, language: { emptyTable: "Gösterilecek öğretmen bulunamadı.", search: "Ara:", lengthMenu: "_MENU_ kayıt göster", info: "_TOTAL_ kayıttan _START_ - _END_ arası gösteriliyor", infoEmpty: "Kayıt bulunamadı", paginate: { first: "İlk", last: "Son", next: "Sonraki", previous: "Önceki" } } }); },
        handleDelete: function (event) { var self = this, id = $(event.currentTarget).data("teacher-delete"); Swal.fire({ icon: "warning", title: "Öğretmen silinsin mi?", text: "Bu işlem öğretmeni pasif duruma getirecek.", showCancelButton: true, confirmButtonText: "Sil", cancelButtonText: "Vazgeç" }).then(function (result) { if (!result.isConfirmed) return; $.ajax({ url: "/api/teachers/" + encodeURIComponent(id), method: "DELETE", headers: { "X-XSRF-TOKEN": app.Common.getAntiforgeryToken(self.$form) } }).done(function () { app.Common.showSuccess("Öğretmen silindi"); self.dataTable.ajax.reload(null, false); }).fail(app.Common.showApiError); }); }
    };
    app.Pages.TeachersPage = teachersPage; app.application.registerPage(teachersPage);
})(window, jQuery);
