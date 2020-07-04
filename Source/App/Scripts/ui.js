// binds the events on links
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


// binds the press events
function bindPressEvents(el) {
    el.onclick = el.ontouchend = el.onmouseout = el.ontouchcancel = function () {
        setClass(this, 'active', false);
        this.pressedEvent = null;
        return false;
    };
    el.ontouchstart = el.onmousedown = function (e) {
        setClass(this, 'active', true);
        this.xhttp = null;
        this.pressedEvent = e;
        keepSending(this, e, true);
        return false;
    };
};


// binds the click events
function bindClickEvents(el) {
    el.onclick = el.onmouseout = el.ontouchcancel = function () {
        setClass(this, 'active', false);
        return false;
    };
    el.ontouchstart = el.onmousedown = function () {
        setClass(this, 'active', true);
        return false;
    };
    el.ontouchend = el.onmouseup = function () {
        sendRequest(this.href);
        setClass(this, 'active', false);
        return false;
    };
};


// keeps sending the request
function keepSending(el, e, applyDelay) {
    if (el.pressedEvent == e) {
        if (!el.xhttp || el.xhttp.readyState == 4)
            el.xhttp = sendRequest(el.href);
        setTimeout(keepSending, applyDelay ? 500 : 31, el, e);
    }
};


// sets the class to the given element
function setClass(el, className, set) {
    el.className = el.className.replace(className, null);
    if (set)
        el.className += ' ' + className;
};