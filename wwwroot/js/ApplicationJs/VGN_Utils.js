/**
 * VGN_Utils.js
 * ============
 * Global utility / helper functions shared across all VGN CRM pages.
 * Include this file BEFORE any page-specific JS files that rely on these helpers.
 *
 * Usage:
 *   <script src="~/js/ApplicationJs/VGN_Utils.js"></script>
 *
 * Available helpers:
 *   - formatIndianNumber(num, decimals)   → formats a number with Indian comma style (e.g. 12,34,567)
 *   - parseIndianNumber(str)              → strips Indian commas and returns a plain float
 */

// ─────────────────────────────────────────────────────────────────────────────
// formatIndianNumber
// ─────────────────────────────────────────────────────────────────────────────
/**
 * Formats a number using the Indian numbering system (lakh / crore grouping).
 *
 * Examples:
 *   formatIndianNumber(1234567)        → "12,34,567"
 *   formatIndianNumber(1234567.89, 2)  → "12,34,567.89"
 *   formatIndianNumber("1,234,567")    → "12,34,567"  (strips existing commas first)
 *   formatIndianNumber(null)           → ""
 *   formatIndianNumber(NaN)            → ""
 *
 * @param  {number|string} num       - The value to format. Strings are parsed automatically.
 * @param  {number}        [decimals=0] - Number of decimal places (0 = whole number).
 * @returns {string}  Formatted string, e.g. "12,34,567" or "12,34,567.50"
 */
function formatIndianNumber(num, decimals) {

    // Default to 0 decimal places
    var decimalPlaces = (typeof decimals === 'number') ? decimals : 0;

    // Handle null / undefined → empty string
    if (num === null || num === undefined || num === '') {
        return '';
    }

    // If it's a string, strip any existing commas and try to parse it
    if (typeof num === 'string') {
        num = parseFloat(num.replace(/,/g, ''));
    }

    // If not a valid number, return empty string
    if (isNaN(num)) {
        return '';
    }

    return num.toLocaleString('en-IN', {
        minimumFractionDigits: decimalPlaces,
        maximumFractionDigits: decimalPlaces
    });
}


// ─────────────────────────────────────────────────────────────────────────────
// parseIndianNumber
// ─────────────────────────────────────────────────────────────────────────────
/**
 * Strips Indian/Western commas and returns a plain JavaScript float.
 * Useful before doing arithmetic on a value that was displayed with formatIndianNumber.
 *
 * @param  {string|number} str - The formatted value, e.g. "12,34,567.50"
 * @returns {number} Plain float, e.g. 1234567.5  (returns NaN if unparseable)
 */
function parseIndianNumber(str) {
    if (str === null || str === undefined || str === '') return 0;
    if (typeof str === 'number') return str;
    return parseFloat(str.toString().replace(/,/g, ''));
}
