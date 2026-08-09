document.addEventListener("DOMContentLoaded", function () {
    const statusFilter = document.getElementById("statusFilter");

    if (!statusFilter) {
        return;
    }

    statusFilter.addEventListener("change", function () {
        const selectedStatus = statusFilter.value;
        const rows = document.querySelectorAll("#requestTable tbody tr");

        rows.forEach(function (row) {
            row.hidden =
                selectedStatus !== "ALL" &&
                row.dataset.status !== selectedStatus;
        });
    });
});
