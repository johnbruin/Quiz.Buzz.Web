const AnswerColor = {
    Red: 'RED',
    Blue: 'BLUE',
    Orange: 'ORANGE',
    Green: 'GREEN',
    Yellow: 'YELLOW'
};

const Command = {
    LightSwitch: 'LIGHTSWITCH',
    LightOn: 'LIGHTON',
    LightOff: 'LIGHTOFF',
    LightFlash: 'LIGHTFLASH'
};

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
        console.log(e.data);

        var playerId = 0;
        var color = "";
        if (e.data.indexOf(AnswerColor.Red) === 0) {
            playerId = e.data.split(AnswerColor.Red).join("");
            color = AnswerColor.Red;
        }
        else if (e.data.indexOf(AnswerColor.Blue) === 0) {
            playerId = e.data.split(AnswerColor.Blue).join("");
            color = AnswerColor.Blue;
        }
        else if (e.data.indexOf(AnswerColor.Orange) === 0) {
            playerId = e.data.split(AnswerColor.Orange).join("");
            color = AnswerColor.Orange;
        }
        else if (e.data.indexOf(AnswerColor.Green) === 0) {
            playerId = e.data.split(AnswerColor.Green).join("");
            color = AnswerColor.Green;
        }
        else if (e.data.indexOf(AnswerColor.Yellow) === 0) {
            playerId = e.data.split(AnswerColor.Yellow).join("");
            color = AnswerColor.Yellow;
        }
        else {
            return;
        }
        buzzerEvent(color, playerId);
    };

    // When connection closes
    ws.onclose = function () {
        console.log('Closed connection');
    };
}

function sendWSCommand(command, playerId) {
    if (playerId > 0) {
        ws.send(command + playerId);
    }
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