(function (window, $) {
    "use strict";

    var app = window.LanguageCourseManagement;

    var auditLogPage = {
        $page: null,
        $table: null,
        dataTable: null,

        canInitialize: function () {
            return $("#audit-log-page").length > 0 && $("#auditLogTable").length > 0;
        },

        init: function () {
            this.$page = $("#audit-log-page");
            this.$table = $("#auditLogTable");

            if (this.$table.length) {
                this.initializeTable();
            }

            this.bindEvents();
        },

        bindEvents: function () {
            var self = this;
        },

        getColumns: function () {
            return [
                {
                    data: "timestamp",
                    render: function (data) {
                        if (!data) return "-";
                        var date = new Date(data);
                        var day = String(date.getDate()).padStart(2, "0");
                        var month = String(date.getMonth() + 1).padStart(2, "0");
                        var year = date.getFullYear();
                        var hours = String(date.getHours()).padStart(2, "0");
                        var minutes = String(date.getMinutes()).padStart(2, "0");
                        var seconds = String(date.getSeconds()).padStart(2, "0");
                        return day + "." + month + "." + year + " " + hours + ":" + minutes + ":" + seconds;
                    }
                },
                {
                    data: "entityName",
                    defaultContent: "-",
                    render: function (data) {
                        return app.Common.escapeHtml(data || "-");
                    }
                },
                {
                    data: "entityId",
                    defaultContent: "-",
                    render: function (data) {
                        var val = data || "-";
                        if (val.length > 36) {
                            val = val.substring(0, 36) + "...";
                        }
                        return '<code>' + app.Common.escapeHtml(val) + '</code>';
                    }
                },
                {
                    data: "action",
                    render: function (data) {
                        var actionMap = {
                            1: { text: "Oluşturuldu", css: "status-active" },
                            2: { text: "Güncellendi", css: "status-pending" },
                            3: { text: "Silindi", css: "status-inactive" }
                        };
                        var actionInfo = actionMap[data] || { text: "Bilinmiyor", css: "" };
                        return '<span class="status-pill ' + actionInfo.css + '">' + actionInfo.text + '</span>';
                    }
                },
                {
                    data: "userName",
                    defaultContent: "-",
                    render: function (data) {
                        return app.Common.escapeHtml(data || "Sistem");
                    }
                },
                {
                    data: "id",
                    orderable: false,
                    searchable: false,
                    render: function (data) {
                        var routeId = encodeURIComponent(data);
                        return '<button type="button" class="btn btn-sm btn-outline-primary" data-audit-log-detail="' + routeId + '">Detay</button>';
                    }
                }
            ];
        },

        initializeTable: function () {
            var self = this;
            this.dataTable = this.$table.DataTable({
                processing: true,
                serverSide: true,
                pageLength: 25,
                lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
                ajax: function (request, callback) {
                    var pageSize = Math.min(Math.max(Number(request.length) || 25, 1), 100);
                    var pageIndex = Math.floor((Number(request.start) || 0) / pageSize);
                    var search = request.search && request.search.value
                        ? request.search.value
                        : "";

                    var url = "/api/audit-logs?pageIndex=" + pageIndex
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
                    emptyTable: "Gösterilecek değişiklik kaydı bulunamadı.",
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
        }
    };

    app.Pages.AuditLogPage = auditLogPage;
    app.application.registerPage(auditLogPage);
})(window, jQuery);
