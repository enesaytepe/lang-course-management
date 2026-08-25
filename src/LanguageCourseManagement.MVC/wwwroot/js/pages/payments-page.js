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
                    data: "enrollmentId",
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
                    data: "method",
                    defaultContent: "-",
                    render: function (data) {
                        return app.Common.escapeHtml(data || "-");
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
                serverSide: false,
                ajax: {
                    url: "/api/payments",
                    dataSrc: function (json) {
                        return json || [];
                    }
                },
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
