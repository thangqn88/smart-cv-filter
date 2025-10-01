// Smart CV Filter - Utils JavaScript
// This file contains utility functions for the application

// Global utility functions
window.Utils = {
  // Format date
  formatDate: function (dateString) {
    if (!dateString) return "";
    const date = new Date(dateString);
    return date.toLocaleDateString("en-US", {
      year: "numeric",
      month: "short",
      day: "numeric",
    });
  },

  // Format date and time
  formatDateTime: function (dateString) {
    if (!dateString) return "";
    const date = new Date(dateString);
    return date.toLocaleString("en-US", {
      year: "numeric",
      month: "short",
      day: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  },

  // Format file size
  formatFileSize: function (bytes) {
    if (bytes === 0) return "0 Bytes";
    const k = 1024;
    const sizes = ["Bytes", "KB", "MB", "GB"];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + " " + sizes[i];
  },

  // Show alert message
  showAlert: function (message, type = "info", container = ".alert-container") {
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

    // Auto-dismiss after 5 seconds
    setTimeout(() => {
      const alertElement = document.getElementById(alertId);
      if (alertElement) {
        const bsAlert = new bootstrap.Alert(alertElement);
        bsAlert.close();
      }
    }, 5000);
  },

  // Confirm dialog
  confirm: function (message, callback) {
    if (confirm(message)) {
      callback();
    }
  },

  // Debounce function
  debounce: function (func, wait) {
    let timeout;
    return function executedFunction(...args) {
      const later = () => {
        clearTimeout(timeout);
        func(...args);
      };
      clearTimeout(timeout);
      timeout = setTimeout(later, wait);
    };
  },

  // Throttle function
  throttle: function (func, limit) {
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
  },

  // Get score class based on score value
  getScoreClass: function (score) {
    if (score >= 90) return "score-excellent";
    if (score >= 80) return "score-good";
    if (score >= 60) return "score-average";
    return "score-poor";
  },

  // Get status class based on status value
  getStatusClass: function (status) {
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
  },
};

// Log that utils.js has been loaded
console.log("Utils.js loaded successfully");
