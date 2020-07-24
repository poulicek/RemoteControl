var APP_URL = document.getElementById('app-url').content;
var APP_VERSION = document.getElementById('app-version').content;

window.onload = onLoad;
window.onhashchange = onHashChange;


// handles the load event
function onLoad() {
    bindEvents();
    preventDoubleTap();
    init();
};


// initializes the application
function init() {
    console.log('Initializing...');
    connect(APP_URL);
};


// handles the change of the hash in the url
function onHashChange() {
    var view = window.location.hash.length > 1 ? window.location.hash.substring(1) : 'media';
    loadView(view);
};


// sets the base url
function setBaseUrl(url) {
    document.getElementById('query-base').href = url ? url : '';
};


// checks the app version
function connect(url) {
    setBaseUrl(url);
    setStatus('Connecting...');

    console.log('Connecting to url: ' + url);
    sendRequest('?c=app&v=getversion', function (data) { onConnectSuccess(data, url); }, function (data) { onConnectError(data, url); });
};


// handles the positive response
function onConnectSuccess(data) {
    console.log('Connected: ' + data);

    var parts = data.split(',');
    if (APP_VERSION == parts[0]) {
        setStatus(parts[1]);
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

    if (url.length)
        connect('');
    else
        setConnStatus('status-error');
        
};


// loads a view with given id
function loadView(id) {
    console.log('Loading view: ' + id);
    sendRequest('?c=view&v=' + id + '&_' + APP_VERSION, onLoadViewSuccess, onLoadViewError, 1000);
};


// sets the view with the given html
function onLoadViewSuccess(html) {
    var viewEl = document.getElementById('view');
    viewEl.style.animationName = '';
    void viewEl.offsetWidth;

    viewEl.innerHTML = html;
    viewEl.style.animationName = 'fadeIn';
    bindLinkEvents();
};


// handles error during view loading
function onLoadViewError(error) {
    console.log('View loading error: ' + error);
    setConnStatus('status-error');
};