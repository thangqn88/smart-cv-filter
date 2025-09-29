// Smart CV Filter - Custom JavaScript

// Global variables
let isLoading = false;

// Initialize when DOM is loaded
document.addEventListener("DOMContentLoaded", function () {
  initializeTooltips();
  initializeAlerts();
  initializeFormValidation();
  initializeDataTables();
});

// Initialize Bootstrap tooltips
function initializeTooltips() {
  var tooltipTriggerList = [].slice.call(
    document.querySelectorAll('[data-bs-toggle="tooltip"]')
  );
  var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
    return new bootstrap.Tooltip(tooltipTriggerEl);
  });
}

// Initialize alerts with auto-dismiss
function initializeAlerts() {
  const alerts = document.querySelectorAll(".alert[data-auto-dismiss]");
  alerts.forEach((alert) => {
    const delay = parseInt(alert.dataset.autoDismiss) || 5000;
    setTimeout(() => {
      const bsAlert = new bootstrap.Alert(alert);
      bsAlert.close();
    }, delay);
  });
}

// Initialize form validation
function initializeFormValidation() {
  const forms = document.querySelectorAll(".needs-validation");
  forms.forEach((form) => {
    form.addEventListener("submit", function (event) {
      if (!form.checkValidity()) {
        event.preventDefault();
        event.stopPropagation();
      }
      form.classList.add("was-validated");
    });
  });
}

// Initialize data tables with search and pagination
function initializeDataTables() {
  const tables = document.querySelectorAll(".data-table");
  tables.forEach((table) => {
    // Add search functionality
    const searchInput = table.querySelector(".table-search");
    if (searchInput) {
      searchInput.addEventListener("input", function () {
        filterTable(table, this.value);
      });
    }
  });
}

// Filter table rows based on search input
function filterTable(table, searchTerm) {
  const tbody = table.querySelector("tbody");
  const rows = tbody.querySelectorAll("tr");

  rows.forEach((row) => {
    const text = row.textContent.toLowerCase();
    const matches = text.includes(searchTerm.toLowerCase());
    row.style.display = matches ? "" : "none";
  });
}

// Show loading state
function showLoading(element) {
  if (isLoading) return;

  isLoading = true;
  if (element) {
    element.disabled = true;
    const originalText = element.innerHTML;
    element.innerHTML =
      '<span class="spinner-border spinner-border-sm me-2"></span>Loading...';
    element.dataset.originalText = originalText;
  }
}

// Hide loading state
function hideLoading(element) {
  isLoading = false;
  if (element && element.dataset.originalText) {
    element.disabled = false;
    element.innerHTML = element.dataset.originalText;
    delete element.dataset.originalText;
  }
}

// Show alert message
function showAlert(message, type = "info", container = ".alert-container") {
  const alertContainer = document.querySelector(container);
  if (!alertContainer) return;

  const alertId = "alert-" + Date.now();
  const alertHtml = `
        <div id="${alertId}" class="alert alert-${type} alert-dismissible fade show" role="alert" data-auto-dismiss="5000">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;

  alertContainer.insertAdjacentHTML("beforeend", alertHtml);
  initializeAlerts();
}

// Confirm dialog
function confirmAction(message, callback) {
  if (confirm(message)) {
    callback();
  }
}

// Delete confirmation
function confirmDelete(itemName, callback) {
  confirmAction(
    `Are you sure you want to delete "${itemName}"? This action cannot be undone.`,
    callback
  );
}

// Format date
function formatDate(dateString) {
  const date = new Date(dateString);
  return date.toLocaleDateString("en-US", {
    year: "numeric",
    month: "short",
    day: "numeric",
  });
}

// Format date and time
function formatDateTime(dateString) {
  const date = new Date(dateString);
  return date.toLocaleString("en-US", {
    year: "numeric",
    month: "short",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

// Format file size
function formatFileSize(bytes) {
  if (bytes === 0) return "0 Bytes";

  const k = 1024;
  const sizes = ["Bytes", "KB", "MB", "GB"];
  const i = Math.floor(Math.log(bytes) / Math.log(k));

  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + " " + sizes[i];
}

// Get score class based on score value
function getScoreClass(score) {
  if (score >= 90) return "score-excellent";
  if (score >= 80) return "score-good";
  if (score >= 60) return "score-average";
  return "score-poor";
}

// Get status class based on status value
function getStatusClass(status) {
  switch (status.toLowerCase()) {
    case "active":
    case "completed":
    case "approved":
      return "status-active";
    case "inactive":
    case "cancelled":
    case "rejected":
      return "status-inactive";
    case "pending":
    case "processing":
      return "status-pending";
    case "error":
    case "failed":
      return "status-error";
    default:
      return "status-inactive";
  }
}

// AJAX helper functions
async function apiCall(url, options = {}) {
  try {
    showLoading();
    const response = await fetch(url, {
      headers: {
        "Content-Type": "application/json",
        ...options.headers,
      },
      ...options,
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    return await response.json();
  } catch (error) {
    console.error("API call failed:", error);
    showAlert("An error occurred while processing your request.", "danger");
    throw error;
  } finally {
    hideLoading();
  }
}

// Export functions
function exportToCSV(tableId, filename = "export.csv") {
  const table = document.getElementById(tableId);
  if (!table) return;

  const rows = Array.from(table.querySelectorAll("tr"));
  const csvContent = rows
    .map((row) => {
      const cells = Array.from(row.querySelectorAll("th, td"));
      return cells.map((cell) => `"${cell.textContent.trim()}"`).join(",");
    })
    .join("\n");

  const blob = new Blob([csvContent], { type: "text/csv" });
  const url = window.URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = filename;
  a.click();
  window.URL.revokeObjectURL(url);
}

// Print function
function printTable(tableId) {
  const table = document.getElementById(tableId);
  if (!table) return;

  const printWindow = window.open("", "_blank");
  printWindow.document.write(`
        <html>
            <head>
                <title>Print</title>
                <style>
                    body { font-family: Arial, sans-serif; }
                    table { border-collapse: collapse; width: 100%; }
                    th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
                    th { background-color: #f2f2f2; }
                </style>
            </head>
            <body>
                ${table.outerHTML}
            </body>
        </html>
    `);
  printWindow.document.close();
  printWindow.print();
}

// Utility functions
function debounce(func, wait) {
  let timeout;
  return function executedFunction(...args) {
    const later = () => {
      clearTimeout(timeout);
      func(...args);
    };
    clearTimeout(timeout);
    timeout = setTimeout(later, wait);
  };
}

function throttle(func, limit) {
  let inThrottle;
  return function () {
    const args = arguments;
    const context = this;
    if (!inThrottle) {
      func.apply(context, args);
      inThrottle = true;
      setTimeout(() => (inThrottle = false), limit);
    }
  };
}
