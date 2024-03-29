﻿var APP_URL = document.getElementById('app-url').content;
var APP_VERSION = document.getElementById('app-version').content;
var APP_TIMESTAMP = document.getElementById('app-timestamp').content;
var DEFAULT_VIEW = 'main';
var CURRENT_VIEW = '';
var DEBUG_MODE = false;
var DEFAULT_HEIGHT = 0;
var LANDSCAPE_EL = null;
var INSTALLED = isAppInstalled();

window.onload = onLoad;
window.onfocus = compensatePadding;
window.onhashchange = onHashChange
window.onorientationchange = compensatePadding;



// handles the load event
function onLoad() {

    LANDSCAPE_EL = document.getElementById('landscape');

    compensatePadding();
    initGuides();
    preventDoubleTap();
    bindEvents();
    init();
};


// compensates iOS bug with screen height after rotation
function compensatePadding() {

    if (window.innerHeight < window.innerWidth) {
        document.body.style.top = '0px';
        document.body.style.height = '100%';
    }
    else {
        if (!DEFAULT_HEIGHT) {
            DEFAULT_HEIGHT = document.body.offsetHeight;
        }

        document.body.style.top = (document.body.offsetHeight - DEFAULT_HEIGHT) + 'px';
        document.body.style.height = DEFAULT_HEIGHT + 'px';
    }
};


// initializes the application
function init() {
    console.log('Initializing...');
    setStatusText();
    connect(APP_URL);
};


// handles the change of the hash in the url
function onHashChange() {
    var view = window.location.hash.length > 1 ? window.location.hash.substring(1) : DEFAULT_VIEW;

    if (view != CURRENT_VIEW)
        loadView(view);
};


// sets the base url
function setBaseUrl(url) {
    document.getElementById('query-base').href = url ? url : '';
};


// checks the app version
function connect(url) {
    setAppStatus();
    setBaseUrl(url);
    console.log('Connecting to url: ' + url);
    sendRequest(url + '?c=app&v=getversion', function (data) { onConnectSuccess(data, url); }, function (data) { onConnectError(data, url); });
};


// handles the positive response
function onConnectSuccess(data) {
    console.log('Connected: ' + data);

    var parts = data.split(',');
    DEBUG_MODE = parts.length > 2 && parts[2] == 'debug';

    if (APP_VERSION == parts[0]) {
        setStatusText(parts[1]);
        onHashChange();
    }
    else {
        console.log('Loading new version');
        document.location.reload(true);
    }   
};


// handles the negative response
function onConnectError(error, url) {
    console.log('Connection error: ' + error);

    if (url && url.length)
        connect('');
    else
        setError(error, true);
};


// loads a view with given id
function loadView(id) {
    console.log('Loading view: ' + id);
    sendRequest('?c=view&v=' + id + '&_' + APP_VERSION, function (r) { onLoadViewSuccess(id, r); }, onLoadViewError);
};


// sets the view with the given html
function onLoadViewSuccess(id, html) {

    CURRENT_VIEW = id;

    setAppStatus();
    var viewEl = document.getElementById('view');
    viewEl.style.animationName = '';
    void viewEl.offsetWidth;

    viewEl.innerHTML = html;
    viewEl.style.animationName = 'fadeIn';
    bindEvents();
};


// handles error during view loading
function onLoadViewError(error) {

    console.log('View loading error: ' + error);
    setError(error);
    window.location.hash = CURRENT_VIEW;
};


// copies the text into clip board
function copyText(str) {
    var el = document.createElement('textarea');
    el.style.position = 'absolute';
    el.style.opacity = '0';
    el.value = str;

    document.body.appendChild(el);
    el.select();
    document.execCommand('copy');
    document.body.removeChild(el);
    
};


// generates a session id
function getSessionId() {
    return Math.floor(1296 * Math.random()).toString(36);
};


// returns true if the device is in landscape mode
function isLandScape() {
    return LANDSCAPE_EL && window.getComputedStyle(LANDSCAPE_EL).visibility == 'visible';
};


// returns true if the app is installed as a PWA
function isAppInstalled() {
    return window.navigator.standalone === true || window.matchMedia('(display-mode: standalone)').matches;
};