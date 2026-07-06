const fs = require('fs');
const path = require('path');

const srcPath = 'e:\\\\VGN_CRM_360_NEW\\\\VGN_360\\\\Content\\\\JS\\\\ApplicationJs\\\\CRMExecutiveDashboard.js';
const destPath = 'c:\\\\Users\\\\VGN\\\\Desktop\\\\New folder\\\\VGN_CRM_CORE\\\\VGN_CRM_CORE\\\\wwwroot\\\\js\\\\CrmExecutiveDashboard.js';

let content = fs.readFileSync(srcPath, 'utf8');

const functionsToExtract = [
    'dataGridCurrentDateExtension',
    'load_CurrentDatEextension',
    'dataGridPreDatEextension',
    'load_PreDatEextension',
    'dataGridPendingWelcomeMail',
    'load_WelcomeMail',
    'dataGridBookingToReg',
    'load_BookingToReg',
    'dataGridCustomerExtension',
    'load_CustomerEextension',
    'dataGridsaleAgreement',
    'load_AgreementDeedNotification'
];

let outJs = "$(document).ready(function () {\\n" +
    "    var role = (window.UserRole || '').toUpperCase();\\n" +
    "    var isExecutive = role.indexOf('CRM_EXECUTIVE') !== -1 || role.indexOf('EXECUTIVE') !== -1;\\n" +
    "    if (isExecutive) {\\n" +
    "        $('#crm_executive_grids').show();\\n" +
    "        load_CurrentDatEextension();\\n" +
    "        load_PreDatEextension();\\n" +
    "        load_WelcomeMail();\\n" +
    "        load_BookingToReg();\\n" +
    "    } else {\\n" +
    "        $('#crm_head_grids').show();\\n" +
    "        load_CustomerEextension();\\n" +
    "        load_AgreementDeedNotification();\\n" +
    "    }\\n" +
    "});\\n\\n" +
    "function safeJson(data) { try { return typeof data === 'string' ? JSON.parse(data) : data; } catch(e) { return data; } }\\n\\n" +
    "function redirect_to_remarkes_page(data) { console.log('Redirecting to remarks', data); }\\n\\n";

for (let fnName of functionsToExtract) {
    const fnStart = content.indexOf('function ' + fnName);
    if (fnStart === -1) {
        console.log('Function ' + fnName + ' not found');
        continue;
    }
    
    let openBraces = 0;
    let i = fnStart;
    let foundFirstBrace = false;
    
    while (i < content.length) {
        if (content[i] === '{') {
            openBraces++;
            foundFirstBrace = true;
        } else if (content[i] === '}') {
            openBraces--;
        }
        
        i++;
        
        if (foundFirstBrace && openBraces === 0) {
            break;
        }
    }
    
    let fnBody = content.substring(fnStart, i);
    fnBody = fnBody.split('/CRMExecutiveDashboard/').join('/GeneralDashboard/');
    
    fnBody = fnBody.split('#gridContainer4').join('#gridContainerCustomerFollowup');
    fnBody = fnBody.split('#gridContainer5').join('#gridContainerSaleDeed');
    fnBody = fnBody.split('#gridContainer1').join('#gridContainerPreDateExtension');
    fnBody = fnBody.split('#gridContainer2').join('#gridContainerWelcomeMailPending');
    fnBody = fnBody.replace(/#gridContainer\\b/g, '#gridContainerCurrentDateExtension');
    
    fnBody = fnBody.split('JSON.parse(data)').join('safeJson(data)');
    
    outJs += fnBody + "\\n\\n";
}

fs.writeFileSync(destPath, outJs);
console.log('Successfully wrote to ' + destPath);
