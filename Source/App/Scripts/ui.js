var SCAN_MODE = 0;
var ERROR_ID = 0;

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

            case 'script':
                bindScriptEvents(el);
                break;

            case 'panzoom':
                bindPanZoomEvents(el);
                break;

            case 'js':
                bindJsEvents(el);
                break;

            default:
                bindDefaultEvents(el);
                break;
        }
    }
};


// binds default events for the link
function bindDefaultEvents(el) {
    el.ontouchend = el.onmouseup = function (e) { window.location.href = el.href; cancelPress(e); };
    el.ontouchstart = el.onmousedown = initPress;
    el.ontouchcancel = el.onclick = el.onmouseout = cancelPress;
    el.ontouchmove = function (e) {
        if (!isTouched(e))
            return cancelPress(e);
    };
};


// binds javascript events
function bindJsEvents(el) {
    el.ontouchstart = el.onmousedown = function(e) { e.passThrough = true; initPress(e); };
    el.ontouchend = el.onmouseup = function(e) { cancelPress(e); };
    el.ontouchcancel = el.onmouseout = cancelPress;
    el.ontouchmove = function (e) {
        if (!isTouched(e))
            return cancelPress(e);
    };

};


// binds the pan-zoom events
function bindPanZoomEvents(el) {
    enableRemoteControl(el);
};


// binds the script events
function bindScriptEvents(el) {
    el.ontouchend = el.onmouseup = el.onclick;
    el.ontouchstart = el.onmousedown = initPress;
    el.ontouchcancel = el.onclick = el.onmouseout = cancelPress;
    el.ontouchmove = function (e) {
        if (!isTouched(e))
            return cancelPress(e);
    };
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
    el.classList.toggle('active', SCAN_MODE);
    el.classList.toggle('selected', SCAN_MODE);
    return false;
};


// constructs the url
function getUrl(url, params) {
    if (url.includes('&a='))
        url = url.replace('&a=', '&a=' + (SCAN_MODE && isLandScape() ? 1 : 0));
    return url + (params ? params : '');
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
    e.currentTarget.classList.toggle('active', true);
    e.currentTarget.pressedEvent = e;
    return false;
};


// cancels the button press
function cancelPress(e) {
    e.currentTarget.classList.toggle('active', false);
    e.currentTarget.pressedEvent = null;
    return false;
};


// returns a touch object if the minimum distance from previous event is exceeded
function getTouch(e, minDistance) {
    if (!e.touches || e.touches.length == 0)
        return null;

    var curTouch = null;
    var lastTouch = e.currentTarget.lastTouch;

    // finding the related touch
    for (var i = 0; i < e.touches.length; i++) {
        var touchTarget = e.touches[i].target;
        if (touchTarget == e.currentTarget || e.currentTarget.contains(touchTarget))
            curTouch = e.touches[i];
    }

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


// connection status
function setAppStatus(className, errorText) {

    if (!className) {
        className = '';
        ERROR_ID = 0;
    }

    // setting the class name if changed
    var el = document.getElementById('app-state');
    if (el.className != className)
        el.className = className;

    // showing of log message
    if (DEBUG_MODE) {

        errorText = errorText ? errorText.trim() : '';
        el = document.getElementById('status-error-text');

        if (el.innerText != errorText)
            el.innerText = errorText;
    }
};


// sets the error status with timeout
function setError(errorText, permanent) {    

    if (permanent) {
        ERROR_ID = -1;
        setAppStatus('status-error', errorText);
    }
    else if (ERROR_ID != -1) {

        var errorId = ERROR_ID = getSessionId();
        setAppStatus('status-error', errorText);

        setTimeout(function () {
            if (ERROR_ID == errorId)
                setAppStatus();
        }, 1000);
    }
}


// sets the status
function setStatusText(statusText) {
    document.getElementById('status-normal').innerText = statusText ? statusText : '';
    setAppStatus();
};


// prevents the double-tap zoom on Safari
function preventDoubleTap() {
    var els = document.getElementsByTagName('*');
    for (var i = 0; i < els.length; i++)
        els[i].ontouchstart = function (e) { if (!e.passThrough) { e.preventDefault(); return false; } };
};


// focuses the input causing appearance of the virtual keyboard
function focusKeyboard() {
    var f = document.getElementById('keyboard');
    f.focus();
};


// handles the key change event
function onKeyChanged(e) {
    try {
        var value = e.currentTarget.value;
        e.currentTarget.value = '  '; // these whitespaces ensure the holding of the backspace repeates the key strokes

        var h = value.length > 2 ? encodeURIComponent(value.substr(2, 1)) : '';
        sendRequest('?c=key&v=' + e.keyCode + '&h=' + h);
    }
    catch (e) { }
};