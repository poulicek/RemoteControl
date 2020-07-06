window.onload = init;

// initalizes the application
function init() {
    connect(getUrl());
    bindLinkEvents();
};


// checks the app version
function connect(url) {
    setBaseUrl(url);
    setStatus('connecting...');

    console.log('Connecting to url: ' + url);
    sendRequest("?c=app&v=getversion", onSuccess, onError);
};


// handles the positive response
function onSuccess(data) {
    console.log('Connected: ' + data);

    var parts = data.split(',');
    if (parts[0] == document.getElementById('app-version').content)
        setStatus(parts[1]);
    else {
        console.log('Loading new version...');
        document.location.reload(true);
    }   
};


// handles the negative response
function onError() {
    if (document.getElementById('query-base').href)
        setConnStatus("status-error");
    else
        connect('', onError);
};


// updates the base url from the hash tag
function getUrl() {
    if (window.location.hash.length > 1)
        return window.location.hash.substring(1);

    return document.getElementById('app-url').content;
};


// sets the base url
function setBaseUrl(url) {
    document.getElementById('query-base').href = url ? url : '';
};