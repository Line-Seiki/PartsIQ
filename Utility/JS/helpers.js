﻿// HELPER get certain color according to data priority level
function getColorAccordingToPriorityLevel(priorityID, isUrgent, statusID) {
    if (isUrgent) return 'urgent';
    if (statusID !== 1) switch (statusID) {
        case 5:
            return 'priority-completed';
        case 4:
            return 'priority-paused';
        case 3:
            return 'priority-started';
        default:
            return 'priority-reserved';
    }
    switch (priorityID) {
        case 1:
            return 'low-priority';
        case 2:
            return 'normal-priority';
        case 3:
            return 'high-priority';
        default:
            return '';
    }
}

// HELPER convert date to date string
function convertDateStringToFormattedString(date) {
    let dateObj;
    if (typeof date === 'string' && date !== null) dateObj = date.substring(6, date.length - 2);
    else return '';
    dateObj = new Date(parseInt(dateObj))
    const year = dateObj.getFullYear();
    const month = String(dateObj.getMonth() + 1).padStart(2, '0');
    const day = String(dateObj.getDate()).padStart(2, '0');

    return `${year}-${month}-${day}`;
}