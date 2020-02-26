var NumberOfQuestions;

const GameState = {
    Joining: 0,
    Answering: 1,
    Start: 2,
    NextQuestion: 3,
    GameOver: 4
};

var sec;
var Players;
var State;
var RightAnswerColor = "";

var Questions;
var QuestionNumber;
var QuestionType;

var AnsweringSound;
var TimeOverSound;
var GameOverSound;

function buzzerEvent(color, playerId) {
    var player;
    var index;

    if (State === GameState.Joining) { 
        if (color === AnswerColor.Red) {
            if (findPlayer(playerId) < 0) {
                if (Players.length === 0) {
                    IntroSound.play();
                    GetQuestions();
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

        //You cannot answer questions with the red button
        if (color === "RED") {
            return;
        }

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
            if (QuestionNumber === NumberOfQuestions) {
                GameOver();
            }
            else {
                NextQuestion();
            }
        }
    }

    if (State === GameState.GameOver) {
        if (color === AnswerColor.Red) {
            sendWSCommand("RESET", -1);
            Init();
        }
    }
}

function GameOver() {
    $("#btnOK").fadeOut();
    $("#Question").html("GAME OVER");
    $("#Players").html("");
    $("#Answers").hide();

    Players.sort(function (a, b) {
        return -(a.Score - b.Score);
    });

    var winners = new Array();
    winners.push(Players[0]);

    var i;
    for (i = 0; i < Players.length; i++) {

        if (i > 0 && winners[0].Score === Players[i].Score) {
            winners.push(Players[i]);
        }
    }

    if (winners.length > 1) {
        $("#PlayerScoresText").html("THE WINNERS ARE");
    }
    else {
        $("#PlayerScoresText").html("THE WINNER IS");
    }

    for (i = 0; i < winners.length; i++) {
        if (winners[i]) {
            $("#PlayerScoresPlayers").append("<div id='Player" + i + "' class='Player'><div id='Name" + i + "'></div><div id='Score" + i + "'></div></div>");
            $("#Name" + i).html(Players[i].Name);
            $("#Score" + i).html(Players[i].Score);
        }
    }

    setTimeout(function () {
        State = GameState.GameOver;
        $("#Question").html("PRESS THE RED BUTTON TO CONTINUE");
        $("#btnOK").fadeIn();
    }, 5000); 
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
            AnsweringSound.stop();
            TimeOverSound.play();

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

        if (State === GameState.Joining) { 
            timeout = 0;
        }

        setTimeout(function () {
            State = GameState.NextQuestion;
            InitPlayers();
            $("#trivia_settings").hide();
            $("#Answers").fadeOut();
            $("#TimerOuter").fadeOut();
            $("#Timer").fadeOut();
            $("#Question").html("PRESS THE RED BUTTON TO CONTINUE");
            $("#btnOK").html("OK");
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

    $("#Question").html("QUESTION " + (QuestionNumber + 1) + " OF " + NumberOfQuestions + "<BR/>" + question.question);

    var correctAnswer;
    if (QuestionType === "boolean") {
        if (question.correct_answer === "True") {
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
    IntroSound.stop();
    AnsweringSound.play();
    StartTimer();

    $("#Question").show();
    $("#Answers").fadeIn();

    QuestionNumber++;
}

function GetQuestions() {

    NumberOfQuestions = Number($("#trivia_amount").val());
    var category = $("#trivia_category option:selected").val();
    var difficulty = $("#trivia_difficulty option:selected").val();

    console.log('GetQuestions: ' + NumberOfQuestions + '; ' + category + "; " + difficulty);

    let url = 'https://opentdb.com/api.php' + '?amount=' + NumberOfQuestions;
    if (category !== "any") {
        url = url + '&category=' + category;
    }
    if (difficulty !== "any") {
        url = url + '&difficulty=' + difficulty;
    }

    console.log(url);

    fetch(url)
        .then(resp => resp.json())
        .then(function (data) {
            questions = data.results;
            questions.map(function (question) {
                Questions.push(question);
                console.log("Question " + Questions.length + " of " + NumberOfQuestions + " added.");
            });
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

    State = GameState.Joining;
    Players = new Array();
    Questions = new Array();
    $("#Players").html("");
    $("#PlayerScoresText").html("");
    $("#PlayerScoresPlayers").html("");
    QuestionNumber = 0;

    $("#Question").html("PRESS THE RED BUTTON ON THE CONTROLLER TO JOIN THE GAME");

    $("#btnOK").html("JOIN");
    $("#btnOK").show();
    
    $("#Answers").hide();
    $("#TimerOuter").hide();
    $("#Timer").hide();
    $("#trivia_settings").show();

    $('#TimerOuter').circleProgress({
        startAngle: -Math.PI / 2,
        fill: { color: 'lightgray' },
        value: 1,
        thickness: 5,
        size: 100,
        animation: false
    });    
}

function Click(clickedColor) {
    buzzerEvent(clickedColor, -1);
}

function InitSound() {

    IntroSound = new Howl({
        src: ['assets/sound/Fruitbank_T001.sid_MOS6581R3.mp3']
    });

    GameOverSound = new Howl({
        src: ['assets/sound/Parallax_T002.sid_MOS6581R2.mp3']
    });

    AnsweringSound = new Howl({
        src: ['assets/sound/Super_Trucker_T001.sid_MOS6581R2.mp3']
    });

    TimeOverSound = new Howl({
        src: ['assets/sound/Fruitbank_T003.sid_MOS6581R3.mp3']
    });
}

$(document).ready(function () {
    initWS();
    Init();
    InitSound();

    $("#btnOK").click(function () { Click(AnswerColor.Red); });
    $("#BLUE").click(function () { Click(AnswerColor.Blue); });
    $("#ORANGE").click(function () { Click(AnswerColor.Orange); });
    $("#GREEN").click(function () { Click(AnswerColor.Green); });
    $("#YELLOW").click(function () { Click(AnswerColor.Yellow); });
});