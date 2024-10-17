mergeInto(LibraryManager.library, {
    GetUserInfo: function() {
        //userInfo from index.html
        var bufferSize = lengthBytesUTF8(userInfo) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(userInfo, buffer, bufferSize);
        return buffer;
    }
});