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
    el.ontouchend = el.ontouchcancel = el.onclick = el.onmouseout = cancelPress;
    el.ontouchmove = cancelPressIfNotTouched;
    el.ontouchstart = el.onmousedown = performPress;
};


// binds the click events
function bindClickEvents(el) {
    el.ontouchstart = el.onmousedown = initPress;
    el.ontouchcancel = el.onclick = el.onmouseout = cancelPress
    el.ontouchmove = cancelPressIfNotTouched;
    el.ontouchend = el.onmouseup = performClick;
};


// performs the click event
function performClick(e) {
    if (e.target.pressedEvent == e)
        sendRequest(e.target.href);
    return cancelPress(e);
};


// performs the button pressing
function performPress(e) {
    e.target.xhttp = null;
    initPress(e);
    keepPressing(e, true);
    return false;
};


// keeps sending the request
function keepPressing(e, applyDelay) {
    var el = e.target;
    if (el.pressedEvent == e) {
        if (!el.xhttp || el.xhttp.readyState == 4)
            el.xhttp = sendRequest(el.href);
        setTimeout(keepPressing, applyDelay ? 500 : 31, e);
    }
};


// initializes the button press
function initPress(e) {
    setClass(e.target, 'active', true);
    e.target.pressedEvent = e;
    return false;
};


// cancels the button press
function cancelPress(e) {
    setClass(e.target, 'active', false);
    e.target.pressedEvent = null;
    return false;
};


// cancels the press if button is not tourched
function cancelPressIfNotTouched(e) {
    if (!isElementTouched(e))
        return cancelPress(e);
};


// returns true or false if the element is touched
function isElementTouched(e) {
    if (!e.touches || e.touches.length == 0)
        return null;

    var touch = e.touches[0];
    return e.target == document.elementFromPoint(touch.pageX, touch.pageY);
};


// sets the class to the given element
function setClass(el, className, set) {
    el.className = el.className.replace(className, null);
    if (set)
        el.className += ' ' + className;
};


// connection status
function setConnStatus(className, xhttp) {
    if (!xhttp || xhttp == document.xhttp)
        document.body.className = className ? className : '';
};


// sets the status
function setStatus(statusText) {
    document.getElementById('status-normal').innerText = statusText;
    setConnStatus();    
};