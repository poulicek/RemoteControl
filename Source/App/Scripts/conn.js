// sends an ajax request
function sendRequest(query, onSuccess, onError, timeout) {
    try {
        var xhttp = new XMLHttpRequest();
        document.xhttp = xhttp;

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
            setConnStatus();

            if (onSuccess)
                onSuccess(xhttp.responseText);
        }
        else {
            if (onError)
                onError(xhttp.responseText ? xhttp.responseText : xhttp.status);
            else {
                setConnStatus('status-error');
                setTimeout(setConnStatus, 1000);
            }
        }
    }
};