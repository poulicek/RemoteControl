window.onload = init;

// initalizes the application
function init() {
    updateBaseFromHash();
    checkVersion();
    bindLinkEvents();
};


// checks the app version
function checkVersion() {
    sendRequest("?c=app&v=getversion", function (v) {
        if (v != document.getElementById('app-version').content)
            document.location.href = "?v=" + v;
    });
};

// updates the base url from the hash tag
function updateBaseFromHash() {
    if (window.location.hash.length > 1)
        document.getElementById('query-base').href = window.location.hash.substring(1);
};