﻿// sends an ajax request
function sendRequest(query, callback) {
    
    var xhttp = new XMLHttpRequest();
    document.xhttp = xhttp;

    xhttp.onreadystatechange = function () { handleResponse(this, callback) };
    xhttp.open("GET", query, true);
    xhttp.timeout = 500;
    xhttp.send();

    return xhttp;
};


// handles the ajax response
function handleResponse(xhttp, callback) {
    if (xhttp.readyState == 4) {
        if (xhttp.status == 200) {
            setStatus('', this);
            if (callback)
                callback(xhttp.responseText);
        }
        else {
            setStatus("request-error", xhttp);
            setTimeout(setStatus, 1000, '', xhttp);
        }
    }
};


// clears the status
function setStatus(status, xhttp) {
    if (!xhttp || xhttp == document.xhttp)
        document.body.className = status;
};