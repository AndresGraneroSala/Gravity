mergeInto(LibraryManager.library, {
  IsMobile: function () {
    var ua = navigator.userAgent || navigator.vendor || window.opera;
    var isMobile = /android|iphone|ipad|ipod|windows phone/i.test(ua);
    return isMobile ? 1 : 0;
  }
});
