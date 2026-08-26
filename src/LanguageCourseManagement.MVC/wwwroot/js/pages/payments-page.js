(function (window, $) {
    "use strict";

    var app = window.LanguageCourseManagement;

    var paymentsPage = {
        $page: null,
        $table: null,
        dataTable: null,

        canInitialize: function () {
            return $("#payments-page").length > 0;
        },

        init: function () {
            this.$page = $("#payments-page");
            this.$table = $("#paymentsTable");

            if (this.$table.length) {
                this.initializeTable();
            }
        },

        getColumns: function () {
            return [
                {
                    data: "studentName",
                    defaultContent: "-",
                    render: function (data) {
                        return app.Common.escapeHtml(data || "-");
                    }
                },
                {
                    data: "courseName",
                    defaultContent: "-",
                    render: function (data) {
                        return app.Common.escapeHtml(data || "-");
                    }
                },
                {
                    data: "amount",
                    render: function (data) {
                        return (Number(data) || 0).toLocaleString("tr-TR", { minimumFractionDigits: 2 }) + " ₺";
                    }
                },
                {
                    data: "status",
                    render: function (data) {
                        var text = data === "Settled" ? "Tamamlandı" : data || "-";
                        return '<span class="status-pill status-active">' + text + '</span>';
                    }
                },
                {
                    data: "settledAt",
                    render: function (data) {
                        if (!data) return "-";
                        var date = new Date(data);
                        return date.toLocaleDateString("tr-TR") + " " + date.toLocaleTimeString("tr-TR", { hour: "2-digit", minute: "2-digit" });
                    }
                },
                {
                    data: "id",
                    orderable: false,
                    searchable: false,
                    render: function (data) {
                        var routeId = encodeURIComponent(data);
                        return '<div class="table-actions">'
                            + '<a class="btn btn-sm btn-outline-primary" href="/Payment/Details/' + routeId + '">Detay</a>'
                            + '</div>';
                    }
                }
            ];
        },

        initializeTable: function () {
            this.dataTable = this.$table.DataTable({
                processing: true,
                serverSide: true,
                ajax: function (request, callback) {
                    var pageSize = Math.min(Math.max(Number(request.length) || 10, 1), 100);
                    var pageIndex = Math.floor((Number(request.start) || 0) / pageSize);
                    var search = request.search && request.search.value ? request.search.value : "";
                    var url = "/api/payments?pageIndex=" + pageIndex
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
                pageLength: 10,
                lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
                columns: this.getColumns(),
                responsive: true,
                language: {
                    emptyTable: "Gösterilecek tahsilat bulunamadı.",
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
        }
    };

    app.Pages.PaymentsPage = paymentsPage;
    app.application.registerPage(paymentsPage);
})(window, jQuery);
