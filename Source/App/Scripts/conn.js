// sends an ajax request
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
function handleResponse(xhttp, onSuccess, onError) {
    if (xhttp.readyState == 4) {
        if (xhttp.status == 200) {
            setConnStatus('', this);

            if (onSuccess)
                onSuccess(xhttp.responseText);
        }
        else {
            if (onError)
                onError(xhttp.responseText);
            else {
                setConnStatus("status-error", xhttp);
                setTimeout(setStatus, 1000, '', xhttp);
            }
        }
    }
};