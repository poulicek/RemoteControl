﻿function enablePanZoom(el, pointerClickHandler) {

    var imgSize = { width: el.clientWidth, height: el.clientHeight }; 

    // viewport object
    var viewport = {
        x: 0,
        y: 0,
        z: 1,

        rangeX: 0,
        rangeY: 0,

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

            this.setPosition(shiftX, shiftY);
        },

        // set the given position
        setPosition: function (x, y) {

            var maxX = Math.max(0, Math.floor(this.z * this.rangeX - window.innerWidth / 2));
            var maxY = Math.max(0, Math.floor(this.z * this.rangeY - window.innerHeight / 2));

            this.x = Math.max(-maxX, Math.min(maxX, x));
            this.y = Math.max(-maxY, Math.min(maxY, y));
        },

        // sets the translation by given offset
        pan: function (offsetX, offsetY) {
            this.setPosition(this.x + offsetX, this.y + offsetY);
        },

        // returns the css transformation value
        getTransformation: function () {
            return 'matrix(' + this.z + ', 0, 0, ' + this.z + ', ' + this.x + ', ' + this.y + ')';
        }
    };


    // event handlers
    var eventHandlers = {

        // context variables
        lastZoom: null,
        lastTouch: null,
        firstTouch: null,

        bind: function (el) {
            window.addEventListener('resize', this.onLoad);
            el.addEventListener('load', this.onLoad);
            el.addEventListener('click', this.onClick);
            el.addEventListener('touchstart', this.onTouchStart);
            el.addEventListener('touchmove', this.onTouchMove);
            el.addEventListener('touchend', this.onTouchEnd);
            el.addEventListener('wheel', this.onWheel);

            this.onLoad();
        },

        onTouchStart: function (e) {
            // reset is needed because Android (new touch may have same Id as previous one)
            // preventDefault cannot be called otherwise onClick doesn't work
            eventHandlers.lastTouch = null;
            eventHandlers.firstTouch = getTouch(e.touches);

            setTimeout(function (e) {
                if (eventHandlers.firstTouch && !eventHandlers.lastTouch) {
                    var e = eventHandlers.firstTouch;
                    e.which = 3;
                    eventHandlers.onClick(e);
                    eventHandlers.firstTouch = null;
                }
            }, 500);
        },

        onTouchEnd: function (e) {
            if (eventHandlers.firstTouch && !eventHandlers.lastTouch)
                eventHandlers.onClick(eventHandlers.firstTouch);

            eventHandlers.firstTouch = null;
        },

        onTouchMove: function (e) {

            try {
                e.preventDefault();

                var touch = getTouch(e.touches);

                // detecting new touch combination
                if (!eventHandlers.lastTouch || touch.id != eventHandlers.lastTouch.id) {
                    eventHandlers.lastZoom = viewport.z;
                    eventHandlers.firstTouch = touch;
                }
                else {

                    // panning and zooming
                    // on it iOS can be handled by gestureMove event providing scale directly but it doesn't support Android
                    if (touch.dist && eventHandlers.firstTouch.dist)
                        viewport.zoom(eventHandlers.lastZoom * touch.dist / eventHandlers.firstTouch.dist, getOffset(touch));
                    viewport.pan(touch.clientX - eventHandlers.lastTouch.clientX, touch.clientY - eventHandlers.lastTouch.clientY);
                }

                eventHandlers.lastTouch = touch;
                update(el);
            }
            catch (e) {
                alert(e.toString());
            }
        },

        onLoad: function (e) {

            imgSize = {
                width: el.naturalWidth ? el.naturalWidth : el.clientWidth,
                height: el.naturalHeight ? el.naturalHeight : el.clientHeight
            }

            setRange(el);
        },

        onClick: function (e) {
            if (!pointerClickHandler)
                return;

            var coords = getRealCoords(e.clientX, e.clientY);
            if (coords)
                pointerClickHandler(coords.x, coords.y, e.which);
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
                time: new Date().getTime()
            };
        }

        var t1 = touches[0];
        var t2 = touches[1];
        return {
            clientX: Math.floor((t1.pageX + t2.pageX) / 2),
            clientY: Math.floor((t1.pageY + t2.pageY) / 2),
            id: t1.identifier + t2.identifier,
            time: new Date().getTime(),
            dist: Math.hypot(t1.pageX - t2.pageX, t1.pageY - t2.pageY)
        };
    };


    // updates the pan range
    function setRange() {

        var size = getRealSize(el);
        viewport.rangeX = Math.floor(size.width / 2);
        viewport.rangeY = Math.floor(size.height / 2);
        update(el);
    };


    // returns the real coordinates in the image
    function getRealCoords(clientX, clientY) {

        var realSize = getRealSize(el);
        realSize.width *= viewport.z;
        realSize.height *= viewport.z;

        // computing the offset from center, because the image and the screen are co-centric
        var imgPoint = {
            x: (clientX - window.innerWidth / 2) - viewport.x + realSize.width / 2,
            y: (clientY - window.innerHeight / 2) - viewport.y + realSize.height / 2
        };

        // computing the real coordinates within the original size of the image
        var coords = {
            x: Math.floor(10000 * imgPoint.x / realSize.width) / 10000,
            y: Math.floor(10000 * imgPoint.y / realSize.height) / 10000
        };

        return coords.x >= 0 && coords.x < 1 && coords.y >= 0 && coords.y < 1 ? coords : null;
    };


    // returns the real size of the image
    function getRealSize(el) {

        var ratio = imgSize.width / imgSize.height;

        var slopeEl = Math.abs(el.clientWidth / el.clientHeight - 1);
        var slopeImg = Math.abs(ratio - 1);

        return slopeImg < slopeEl
            ? { width: Math.floor(el.clientHeight * ratio), height: el.clientHeight }
            : { width: el.clientWidth, height: Math.floor(el.clientWidth / ratio) };
    };


    // returns the position relative to the center of the element
    function getOffset(e) {
        return { x: e.clientX - el.clientWidth / 2, y: e.clientY - el.clientHeight / 2 };
    };


    // updates the object's transformation
    function update(el) {
        el.style.transform = viewport.getTransformation();
    };

    // EXECUTION

    eventHandlers.bind(el);
};