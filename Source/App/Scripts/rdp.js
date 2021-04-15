function enableRemoteControl(el) {

    var cutout = '';
    var img = null;
    var session = 0;
    var isEmpty = true;

    // binds the actions
    function bindActions() {
        el.onclick = function () { return false; };
        bindFirstImage();
    }


    // binds the fist found image elment
    function bindFirstImage() {
        var imgEls = el.getElementsByTagName('img');
        if (imgEls.length > 0)
            bindImage(img = imgEls[0]);
    }


    // binds the image element
    function bindImage(img) {

        if (!img.bound) {

            // enabling the pan and zoom
            enablePanZoom(img, onClick, onViewPortChanged);

            // using primarily the data-src attribute as the 'src'
            // attribute would cause automatic load even when the image isn't visible
            if (img.src)
                img.setAttribute('data-src', img.src);

            // automatic loading of the image
            window.addEventListener('resize', onResize);
            img.addEventListener('load', onLoad);
            img.addEventListener('error', onError);
            img.bound = true;
        }

        reloadImage();
    };


    // windows resize handler
    function onResize(e) {
        reloadImage();
    };


    // handles the image load event
    function onLoad(e) {

        if (isEmpty) {
            el.classList.remove('loaded');
        }
        else {
            setAppStatus();
            el.classList.add('loaded');
            reloadImage();
        }
    };


    // handles the image error event
    function onError(e) {
        setError(null, true);
        el.classList.remove('loaded');
        setTimeout(function () { reloadImage(); }, 500);
    };


    // onclick handler definition
    function onClick(e, x, y, b) {
        sendRequest(getUrl(el.href, '&x=' + x + "&y=" + y + "&b=" + (b ? b : '')));
        reloadImage();
        showTouchEffect(document.getElementById('click-spot'), e.clientX, e.clientY, b == 3);
    };


    // onviewchanged handler definition
    function onViewPortChanged(vp) {
        cutout = vp.cutout.join();

        var className = '';

        if (vp.z == 1)
            className = '';
        else if (vp.x == -vp.maxX && vp.y == -vp.maxY)
            className = 'bottomright';
        else if (vp.x == -vp.maxX && vp.y == vp.maxY)
            className = 'topright';
        else if (vp.x == vp.maxX && vp.y == -vp.maxY)
            className = 'bottomleft';
        else if (vp.x == vp.maxX && vp.y == vp.maxY)
            className = 'topleft';

        if (el.parentNode.className != className)
            el.parentNode.className = className;
    };


    // reloading the image if necessary
    function reloadImage() {

        if (!img)
            return;

        if (window.getComputedStyle(img).visibility == 'visible') {

            if (isEmpty) {
                isEmpty = false;
                session = getSessionId();
            }

            img.src = img.getAttribute('data-src') + '&w=' + cutout + '&s=' + session;
        }
        else if (!isEmpty) {
            isEmpty = true;
            img.src = 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNgYAAAAAMAASsJTYQAAAAASUVORK5CYII=';
        }
    };


    // shows the touch effect
    function showTouchEffect(el, clientX, clientY, secondary) {

        // initialisation
        el.className = '';
        el.style.left = clientX + 'px';
        el.style.top = clientY + 'px';

        el.classList.add('fadeIn');
        void el.offsetWidth;
        el.classList.add('animate');
        el.classList.add(secondary ? 'fadeOutBig' : 'fadeOutSmall');
    };

    // binding the actions
    bindActions();
};