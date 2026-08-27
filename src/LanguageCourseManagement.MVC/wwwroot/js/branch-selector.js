(function (window, $) {
    "use strict";

    var app = window.LanguageCourseManagement = window.LanguageCourseManagement || {};

    var BranchSelector = {
        STORAGE_KEY: 'selectedBranchId',

        init: function () {
            var self = this;
            var $select = $('#topbarBranchFilter');

            if (!$select.length) {
                return;
            }

            self.loadBranches($select, function () {
                $(document).trigger('branch:loaded');
            });

            $select
                .off('change.branchSelector')
                .on('change.branchSelector', function () {
                    var val = $(this).val();
                    if (val) {
                        localStorage.setItem(self.STORAGE_KEY, val);
                    } else {
                        localStorage.removeItem(self.STORAGE_KEY);
                    }
                    $(document).trigger('branch:changed', val);
                });
        },

        getBranchId: function () {
            var val = localStorage.getItem(this.STORAGE_KEY);
            return val && val !== '' ? val : null;
        },

        getBranchQueryParam: function () {
            var id = this.getBranchId();
            return id ? '&branchId=' + encodeURIComponent(id) : '';
        },

        loadBranches: function ($select, callback) {
            var url = "/api/branches?pageIndex=0&pageSize=100&isActive=true";

            $.getJSON(url)
                .done(function (response) {
                    var items = app.Common.getValue(response, "Items") || [];
                    $select.find("option:gt(0)").remove();
                    $.each(items, function (_, item) {
                        $select.append($("<option>", {
                            value: app.Common.getValue(item, "Id"),
                            text: app.Common.getValue(item, "Name") || "-"
                        }));
                    });

                    var saved = localStorage.getItem(BranchSelector.STORAGE_KEY);
                    if (saved) {
                        $select.val(saved);
                    }

                    if (typeof callback === 'function') {
                        callback(items);
                    }
                })
                .fail(function () {
                    if (typeof callback === 'function') {
                        callback([]);
                    }
                });
        }
    };

    app.BranchSelector = BranchSelector;

    $(function () {
        BranchSelector.init();
    });
})(window, jQuery);
