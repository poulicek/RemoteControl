var SCAN_MODE = false;

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

            case 'grip':
                bindGripEvents(el);
                break;

            default:
                bindDefaultEvents(el);
                break;
        }
    }
};


function bindDefaultEvents(el) {
    el.ontouchend = el.onmouseup = el.onclick;
    el.ontouchstart = el.onmousedown = initPress;
    el.ontouchcancel = el.onclick = el.onmouseout = cancelPress;
    el.ontouchmove = function (e) {
        if (!isTouched(e))
            return cancelPress(e);
    };
};


// binds the link events
function bindLinkEvents(el) {
    el.ontouchstart = el.onmousedown = initPress;
    el.ontouchcancel = el.onmouseup = el.onmouseout = cancelPress;
    el.ontouchend = function (e) { document.location.href = e.currentTarget.href; cancelPress(e); };
};


// binds the down events
function bindDownEvents(el) {
    el.ontouchstart = el.onmousedown = sendKeyDown;
    el.ontouchend = el.ontouchcancel = el.onclick = el.onmouseup = el.onmouseout = sendKeyUp;
    el.ontouchmove = function(e) {
        if (!isTouched(e))
            return sendKeyUp(e);
    };
};


// binds the press events
function bindPressEvents(el) {
    el.ontouchstart = el.onmousedown = sendKeyPress;
    el.ontouchend = el.ontouchcancel = el.onclick = el.onmouseup = el.onmouseout = cancelPress;
    el.ontouchmove = function(e) {
        if (!isTouched(e))
            return cancelPress(e);
    };
};


// binds the click events
function bindClickEvents(el) {
    el.ontouchend = el.onmouseup = sendClick;
    el.ontouchstart = el.onmousedown = initPress;
    el.ontouchcancel = el.onclick = el.onmouseout = cancelPress;
    el.ontouchmove = function(e) {
        if (!isTouched(e))
            return cancelPress(e);
    };
};

// binds the grip events
function bindGripEvents(el) {
    el.ontouchstart = el.onmousedown = initPress;
    el.ontouchend = el.onclick = el.onmouseup = el.onmouseout = sendKeyUp;
    el.ontouchmove = sendTouchCoords;
};


// switches the scan mode
function switchScanMode(el) {
    SCAN_MODE = !SCAN_MODE;
    setClass(el, 'active', SCAN_MODE);
    setClass(el, 'selected', SCAN_MODE);
    return false;
};


// constructs the url
function getUrl(url, params) {
    if (url.includes('&a='))
        url = url.replace('&a=', '&a=' + (SCAN_MODE ? 1 : 0));
    return url + params;
};


// performs the click event
function sendClick(e) {
    if (e.currentTarget.pressedEvent)
        sendRequest(getUrl(e.currentTarget.href, '&s=1'));
    return cancelPress(e);
};


// ends the press
function sendKeyUp(e) {
    if (e.currentTarget.pressedEvent)
        sendRequest(getUrl(e.currentTarget.href, '&s=0'));
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
    sendRequest(getUrl(e.currentTarget.href, '&s=1'));
    return false;
};


// sends the touch coordinates
function sendTouchCoords(e) {
    var touch = getTouch(e, 10);
    if (!touch)
        return false;

    var rect = e.currentTarget.getBoundingClientRect();
    var ctrX = rect.left + rect.width / 2;
    var ctrY = rect.top + rect.height / 2;

    var s = Math.sqrt(Math.pow(rect.width / 2, 2) / 2);
    var x = (touch.pageX - ctrX) / s;
    var y = (ctrY - touch.pageY) / s;
    var deadZone = 0.2;

    if (Math.abs(x) < deadZone && Math.abs(y) < deadZone) {
        x = 0;
        y = 0;
    }

    if (e.currentTarget.lastX != x || e.currentTarget.lastY != y)
        sendRequest(getUrl(e.currentTarget.href, '&s=1&o=' + x.toFixed(2) + ',' + y.toFixed(2)));

    e.currentTarget.lastX = x;
    e.currentTarget.lastY = y;
    return false;
};


// keeps sending the request
function keepPressing(el, applyDelay) {
    if (el.pressedEvent) {
        if (!el.xhttp || el.xhttp.readyState == 4)
            el.xhttp = sendRequest(getUrl(el.href, '&s=1'));
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


// returns a touch object if the minimum distance from previous event is exceeded
function getTouch(e, minDistance) {
    if (!e.touches || e.touches.length == 0)
        return false;

    var curTouch = null;
    var lastTouch = e.currentTarget.lastTouch;

    // finding the related touch
    for (var i = 0; i < e.touches.length; i++)
        if (e.touches[i].target == e.currentTarget)
            curTouch = e.touches[i];

    // application of deadzone (minimum distance)
    if (curTouch && minDistance && lastTouch) {        
        var dist = Math.sqrt(Math.pow(Math.abs(curTouch.pageX - lastTouch.pageX), 2) + Math.pow(Math.abs(curTouch.pageY - lastTouch.pageY), 2));
        if (dist < minDistance)
            return null;
    }

    e.currentTarget.lastTouch = curTouch;
    return curTouch;
};


// returns true or false if the element is touched
function isTouched(e) {
    var touch = getTouch(e);
    if (!touch)
        return false;

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
function setAppStatus(className, errorText) {
    document.getElementById('app-state').className = className ? className : '';

    // showing of log message
    if (DEBUG_MODE)
        document.getElementById('status-error-text').innerText = errorText ? errorText.trim() : '';
};


// sets the status
function setStatusText(statusText) {
    document.getElementById('status-normal').innerText = statusText ? statusText : '';
    setAppStatus();
};


// prevents the double-tap zoom on Safari
function preventDoubleTap() {
    var els = document.getElementsByTagName('*');
    for (var i = 0; i < els.length; i++)
        els[i].ontouchstart = function (e) { e.preventDefault(); };
};