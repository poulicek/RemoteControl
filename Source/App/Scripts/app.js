window.onload = init;


// initalizes the application
function init() {
    updateBaseFromHash();
    bindLinkEvents();
};


// updates the base url from the hash tag
function updateBaseFromHash() {
    if (window.location.hash.length > 1)
        document.getElementById('query-base').href = window.location.hash.substring(1);
};