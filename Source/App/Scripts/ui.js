// binds the events on links
function bindEvents() {
    var els = document.getElementsByTagName('a');
    for (var i = 0; i < els.length; i++) {
        var el = els[i];
        switch (el.rel) {

            case 'link':
                bindLinkEvents(el);
                break;

            case 'down':
                bindDownEvents(el);
                break;

            case 'press':
                bindPressEvents(el);
                break;

            case 'click':
                bindClickEvents(el);
                break;
        }
    }
};

// binds the link events
function bindLinkEvents(el) {
    el.ontouchstart = el.onmousedown = function(e) { initPress(e); };
    el.ontouchend = el.ontouchcancel = el.onclick = el.onmouseout = function(e) { cancelPress(e); };
};


// binds the down events
function bindDownEvents(el) {
    el.ontouchstart = el.onmousedown = sendKeyDown;
    el.ontouchend = el.ontouchcancel = el.onclick = el.onmouseout = sendKeyUp;
    el.ontouchmove = function(e) {
        if (!isTouched(e))
            return sendKeyUp(e);
    };
};


// binds the press events
function bindPressEvents(el) {
    el.ontouchstart = el.onmousedown = sendKeyPress;
    el.ontouchend = el.ontouchcancel = el.onclick = el.onmouseout = cancelPress;
    el.ontouchmove = function(e) {
        if (!isTouched(e))
            return cancelPress(e);
    };
};


// binds the click events
function bindClickEvents(el) {
    el.ontouchend = el.onmouseup = sendClick;
    el.ontouchstart = el.onmousedown = initPress;
    el.ontouchcancel = el.onclick = el.onmouseout = cancelPress
    el.ontouchmove = function(e) {
        if (!isTouched(e))
            return cancelPress(e);
    };
};


// performs the click event
function sendClick(e) {
    if (e.currentTarget.pressedEvent)
        sendRequest(e.currentTarget.href + '&s=1');
    return cancelPress(e);
};


// ends the press
function sendKeyUp(e) {
    if (e.currentTarget.pressedEvent)
        sendRequest(e.currentTarget.href + '&s=0');
    return cancelPress(e);
};


// performs the button pressing
function sendKeyPress(e) {
    e.currentTarget.xhttp = null;
    initPress(e);
    keepPressing(e.currentTarget, true);
    return false;
};


// starts the button press
function sendKeyDown(e) {
    initPress(e);
    sendRequest(e.currentTarget.href + '&s=1');
    return false;
};


// keeps sending the request
function keepPressing(el, applyDelay) {
    if (el.pressedEvent) {
        if (!el.xhttp || el.xhttp.readyState == 4)
            el.xhttp = sendRequest(el.href + '&s=1');
        setTimeout(keepPressing, applyDelay ? 500 : 31, el);
    }
};


// initializes the button press
function initPress(e) {
    setClass(e.currentTarget, 'active', true);
    e.currentTarget.pressedEvent = e;
    return false;
};


// cancels the button press
function cancelPress(e) {
    setClass(e.currentTarget, 'active', false);
    e.currentTarget.pressedEvent = null;
    return false;
};


// returns true or false if the element is touched
function isTouched(e) {
    if (!e.touches || e.touches.length == 0)
        return null;

    var touch = e.touches[0];
    var el = document.elementFromPoint(touch.pageX, touch.pageY);
    return e.currentTarget == el || e.currentTarget.contains(el);
};


// sets the class to the given element
function setClass(el, className, set) {
    if (set)
        el.classList.add(className);
    else
        el.classList.remove(className);
};


// connection status
function setConnStatus(className) {
    document.body.className = className ? className : '';
};


// sets the status
function setStatus(statusText) {
    document.getElementById('status-normal').innerText = statusText ? statusText : '';
    setConnStatus();
};


// prevents the double-tap zoom on Safari
function preventDoubleTap() {
    var els = document.getElementsByTagName('*');
    for (var i = 0; i < els.length; i++)
        els[i].ontouchstart = function (e) { e.preventDefault(); };
};