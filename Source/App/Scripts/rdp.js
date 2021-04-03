﻿function enableRemoteControl(el) {

    // binds the actions
    function bindActions() {
        el.onclick = function () { return false; };
        bindFirstImage();
    }


    // binds the fist found image elment
    function bindFirstImage() {
        var imgEls = el.getElementsByTagName('img');
        if (imgEls.length > 0)
            bindImage(imgEls[0]);
    }


    // binds the image element
    function bindImage(img) {

        // enabling the pan and zoom
        enablePanZoom(img, onClick);

        // using primarily the data-src attribute as the 'src'
        // attribute would cause automatic load even when the image isn't visible
        if (img.src)
            img.setAttribute('data-src', img.src);

        // automatic loading of the image
        window.addEventListener('resize', function () { reloadImage(img); });
        img.addEventListener('load', onLoad);
        img.addEventListener('error', onError);

        reloadImage(img);
    };


    // reloading the image if necessary
    function reloadImage(img) {
        if (window.getComputedStyle(img).visibility == 'visible')
            img.src = img.getAttribute('data-src') + '&w=' + img.cutout;
    };


    // handles the image load event
    function onLoad(e) {
        setAppStatus();
        el.classList.add('loaded');
        reloadImage(e.currentTarget);
    }


    // handles the image error event
    function onError(e) {
        setAppStatus('status-error');
        el.classList.remove('loaded');
        var img = e.currentTarget;
        setTimeout(function () { reloadImage(img); }, 500);
    }


    // onclick function definition
    function onClick(e, x, y, b) {

        sendRequest(getUrl(el.href, '&x=' + x + "&y=" + y + "&b=" + b));
        reloadImage(e.currentTarget);
        showTouchEffect(document.getElementById('click-spot'), e.clientX, e.clientY);
    };


    // shows the touch effect
    function showTouchEffect(el, clientX, clientY) {

        // initialisation
        el.className = '';
        el.style.left = clientX + 'px';
        el.style.top = clientY + 'px';

        el.classList.add('fadeIn');
        void el.offsetWidth;
        el.classList.add('animate');
        el.classList.add('fadeOut');
    };

    // binding the actions
    bindActions();
};