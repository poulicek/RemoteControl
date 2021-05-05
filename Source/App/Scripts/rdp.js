function enableRemoteControl(el) {

    var cutout = '';
    var img = null;
    var session = 0;
    var screen = 0;
    var isEmpty = true;
    var scrollDir = { x: 0, y: 0 };
    var lastClick = null;
    var lastCoords = { x: 0, y: 0 };
    var cursorOn = false;

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
            img.addEventListener('transitionend', onTransitionEnd);
            img.bound = true;
            img.zoomOut = zoomOut;
            img.switchScreen = switchScreen;
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


    // handles transition end event
    function onTransitionEnd(e) {
        img.classList.remove('zooming');
    };


    // onclick handler definition
    function onClick(e, x, y, z, b) {
        var time = new Date().getTime();
        if (lastClick && time - lastClick.time < 500 && lastClick.b == b) {
            x = lastClick.x;
            y = lastClick.y;
        }

        if (isLandScape()) {

            if (b == 3 && z == 1)
                zoomIn(e);
            else {
                sendRequest(getUrl(el.href, '&x=' + Math.floor(100000 * x) + "&y=" + Math.floor(100000 * y) + "&b=" + (b ? b : '') + '&e=' + screen));
                showTouchEffect(document.getElementById('click-spot'), e.clientX, e.clientY, b == 3);
                lastClick = { x: x, y: y, b: b, time: time };
                reloadImage();
            }
        }
    };


    // performs scrolling
    function onScroll(overflowX, overflowY, x, y) {
        if (isLandScape())
            sendRequest(getUrl(el.href, '&x=' + Math.floor(100000 * overflowX) + "&y=" + Math.floor(100000 * overflowY) + '&mx=' + Math.floor(100000 * x) + "&my=" + Math.floor(100000 * y) + "&b=2" + '&e=' + screen));
        reloadImage();
    };


    // moves the mose to the given position
    function onMouseMove(x, y) {
        sendRequest(getUrl(el.href, '&x=' + Math.floor(100000 * x) + "&y=" + Math.floor(100000 * y) + "&b=0" + '&e=' + screen));
    };


    // handles the alternative mouse event
    function onAlternativeMouse(e, holding, moved) {

        if (!holding && !moved) {
            sendRequest(getUrl(el.href, '&x=' + Math.floor(100000 * e.x) + "&y=" + Math.floor(100000 * e.y) + "&b=" + e.b + '&e=' + screen));
            reloadImage();
            lastClick = null;
        }        
    };


    // onviewchanged handler definition
    function onViewPortChanged(vp, isPanning, moved) {

        // updating the view
        cutout = vp.cutout.join();
        cursorOn = false; // INSTALLED && isPanning;
        updateView(el, vp, isPanning);

        // handling scorlling
        tryScroll(vp, isPanning);

        // moving the mouse of the viewport coordinates got updated
        if (vp.coords && (lastCoords.x != vp.coords.x || lastCoords.y != vp.coords.y)) {
            onMouseMove(vp.coords.x, vp.coords.y);
            lastCoords = vp.coords;
        }
    };


    // updates the UI of the view
    function updateView(el, vp, isPanning) {

        var className = isPanning ? 'panning' : '';

        if (cursorOn)
            className += ' cursor';

        if (vp.z == 1)
            className += ' zoomed-out';

        if (vp.x == -vp.maxX && vp.y == vp.maxY)
            className += ' topright';
        else if (vp.x == -vp.maxX && vp.y == -vp.maxY)
            className += ' bottomright';
        else if (vp.x == vp.maxX && vp.y == -vp.maxY)
            className += ' bottomleft';
        else if (vp.x == vp.maxX && vp.y == vp.maxY)
            className += ' topleft';

        if (el.parentNode.className != className)
            el.parentNode.className = className;

    };


    // tries to perform scrolling depending on viewport parameters
    function tryScroll(vp, isPanning) {

        // recording the allowed scrolling direction when the panning stops
        // this is to avoid unexpected scrolling (still may happen during zooming)
        if (!isPanning) {
            scrollDir.x = vp.maxX == Math.abs(vp.x) ? Math.sign(vp.x) : NaN;
            scrollDir.y = vp.maxY == Math.abs(vp.y) ? Math.sign(vp.y) : NaN;
        }

        if (vp.overflowX || vp.overflowY) {

            // detecting the scrolling distance if the overflow matches the alled scrolling position
            var scrollX = scrollDir.x == 0 || Math.sign(vp.overflowX) == scrollDir.x ? vp.overflowX / vp.rangeX : 0;
            var scrollY = scrollDir.y == 0 || Math.sign(vp.overflowY) == scrollDir.y ? vp.overflowY / vp.rangeY : 0;

            if (scrollX || scrollY)
                onScroll(scrollX, scrollY, vp.coords ? vp.coords.x : 0, vp.coords ? vp.coords.y : 0);
        }
    };


    // reloading the image if necessary
    function reloadImage() {

        if (!img || !document.getElementById(img.id)) {
            img = null;
            return;
        }

        if (isLandScape()) {

            if (isEmpty) {
                isEmpty = false;
                session = getSessionId();
            }

            img.src = img.getAttribute('data-src') + '&w=' + cutout + '&s=' + session + '&e=' + screen +'&u=' + (cursorOn ? 1 : 0);
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


    // zooms in the canvas
    function zoomIn(e) {
        img.classList.add('zooming');
        img.zoomIn(e, 5);
    };


    // zooms in the canvas
    function zoomOut(e) {
        img.classList.add('zooming');
        img.resetView();
        reloadImage();
    };

    // switches the screens
    function switchScreen(e) {
        img.resetView();
        screen++;
        reloadImage();
    };

    // binding the actions
    bindActions();
};