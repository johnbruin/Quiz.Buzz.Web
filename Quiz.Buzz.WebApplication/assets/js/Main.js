const NUMBER_OF_QUESTIONS = 10;

const GameState = {
    Joining: 0,
    Answering: 1,
    Start: 2,
    NextQuestion: 3
};

var sec;
var Players = new Array();
var State;
var RightAnswerColor = "";
var Questions = new Array();
var QuestionNumber = 0;
var QuestionType;

function buzzerEvent(color, playerId) {
    var player;
    var index;

    if (State === GameState.Joining) { 
        if (color === AnswerColor.Red) {
            if (findPlayer(playerId) < 0) {
                if (Players.length === 0) {
                    StartTimer();
                }                
                player = {
                    PlayerId: playerId,
                    Score: 0,
                    Answer: ""
                };
                Players.push(player);
                index = Players.length - 1;
                Players[index].Name = "PLAYER " + (index + 1);
                $("#Players").append("<div id='Player" + index + "' class='Player'><div id='Name" + index + "'></div><div id='Score" + index + "'></div></div>");
                $("#Name" + index).html(Players[index].Name);
                $("#Score" + index).html(Players[index].Score);
                sendWSCommand(Command.LightOn, playerId);                
            }
        }
    }

    if (State === GameState.Answering) {
        //You cannot answer BOOLEAN questions with the green or yellow buttons
        if (QuestionType === "boolean" && (color === "GREEN" || color === "YELLOW")) {
            return;
        }

        index = findPlayer(playerId);
        if (Players[index].Answer === "") {
            Players[index].Answer = color;
            $("#Player" + index).addClass("Active");
            sendWSCommand(Command.LightOff, playerId);
        }        
    }

    if (State === GameState.NextQuestion) {
        if (color === AnswerColor.Red) {
            NextQuestion();
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

function StartTimer() {
    $("#TimerOuter").show();
    $("#Timer").show();
    sec = 10;
    Timer();
}

function Timer() {
    if (sec > 0) {
        sec = sec - .01;
        tim = setTimeout(Timer, 10);
    }
    else {
        var timeout = 5000; 
        if (State === GameState.Answering) {

            //Show correct answer
            $("#Answers li").addClass("Wrong");
            $("#" + RightAnswerColor).removeClass("Wrong");

            sendWSCommand("RESET", -1);
            $("[id^=Player]").addClass("Wrong");
            $("[id^=Player]").removeClass("Active");
            for (var i = 0; i < Players.length; i++) {
                if (Players[i].Answer === RightAnswerColor) {
                    $("#Player" + i).removeClass("Wrong");
                    $("#Player" + i).addClass(RightAnswerColor);
                    sendWSCommand("LIGHTFLASH", Players[i].PlayerId);
                    Players[i].Score = Players[i].Score + 100;
                }
                $("#Score" + i).html(Players[i].Score);
            }
            $("#TimerOuter").fadeOut();
            $("#Timer").fadeOut();
        }
        else {
            timeout = 0;
        }

        setTimeout(function () {
            State = GameState.NextQuestion;
            InitPlayers();
            $("#Answers").fadeOut();
            $("#TimerOuter").fadeOut();
            $("#Timer").fadeOut();
            $("#Question").html("PRESS THE RED BUTTON TO CONTINUE");
            $("#btnOK").fadeIn();
        }, timeout);        
    }
    $('#Timer').circleProgress({
        startAngle: -Math.PI / 2,
        fill: { color: 'lightgray' },
        value: 1 - sec / 10,
        thickness: 40,
        size: 80,
        animation: false
    });
}

function InitPlayers() {
    for (var i = 0; i < Players.length; i++) {
        Players[i].Answer = "";
        $("#Player" + i).removeClass(AnswerColor.Blue);
        $("#Player" + i).removeClass(AnswerColor.Orange);
        $("#Player" + i).removeClass(AnswerColor.Green);
        $("#Player" + i).removeClass(AnswerColor.Yellow);
        $("#Player" + i).removeClass("Wrong");
        sendWSCommand(Command.LightOn, Players[i].PlayerId);
    }
}

function NextQuestion() {
    $("#btnOK").fadeOut();
    var question = Questions[QuestionNumber];
    QuestionType = question.type;

    $("#Question").html(question.question);

    var correctAnswer;
    if (QuestionType === "boolean") {
        if (question.correctanswer === "True") {
            correctAnswer = 0;
        }
        else {
            correctAnswer = 1;
        }
        $("#BLUE").html("TRUE");
        $("#ORANGE").html("FALSE");
        $("#GREEN").hide();
        $("#YELLOW").hide();
    }
    else {
        var answers = new Array();
        answers.push(question.correct_answer);
        answers.push(question.incorrect_answers[0]);
        answers.push(question.incorrect_answers[1]);
        answers.push(question.incorrect_answers[2]);
        answers = shuffle(answers);
        console.log(answers);

        for (var i = 0; i < 4; i++) {
            if (answers[i] === question.correct_answer) {
                correctAnswer = i;
            }
        }

        $("#BLUE").html(answers[0]);
        $("#ORANGE").html(answers[1]);
        $("#GREEN").html(answers[2]);
        $("#YELLOW").html(answers[3]);

        $("#GREEN").show();
        $("#YELLOW").show();
    }

    $("#Answers li").removeClass("Wrong");
    setAnswers(correctAnswer);
    State = GameState.Answering;
    StartTimer();

    $("#Question").show();
    $("#Answers").fadeIn();

    QuestionNumber++;
}

function GetQuestions() {
    let url = 'https://opentdb.com/api.php?amount=' + NUMBER_OF_QUESTIONS + '&category=9&difficulty=easy';

    fetch(url)
        .then(resp => resp.json())
        .then(function (data) {
            questions = data.results;
            questions.map(function (question) {
                Questions.push(question);
            });
            console.log(Questions[1]);
            State = GameState.Joining;
        })
        .catch(err => { throw err; });
}

function shuffle(array) {
    var currentIndex = array.length, temporaryValue, randomIndex;

    // While there remain elements to shuffle...
    while (0 !== currentIndex) {

        // Pick a remaining element...
        randomIndex = Math.floor(Math.random() * currentIndex);
        currentIndex -= 1;

        // And swap it with the current element.
        temporaryValue = array[currentIndex];
        array[currentIndex] = array[randomIndex];
        array[randomIndex] = temporaryValue;
    }

    return array;
}

function Init() {
    initWS();

    $("#Question").html("PRESS THE RED BUTTON ON THE CONTROLLER TO JOIN THE GAME");

    $("#btnOK").hide();
    $("#Answers").hide();
    $("#TimerOuter").hide();
    $("#Timer").hide();

    $('#TimerOuter').circleProgress({
        startAngle: -Math.PI / 2,
        fill: { color: 'lightgray' },
        value: 1,
        thickness: 5,
        size: 100,
        animation: false
    });

    GetQuestions();
}

$(document).ready(function () {
    Init();    
});