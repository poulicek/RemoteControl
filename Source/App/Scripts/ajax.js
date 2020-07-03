﻿// sends an ajax request
function sendRequest(query) {
    query = this.redirectToHash(query);

    var xhttp = new XMLHttpRequest();
    document.xhttp = xhttp;

    xhttp.onreadystatechange = handleResponse;    
    xhttp.open("GET", query + "&" + Math.random(), true);
    xhttp.timeout = 500;
    xhttp.send();

    return xhttp;
};


// handles the ajax response
function handleResponse(xhttp) {
    if (xhttp.readyState == 4) {
        if (xhttp.status == 200)
            setStatus(null, this);
        else {
            setStatus("request-error", this);
            setTimeout(setStatus, 1000, null, this);
        }
    }
};

// redirects the query to address provided in hash
function redirectToHash(query) {
    return window.location.hash.length > 1
        ? query = window.location.hash.substring(1) + '?' + query.split('?')[1]
        : query;
};

// clears the status
function setStatus(status, xhttp) {
    if (!xhttp || xhttp == document.xhttp)
        document.body.className = status;
};