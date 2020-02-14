var ws;
var tests = {
    websocket: 'WebSocket' in window
};

function setupWebSocket() {
    // Instantiate a new WebSocket, passing in server url.
    // A handshake is made and a connection is established.
    ws = new WebSocket(WebSocketServerURL);

    // Captures errors
    ws.onerror = function (e) {
        console.log('Problem with connection: ' + e.message);
    };

    // When connection opens
    ws.onopen = function () {
        console.log('Client connected');
        ws.send("CLIENT");
    };

    // Captures messages from the server.
    ws.onmessage = function (e) {
        if (e.data === "CLIENT" || e.data === "QUIT") {
            return;
        }        
        console.log(e.data);

        if (e.data.indexOf('RED') === 0) {
            var playerId = e.data.split("RED").join("");
            ws.send("LIGHTSWITCH" + playerId);
        }
    };

    // When connection closes
    ws.onclose = function () {
        console.log('Closed connection');
    };
}

function initWS() {
    // WebSocket support test
    if (tests.websocket) {
        // Setting up the WebSocket
        setupWebSocket();
    } else {
        console.log('Your browser doesn´t support WebSocket!');
        return;
    }
}

// Run init function when DOM  content has been loaded
document.addEventListener('DOMContentLoaded', initWS, false);