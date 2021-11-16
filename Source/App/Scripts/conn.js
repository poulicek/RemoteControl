REQUEST_ID = 0;


// sends an ajax request
function sendRequest(query, onSuccess, onError, timeout) {
    try {
        setLoading();

        var xhttp = new XMLHttpRequest();
        xhttp.requestid = ++REQUEST_ID;
        xhttp.query = query;
        xhttp.onreadystatechange = function () { handleResponse(this, onSuccess, onError) };
        xhttp.open('GET', query, true);
        xhttp.timeout = timeout ? timeout : 2000;
        xhttp.send();

        return xhttp;
    }
    catch (e) {
        raiseError(e.toString(), onError);
	}
};


// handles the ajax response
function handleResponse(xhttp, onSuccess, onError) {
    if (xhttp.readyState == 4) {
        if (xhttp.status == 200) {

            setTimeout(function () {
                if (REQUEST_ID == xhttp.requestid)
                    setAppStatus();
            }, 200);

            if (onSuccess)
                onSuccess(xhttp.responseText);
        }
        else {
            var errorText = xhttp.status == 0
                ? "Server unrecheable: " + xhttp.query
                : (xhttp.responseText ? xhttp.responseText : xhttp.status);

            raiseError(errorText, onError);
        }
    }
};


// raises an error state
function raiseError(errorText, onError) {

    if (onError)
        onError(errorText);
    else
        setError(errorText);
};