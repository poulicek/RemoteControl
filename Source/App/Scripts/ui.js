﻿// binds the events on links
function bindLinkEvents() {
    var els = document.getElementsByTagName('a');
    for (var i = 0; i < els.length; i++) {
        var el = els[i];
        switch (el.rel) {

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
    el.ontouchend = el.ontouchcancel = el.onclick = el.onmouseout = sendKeyUp;
    el.ontouchmove = function(e) {
        if (!isTouched(e))
            return sendKeyUp(e);
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
    if (e.target.pressedEvent)
        sendRequest(e.target.href + '&s=1');
    return cancelPress(e);
};


// ends the press
function sendKeyUp(e) {
    if (e.target.pressedEvent)
        sendRequest(e.target.href + '&s=0');
    return cancelPress(e);
};


// performs the button pressing
function sendKeyPress(e) {
    e.target.xhttp = null;
    initPress(e);
    keepPressing(e, true);
    return false;
};


// starts the button press
function sendKeyDown(e) {
    initPress(e);
    sendRequest(e.target.href + '&s=1');
    return false;
};


// keeps sending the request
function keepPressing(e, applyDelay) {
    var el = e.target;
    if (el.pressedEvent == e) {
        if (!el.xhttp || el.xhttp.readyState == 4)
            el.xhttp = sendRequest(el.href + '&s=1');
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


// returns true or false if the element is touched
function isTouched(e) {
    if (!e.touches || e.touches.length == 0)
        return null;

    var touch = e.touches[0];
    return e.target == document.elementFromPoint(touch.pageX, touch.pageY);
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