const GameState = {
    Joining: 0,
    Answering: 1
};

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
        $("#Name" + i).removeClass("Wrong");
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
    console.log(rightAnswer);
    console.log(RightAnswerColor);
}

function StartTimer(seconds) {
    sec = seconds;
    Timer();
}

function Timer() {
    if (sec > 0) {
        sec--;
        tim = setTimeout(Timer, 1000);
    }
    else {
        var timeout = 5000; 
        if (State === GameState.Answering) {

            //Show correct answer
            $("#Answers li").addClass("Wrong");
            $("#" + RightAnswerColor).removeClass("Wrong");

            sendWSCommand("RESET", -1);
            $("[id^=Name]").addClass("Wrong");
            for (var i = 0; i < Players.length; i++) {
                if (Players[i].Answer === RightAnswerColor) {
                    $("#Name" + i).removeClass("Wrong");
                    sendWSCommand("LIGHTFLASH", Players[i].PlayerId);
                    Players[i].Score = Players[i].Score + 100;
                }
                $("#Score" + i).html(Players[i].Score);
            }
        }
        else {
            timeout = 0;
        }

        setTimeout(function () {
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
            StartTimer(10);

            $("#Question").show();
            $("#Answers").show();

            QuestionNumber++;
        }, timeout);        
    }
    $("#Timer").html("00:" + ("00" + sec).slice(-2));
}

function GetQuestions() {
    let url = 'https://opentdb.com/api.php?amount=10&category=9&difficulty=easy';

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
    $("#Question").hide();
    $("#Answers").hide();
    $("#Answers li").addClass("Wrong");
    GetQuestions();
}

$(document).ready(function () {
    Init();    
});