﻿// sends an ajax request
function sendRequest(query, onSuccess, onError, timeout) {
    try {
        var xhttp = new XMLHttpRequest();
        document.xhttp = xhttp;

        xhttp.query = query;
        xhttp.onreadystatechange = function () { handleResponse(this, onSuccess, onError) };
        xhttp.open('GET', query, true);
        xhttp.timeout = timeout ? timeout : 500;
        xhttp.send();

        return xhttp;
    }
    catch (e) {
        if (onError)
            onError(e.toString());
	}
};


// handles the ajax response
function handleResponse(xhttp, onSuccess, onError) {
    if (xhttp.readyState == 4) {
        if (xhttp.status == 200) {
            setAppStatus();

            if (onSuccess)
                onSuccess(xhttp.responseText);
        }
        else {
            var errorText = xhttp.status == 0
                ? "Server unrecheable: " + xhttp.query
                : (xhttp.responseText ? xhttp.responseText : xhttp.status);

            if (onError)
                onError(errorText);
            else {
                setAppStatus('status-error');
                setTimeout(function () {
                    if (document.xhttp == xhttp)
                        setAppStatus();
                }, 1000);
            }
        }
    }
};