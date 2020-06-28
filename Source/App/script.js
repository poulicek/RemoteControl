function sendRequest(query) {
    document.body.className = "requestInProgress";

    query = this.redirectToHash(query);

    var xhttp = new XMLHttpRequest();
    xhttp.onreadystatechange = function () {
        if (this.readyState == 4)
            document.body.className = this.status == 200 ? null : "requestError";
    };    
    xhttp.open("GET", query + "&" + Math.random(), true);
    xhttp.send();
    return xhttp;
}


function redirectToHash(query) {
    return window.location.hash.length > 1
        ? query = window.location.hash.substring(1) + '?' + query.split('?')[1]
        : query;
}


function bindLinkEvents() {
    var els = document.getElementsByTagName('a');
    for (var i = 0; i < els.length; i++) {
        var el = els[i];

        switch (el.rel) {
            case 'press':
                bindPressEvents(el);
                break;

            case 'click':
                bindClickEvents(el);
                break;
        }
    }
};

function bindPressEvents(el) {
    el.onclick = el.ontouchend = el.onmouseout = function () {
        this.pressedEvent = null;
        return false;
    };
    el.ontouchstart = el.onmousedown = function (e) {
        this.xhttp = null;
        this.pressedEvent = e;
        keepSending(this, e, true);
        return false;
    };
};

function bindClickEvents(el) {
    el.onclick = el.ontouchstart = el.onmousedown = function () {
        return false;
    };
    el.ontouchend = el.onmouseup = function () {
        sendRequest(this.href);
        return false;
    };
};


function keepSending(el, e, applyDelay) {
    if (el.pressedEvent == e) {
        if (!el.xhttp || el.xhttp.readyState == 4)
            el.xhttp = sendRequest(el.href);
        setTimeout(function () { keepSending(el, e); }, applyDelay ? 500 : 31);
    }
};

window.onload = bindLinkEvents;