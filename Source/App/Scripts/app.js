var APP_URL = document.getElementById('app-url').content;
var APP_VERSION = document.getElementById('app-version').content;
var DEFAULT_VIEW = 'main';
var CURRENT_VIEW = '';
var DEBUG_MODE = false;
var DEFAULT_HEIGHT = 0;

window.onload = onLoad;
window.onhashchange = onHashChange;
window.onorientationchange = onOrientationChanged;


// handles the load event
function onLoad() {

    if (window.innerHeight > window.innerWidth)
        DEFAULT_HEIGHT = document.body.offsetHeight;

    preventDoubleTap();
    bindEvents();
    init();
};


// compensates iOS bug width screen height after rotation
function onOrientationChanged() {
    document.body.className =
        window.innerHeight > window.innerWidth && DEFAULT_HEIGHT && document.body.offsetHeight != DEFAULT_HEIGHT
            ? 'compensate-ios-padding'
            : null;
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
    sendRequest('?c=view&v=' + id + '&_' + APP_VERSION, function (r) { onLoadViewSuccess(id, r); }, onLoadViewError, 1000);
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
    return new Date().getTime().toString(32);
};