window.onload = init;

// initalizes the application
function init() {
    updateBaseFromHash();
    checkVersion();
    bindLinkEvents();
};


// checks the app version
function checkVersion() {
    sendRequest("?command=app&value=getversion", function (v) {
        if (v != document.getElementById('app-version').content)
            window.location.reload(true);
    });
};

// updates the base url from the hash tag
function updateBaseFromHash() {
    if (window.location.hash.length > 1)
        document.getElementById('query-base').href = window.location.hash.substring(1);
};