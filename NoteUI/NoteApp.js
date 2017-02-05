var NoteApp = function () {
    var self = this;
    self.ws = {};

    self.onopen = function () {
        $(".connStatus").html("connected");
    };

    self.onmessage = function (event) {
        console.log(event);

        if (event.data instanceof ArrayBuffer) {
            var binary = '';
            var bytes = new Uint8Array(event.data);
            for (var i = 0; i < bytes.byteLength; i++) {
                binary += String.fromCharCode(bytes[i]);
            }            
        }

        var returnAction = JSON.parse(event.data);

        $("#note").find('ul').prepend('<li><span>' + returnAction.Message + '</span></li>');
    };

    self.onerror = function (evt) {
        console.log(evt);
        $(".connStatus").html("disconnected");
    };

    self.onclose = function (evt) {
        console.log(evt);
    };

    self.init = function () {
        if ('WebSocket' in window) {
            self.ws = new WebSocket("ws://localhost:8181/api/Note");
        } else {
            return;
        }

        self.ws.binaryType = "arraybuffer";
        $(".connStatus").html("connecting....");
        self.setupSocketEvents();
        self.setupDomEvents();
    };

    self.send = function (message) {
        if (self.ws.readyState == WebSocket.OPEN) {
            var str = JSON.stringify({
                Message: message
            });
            self.ws.send(str);
        }
    };

    self.setupDomEvents = function () {
        $("#submit").click(function() {
            // getting the values that user typed
            var message = $("#message").val();
            if(message != '') {
                self.send(message);
            }
        });
    }

    self.setupSocketEvents = function () {
        self.ws.onopen = function (evt) { self.onopen(evt); };
        self.ws.onmessage = function (evt) { self.onmessage(evt); };
        self.ws.onerror = function (evt) { self.onerror(evt); };
        self.ws.onclose = function (evt) { self.onclose(evt); };
    }

}