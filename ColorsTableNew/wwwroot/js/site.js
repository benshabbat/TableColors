// Global error handler
window.onerror = function (msg, url, lineNo, columnNo, error) {
    console.error('Global error:', { msg, url, lineNo, columnNo, error });
    return false;
};

$(document).ready(function () {
    initializeSortable();

    // Show modal popup
    window.showInPopUp = async (url, title) => {
        try {
            const res = await $.ajax({
                type: "GET",
                url: url
            });
            $('#form-modal .modal-body').html(res);
            $('#form-modal .modal-title').html(title);
            $('#form-modal').modal('show');
        } catch (err) {
            console.error('Error:', err);
        }
    };

    // Submit form using AJAX
    window.jQueryAjaxPost = form => {
        if (!form || !form.checkValidity()) {
            return false;
        }

        try {
            const formData = new FormData(form);
            $.ajax({
                type: 'POST',
                url: form.action,
                data: formData,
                contentType: false,
                processData: false,
                success: function (res) {
                    if (res.isValid) {
                        $('#view-all tbody').html(res.html);
                        $('#form-modal').modal('hide');
                        initializeSortable();
                    } else {
                        $('#form-modal .modal-body').html(res.html);
                    }
                },
                error: function (err) {
                    console.error('Error:', err);
                }
            });
        } catch (ex) {
            console.error('Exception:', ex);
        }
        return false;
    };

    // Delete record
    window.jQueryAjaxDelete = (form, id) => {
        showInPopUp(`/Color/Delete/${id}`, 'Delete Color');
        return false;
    };

    // Helper functions for sorting
    const collectRowData = () => {
        return $('.sortable-row').map((index, row) => ({
            id: $(row).data('id'),
            order: index + 1
        })).get();
    };

    const updateRowOrder = (items) => {
        return $.ajax({
            url: '/Color/UpdateOrder',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(items)
        });
    };

    const debouncedUpdateOrder = _.debounce(async (items) => {
        try {
            const response = await updateRowOrder(items);
        } catch (ex) {
            console.error('Exception:', ex);
        }
    }, 500);

    function initializeSortable() {
        $('#view-all tbody').sortable({
            handle: ".handle",
            items: "tr.sortable-row",
            axis: "y",
            cursor: "move",
            helper: function (e, tr) {
                var $originals = tr.children();
                var $helper = tr.clone();
                $helper.children().each(function (index) {
                    $(this).width($originals.eq(index).width());
                });
                return $helper;
            },
            update: function (event, ui) {
                const items = collectRowData();
                $('.sortable-row').each(function (index) {
                    $(this).find('.display-order').text(index + 1);
                });
                debouncedUpdateOrder(items);
            }
        });
    }

    // Make sure jQuery UI is properly initialized
    $(document).ready(function () {
        initializeSortable();

        // Re-initialize sortable after any AJAX updates
        $(document).ajaxComplete(function () {
            initializeSortable();
        });
    });
});