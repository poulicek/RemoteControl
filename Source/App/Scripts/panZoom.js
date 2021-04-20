function enablePanZoom(el, onClickHandler, onViewChangedHandler) {

    var imgSize = { width: 0, height: 0 };

    // viewport object
    var viewport = {
        x: 0,
        y: 0,
        z: 1,

        rawX: -1,
        rawY: -1,

        maxX: 0,
        maxY: 0,

        rangeX: 0,
        rangeY: 0,

        overflowX: 0,
        overflowY: 0,

        cutout: [],

        // resets the viewport's position
        resetPosition: function () {
            this.rawX = -1;
            this.rawY = -1;
            this.z = 1;
            this.setPosition(0, 0, false);
        },

        // sets the given zoom
        zoom: function (zoom, offset) {

            zoom = Math.round(100 * zoom) / 100;

            if (zoom < 1)
                zoom = 1;

            if (zoom == this.z)
                return;

            var ratio = zoom / this.z;
            this.z = zoom;

            var shiftX = Math.round(this.x * ratio - offset.x * (ratio - 1));
            var shiftY = Math.round(this.y * ratio - offset.y * (ratio - 1));

            this.setPosition(shiftX, shiftY, true);
        },

        // set the given position
        setPosition: function (x, y, panning) {

            if (this.rawX == x && this.rawY == y)
                return;

            this.rawX = x;
            this.rawY = y;

            this.maxX = Math.max(0, Math.floor(this.z * this.rangeX - window.innerWidth / 2));
            this.maxY = Math.max(0, Math.floor(this.z * this.rangeY - window.innerHeight / 2));

            this.x = Math.max(-this.maxX, Math.min(this.maxX, x));
            this.y = Math.max(-this.maxY, Math.min(this.maxY, y));

            this.overflowX = Math.sign(x) * Math.max(0, Math.abs(x) - this.maxX);
            this.overflowY = Math.sign(y) * Math.max(0, Math.abs(y) - this.maxY);

            this.cutout = getCutout(this.x, this.y, this.z, this.maxX, this.maxY);

            this.report(panning);
        },

        // sets the translation by given offset
        pan: function (offsetX, offsetY) {
            this.setPosition(this.x + offsetX, this.y + offsetY, true);
        },

        // returns the css transformation value
        getTransformation: function () {
            return 'matrix(' + this.z + ', 0, 0, ' + this.z + ', ' + this.x + ', ' + this.y + ')';
        },

        // propagates the current state of the viewpoirt
        report: function (panning) {
            if (onViewChangedHandler && isLandScape())
                onViewChangedHandler(this, panning);
        }
    };


    // event handlers
    var eventHandlers = {

        // context variables
        lastZoom: null,
        lastTouch: null,
        firstTouch: null,
        touchMoved: false,

        bind: function (el) {
            window.addEventListener('resize', this.onWindowResize);
            el.addEventListener('load', this.onLoad);
            el.addEventListener('touchstart', this.onTouchStart);
            el.addEventListener('touchmove', this.onTouchMove);
            el.addEventListener('touchend', this.onTouchEnd);
            el.addEventListener('wheel', this.onWheel);
            el.addEventListener('click', this.onClick);

            this.onLoad();
        },

        onTouchStart: function (e) {

            // reset is needed because Android (new touch may have same Id as previous one)
            var touch = getTouch(e.touches);

            eventHandlers.lastZoom = viewport.z;
            eventHandlers.firstTouch = touch;
            eventHandlers.lastTouch = touch;
            eventHandlers.touchMoved = touch.touchesCount > 1;

            if (touch.touchesCount == 1) {
                setTimeout(function () { eventHandlers.onLongTouch(touch); }, 800);
            }
        },


        onLongTouch: function (t) {

            if (eventHandlers.touchMoved || eventHandlers.firstTouch !== t)
                return;

            t.which = 3; // setting the secondary button
            eventHandlers.onClick(t);
        },

        onTouchEnd: function () {

            viewport.report(false);

            if (!eventHandlers.touchMoved)
                eventHandlers.onClick(eventHandlers.firstTouch);

            eventHandlers.firstTouch = null;
            eventHandlers.lastTouch = null;
        },

        onTouchMove: function (e) {

            try {
                var touch = getTouch(e.touches);

                // detecting new touch combination
                if (!eventHandlers.lastTouch || touch.id != eventHandlers.lastTouch.id) {
                    eventHandlers.lastZoom = viewport.z;
                    eventHandlers.firstTouch = touch;

                    // consider the scene moved if multiple touches are detected
                    if (touch.touchesCount > 1)
                        eventHandlers.touchMoved = true;
                }
                else {
                    // panning and zooming
                    // on iOS it can be handled by gestureMove event providing scale directly but it doesn't support Android
                    if (touch.dist && eventHandlers.firstTouch.dist)
                        viewport.zoom(eventHandlers.lastZoom * touch.dist / eventHandlers.firstTouch.dist, getOffset(touch));
                    viewport.pan(touch.clientX - eventHandlers.lastTouch.clientX, touch.clientY - eventHandlers.lastTouch.clientY);
                }


                // setting the moved property if is threshold exceeded
                if (!eventHandlers.touchMoved) {
                    var dist = Math.hypot(touch.clientX - eventHandlers.firstTouch.clientX, touch.clientY - eventHandlers.firstTouch.clientY);
                    if (dist > 5)
                        eventHandlers.touchMoved = true;
                }

                eventHandlers.lastTouch = touch;
                update(el);
            }
            catch (e) {
                alert(e.toString());
            }
        },

        onWindowResize: function () {

            setRange(el);
            viewport.resetPosition();
            update(el);
        },

        onLoad: function () {

            imgSize = {
                width: el.naturalWidth ? el.naturalWidth : el.clientWidth,
                height: el.naturalHeight ? el.naturalHeight : el.clientHeight
            }

            setRange(el);
            update(el);
        },

        onClick: function (e) {

            if (eventHandlers.touchMoved)
                return;

            // preventing the onClick to be called twice
            eventHandlers.touchMoved = true;

            if (!onClickHandler)
                return;

            var coords = getRelativeCoords(e.clientX, e.clientY);
            if (coords)
                onClickHandler(e, coords.x, coords.y, e.which);
        },

        onWheel: function (e) {

            e.preventDefault();

            // support of mouse on PC
            if (e.ctrlKey)
                viewport.zoom(Math.max(1, viewport.z * Math.pow(2, -e.deltaY / 100)), getOffset(e));
            else if (e.shiftKey)
                viewport.pan(-e.deltaY, 0);
            else
                viewport.pan(-e.deltaX, -e.deltaY);

            update(el);
        }
    };

    // HELPERS

    // returns the central touch object
    function getTouch(touches) {

        if (touches.length == 0)
            return null;

        if (touches.length == 1) {
            var t = touches[0];
            return {
                clientX: Math.floor(t.pageX),
                clientY: Math.floor(t.pageY),
                id: t.identifier,
                touchesCount: touches.length,
                currentTarget: t.target
            };
        }

        var t1 = touches[0];
        var t2 = touches[1];
        return {
            clientX: Math.floor((t1.pageX + t2.pageX) / 2),
            clientY: Math.floor((t1.pageY + t2.pageY) / 2),
            id: t1.identifier + t2.identifier,
            touchesCount: touches.length,
            currentTarget: t1.target,
            dist: Math.hypot(t1.pageX - t2.pageX, t1.pageY - t2.pageY)
        };
    };


    // returns the relative cutout window to be displyed
    function getCutout(x, y, z, maxX, maxY) {

        // getting the virtual size of the image if it was stretched to the whole screen
        var s = getViewportSize(el);

        if (!z || !s.height || !s.height)
            return [];

        // relative width of the cutout 0-1 (size of the screen vs. size of the image)
        var w = (s.width / z) / imgSize.width;
        var h = (s.height / z) / imgSize.height

        // relative location of the cutout 0-1 (max values mean ho much can viewpoirt shift from the center)
        var ratioX = maxX == 0 ? 0 : (1 + -x / maxX) / 2;
        var ratioY = maxY == 0 ? 0 : (1 + -y / maxY) / 2;

        // multiplier to ensure correct resolution of numbers
        var r = 100000;

        return [
            Math.floor(r * ratioX),
            Math.floor(r * ratioY),
            Math.ceil(r * w),
            Math.ceil(r * h)];
    };


    // updates the pan range
    function setRange() {

        var size = getImageSize(el);
        viewport.rangeX = Math.floor(size.width / 2);
        viewport.rangeY = Math.floor(size.height / 2);

        viewport.setPosition(viewport.x, viewport.y, false);
    };


    // returns the relative coordinates in the image
    function getRelativeCoords(clientX, clientY) {

        var bounds = getImageBounds();
        var imgPoint = getImagePoint(bounds, clientX, clientY);

        // computing the real coordinates within the original size of the image
        var coords = {
            x: Math.floor(10000 * imgPoint.x / bounds.width) / 10000,
            y: Math.floor(10000 * imgPoint.y / bounds.height) / 10000
        };

        return coords.x >= 0 && coords.x < 1 && coords.y >= 0 && coords.y < 1 ? coords : null;
    };


    // returns the current image bounds
    function getImageBounds() {

        var clientSize = getImageSize(el);
        clientSize.width *= viewport.z;
        clientSize.height *= viewport.z;

        // computing the offset from center, because the image and the screen are co-centric
        return {
            x: Math.floor(clientSize.width / 2 - viewport.x),
            y: Math.floor(clientSize.height / 2 - viewport.y),
            width: Math.floor(clientSize.width),
            height: Math.floor(clientSize.height),
        };

    };


    // returns the pixel coordinates in the image
    function getImagePoint(bounds, clientX, clientY) {

        // computing the offset from center, because the image and the screen are co-centric
        return {
            x: Math.floor((clientX - window.innerWidth / 2) + bounds.x),
            y: Math.floor((clientY - window.innerHeight / 2) + bounds.y)
        };
    };


    // returns the size of the image on the screen
    function getImageSize(el) {

        if (!imgSize.height)
            return { width: 0, height: 0 };

        var ratio = imgSize.width / imgSize.height;

        var slopeEl = Math.abs(el.clientWidth / el.clientHeight - 1);
        var slopeImg = Math.abs(ratio - 1);

        return slopeImg < slopeEl
            ? { width: Math.floor(el.clientHeight * ratio), height: el.clientHeight }
            : { width: el.clientWidth, height: Math.floor(el.clientWidth / ratio) };
    };


    // returns the resolution of the viewport depending on the image size
    function getViewportSize(el) {

        if (!imgSize.height)
            return { width: 0, height: 0 };

        var ratio = el.clientWidth / el.clientHeight;

        var slopeEl = Math.abs(ratio - 1);
        var slopeImg = Math.abs(imgSize.width / imgSize.height - 1);

        return slopeImg < slopeEl
            ? { width: Math.floor(imgSize.height * ratio), height: imgSize.height }
            : { width: imgSize.width, height: Math.floor(imgSize.width / ratio) };
    };


    // returns the position relative to the center of the element
    function getOffset(e) {
        return { x: e.clientX - el.clientWidth / 2, y: e.clientY - el.clientHeight / 2 };
    };


    // updates the object's transformation
    function update(el) {
        el.style.transform = viewport.getTransformation();
    };


    // resets the view
    function resetView() {
        viewport.resetPosition();
        update(el);
    };

    // EXECUTION

    el.resetView = resetView;
    eventHandlers.bind(el);
};