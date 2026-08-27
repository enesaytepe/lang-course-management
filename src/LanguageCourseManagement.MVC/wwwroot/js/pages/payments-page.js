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

            this.loadBranchOptions();

            if (this.$table.length) {
                this.initializeTable();
            }

            this.bindEvents();
            this.bindBranchChange();
        },

        bindBranchChange: function () {
            var self = this;
            $(document).off("branch.changed.paymentsPage").on("branch.changed.paymentsPage", function (_, branchId) {
                if (branchId) {
                    $("#branchFilter").val(branchId);
                } else {
                    $("#branchFilter").val("");
                }
                if (self.dataTable) {
                    self.dataTable.ajax.reload(null, false);
                }
            });
        },

        bindEvents: function () {
            var self = this;

            $("#branchFilter")
                .off("change.paymentsFilter")
                .on("change.paymentsFilter", function () {
                    self.dataTable.ajax.reload(null, false);
                });
        },

        loadBranchOptions: function () {
            var $branch = $("#branchFilter");
            var pageSize = 100;
            var url = "/api/branches?pageIndex=0&pageSize=" + pageSize + "&isActive=true";

            $.getJSON(url)
                .done(function (response) {
                    var items = app.Common.getValue(response, "Items") || [];
                    $branch.find("option:gt(0)").remove();
                    $.each(items, function (_, item) {
                        $branch.append($("<option>", {
                            value: app.Common.getValue(item, "Id"),
                            text: app.Common.getValue(item, "Name") || "-"
                        }));
                    });
                })
                .fail(app.Common.showApiError);
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
                    data: "branchName",
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
                    render: function (data) {
                        var text = data === "Cash" ? "Nakit"
                            : data === "CreditCard" ? "Kredi Kartı"
                            : data === "BankTransfer" ? "Havale"
                            : data || "-";
                        return app.Common.escapeHtml(text);
                    }
                },
                {
                    data: "installmentNumber",
                    render: function (data) {
                        return data ? '#' + data : '-';
                    }
                },
                {
                    data: "status",
                    render: function (data) {
                        var text = data === "Settled" ? "Tamamlandı"
                            : data === "Overdue" ? "Gecikmiş"
                            : data || "-";
                        var cssClass = data === "Overdue"
                            ? "status-pill status-danger"
                            : "status-pill status-active";
                        return '<span class="' + cssClass + '">' + text + '</span>';
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
                pageLength: 10,
                lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
                ajax: function (request, callback) {
                    var pageSize = Math.min(Math.max(Number(request.length) || 10, 1), 100);
                    var pageIndex = Math.floor((Number(request.start) || 0) / pageSize);
                    var search = request.search && request.search.value ? request.search.value : "";
                    var branchId = $("#branchFilter").val() || "";
                    var url = "/api/payments?pageIndex=" + pageIndex
                        + "&pageSize=" + pageSize
                        + "&search=" + encodeURIComponent(search);
                    if (branchId) {
                        url += "&branchId=" + encodeURIComponent(branchId);
                    }

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
