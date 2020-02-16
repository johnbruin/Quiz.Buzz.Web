const GameState = {
    Joining: 0,
    Answering: 1
};

var Players = new Array();
var State;
var RightAnswerColor = "";

function buzzerEvent(color, playerId) {
    var player;
    var index;
    if (State === GameState.Joining) { 
        if (color === AnswerColor.Red) {
            if (findPlayer(playerId) < 0) {
                if (Players.length === 0) {
                    StartTimer(10);
                }                
                player = {
                    PlayerId: playerId,
                    Score: 0                    
                };
                Players.push(player);
                index = Players.length - 1;
                Players[index].Name = "PLAYER " + (index + 1);
                $("#Players").append("<div id='Player" + index + "' class='Player'><div id='Name" + index + "'></div><div id='Score" + index + "'></div></div>");
                $("#Name" + index).html(Players[index].Name);
                $("#Score" + index).html(("0000000" + Players[index].Score).slice(-7));
                sendWSCommand(Command.LightOn, playerId);                
            }
        }
    }

    if (State === GameState.Answering) {
        index = findPlayer(playerId);
        if (Players[index].Answer === "") {
            Players[index].Answer = color;
            $("#Name" + index).addClass(color);
            sendWSCommand(Command.LightOff, playerId);
        }        
    }
}

function findPlayer(playerId) {
    for (var i = 0; i < Players.length; i++) {
        if (Players[i].PlayerId === playerId) {
            return i;
        }
    }
    return -1;
}

function setAnswers(rightAnswer) {

    //Init player answers
    for (var i = 0; i < Players.length; i++) {
        Players[i].Answer = "";
        $("#Name" + i).removeClass(AnswerColor.Blue);
        $("#Name" + i).removeClass(AnswerColor.Orange);
        $("#Name" + i).removeClass(AnswerColor.Green);
        $("#Name" + i).removeClass(AnswerColor.Yellow);
        sendWSCommand(Command.LightOn, Players[i].PlayerId);             
    }

    switch (rightAnswer) {
        case 0:
            RightAnswerColor = AnswerColor.Blue;
            break;
        case 1:
            RightAnswerColor = AnswerColor.Orange;
            break;
        case 2:
            RightAnswerColor = AnswerColor.Green;
            break;
        case 3:
            RightAnswerColor = AnswerColor.Yellow;
            break;
    }
}

function StartTimer(seconds) {
    sec = 10;
    Timer();
}

function Timer() {
    if (sec > 0) {
        sec--;
        tim = setTimeout(Timer, 1000);
    }
    else {
        if (State === GameState.Answering) {
            sendWSCommand("RESET", -1);
            for (var i = 0; i < Players.length; i++) {
                if (Players[i].Answer === RightAnswerColor) {
                    sendWSCommand("LIGHTFLASH", Players[i].PlayerId);
                    Players[i].Score = Players[i].Score + 100;
                }
                $("#Score" + i).html(("0000000" + Players[i].Score).slice(-7));
            }            
        }
        setTimeout(function () {
            setAnswers(1);
            State = GameState.Answering;
            StartTimer(10);
            $("#Answers").show();
        }, 5000);        
    }
    var secDisplay = sec % 60;
    $("#Timer").html(secDisplay + " Seconds");
}

function Init() {
    initWS();
    $("#Answers").hide();
    State = GameState.Joining; 
}

$(document).ready(function () {
    Init();    
});